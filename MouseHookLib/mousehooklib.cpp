#include "pch.h"

#if defined(_DEBUG)
#define DEBUG_PRINTF(...) DebugPrintf(__VA_ARGS__)
#else
#define DEBUG_PRINTF(...)
#endif

// WM_NOTIFY_LOCK_STATE
//   wParam: LOCKTYPE_LEFT or LOCKTYPE_RIGHT
//   lParam: One of the LOCKSTATE
#define WM_NOTIFY_LOCK_STATE    (WM_USER + 1)

// WM_NOTIFY_LOCK_STATE wParam
#define LOCKTYPE_LEFT       1
#define LOCKTYPE_RIGHT      2


enum LOCKSTATE {
    LOCKSTATE_OFF = 0,
    LOCKSTATE_ON = 1,
	LOCKSTATE_SUPPRESSED = 2
};

struct BUTTONCONTEXT {
	LPCTSTR name;
    WPARAM  wParamLockType;
    HANDLE  hClickTimer;
    bool    isButtonDown;
	LOCKSTATE   lockState;
	bool    isTimerEventFired;
	bool    isTimerCanceled;
};


static HANDLE g_hTimerQueue = nullptr;

static HWND g_hWndMarker = nullptr;

static HHOOK g_hMouseHook = nullptr;

static volatile bool g_isClickLockEnabled = false;

static volatile LPARAM g_lastMousePos = 0;      // 最新のマウス位置。アトミックに読み書きできるようにMAKELPARAM(x, y)で結合したマウス座標
static volatile LPARAM g_leftDownMousePos = 0;  // 左クリックマウス開始。アトミックに読み書きできるようにMAKELPARAM(x, y)で結合したマウス座標
static volatile LPARAM g_rightDownMousePos = 0; // 右クリックマウス開始。アトミックに読み書きできるようにMAKELPARAM(x, y)で結合したマウス座標
static volatile LPARAM g_markerOffset = 0;      // アトミックに読み書きできるようにMAKELPARAM(xOffset, yOffset)で結合したマーカーオフセット
static volatile bool g_allowMouseMove = false; // クリックロック判定中にマウス移動を許容するか
static volatile int g_allowMouseMoveDistancePx = 0; // クリックロック判定中に許容するマウス移動距離
static volatile int g_clickLockDelayMS = 0;
static volatile bool g_isMarkerPreview = false;


static CRITICAL_SECTION g_cs;
// ここから g_cs で保護された状態で参照・変更すること
static BUTTONCONTEXT g_leftButtonClickData;
static BUTTONCONTEXT g_rightButtonClickData;
// ここまで g_cs で保護された状態で参照・変更すること


inline int abs(int a)
{
    return (a < 0) ? -a : a;
}


#if defined(_DEBUG)

static void DebugPrintf(LPCTSTR format, ...)
{
    va_list args;
    va_start(args, format);
#ifdef UNICODE
	wchar_t buffer[256];
	vswprintf_s(buffer, sizeof(buffer) / sizeof(buffer[0]), format, args);
    OutputDebugStringW(buffer);
#else
    char buffer[256];
    vsprintf_s(buffer, sizeof(buffer) / sizeof(buffer[0]), format, args);
    OutputDebugStringA(buffer);
#endif
	va_end(args);
}

#endif


/*
 * @brief マーカーウィンドウの位置を更新する
 */
static void UpdateMarkerPos(bool setZOrder)
{
    LPARAM lastMousePos = g_lastMousePos;
	LPARAM markerOffset = g_markerOffset;
    int x = GET_X_LPARAM(lastMousePos) + GET_X_LPARAM(markerOffset);
    int y = GET_Y_LPARAM(lastMousePos) + GET_Y_LPARAM(markerOffset);
#if 1
    if (!g_leftButtonClickData.isButtonDown  &&
            !g_rightButtonClickData.isButtonDown &&
            (g_leftButtonClickData.lockState != LOCKSTATE_ON) &&
            (g_rightButtonClickData.lockState != LOCKSTATE_ON)){
        x = 0;
        y = 0;
	}
#endif
	UINT flags = SWP_NOACTIVATE | SWP_NOSIZE;
    if (!setZOrder) {
        flags |= SWP_NOZORDER;
	}
    SetWindowPos(g_hWndMarker, HWND_TOPMOST, x, y, 0, 0, flags);
}


/*
 * @brief クリックロック検出用のタイマーコールバック関数
 * @param lpParam BUTTONCONTEXTへのポインタ
 * @param TimerOrWaitFired タイマーが起動した場合はTRUE、待機オブジェクトがシグナル状態になった場合はFALSE
 */
static void CALLBACK ClickTimerProc(PVOID lpParam, BOOLEAN TimerOrWaitFired)
{
    // LowLevelMouseProc()内でg_csを取得した状態で DeleteTimerQueueTimer() が呼ばれたとき、
    // 同時にClickTimerProcが走りだすとデッドロックになってしまう。
    // そこでLowLevelMouseProc()内でDeleteTimerQueueTimerを呼ぶ前に isTimerCanceled を true にする。
    // ClickTimerProc内ではg_csを取得出来なくても、isTimerCanceledがtrueならば処理を中止する。
    // 単純にClickTimerProcの先頭でisTimerCanceledをチェックするだけだと、
    // isTimerCanceledのチェックとg_csの取得の間にLowLevelMouseProc()内でg_csを取得されて
    // デッドロックする可能性があるのでダメ。
    BUTTONCONTEXT* pBtnCtx = (BUTTONCONTEXT*)lpParam;
    while (TryEnterCriticalSection(&g_cs) == FALSE) {
        if (pBtnCtx->isTimerCanceled) {
            return;
        }
        Sleep(0);
    }
    DEBUG_PRINTF(TEXT("Click lock is activated. name: %s\r\n"), pBtnCtx->name);
    pBtnCtx->isTimerEventFired = true;
	pBtnCtx->lockState = (g_isClickLockEnabled) ? LOCKSTATE_ON : LOCKSTATE_SUPPRESSED;
    PostMessage(g_hWndMarker, WM_NOTIFY_LOCK_STATE, pBtnCtx->wParamLockType, pBtnCtx->lockState);
    bool r = DeleteTimerQueueTimer(g_hTimerQueue, pBtnCtx->hClickTimer, NULL);
    DEBUG_PRINTF(TEXT("DeleteTimerQueueTimer. name: %s, r: %d, hClickTimer: %p\n"), pBtnCtx->name, pBtnCtx->hClickTimer);
    pBtnCtx->hClickTimer = nullptr;
    LeaveCriticalSection(&g_cs);
}

/*
 * @brief クリックロック検出用のタイマーをキャンセルする
 * @param btnCtx ボタン状態のコンテキスト
 * @note g_cs で保護された状態で呼び出すこと
 */
static void CancelClickTimer(BUTTONCONTEXT& btnCtx)
{
	HANDLE hClickTimer = btnCtx.hClickTimer;
    if (hClickTimer != nullptr){
		btnCtx.isTimerCanceled = true;		// ClickTimerProcでg_csを取得出来なくても良いようにする
        bool r = DeleteTimerQueueTimer(g_hTimerQueue, hClickTimer, INVALID_HANDLE_VALUE);
        DEBUG_PRINTF(TEXT("DeleteTimerQueueTimer. name: %s, r: %d, hClickTimer: %p\n"), btnCtx.name, hClickTimer);
		btnCtx.hClickTimer = nullptr;
    }
}

/*
 * @brief マウスボタンが押されたときの処理
 * @param btnCtx ボタン状態のコンテキスト
 * @return true: マウスイベントを無視する
 */
static bool OnButtonDown(BUTTONCONTEXT& btnCtx)
{
    EnterCriticalSection(&g_cs);
    btnCtx.isButtonDown = true;
	CancelClickTimer(btnCtx);
	btnCtx.isTimerEventFired = false;
    btnCtx.isTimerCanceled = false;
    bool rct = CreateTimerQueueTimer(&btnCtx.hClickTimer, g_hTimerQueue, ClickTimerProc, &btnCtx, g_clickLockDelayMS, 0, WT_EXECUTEDEFAULT);
    DEBUG_PRINTF(TEXT("CreateTimerQueueTimer. name: %s rct: %d, hClickTimer: %p\n"), btnCtx.name, rct, btnCtx.hClickTimer);
    bool res = (btnCtx.lockState == LOCKSTATE_ON);
    LeaveCriticalSection(&g_cs);
    return res;
}

/*
 * @brief マウスボタンが放されたときの処理
 * @param btnCtx ボタン状態のコンテキスト
 * @return true: マウスイベントを無視する
 */
static bool OnButtonUp(BUTTONCONTEXT& btnCtx)
{
    EnterCriticalSection(&g_cs);
	btnCtx.isButtonDown = false;
	CancelClickTimer(btnCtx);
    if (!btnCtx.isTimerEventFired) {
        DEBUG_PRINTF(TEXT("Click lock is deactivated. name: %s\r\n"), btnCtx.name);
		btnCtx.isTimerEventFired = false;
        if (btnCtx.lockState != LOCKSTATE_OFF) {
            btnCtx.lockState = LOCKSTATE_OFF;
            PostMessage(g_hWndMarker, WM_NOTIFY_LOCK_STATE, btnCtx.wParamLockType, btnCtx.lockState);
        }
    }
    bool res = (btnCtx.lockState == LOCKSTATE_ON);
    LeaveCriticalSection(&g_cs);
    return res;
}

static void OnMouseMove(BUTTONCONTEXT& btnCtx, LPARAM downMousePos)
{
    if (!btnCtx.isButtonDown) {
        return;
    }
    if (g_allowMouseMove) {
        return;
    }
    EnterCriticalSection(&g_cs);
    if ((btnCtx.hClickTimer != nullptr) && (btnCtx.isTimerEventFired == false)) {
        int moveDx = abs(GET_X_LPARAM(g_lastMousePos) - GET_X_LPARAM(downMousePos));
        int moveDy = abs(GET_Y_LPARAM(g_lastMousePos) - GET_Y_LPARAM(downMousePos));
        if ((moveDx > g_allowMouseMoveDistancePx) || (moveDy > g_allowMouseMoveDistancePx)) {
            CancelClickTimer(btnCtx);
        }
    }
    LeaveCriticalSection(&g_cs);
}


/*
 * @brief WH_MOUSE_HOOK_LL コールバック関数
 * @param nCode フックコード
 * @param wParam マウスメッセージ
 * @param lParam MSLLHOOKSTRUCTへのポインタ
 * @return 1: マウスイベントを無視する, それ以外は CallNextHookEx() の戻り値
 */
static LRESULT CALLBACK LowLevelMouseProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	bool ignoreMouseEvent = false;
    if (nCode >= 0) {
        const MSLLHOOKSTRUCT* pMouse = reinterpret_cast<const MSLLHOOKSTRUCT*>(lParam);
		// マウスカーソル位置をグローバル変数に格納するにあたって
        // x, y座標をLPARAMに結合してアトミックに格納するので排他制御は要らない
		g_lastMousePos = MAKELPARAM(pMouse->pt.x, pMouse->pt.y);
        switch (wParam)
        {
        case WM_LBUTTONDOWN:
            g_leftDownMousePos = g_lastMousePos;
			ignoreMouseEvent = OnButtonDown(g_leftButtonClickData);
			UpdateMarkerPos(!g_rightButtonClickData.isButtonDown); // 左ボタンのみ押下時はZオーダーを更新
            break;
        case WM_LBUTTONUP:
            ignoreMouseEvent = OnButtonUp(g_leftButtonClickData);
            UpdateMarkerPos(false);
            break;
        case WM_RBUTTONDOWN:
            g_rightDownMousePos = g_lastMousePos;
            ignoreMouseEvent = OnButtonDown(g_rightButtonClickData);
			UpdateMarkerPos(!g_leftButtonClickData.isButtonDown); // 右ボタンのみ押下時はZオーダーを更新
            break;
        case WM_RBUTTONUP:
            ignoreMouseEvent = OnButtonUp(g_rightButtonClickData);
            UpdateMarkerPos(false);
            break;
		case WM_MOUSEMOVE:
            OnMouseMove(g_leftButtonClickData, g_leftDownMousePos);
            OnMouseMove(g_rightButtonClickData, g_rightDownMousePos);
            if (g_leftButtonClickData.isButtonDown ||
                    (g_leftButtonClickData.lockState == LOCKSTATE_ON) ||
                    g_rightButtonClickData.isButtonDown ||
                    (g_rightButtonClickData.lockState == LOCKSTATE_ON) ||
                    g_isMarkerPreview) {
                UpdateMarkerPos(false);
			}
            break;
        default:
            break;
        }
    }
    if (ignoreMouseEvent) {
        return 1;
    }
    // 次のフックプロシージャへ
    return CallNextHookEx(g_hMouseHook, nCode, wParam, lParam);
}

/*
 * @brief クリックロックを解除する
 * @param btxCtx ボタン状態のコンテキスト
 * @note g_cs で保護された状態で呼び出すこと
 */
static void DeactivateClickLock(BUTTONCONTEXT& btxCtx)
{
    CancelClickTimer(btxCtx);
    btxCtx.isTimerEventFired = false;
    btxCtx.lockState = LOCKSTATE_OFF;
	PostMessage(g_hWndMarker, WM_NOTIFY_LOCK_STATE, btxCtx.wParamLockType, btxCtx.lockState);
}

/*
 * @brief 初期化
 * @note 他の関数呼び出しに先だって一度だけ呼び出すこと
 */
extern "C" __declspec(dllexport) void Initialize()
{
    g_hTimerQueue = CreateTimerQueue();
	InitializeCriticalSection(&g_cs);
    g_leftButtonClickData.name = TEXT("LeftButton");
    g_leftButtonClickData.wParamLockType = LOCKTYPE_LEFT;
    g_rightButtonClickData.name = TEXT("RightButton");
    g_rightButtonClickData.wParamLockType = LOCKTYPE_RIGHT;
}

/*
 * @brief クリックロックの有効・無効設定
 * @param enable true: 有効, false: 無効
 */
extern "C" __declspec(dllexport) void EnableClickLock(bool enable)
{
    g_isClickLockEnabled = enable;
    if (!enable) {
        EnterCriticalSection(&g_cs);
        DeactivateClickLock(g_leftButtonClickData);
        DeactivateClickLock(g_rightButtonClickData);
        LeaveCriticalSection(&g_cs);
    }
}

/*
 * @brief クリックロックまでの長押し時間の設定
 * @param delayMs 遅延時間(ミリ秒)
 */
extern "C" __declspec(dllexport) void SetClickLockDelayMS(int delayMs)
{
    g_clickLockDelayMS = delayMs;
}

/*
 * @brief カーソル位置に対するマーカー位置のオフセットの設定
 * @param xOffset X方向のオフセット
 * @param yOffset Y方向のオフセット
 */
extern "C" __declspec(dllexport) void SetMarkerOffset(int xOffset, int yOffset)
{
    g_markerOffset = MAKELPARAM(xOffset, yOffset);
    if (g_leftButtonClickData.isButtonDown ||
            (g_leftButtonClickData.lockState == LOCKSTATE_ON) ||
            g_rightButtonClickData.isButtonDown ||
            (g_rightButtonClickData.lockState == LOCKSTATE_ON) ||
            g_isMarkerPreview) {
        UpdateMarkerPos(false);
    }
}

extern "C" __declspec(dllexport) void SetMarkerPreview(bool markerPreview)
{
    g_isMarkerPreview = markerPreview;
    if (g_isMarkerPreview) {
        UpdateMarkerPos(true);
    }
}

extern "C" __declspec(dllexport) void AllowMouseMove(bool allow, int distancePx)
{
    g_allowMouseMove = allow;
    g_allowMouseMoveDistancePx = distancePx;
}

/*
 * @brief フックの設定
 * @param hWndMarker マーカーウィンドウのウィンドウハンドル
 * @return TRUE: 成功, FALSE: 失敗
 */
extern "C" __declspec(dllexport) void SetMouseHook(HWND hWndMarker)
{
	g_hWndMarker = hWndMarker;
    g_hMouseHook = SetWindowsHookExW(
        WH_MOUSE_LL,
        LowLevelMouseProc,
        GetModuleHandleW(nullptr),
        0 // グローバルフック
    );
}

/*
 * @brief フックの解除
 */
extern "C" __declspec(dllexport) void UnsetMouseHook()
{
    if (g_hMouseHook) {
        UnhookWindowsHookEx(g_hMouseHook);
        g_hMouseHook = nullptr;
    }
}


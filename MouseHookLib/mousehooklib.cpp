#include "pch.h"

// WM_NOTIFY_LOCK_STATE
//   wParam LOCK_STATE_LEFT or LOCK_STATE_RIGHT
//   lParam 1: locked, 0: unlocked
#define WM_NOTIFY_LOCK_STATE    (WM_USER + 1)
#define LOCK_STATE_LEFT       1
#define LOCK_STATE_RIGHT      2


#define CLICK_LOCK_DELAY_MS 1200  // ミリ秒


struct CLICKDATA {
	LPCTSTR name;
	bool    isButtonDown;
	volatile bool   isClickLocked;
	volatile bool   isTimerEventFired;
	volatile bool   isTimerCanceled;
	HANDLE  hClickTimer;
    WPARAM  wParamLockState;
};


// グローバル変数
static HWND g_hWndMain = nullptr;
static HWND g_hWndMarker = nullptr;
static HHOOK g_hMouseHook = nullptr;
static bool g_isClickLockActivated = false;
static LPARAM g_lastMousePos = 0;
static CRITICAL_SECTION g_cs;
static HANDLE g_hTimerQueue = nullptr;
static int g_clickLockDelayMS = 0;
static int g_markerXOffset = 16;
static int g_markerYOffset = 16;
static bool g_isMarkerOffsetPreview = false;
static CLICKDATA g_leftButtonClickData;
static CLICKDATA g_rightButtonClickData;


void DebugPrintf(LPCTSTR format, ...)
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

static void SetMarkerPos()
{
    LPARAM lastMousePos = g_lastMousePos;
    int x = GET_X_LPARAM(lastMousePos) + g_markerXOffset;
    int y = GET_Y_LPARAM(lastMousePos) + g_markerYOffset;
    SetWindowPos(g_hWndMarker, HWND_TOPMOST, x, y, 0, 0, SWP_NOACTIVATE | SWP_NOSIZE);
}


static void CALLBACK ClickTimerProc(PVOID lpParam, BOOLEAN TimerOrWaitFired)
{
    // LowLevelMouseProc()内でg_csを取得した状態で DeleteTimerQueueTimer() が呼ばれたとき、
    // 同時にClickTimerProcが走りだすとデッドロックになってしまう。
    // そこでLowLevelMouseProc()内でDeleteTimerQueueTimerを呼ぶ前に isTimerCanceled を true にする。
    // ClickTimerProc内ではg_csを取得出来なくても、isTimerCanceledがtrueならば処理を中止する。
    // 単純にClickTimerProcの先頭でisTimerCanceledをチェックするだけだと、
    // isTimerCanceledのチェックとg_csの取得の間にLowLevelMouseProc()内でg_csを取得されて
    // デッドロックする可能性があるのでダメ。
    CLICKDATA* pClickData = (CLICKDATA*)lpParam;
    while (TryEnterCriticalSection(&g_cs) == FALSE) {
        if (pClickData->isTimerCanceled) {
            return;
        }
        Sleep(0);
    }
    DebugPrintf(TEXT("Click lock is activated. name: %s\r\n"), pClickData->name);
    pClickData->isTimerEventFired = true;
    pClickData->isClickLocked = true;
    PostMessage(g_hWndMarker, WM_NOTIFY_LOCK_STATE, pClickData->wParamLockState, TRUE);
    LeaveCriticalSection(&g_cs);
}

/*
 * g_cs で保護された状態で呼び出すこと
 */
void CancelClickTimer(CLICKDATA& clickData)
{
	HANDLE hClickTimer = clickData.hClickTimer;
    if (hClickTimer != nullptr){
		// ClickTimerProcでg_csを取得出来なくても良いようにする
		clickData.isTimerCanceled = true;
        bool r = DeleteTimerQueueTimer(g_hTimerQueue, hClickTimer, INVALID_HANDLE_VALUE);
        DebugPrintf(TEXT("DeleteTimerQueueTimer. name: %s, r: %d, hClickTimer: %p\n"), clickData.name, hClickTimer);
		clickData.hClickTimer = nullptr;
    }
}

void OnButtonDown(CLICKDATA& clickData)
{
    EnterCriticalSection(&g_cs);
    clickData.isButtonDown = true;
	CancelClickTimer(clickData);
    if (g_isClickLockActivated) {
		clickData.isTimerEventFired = false;
        clickData.isTimerCanceled = false;
        bool r = CreateTimerQueueTimer(&clickData.hClickTimer, g_hTimerQueue, ClickTimerProc, &clickData, CLICK_LOCK_DELAY_MS, 0, WT_EXECUTEDEFAULT);
        DebugPrintf(TEXT("CreateTimerQueueTimer. name: %s r: %d, hClickTimer: %p\n"), clickData.name, r, clickData.hClickTimer);
    }
    LeaveCriticalSection(&g_cs);
}

void OnButtonUp(CLICKDATA& clickData, CLICKDATA& clickDataAlt)
{
    EnterCriticalSection(&g_cs);
	clickData.isButtonDown = false;
	CancelClickTimer(clickData);
    if (clickData.isClickLocked && !clickData.isTimerEventFired) {
        DebugPrintf(TEXT("Click lock is deactivated. name: %s\r\n"), clickData.name);
        clickData.isClickLocked = false;
		PostMessage(g_hWndMarker, WM_NOTIFY_LOCK_STATE, clickData.wParamLockState, FALSE);
    }
    LeaveCriticalSection(&g_cs);
}


// コールバック関数
LRESULT CALLBACK LowLevelMouseProc(int nCode, WPARAM wParam, LPARAM lParam)
{
    if (nCode >= 0) {
        const MSLLHOOKSTRUCT* pMouse = reinterpret_cast<const MSLLHOOKSTRUCT*>(lParam);
		g_lastMousePos = MAKELPARAM(pMouse->pt.x, pMouse->pt.y);
        switch (wParam)
        {
        case WM_LBUTTONDOWN:
			OnButtonDown(g_leftButtonClickData);
            SetMarkerPos();
            if (g_leftButtonClickData.isClickLocked) {
                // 左ボタンダウンの無効化
                return 1;
            }
            break;
        case WM_LBUTTONUP:
            OnButtonUp(g_leftButtonClickData, g_rightButtonClickData);
            SetMarkerPos();
            if (g_leftButtonClickData.isClickLocked) {
                // 左ボタンアップの無効化
                return 1;
            }
            break;
        case WM_RBUTTONDOWN:
            OnButtonDown(g_rightButtonClickData);
            SetMarkerPos();
            if (g_rightButtonClickData.isClickLocked) {
                // 右ボタンダウンの無効化
                return 1;
            }
            break;
        case WM_RBUTTONUP:
            OnButtonUp(g_rightButtonClickData, g_leftButtonClickData);
            SetMarkerPos();
            if (g_rightButtonClickData.isClickLocked) {
                // 右ボタンアップの無効化
                return 1;
			}
            break;
		case WM_MOUSEMOVE:
            if (g_leftButtonClickData.isButtonDown || g_leftButtonClickData.isClickLocked ||
                    g_rightButtonClickData.isButtonDown || g_rightButtonClickData.isClickLocked ||
                    g_isMarkerOffsetPreview) {
                SetMarkerPos();
            }
            break;
        default:
            break;
        }
    }
    // 次のフックプロシージャへ
    return CallNextHookEx(g_hMouseHook, nCode, wParam, lParam);
}

/*
 * g_cs で保護された状態で呼び出すこと
 */
static void DeactivateClickLock(CLICKDATA& clickData)
{
    CancelClickTimer(clickData);
    clickData.isClickLocked = false;
	clickData.isTimerEventFired = false;
	PostMessage(g_hWndMarker, WM_NOTIFY_LOCK_STATE, clickData.wParamLockState, FALSE);
}

extern "C" __declspec(dllexport) void Initialize()
{
    g_hTimerQueue = CreateTimerQueue();
	InitializeCriticalSection(&g_cs);
    g_leftButtonClickData.name = TEXT("LeftButton");
    g_leftButtonClickData.wParamLockState = LOCK_STATE_LEFT;
    g_rightButtonClickData.name = TEXT("RightButton");
    g_rightButtonClickData.wParamLockState = LOCK_STATE_RIGHT;
}


// フックの設定
extern "C" __declspec(dllexport) BOOL SetMouseHook(HWND hWndMain, HWND hWndMarker)
{
	g_hWndMain = hWndMain;
	g_hWndMarker = hWndMarker;
    if (g_hMouseHook == nullptr) {
        InitializeCriticalSection(&g_cs);
        g_hMouseHook = SetWindowsHookExW(
            WH_MOUSE_LL,
            LowLevelMouseProc,
            GetModuleHandleW(nullptr),
            0 // グローバルフック
        );
    }
    return g_hMouseHook != nullptr;
}

// フックの解除
extern "C" __declspec(dllexport) void UnsetMouseHook()
{
    if (g_hMouseHook) {
        UnhookWindowsHookEx(g_hMouseHook);
        g_hMouseHook = nullptr;
        DeleteCriticalSection(&g_cs);
    }
}

extern "C" __declspec(dllexport) void EnableClickLock(bool enable)
{
    g_isClickLockActivated = enable;
    if (!enable) {
        EnterCriticalSection(&g_cs);
		DeactivateClickLock(g_leftButtonClickData);
        DeactivateClickLock(g_rightButtonClickData);
        LeaveCriticalSection(&g_cs);
    }
}

extern "C" __declspec(dllexport) void SetClickLockDelay(int delayMs)
{
    g_clickLockDelayMS = delayMs;
}

extern "C" __declspec(dllexport) void SetMarkerOffset(int xOffset, int yOffset)
{
    g_markerXOffset = xOffset;
    g_markerYOffset = yOffset;
    if (g_leftButtonClickData.isButtonDown || g_leftButtonClickData.isClickLocked ||
            g_rightButtonClickData.isButtonDown || g_rightButtonClickData.isClickLocked ||
            g_isMarkerOffsetPreview) {
        SetMarkerPos();
    }
}

extern "C" __declspec(dllexport) void SetMarkerOffsetPreview(bool markerOffsetPreview)
{
    g_isMarkerOffsetPreview = markerOffsetPreview;
    if (g_isMarkerOffsetPreview) {
        SetMarkerPos();
    }
}

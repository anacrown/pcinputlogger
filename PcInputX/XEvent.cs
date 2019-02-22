using System;

namespace PcInputX
{
    public abstract class XEvent
    {
        public WM EventType { get; set; }
        public int Time { get; set; }

        public abstract TEvent ToTransferData();

        public abstract void Inject(DateTime starTime);

        public static XEvent FromTransferData(TEvent tEvent)
        {
            if (tEvent is TMouse tMouse)
                return MouseXEvent.FromTransferData(tMouse);

            if (tEvent is TKeyboard tKeyboard)
                return KeyboardXEvent.FromTransferData(tKeyboard);

            throw new Exception("Invalid type TEvent");
        }
    }

    static class WMExtention
    {
        public static TXEventType ToTransferData(this WM wParam)
        {
            switch (wParam)
            {
                case WM.LBUTTONDOWN: return TXEventType.WM_LBUTTONDOWN;
                case WM.LBUTTONUP: return TXEventType.WM_LBUTTONUP;
                case WM.MOUSEMOVE: return TXEventType.WM_MOUSEMOVE;
                case WM.MOUSEWHEEL: return TXEventType.WM_MOUSEWHEEL;
                case WM.RBUTTONDOWN: return TXEventType.WM_RBUTTONDOWN;
                case WM.RBUTTONUP: return TXEventType.WM_RBUTTONUP;
                case WM.KEYDOWN: return TXEventType.WM_KEYDOWN;
                case WM.KEYUP: return TXEventType.WM_KEYUP;
                case WM.SYSKEYDOWN: return TXEventType.WM_SYSKEYDOWN;
                case WM.SYSKEYUP: return TXEventType.WM_SYSKEYUP;
                default: throw new ArgumentOutOfRangeException(nameof(wParam), wParam, null);
            }
        }

        public static WM ToData(this TXEventType txEventType)
        {
            switch (txEventType)
            {
                case TXEventType.WM_LBUTTONDOWN: return WM.LBUTTONDOWN;
                case TXEventType.WM_LBUTTONUP: return WM.LBUTTONUP;
                case TXEventType.WM_MOUSEMOVE: return WM.MOUSEMOVE;
                case TXEventType.WM_MOUSEWHEEL: return WM.MOUSEWHEEL;
                case TXEventType.WM_RBUTTONDOWN: return WM.RBUTTONDOWN;
                case TXEventType.WM_RBUTTONUP: return WM.RBUTTONUP;
                case TXEventType.WM_KEYDOWN: return WM.KEYDOWN;
                case TXEventType.WM_KEYUP: return WM.KEYUP;
                case TXEventType.WM_SYSKEYDOWN: return WM.SYSKEYDOWN;
                case TXEventType.WM_SYSKEYUP: return WM.SYSKEYUP;
                default: throw new ArgumentOutOfRangeException(nameof(txEventType), txEventType, null);
            }
        }
    }
}
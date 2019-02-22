using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace PcInputX
{
    public class MouseXEvent : XEvent
    {
        public int Delta { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool StopEventPropagation { get; set; }

        public static MouseXEvent FromMsllhookstruct(WM wParam, MouseX.Msllhookstruct lParam) => new MouseXEvent
        {
            EventType = wParam,
            Delta = lParam.mouseData >> 16,
            X = lParam.pt.x,
            Y = lParam.pt.y,
            Time = lParam.time
        };

        public override string ToString() => EventType == WM.MOUSEWHEEL
            ? $"{Time} {EventType} {Delta}"
            : $"{Time} {EventType} {X} {Y}";

        public override TEvent ToTransferData() => new TMouse
        {
            EventType = EventType.ToTransferData(),
            Duration = Time,
            Delta = Delta,
            X = X,
            Y = Y
        };

        public override void Inject(DateTime starTime) => MouseX.Inject(this, starTime);

        public static MouseXEvent FromTransferData(TMouse tMouse) => new MouseXEvent
        {
            EventType = tMouse.EventType.ToData(),
            Time = tMouse.Duration,
            Delta = tMouse.Delta,
            X = tMouse.X,
            Y = tMouse.Y
        };
    }

    public static class MouseX
    {
        private static IntPtr _mHook = IntPtr.Zero;

        private delegate IntPtr LowLevelMouseProc(int nCode, WM wParam, IntPtr lParam);
        private static readonly LowLevelMouseProc LowLevelMouseProcDelegate = new LowLevelMouseProc(MouseHookProc);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(HookType code, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern void mouse_event(MOUSEEVENTF dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);

        [DllImport("kernel32.dll")]
        static extern uint GetTickCount();

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Msllhookstruct
        {
            public Point pt;
            public int mouseData;
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum MOUSEEVENTF : uint
        {
            MOUSEEVENTF_ABSOLUTE = 0x8000,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_HWHEEL = 0x01000,
        }

        public static void Hook()
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
                _mHook = SetWindowsHookEx(HookType.WH_MOUSE_LL, LowLevelMouseProcDelegate, GetModuleHandle(curModule.ModuleName), 0);
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(_mHook);
        }

        public static void Inject(MouseXEvent xEvent, DateTime starTime)
        {
            var sleep = xEvent.Time - (int)(DateTime.Now - starTime).TotalMilliseconds;

            if (sleep > 0)
                Thread.Sleep(sleep);

            Console.WriteLine(xEvent);

            var dwFlags = WMToEventF(xEvent.EventType);

            SetCursorPos(xEvent.X, xEvent.Y);
            mouse_event(dwFlags, 0, 0, xEvent.Delta, UIntPtr.Zero);
        }

        private static MOUSEEVENTF WMToEventF(WM wParam)
        {
            switch (wParam)
            {
                case WM.LBUTTONDOWN: return MOUSEEVENTF.MOUSEEVENTF_LEFTDOWN;
                case WM.LBUTTONUP: return MOUSEEVENTF.MOUSEEVENTF_LEFTUP;
                case WM.RBUTTONDOWN: return MOUSEEVENTF.MOUSEEVENTF_RIGHTDOWN;
                case WM.RBUTTONUP: return MOUSEEVENTF.MOUSEEVENTF_RIGHTUP;
                case WM.MOUSEMOVE: return MOUSEEVENTF.MOUSEEVENTF_MOVE;
                case WM.MOUSEWHEEL: return MOUSEEVENTF.MOUSEEVENTF_WHEEL;
                default: throw new Exception();
            }
        }

        private static IntPtr MouseHookProc(int nCode, WM wParam, IntPtr lParam)
        {
            var md = (Msllhookstruct)Marshal.PtrToStructure(lParam, typeof(Msllhookstruct));
            var mouseXEvent = MouseXEvent.FromMsllhookstruct(wParam, md);

#if DEBUG
            Console.WriteLine($@"Raise MouseEvent {mouseXEvent}");
#endif
            MouseEvent?.Invoke(null, mouseXEvent);

            var res = mouseXEvent.StopEventPropagation ? new IntPtr(1) : CallNextHookEx(_mHook, nCode, (IntPtr)wParam, lParam);

#if DEBUG
            Console.WriteLine($@"Raise AfterMouseEvent {mouseXEvent}");
#endif
            AfterMouseEvent?.Invoke(null, mouseXEvent);

            return res;
        }

        public static event EventHandler<MouseXEvent> MouseEvent;

        public static event EventHandler<MouseXEvent> AfterMouseEvent;
    }
}

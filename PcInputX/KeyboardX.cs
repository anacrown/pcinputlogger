using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PcInputX
{
    public class KeyboardXEvent : XEvent
    {
        public KeyboardX.KbdllhookstructFlags EventFlags { get; set; }
        public Keys Key { get; set; }
        public uint KeyCode { get; set; }
        public bool StopEventPropagation { get; set; }

        public static KeyboardXEvent FromKbdllhookstruct(WM wParam, KeyboardX.Kbdllhookstruct lParam) => new KeyboardXEvent
        {
            EventType = wParam,
            EventFlags = lParam.flags,
            Key = (Keys)lParam.vkCode,
            KeyCode = lParam.vkCode,
            Time = lParam.time
        };

        public override string ToString() => $"{EventType} {EventFlags} {Time} {Key}";

        public override TEvent ToTransferData() => new TKeyboard
        {
            EventType = EventType.ToTransferData(),
            Duration = Time,
            KeyName = Key.ToString(),
            KeyCode = KeyCode
        };

        public override void Inject(DateTime starTime) => KeyboardX.Inject(this, starTime);

        public static KeyboardXEvent FromTransferData(TKeyboard tKeyboard) => new KeyboardXEvent
        {
            EventType = tKeyboard.EventType.ToData(),
            Time = tKeyboard.Duration,
            Key = (Keys)tKeyboard.KeyCode,
            KeyCode = tKeyboard.KeyCode
        };
    }

    public static class KeyboardX
    {
        private static IntPtr _mHook = IntPtr.Zero;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, WM wParam, IntPtr lParam);
        private static readonly LowLevelKeyboardProc LowLevelKeyboardProcDelegate = new LowLevelKeyboardProc(KeyboardProc);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(HookType code, LowLevelKeyboardProc func, IntPtr hInstance, uint dwThreadId);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(int hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [StructLayout(LayoutKind.Sequential)]
        public class Kbdllhookstruct
        {
            public uint vkCode;
            public uint scanCode;
            public KbdllhookstructFlags flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum KbdllhookstructFlags : uint
        {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80
        }

        public static void Hook()
        {
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
                _mHook = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, LowLevelKeyboardProcDelegate, GetModuleHandle(module.ModuleName), 0);
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(_mHook);
        }

        public static void Inject(KeyboardXEvent xEvent, DateTime starTime)
        {
            var sleep = xEvent.Time - (int)(DateTime.Now - starTime).TotalMilliseconds;

            if (sleep > 0)
                Thread.Sleep(sleep);

            Console.WriteLine(xEvent);

            uint dwFlags = 0;
            if (xEvent.EventType == WM.KEYUP || xEvent.EventType == WM.SYSKEYUP)
                dwFlags = 0x0002;

            keybd_event((byte) xEvent.KeyCode, 0, dwFlags, 0);
        }

        private static IntPtr KeyboardProc(int nCode, WM wParam, IntPtr lParam)
        {
            var kbd = (Kbdllhookstruct)Marshal.PtrToStructure(lParam, typeof(Kbdllhookstruct));
            var keyboardXEvent = KeyboardXEvent.FromKbdllhookstruct(wParam, kbd);

#if DEBUG
            Console.WriteLine($@"Raise KeyboardEvent {keyboardXEvent}");
#endif

            KeyboardEvent?.Invoke(null, keyboardXEvent);

            var res = keyboardXEvent.StopEventPropagation ? new IntPtr(1) : CallNextHookEx(nCode, nCode, (IntPtr) wParam, lParam);

#if DEBUG
            Console.WriteLine($@"Raise AfterKeyboardEvent {keyboardXEvent}");
#endif

            AfterKeyboardEvent?.Invoke(null, keyboardXEvent);

            return res;
        }

        public static event EventHandler<KeyboardXEvent> KeyboardEvent;

        public static event EventHandler<KeyboardXEvent> AfterKeyboardEvent;
    }
}

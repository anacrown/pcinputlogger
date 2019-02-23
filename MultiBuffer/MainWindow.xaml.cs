using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PcInputX;
using MenuItem = System.Windows.Forms.MenuItem;

namespace MultiBuffer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        static IntPtr _nextClipboardViewer;

        private bool _isCancel = true;
        public readonly NotifyIcon NotifyIcon = new NotifyIcon();

        public MainWindow()
        {
            InitializeComponent();

            KeyLocker.Init();

            NotifyIcon.Icon = Properties.Resources.ClipIcon;
            NotifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new[]
            {
                new MenuItem("Exit", (sender, args) =>
                {
                    _isCancel = false;
                    Close();
                }),
            });
            NotifyIcon.DoubleClick += (sender, args) => ShowWindow();

            HideWindow();
        }

        private void ShowWindow()
        {
            Show();
            Focus();
            NotifyIcon.Visible = false;
            WindowState = WindowState.Normal;
        }

        private void HideWindow()
        {
            NotifyIcon.Visible = true;
            WindowState = WindowState.Minimized;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Close();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                NotifyIcon.Visible = true;
            }
            base.OnStateChanged(e);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (e.Cancel = _isCancel)
                HideWindow();

            KeyboardX.UnHook();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);

            _nextClipboardViewer = (IntPtr)SetClipboardViewer(new WindowInteropHelper(this).Handle);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (msg)
            {
                case WM_DRAWCLIPBOARD:
                    Console.WriteLine("WM_DRAWCLIPBOARD");

                    DrawClipboard();
                    SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (wParam == _nextClipboardViewer)
                        _nextClipboardViewer = lParam;
                    else
                        SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    break;

                default: break;
            }

            return IntPtr.Zero;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            KeyboardX.Hook();
            KeyboardX.KeyboardEvent += KeyboardXOnKeyboardEvent;
        }

        private bool _isCtrlDown, _isShiftDown, _isVDown;

        private void KeyboardXOnKeyboardEvent(object sender, KeyboardXEvent e)
        {
            switch (e.Key)
            {
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                    _isCtrlDown = e.EventType == WM.KEYDOWN;
                    break;

                case Keys.Shift:
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    _isShiftDown = e.EventType == WM.KEYDOWN;
                    break;

                case Keys.V:
                    var isVDownNow = e.EventType == WM.KEYDOWN;

                    if (_isCtrlDown && _isShiftDown && !_isVDown && isVDownNow)
                        e.StopEventPropagation = Properties.Settings.Default.StopEventPropagation;

                    if (_isCtrlDown && _isShiftDown && _isVDown && !isVDownNow)
                    {
                        CtrlShiftVKeyUp();
                        e.StopEventPropagation = Properties.Settings.Default.StopEventPropagation;
                    }

                    _isVDown = isVDownNow;
                    break;

                case Keys.Escape:
                    if (IsVisible) HideWindow();
                    break;
            }
        }

        private void CtrlShiftVKeyUp()
        {
            Console.WriteLine("-------------> Ctrl+Shift+V Detected!!!");

            ShowWindow();
        }

        private void DrawClipboard()
        {
            ListView.Items.Add(ClipboardData.ExtractClipboardData());
        }
    }

    public static class KeyLocker
    {
        public static bool NeedLock
        {
            get => _needLock;
            private set
            {
                _needLock = value;
                NeedLockChanged();
            }
        }

        private static Dictionary<Keys, KeyboardXEvent> _pressedKeys = new Dictionary<Keys, KeyboardXEvent>();
        private static bool _needLock;

        private static BackgroundWorker _worker;

        public static void Init()
        {
            KeyboardX.Hook();
            KeyboardX.KeyboardEvent += KeyboardXOnKeyboardEvent;

            _worker = new BackgroundWorker();
            _worker.DoWork += WorkerOnDoWork;
            _worker.WorkerSupportsCancellation = true;

            NeedLock = KeyboardX.IsScrollLockEnabled;
        }

        private static void WorkerOnDoWork(object o, DoWorkEventArgs e)
        {
            var worker = o as BackgroundWorker;

            var events = e.Argument as KeyboardXEvent[];
            if (events == null) return;

            while (true)
            {
                if (worker != null && worker.CancellationPending) break;

                foreach (var xEvent in events)
                    xEvent.InjectNow();

                Thread.Sleep(25);
            }

            Console.WriteLine("Worker STOPING");

            foreach (var xEvent in events)
            {
                xEvent.EventType = WM.KEYUP;
                xEvent.InjectNow();
            }

            Console.WriteLine("Worker STOPED");
        }

        private static void KeyboardXOnKeyboardEvent(object o, KeyboardXEvent e)
        {
            Console.WriteLine($@"Raise KeyboardEvent {e} NeedLock {NeedLock}");

            if (e.Key == Keys.Scroll)
            {
                e.StopEventPropagation = true;

                if (e.EventType != WM.KEYUP) return;

                NeedLock = !NeedLock;
                Console.WriteLine($@"NeedLock = {NeedLock}");
                
                return;
            }

            if (!NeedLock)
            {
                switch (e.EventType)
                {
                    case WM.KEYDOWN:
                        if (!_pressedKeys.ContainsKey(e.Key))
                            _pressedKeys.Add(e.Key, e);
                        break;

                    case WM.KEYUP:
                        if (_pressedKeys.ContainsKey(e.Key))
                            _pressedKeys.Remove(e.Key);
                        break;
                }
            }
            else
            {
                if (_pressedKeys.ContainsKey(e.Key) && e.EventFlags != KeyboardX.KbdllhookstructFlags.LLKHF_INJECTED)
                    e.StopEventPropagation = true;
            }
        }

        private static void NeedLockChanged()
        {
            if (NeedLock)
            {
                if (!_worker.IsBusy)
                {
                    Console.WriteLine("Worker RUN");
                    foreach (var xEvent in _pressedKeys.Values)
                        Console.WriteLine($"EVENT {xEvent}");

                    _worker.RunWorkerAsync(_pressedKeys.Values.ToArray());
                }
            }
            else
            {
                if (_worker.IsBusy)
                {
                    Console.WriteLine("Worker STOPING");
                    _worker.CancelAsync();
                }
            }
        }
    }
}

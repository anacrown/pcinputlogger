using System;
using System.ComponentModel;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
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
}

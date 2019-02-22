using System;
using System.Collections.Generic;
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
using Clipboard = System.Windows.Clipboard;
using MenuItem = System.Windows.Forms.MenuItem;

//using PcInputX;

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

                    if (_isCtrlDown && _isShiftDown && _isVDown && !isVDownNow)
                        CtrlShiftVKeyUp();

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

        //        private void KeyboardXOnKeyboardEvent(object sender, KeyboardXEvent keyboardXEvent)
        //        {
        //            
        //        }
    }

    public abstract class ClipboardData
    {
        public ClipboardDataType DataType { get; protected set; }

        public static ClipboardData ExtractClipboardData()
        {
            if (Clipboard.ContainsText())
            {
                return new ClipboardTextData(Clipboard.GetText());
            }

            if (Clipboard.ContainsAudio())
            {
                return new ClipboardAudioData(Clipboard.GetAudioStream());
            }

            if (Clipboard.ContainsImage())
            {
                return new ClipboardImageData(Clipboard.GetImage());
            }

            if (Clipboard.ContainsFileDropList())
            {
                var arr = new string[Clipboard.GetFileDropList().Count];
                Clipboard.GetFileDropList().CopyTo(arr, 0);
                return new ClipboardFileDropListData(arr);
            }

            throw new Exception("UnSupport Clipboard data type");
        }

        public enum ClipboardDataType
        {
            Text, Audio, Image, FileDropList
        }
    }

    public class ClipboardTextData : ClipboardData
    {
        public string TextData { get; }

        protected internal ClipboardTextData(string textData)
        {
            TextData = textData;
            DataType = ClipboardDataType.Text;
        }
    }

    public class ClipboardAudioData : ClipboardData
    {
        public Stream AudioStream { get; }

        protected internal ClipboardAudioData(Stream audioStream)
        {
            AudioStream = audioStream;
            DataType = ClipboardDataType.Audio;
        }
    }

    public class ClipboardImageData : ClipboardData
    {
        public BitmapSource ImageData { get; }

        protected internal ClipboardImageData(BitmapSource imageData)
        {
            ImageData = imageData;
            DataType = ClipboardDataType.Image;
        }
    }

    public class ClipboardFileDropListData : ClipboardData
    {
        public IEnumerable<string> FileDropList { get; }

        protected internal ClipboardFileDropListData(IEnumerable<string> fileDropList)
        {
            FileDropList = fileDropList;
            DataType = ClipboardDataType.Image;
        }
    }

}

using PcInputX;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace Macro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isPlay;
        private PcInputX.Macro _macro;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            KeyboardX.KeyboardEvent += KeyboardXOnKeyboardEvent;
            KeyboardX.Hook();
        }

        private void KeyboardXOnKeyboardEvent(object sender, KeyboardXEvent e)
        {
            if (e.Key == Keys.Scroll)
            {
                if (e.EventType != WM.KEYUP) return;

                _isPlay = !_isPlay;
                Background = _isPlay ? Brushes.Green : Brushes.White;

                if (_isPlay)
                    PlayInLoop();

                if (!_isPlay)
                    _macro.Cancel = true;
            }
        }

        private void PlayInLoop()
        {
            _macro = PcInputX.Macro.Load("../../../Macro.xml");
            while (true)
            {
                if (!_isPlay) return;

                _macro.XPlay();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PcInputX;

namespace KeyBinder
{
    class Program
    {
        static void Main(string[] args)
        {
            KeyboardX.Hook();
            KeyboardX.KeyboardEvent += OnKeyboardEvent;

            Application.Run();
        }

        private static void OnKeyboardEvent(object sender, KeyboardXEvent e)
        {
            if (e.Key != Keys.Insert) return;

            if (XEvent.FromTransferData(e.ToTransferData()) is KeyboardXEvent xEvent)
            {
                xEvent.Key = Keys.B;
                xEvent.KeyCode = 93;
                xEvent.InjectNow();
            }

            e.StopEventPropagation = true;
        }
    }
}

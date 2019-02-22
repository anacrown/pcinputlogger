using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace PcInputX
{
    public class Macro
    {
        private static bool _isRecord;
        private static readonly List<XEvent> XEvents;

        static Macro()
        {
            XEvents = new List<XEvent>();
            MouseX.MouseEvent += MouseXOnMouseEvent;
            KeyboardX.KeyboardEvent += KeyboardXOnKeyboardEvent;
        }

        private static void KeyboardXOnKeyboardEvent(object sender, KeyboardXEvent e)
        {
            if (e.Key == Keys.Scroll)
            {
                if (e.EventType != WM.KEYUP) return;

                _isRecord = !_isRecord;

                if (_isRecord)
                {
                    XEvents.Clear();
                }
                else
                {
                    Application.Exit();
                }

                return;
            }

            if (!_isRecord) return;

            XEvents.Add(e);
            Console.WriteLine(e);
        }

        private static void MouseXOnMouseEvent(object sender, MouseXEvent e)
        {
            if (!_isRecord) return;

            XEvents.Add(e);
            Console.WriteLine(e);
        }

        public static Macro Record()
        {
            XEvents.Clear();

            MouseX.Hook();
            KeyboardX.Hook();

            Application.Run();

            MouseX.UnHook();
            KeyboardX.UnHook();

            return new Macro(XEvents.ToArray());
        }

        public static Macro Load(string fileName)
        {
            using (var fs = File.Open(fileName, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(TMacro));
                return new Macro((TMacro)serializer.Deserialize(fs));
            }
        }

        public XEvent[] Events { get; }

        protected Macro(XEvent[] events)
        {
            Events = events;
            EventsSimplify();
        }

        protected Macro(TMacro tMacro)
        {
            Events = tMacro.Event?.Select(XEvent.FromTransferData).ToArray();
        }

        public void EventsSimplify()
        {
            var startTime = Events.FirstOrDefault()?.Time ?? 0;
            foreach (var xEvent in Events)
                xEvent.Time -= startTime;
        }

        public void Save(string fileName)
        {
            using (var fs = File.Open(fileName, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(TMacro));
                serializer.Serialize(fs, ToTransferData());
            }
        }

        public void XPlay()
        {
            var startTime = DateTime.Now;
            foreach (var xEvent in Events)
                xEvent.Inject(startTime);
        }

        private TMacro ToTransferData() => new TMacro
        {
            Event = new List<TEvent>(Events.Select(t => t.ToTransferData()).ToArray())
        };
    }
}

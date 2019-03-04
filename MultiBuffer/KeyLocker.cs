using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using PcInputX;

namespace MultiBuffer
{
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
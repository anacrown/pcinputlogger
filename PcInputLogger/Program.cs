using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using PcInputX;

namespace PcInputLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            var isExit = false;
            do
            {
                switch (DrawMenu())
                {
                    case 0: Record(); break;
                    case 1: Play(); break;
                    case 2: Play(true); break;
                    case 3: isExit = true; break;
                    default: throw new ArgumentOutOfRangeException();
                }
            } while (!isExit);
        }

        static int DrawMenu()
        {
            ConsoleKeyInfo keyInfo;
            MenuPoint menuPoint = MenuPoint.Record;
            do
            {
                Console.Clear();

                if (Equals(menuPoint.Value, MenuPoint.Record))
                    Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("Записать макрос");
                Console.ResetColor();

                if (Equals(menuPoint.Value, MenuPoint.Play))
                    Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("Воспроизвести макрос");
                Console.ResetColor();

                if (Equals(menuPoint.Value, MenuPoint.PlayInLoop))
                    Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine("Воспроизвести и зациклить макрос");
                Console.ResetColor();

                keyInfo = Console.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow: menuPoint--; break;
                    case ConsoleKey.DownArrow: menuPoint++; break;
                }

            } while (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Escape);

            if (keyInfo.Key == ConsoleKey.Escape)
                menuPoint = MenuPoint.Exit;

            return menuPoint.Value;
        }

        static void Record()
        {
            
            var name = Console.ReadLine();

            var macro = Macro.Record();
            macro.Save("../../../Macro.xml");
        }

        static string ReadMecrosName()
        {
            var name = string.Empty;

            Console.Clear();
            Console.ResetColor();
            Console.Write($"Enter macros name: {name}");

            try
            {
                var path = Path.Combine("../../..", $"{name}.macro");
            }
            catch (Exception e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                throw;
            }

            return name;
        }

        static void Play(bool inLopp = false)
        {
            var macro = Macro.Load("../../../Macro.xml");

            KeyboardX.Hook();
            KeyboardX.KeyboardEvent += (o, e) =>
            {
                if (e.Key == Keys.Scroll && e.EventType == WM.KEYUP)
                    macro.Cancel = true;
            };
            
            macro.XPlay(inLopp);

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}

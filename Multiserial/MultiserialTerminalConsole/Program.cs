using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiserialTerminalConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Opening...");
            SerialFrontend frontend = new SerialFrontend("COM6");
            Console.WriteLine("OK");

            frontend.ByteAvailable += (o, e) =>
            {
                Console.WriteLine(string.Format("{0:X2} {1} ", e.TheByte, (char)e.TheByte));
            };

            bool ok = frontend.OpenAndDemultipex();
            if (!ok)
            {
                Console.WriteLine("Errore di apertura");
            }

            Console.ReadLine();
        }
    }
}

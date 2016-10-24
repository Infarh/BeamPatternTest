using System;
using System.IO;

namespace BeamPatternTest
{
    class Program
    {
        static void OnIndexChanged()
        {
            Console.WriteLine("Индекс изменился");
        }

        static void OnIndexChanged1()
        {
            Console.WriteLine("Index changed");
        }

        static void Main()
        {
            var watcher = new FileSystemWatcher("c:\\123\\");

            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;
            Console.ReadLine();
            //return;


            //var a = new LinearAntennaArray(0.5, 5, i => new Dipole());

            //var D = 0.5;
            //a.A = LinearAntennaArray.AmplitudeDistribution.CosOnPedistal(D);

            //Console.WriteLine(a.D);

            //Console.ReadLine();
        }

        private static void OnFileCreated(
            object Sender, 
            FileSystemEventArgs E)
        {
            Console.WriteLine(E.Name);   
        }
    }
}

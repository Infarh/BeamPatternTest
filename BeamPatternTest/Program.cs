using System;

namespace BeamPatternTest
{
    class Program
    {
        static void Main()
        {
            var a = new LinearAntennaArray(0.5, 5, i => new Dipole());

            var D = 0.5;
            a.A = LinearAntennaArray.AmplitudeDistribution.CosOnPedistal(D);

            Console.WriteLine(a.D);

            Console.ReadLine();
        }
    }
}

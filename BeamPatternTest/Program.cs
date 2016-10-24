using System;

namespace BeamPatternTest
{
    class Program
    {
        static void Main()
        {
            var a = new LinearAntennaArray(0.5, 5, i => new Dipole());

            var D = 0.5;
            a.A = x => (1 - D) * Math.Cos(Math.PI * x / a.L) + D;

            Console.WriteLine(a.D);

            Console.ReadLine();
        }
    }

    public static class Service
    {
        public static double Integrate(this Func<double, double> f, double a, double b, double dx)
        {
            if(a > b) return f.Integrate(b, a, dx);
            if(a >= b - dx) return 0;
            var s = 0.5 * (f(a) + f(b));
            b -= dx;
            while(a < b) s += f(a += dx);
            return s * dx;
        }

    }
}

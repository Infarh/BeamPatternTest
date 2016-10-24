using System;

namespace BeamPatternTest
{
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
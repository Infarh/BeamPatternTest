using System;
using System.Numerics;
using BeamPatternTest;

namespace BeamPatternWPF
{
    internal class GenericAntenna : Antenna
    {
        private readonly Complex[] f_BeamData;
        private readonly double f_dTh;

        public GenericAntenna(Complex[] BeamData, double dTh)
        {
            f_BeamData = BeamData;
            f_dTh = dTh;
        }

        private const double pi2 = Math.PI * 2;
        private const double toRad = Math.PI / 180;
        public override Complex Pattern(double th)
        {
            th %= pi2;
            if(th < 0) th += pi2;
            var i = (int)(th / (f_dTh * toRad));
            var th1 = i * f_dTh * toRad;
            var th2 = (i + 1) * f_dTh * toRad;
            var f1 = f_BeamData[i];
            var f2 = f_BeamData[i + 1];
            return Interpolate(th, th1, f1, th2, f2);
        }

        private static Complex Interpolate(double th, double th1, Complex F1, double th2, Complex F2)
        {
            return new Complex(
                real: Interpolate(th, th1, F1.Real, th2, F2.Real),
                imaginary: Interpolate(th, th1, F1.Imaginary, th2, F2.Imaginary));
        }

        private static double Interpolate(double th, double th1, double F1, double th2, double F2)
        {
            return F1 + (th - th1) * (F2 - F1) / (th2 - th1);
        }
    }
}
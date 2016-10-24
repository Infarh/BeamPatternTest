using System;
using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>
    /// Диполь Герца
    /// </summary>
    public class Dipole : Antenna
    {
        public override Complex Pattern(double th)
        {
            return Math.Cos(th);
        }
    }
}
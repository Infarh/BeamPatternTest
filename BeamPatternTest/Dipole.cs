using System;
using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>Диполь Герца</summary>
    public class Dipole : Antenna
    {
        /// <inheritdoc />
        public override Complex Pattern(double th) => Math.Cos(th);
    }
}
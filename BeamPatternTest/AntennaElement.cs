using System;
using System.Numerics;

namespace BeamPatternTest
{
    public class AntennaElement : Antenna
    {
        private readonly double kx;
        private readonly double ky;

        /// <summary>Вложенный антенный элемент</summary>
        public Antenna Element { get; }

        /// <summary>Пространственное смещение</summary>
        public double X { get; }

        public double Y { get; }

        public AntennaElement(Antenna element, double x, double y)
        {
            Element = element;
            X = x;
            Y = y;
            kx = Service.pi2 * x;
            ky = Service.pi2 * y;
        }

        #region Overrides of Antenna

        /// <inheritdoc />
        public override Complex Pattern(double th)
        {
            var e = kx * Math.Sin(th) + ky * Math.Cos(th);
            return Element.Pattern(th) * new Complex(Math.Cos(e), Math.Sin(-e));
        }

        #endregion
    }
}

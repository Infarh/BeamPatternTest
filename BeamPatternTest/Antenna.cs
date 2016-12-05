using System;
using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>Антенна</summary>
    public abstract class Antenna
    {
        /// <summary>КНД</summary>
        public double D => GetKND();

        /// <summary>Значение диаграммы направленности в секторе углов от 0 до 360 градусов с шагом 1 градус</summary>
        public virtual BeamPattern Beam => new BeamPattern(Pattern, -Math.PI, Math.PI, 1 * Service.toRad);

        /// <summary>Метод определения КНД по ДН антенны</summary>
        /// <returns>2 / Интеграл от -пи/2 до пи/2 от квадрата ДН, умншженного на косинус угла места</returns>
        private double GetKND()
        {
            Func<double, double> f = th =>
            {
                var dn = Pattern(th).Magnitude;
                return dn * dn * Math.Cos(th);
            };

            return 2 / f.Integrate(-Service.pi05, Service.pi05, 0.5 * Service.toRad);
        }

        /// <summary>Диаграмма направленности</summary>
        /// <param name="th">Угол места</param>
        /// <returns>Вещественное значение диаграммы направленности</returns>
        public abstract Complex Pattern(double th);
    }
}

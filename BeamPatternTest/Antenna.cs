using System;
using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>
    /// Антенна
    /// </summary>
    public abstract class Antenna
    {
        public BeamPatern Beam => new BeamPatern(Pattern, -Math.PI, Math.PI, 1 * Math.PI / 180);

        /// <summary>КНД</summary>
        public double D => GetKND();

        /// <summary>Метод определения КНД по ДН антенны</summary>
        /// <returns>2 / Интеграл от -пи/2 до пи/2 от квадрата ДН, умншженного на косинус угла места</returns>
        private double GetKND()
        {
            Func<double, double> f = th =>
            {
                var dn = Pattern(th).Magnitude;
                return dn * dn * Math.Cos(th);
            };

            return 2 / f.Integrate(-Math.PI / 2, Math.PI / 2, Math.PI / 100);
        }

        /// <summary>
        /// Диаграмма направленности
        /// </summary>
        /// <param name="th">Угол места</param>
        /// <returns>Вещественное значение диаграммы направленности</returns>
        public abstract Complex Pattern(double th);
    }
}

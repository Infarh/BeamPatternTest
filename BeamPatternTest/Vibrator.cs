using System;
using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>Симметричный вибратор</summary>
    public class Vibrator : Antenna
    {
        /// <summary>Длина вибратора</summary>
        private double l;

        /// <summary>Длина вибратора в длинах волн</summary>
        public double L { get { return l; } set { l = value; } }

        public Vibrator() : this(0.5) { }

        /// <summary>Новый вибратор</summary>             
        /// <param name="l">Длина вибратора в длинах волн</param>
        public Vibrator(double l) { this.l = l; }

        /// <summary>ДИаграмма направленности вибратора</summary>
        /// <param name="th">Угол места в радианах</param>
        /// <returns>Значение диаграммы направленности для указанного значения угла</returns>
        public override Complex Pattern(double th)
        {
            var result = Service.pi2 * l * Math.Sin(th);
            result = Math.Cos(result) - Math.Cos(Service.pi2 * l);
            result /= Math.Cos(th);
            result /= 1 - Math.Cos(Service.pi2 * l);
            return result;
        }
    }
}
using System;
using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>
    /// Антенна
    /// </summary>
    abstract class Antenna
    {
        public double D
        {
            get { return GetKND(); }
        }

        private double GetKND()
        {
            Func<double, double> f;

            f = th =>
            {
                var dn = Pattern(th).Magnitude;
                return dn * dn * Math.Cos(th);
            };

            var D = Service.Integrate(f, -Math.PI / 2, Math.PI / 2, Math.PI / 100);
            return 2 / D;
        }

        /// <summary>
        /// Диаграмма направленности
        /// </summary>
        /// <param name="th">Угол места</param>
        /// <returns>Вещественное значение диаграммы направленности</returns>
        public abstract Complex Pattern(double th);
    }

    /// <summary>
    /// Диполь Герца
    /// </summary>
    class Dipole : Antenna
    {
        public override Complex Pattern(double th)
        {
            return Math.Cos(th);
        }
    }

    class Vibrator : Antenna
    {
        private double l;

        /// <summary>Длина вибратора в длинах волн</summary>
        public double L
        {
            get { return l; }
            set { l = value; }
        }

        /// <summary>
        /// Новый вибратор
        /// </summary>
        /// <param name="l">Длина вибратора в длинах волн</param>
        public Vibrator(double l)
        {
            this.l = l;
            Console.WriteLine("Создан новый вибратор длины {0}", l);
        }

        public override Complex Pattern(double th)
        {
            double result = 2 * Math.PI * l * Math.Sin(th);
            result = Math.Cos(result);
            result -= Math.Cos(2 * Math.PI * l);
            result /= Math.Cos(th);
            result /= (1 - Math.Cos(2 * Math.PI * l));
            return result;
        }
    }

    class LinearAntennaArray : Antenna
    {
        private double d;

        private Antenna[] elements;

        private Complex[] K;

        private Func<double, double> a = x => 1;
        private Func<double, double> ph = x => 0;

        public double L => d * (N - 1);
        public int N => elements.Length;

        public Func<double, double> A
        {
            set
            {
                if(value == null) return;
                a = value;
                CalculateK();
            }
        }

        public Func<double, double> Ph
        {
            set
            {
                if(value == null) return;
                ph = value;
                CalculateK();
            }
        }

        private void CalculateK()
        {
            K = new Complex[N];
            var l05 = L / 2;
            for(var i = 0; i < N; i++)
            {
                var x = i * d - l05;
                K[i] = a(x)*Complex.Exp(new Complex(0, ph(x)));
            }
        }

        public LinearAntennaArray(double d, int N, Func<int, Antenna> Creator)
        {
            this.d = d;
            this.elements = new Antenna[N];
            for(var i = 0; i < N; i++)
                elements[i] = Creator(i);
        }

        public LinearAntennaArray(Antenna[] elements, double d)
        {
            this.d = d;
            this.elements = elements;
            CalculateK();
        }

        public override Complex Pattern(double th)
        {
            var d = this.d;
            var l05 = L / 2d;
            var dl05 = d / l05;
            var sin_th = -Math.Sin(th) * Math.PI * 2 / l05;

            var result = new Complex(0, 0);

            for(var i = 0; i < N; i++)
            {
                var a = elements[i];
                var dn = a.Pattern(th);
                result += K[i] * dn * Complex.Exp(new Complex(0, (i * dl05 - 1) * sin_th));
            }

            return result / N;
        }
    }

    class Uniform : Antenna
    {
        public override Complex Pattern(double th)
        {
            return 1;
        }
    }
}

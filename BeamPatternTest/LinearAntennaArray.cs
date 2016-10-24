using System;
using System.Linq;
using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>ЛИнейная антенная решётка</summary>
    class LinearAntennaArray : Antenna
    {
        public static class AmplitudeDistribution
        {
            public static Func<double, double> CosOnPedistal(double Delta) => x => (1 - Delta) * Math.Cos(Math.PI * x) + Delta;
        }

        /// <summary>Шаг между излучателями</summary>
        private double d;

        /// <summary>Массив антенных элементов</summary>
        private Antenna[] elements;

        /// <summary>Комплексные коэффициенты возбуждения АР</summary>
        private Complex[] K;

        private double norma;

        /// <summary>Амплитудное распределение по полотну</summary>
        private Func<double, double> a = x => 1;

        /// <summary>Фазовое распределение по полотну</summary>
        private Func<double, double> ph = x => 0;

        /// <summary>Длина решётки</summary>
        public double L => d * (N - 1);

        /// <summary>Число элементов решётки</summary>
        public int N => elements.Length;

        /// <summary>Функция, определяющая амплитудное распределение в апертуре. A(x), где x лежит в интервале от -1/2 до 1/2</summary>
        public Func<double, double> A
        {
            get { return a; }
            set
            {
                if(value == null) return;
                a = value;
                CalculateK();
            }
        }

        /// <summary>Функция, определяющая фазовое распределение в апертуре. Ф(x), где x лежит в интервале от -1/2 до 1/2</summary>
        public Func<double, double> Ph
        {
            get { return ph; }
            set
            {
                if(value == null) return;
                ph = value;
                CalculateK();
            }
        }

        /// <summary>
        /// Метод, определяющий новый массив комплексных коэффициентов возбуждения решётки. 
        /// Вызывается в конструкторах, а также при смене амплитудного, или фазового распределения
        /// </summary>
        private void CalculateK()
        {
            K = new Complex[N];
            norma = 0;
            for(var i = 0; i < N; i++)
            {
                var x = (i * d - 1) / 2d;
                K[i] = a(x) * Complex.Exp(new Complex(0, ph(x)));
                norma += K[i].Magnitude;
            }
        }

        /// <summary>Инициализация новой линейной антенной решётки с указанием шага между элементами, числа элементов и порождающей элементы функции</summary>
        /// <param name="d">Шаг между элементами в длинах волн</param>
        /// <param name="N">Число элементов решётки</param>
        /// <param name="Creator">Метод генерации элементов</param>
        public LinearAntennaArray(double d, int N, Func<int, Antenna> Creator)
            :this(d, Enumerable.Range(0, N).Select(Creator).ToArray()) { }

        /// <summary>Инициализация новой линейной антенной решётки</summary>
        /// <param name="d">Шаг между излучателями</param>
        /// <param name="elements">Массив элементов</param>
        public LinearAntennaArray(double d, params Antenna[] elements)
        {
            this.d = d;
            this.elements = elements;
            CalculateK();
        }

        /// <summary>Диаграмма направленности</summary>
        /// <param name="th">Угол места в радианах</param>
        /// <returns>Значение ДН для указанного угла</returns>
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

            return result / norma;
        }
    }
}
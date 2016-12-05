using System;
using System.Linq;
using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>ЛИнейная антенная решётка</summary>
    public class LinearAntennaArray : Antenna
    {
        /// <summary>Амплитудные распределения</summary>
        public static class AmplitudeDistribution
        {
            /// <summary>Косинус на пъедестале <paramref name="Delta"/> + (1 - <paramref name="Delta"/>) * <see cref="Math.Cos"/>(<see cref="Math.PI"/> * x)</summary>
            /// <param name="Delta">Пъедестал распределения</param>
            /// <returns>Фунция <paramref name="Delta"/> + (1 - <paramref name="Delta"/>) * <see cref="Math.Cos"/>(<see cref="Math.PI"/> * x)</returns>
            public static Func<double, double> CosOnPedistal(double Delta) => x => (1 - Delta) * Math.Cos(Math.PI * x) + Delta;
        }

        /// <summary>Шаг между излучателями</summary>
        private double f_d;

        /// <summary>Массив антенных элементов</summary>
        private readonly Antenna[] f_Elements;

        /// <summary>Комплексные коэффициенты возбуждения АР</summary>
        private Complex[] f_K;

        private double f_Norma;

        /// <summary>Амплитудное распределение по полотну</summary>
        private Func<double, double> f_A = x => 1;

        /// <summary>Фазовое распределение по полотну</summary>
        private Func<double, double> f_Ph = x => 0;

        /// <summary>Длина решётки</summary>
        public double L => f_d * (N - 1);

        public double BeamWidth07 => 0.89 / L;

        public double BeamWidth07Deg => 50.9932437666433D / L;

        public override BeamPattern Beam => new BeamPattern(Pattern, -Math.PI, Math.PI, Math.Min(BeamWidth07 / 25, Service.toDeg));

        public double dx
        {
            get { return f_d; }
            set
            {
                f_d = value;
                CalculateK();
            }
        }

        /// <summary>Число элементов решётки</summary>
        public int N => f_Elements.Length;

        public Antenna[] Elements => f_Elements;

        /// <summary>Функция, определяющая амплитудное распределение в апертуре. A(x), где x лежит в интервале от -1/2 до 1/2</summary>
        public Func<double, double> A
        {
            get { return f_A; }
            set
            {
                if(value == null) return;
                f_A = value;
                CalculateK();
            }
        }

        /// <summary>Функция, определяющая фазовое распределение в апертуре. Ф(x), где x лежит в интервале от -1/2 до 1/2</summary>
        public Func<double, double> Ph
        {
            get { return f_Ph; }
            set
            {
                if(value == null) return;
                f_Ph = value;
                CalculateK();
            }
        }

        /// <summary>
        /// Метод, определяющий новый массив комплексных коэффициентов возбуждения решётки. 
        /// Вызывается в конструкторах, а также при смене амплитудного, или фазового распределения
        /// </summary>
        private void CalculateK()
        {
            f_K = new Complex[N];
            f_Norma = 0;
            for(var i = 0; i < N; i++)
            {
                var x = (i * f_d - 1) / 2d;
                f_K[i] = f_A(x) * Complex.Exp(new Complex(0, f_Ph(x)));
                f_Norma += f_K[i].Magnitude;
            }
        }

        /// <summary>Инициализация новой линейной антенной решётки с указанием шага между элементами, числа элементов и порождающей элементы функции</summary>
        /// <param name="d">Шаг между элементами в длинах волн</param>
        /// <param name="N">Число элементов решётки</param>
        /// <param name="Creator">Метод генерации элементов</param>
        public LinearAntennaArray(double d, int N, Func<int, Antenna> Creator)
            : this(d, Enumerable.Range(0, N).Select(Creator).ToArray()) { }

        /// <summary>Инициализация новой линейной антенной решётки</summary>
        /// <param name="d">Шаг между излучателями</param>
        /// <param name="elements">Массив элементов</param>
        public LinearAntennaArray(double d, params Antenna[] elements)
        {
            f_d = d;
            f_Elements = elements;
            CalculateK();
        }

        /// <inheritdoc />
        public override Complex Pattern(double th)
        {
            var sin_th = Math.Sin(th);
            var d = f_d;

            var result = new Complex(0, 0);
            var n = N;
            var pi2d = Service.pi2 * d;
            var e0 = pi2d * sin_th;
            var e = pi2d * ((n - 1) / 2d) * sin_th;
            for(var i = 0; i < n; i++)
            {
                var dn = f_Elements[i].Pattern(th);
                result += f_K[i] * dn * new Complex(Math.Cos(e), Math.Sin(e));
                e -= e0;
            }

            return result / f_Norma;
        }
    }
}
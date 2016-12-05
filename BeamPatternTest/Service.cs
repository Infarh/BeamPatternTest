using System;
using System.Collections.Generic;

namespace BeamPatternTest
{
    public static class Service
    {
        public const double pi2 = 2 * Math.PI;
        public const double pi05 = 0.5 * Math.PI;
        public const double toRad = Math.PI / 180;
        public const double toDeg = 180 / Math.PI;

        /// <summary>Интегрирование функции методом трапеций</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="a">Начало интервала интегрирования</param>
        /// <param name="b">Конец интервала интегрирования</param>
        /// <param name="dx">Шаг интегрирования</param>
        /// <returns>Значение интегала функции</returns>
        public static double Integrate(this Func<double, double> f, double a, double b, double dx)
        {
            if(a > b) return f.Integrate(b, a, dx);
            if(a >= b - dx) return 0;
            var s = 0.5 * (f(a) + f(b));
            b -= dx;
            while(a < b) s += f(a += dx);
            return s * dx;
        }

        /// <summary>Интегрирование последовательности отсчётов функции методом тропеций</summary>
        /// <typeparam name="T">Тип структуры отсчёта функции</typeparam>
        /// <param name="values">Интегрируемая последовательнность отсчётов функции</param>
        /// <param name="F">Метод извлечения значения функции из структуры отсчёта</param>
        /// <param name="X">Метод извлечения аргумента функции из структуры отсчёта</param>
        /// <param name="f0">Начальное значение функции</param>
        /// <returns>Численное значение интеграла последовательности отсчётов функции методом тропеций</returns>
        public static double Integrate<T>(this IEnumerable<T> values, Func<T, double> F, Func<T, double> X, double f0 = 0)
        {
            using(var value = values?.GetEnumerator())
            {
                if(value?.MoveNext() != true) return double.NaN;
                var last = value.Current;
                var last_f = F(last);
                var last_x = X(last);
                while(value.MoveNext())
                {
                    var v = value.Current;
                    var f = F(v);
                    var x = X(v);
                    f0 += 0.5 * (f + last_f) * (x - last_x);
                    last_x = x;
                    last_f = f;
                }
                return f0;
            }
        }

        public static double to_db(this double value) => 20 * Math.Log10(value);

        public static double to_dbP(this double value) => 10 * Math.Log10(value);

        public static double from_db(this double db) => Math.Pow(10, db / 20);

        public static double from_pbP(this double dbP) => Math.Pow(10, dbP / 10);

        public static T GetMax<T>(this IEnumerable<T> elements, Func<T, double> selector)
        {
            var max_t = default(T);
            var max = double.NegativeInfinity;
            foreach(var element in elements)
            {
                var v = selector(element);
                if(!(v > max)) continue;
                max = v;
                max_t = element;
            }
            return max_t;
        }

        public static int MaxIndex<T>(this T[] array, Func<T, double> selector, int start = 0, int end = -1)
        {
            if(end < 0 || end > array.Length) end = array.Length;
            var max = double.NegativeInfinity;
            var index = 0;
            for(var i = start; i < end; i++)
            {
                var e = array[i];
                var v = selector(e);
                if(!(v > max)) continue;
                max = v;
                index = i;
            }
            return index;
        }


        public static int IndexOf<T>(this T[] array, Func<T, bool> selector, int start = 0, int end = -1)
        {
            if(end < 0 || end > array.Length) end = array.Length;
            for(var i = start; i < end; i++)
                if(selector(array[i])) return i;
            return -1;
        }
    }
}
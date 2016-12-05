using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>Значение диаграммы направленности в точке</summary>
    [DebuggerDisplay("{DebugView()}")]
    public struct BeamPatternValue
    {
        private string DebugView() => $"{AngleDeg:0.##}°:{Abs:0.##}({db:0.##})e^j{PhaseDeg:0.##}°";

        /// <summary>Угловое положение точки</summary>
        public double Angle { get; }

        public double AngleDeg => Angle * Service.toDeg;

        /// <summary>Значение ДН в точке</summary>
        public Complex Value { get; }

        public double Abs => Value.Magnitude;

        public double Power
        {
            get
            {
                var abs = Abs;
                return abs * abs;
            }
        }

        public double db => 20 * Math.Log10(Value.Magnitude);

        public double Phase => Value.Phase;
        public double PhaseDeg => Value.Phase * Service.toDeg;
        public double Re => Value.Real;
        public double Im => Value.Imaginary;

        /// <summary>Инициализация нового значения ДН</summary>
        /// <param name="Angle">Угол</param>
        /// <param name="Value">Значение ДН для указанного угла</param>
        public BeamPatternValue(double Angle, Complex Value)
        {
            this.Angle = Angle;
            this.Value = Value;
        }

        /// <summary>Инициализация нового значения ДН</summary>
        /// <param name="Angle">Угол</param>
        /// <param name="Abs">Модуль ДН</param>
        /// <param name="Phase">Фаза ДН</param>
        public BeamPatternValue(double Angle, double Abs, double Phase = 0)
        {
            this.Angle = Angle;
            Value = new Complex(Abs * Math.Cos(Phase), Abs * Math.Sin(Phase));
        }
    }

    /// <summary>Значение диаграммы направленности</summary>
    public class BeamPattern : IEnumerable<BeamPatternValue>
    {
        private readonly double f_th1;
        private readonly double f_th2;

        /// <summary>Шаг измерения угла диаграммы направленности</summary>
        private readonly double f_dth;
        /// <summary>Отсчёты диаграммы направленности</summary>
        private readonly BeamPatternValue[] f_Values;

        /// <summary>Шаг измерения угла диаграммы направленности</summary>
        public double dth => f_dth;
        public double dth_deg => f_dth * Service.toDeg;

        /// <summary>Угол начала отсчёта сектора определения ДН</summary>
        public double th1 => f_th1;

        public double th1_deg => f_th1 * Service.toDeg;
        /// <summary>Угол конца отсчёта сектора определения ДН</summary>
        public double th2 => f_th2;
        public double th2_deg => f_th2 * Service.toDeg;

        /// <summary>Ширина сектора определения ДН</summary>
        public double DeltaTh => f_th2 - f_th1;
        public double DeltaTh_deg => (f_th2 - f_th1) * Service.toDeg;

        /// <summary>КНД</summary>
        public double D => 2 / f_Values.Where(v => Math.Abs(v.Angle) <= Service.pi05).Integrate(v => v.Power * Math.Cos(v.Angle), v => v.Angle);

        public double Ddb => D.to_db();

        public double BeamWidthDeg => BeamWidth * Service.toDeg;

        private double f_BeamWidth = double.NaN;
        public double BeamWidth
        {
            get
            {
                if(!double.IsNaN(f_BeamWidth)) return f_BeamWidth;
                var max_index = MaxIndex;
                if(max_index < 0) return double.NaN;
                var max_value = f_Values[max_index].Abs;

                const double treshold_level = 0.707106781186547;
                var left_edge_index = -1;
                for(var i = max_index - 1; i >= 0 && left_edge_index == -1; i--)
                    if(f_Values[i].Abs / max_value <= treshold_level)
                        left_edge_index = i;
                if(left_edge_index == -1) return double.NaN;
                var left_edge_angle = GetArgument(max_value * treshold_level,
                    f_Values[left_edge_index].Angle, f_Values[left_edge_index].Abs,
                    f_Values[left_edge_index + 1].Angle, f_Values[left_edge_index + 1].Abs);

                var right_edge_index = -1;
                for(var i = max_index + 1; i < f_Values.Length && right_edge_index == -1; i++)
                    if(f_Values[i].Abs / max_value <= treshold_level)
                        right_edge_index = i;
                if(right_edge_index == -1) return double.NaN;
                var right_edge_angle = GetArgument(max_value * treshold_level,
                    f_Values[right_edge_index - 1].Angle, f_Values[right_edge_index - 1].Abs,
                    f_Values[right_edge_index].Angle, f_Values[right_edge_index].Abs);
                return f_BeamWidth = right_edge_angle - left_edge_angle;
            }
        }

        public double LeftBeamEdge07Deg => LeftBeamEdge07 * Service.toDeg;

        private double f_LeftBeamEdge07 = double.NaN;
        public double LeftBeamEdge07
        {
            get
            {
                if(!double.IsNaN(f_LeftBeamEdge07)) return f_LeftBeamEdge07;
                var max_index = MaxIndex;
                if(max_index == -1) return double.NaN;
                var max_value = f_Values[max_index].Abs;

                const double treshold_level = 0.707106781186547;
                var left_edge_index = -1;
                for(var i = max_index - 1; i >= 0 && left_edge_index == -1; i--)
                    if(f_Values[i].Abs / max_value <= treshold_level)
                        left_edge_index = i;
                var left_beam_edge_angle = GetArgument(max_value * treshold_level,
                    f_Values[left_edge_index].Angle, f_Values[left_edge_index].Abs,
                    f_Values[left_edge_index + 1].Angle, f_Values[left_edge_index + 1].Abs);
                return f_LeftBeamEdge07 = left_beam_edge_angle;
            }
        }

        public double RightBeamEdge07Deg => RightBeamEdge07 * Service.toDeg;

        private double f_RightBeamEdge07 = double.NaN;
        public double RightBeamEdge07
        {
            get
            {
                if(!double.IsNaN(f_RightBeamEdge07)) return f_RightBeamEdge07;
                var max_index = MaxIndex;
                if(max_index == -1) return double.NaN;
                var max_value = f_Values[max_index].Abs;

                const double treshold_level = 0.707106781186547;
                var right_edge_index = -1;
                for(var i = max_index + 1; i < f_Values.Length && right_edge_index == -1; i++)
                    if(f_Values[i].Abs / max_value <= treshold_level)
                        right_edge_index = i;
                if(right_edge_index == -1) return double.NaN;
                var right_edge_angle = GetArgument(max_value * treshold_level,
                    f_Values[right_edge_index - 1].Angle, f_Values[right_edge_index - 1].Abs,
                    f_Values[right_edge_index].Angle, f_Values[right_edge_index].Abs);
                return f_RightBeamEdge07 = right_edge_angle;
            }
        }

        public double LeftBeamEdge0Deg => LeftBeamEdge0 * Service.toDeg;

        private double f_LeftBeamEdge0 = double.NaN;
        public double LeftBeamEdge0
        {
            get
            {
                if(!double.IsNaN(f_LeftBeamEdge0)) return f_LeftBeamEdge0;
                var max_index = MaxIndex;
                if(max_index == -1) return double.NaN;
                var max_value = f_Values[max_index].Abs;

                const double treshold_level = 0.707106781186547;

                var left_edge_index = -1;
                for(var i = max_index - 1; i >= 0 && left_edge_index == -1; i--)
                    if(f_Values[i].Abs / max_value <= treshold_level)
                        left_edge_index = i;
                if(left_edge_index == -1) return double.NaN;

                while(left_edge_index > 0 && f_Values[left_edge_index].Abs < f_Values[left_edge_index + 1].Abs) left_edge_index--;
                return f_LeftBeamEdge0 = f_Values[left_edge_index].Angle;
            }
        }

        public double RightBeamEdge0Deg => RightBeamEdge0 * Service.toDeg;

        private double f_RightBeamEdge0 = double.NaN;
        public double RightBeamEdge0
        {
            get
            {
                if(!double.IsNaN(f_RightBeamEdge0)) return f_RightBeamEdge0;
                var max_index = MaxIndex;
                if(max_index == -1) return double.NaN;
                var max_value = f_Values[max_index].Abs;

                const double treshold_level = 0.707106781186547;

                var right_edge_index = -1;
                for(var i = max_index + 1; i < f_Values.Length && right_edge_index == -1; i++)
                    if(f_Values[i].Abs / max_value <= treshold_level)
                        right_edge_index = i;
                if(right_edge_index == -1) return double.NaN;

                while(right_edge_index > 0 && f_Values[right_edge_index].Abs < f_Values[right_edge_index - 1].Abs) right_edge_index++;

                return f_RightBeamEdge0 = f_Values[right_edge_index].Angle;
            }
        }

        private static double GetArgument(double y, double x1, double y1, double x2, double y2) =>
            x1 + (y - y1) * (x2 - x1) / (y2 - y1);

        public BeamPatternValue Maximum => f_Values.Where(v => Math.Abs(v.Angle) <= Service.pi05).GetMax(v => v.Abs);

        public double SideLobLevel_db => 20 * Math.Log10(SideLobeLevel);

        private double f_SideLobeLevel = double.NaN;
        public double SideLobeLevel
        {
            get
            {
                if(!double.IsNaN(f_SideLobeLevel)) return f_SideLobeLevel;
                var max_index = MaxIndex;
                if(max_index == -1) return double.NaN;
                var max_value = f_Values[max_index].Abs;

                const double treshold_level = 0.707106781186547;

                var left_edge_index = -1;
                for(var i = max_index - 1; i >= 0 && left_edge_index == -1; i--)
                    if(f_Values[i].Abs / max_value <= treshold_level)
                        left_edge_index = i;
                if(left_edge_index == -1) return double.NaN;

                var right_edge_index = -1;
                for(var i = max_index + 1; i < f_Values.Length && right_edge_index == -1; i++)
                    if(f_Values[i].Abs / max_value <= treshold_level)
                        right_edge_index = i;
                if(right_edge_index == -1) return double.NaN;

                while(left_edge_index > 0 && f_Values[left_edge_index].Abs < f_Values[left_edge_index + 1].Abs) left_edge_index--;
                while(right_edge_index > 0 && f_Values[right_edge_index].Abs < f_Values[right_edge_index - 1].Abs) right_edge_index++;
                var t1 = f_Values.Select(v => v.Angle > -Service.pi05 && v.Angle < Service.pi05 ? v.Abs : 0);
                var t2 = t1.Where((v, i) => !(i > left_edge_index && i < right_edge_index));
                return f_SideLobeLevel = t2.Max();
            }
        }

        private int f_MaxIndex = -1;
        private int MaxIndex
        {
            get
            {
                if(f_MaxIndex >= 0) return f_MaxIndex;
                var start_index = f_Values.IndexOf(v => v.Angle > -Service.pi05);
                if(start_index == -1) return -1;
                var end_index = f_Values.IndexOf(v => v.Angle > Service.pi05, start_index) - 1;
                if(end_index < 0) return -1;
                return f_MaxIndex = f_Values.MaxIndex(v => v.Abs, start_index, end_index);
            }
        }

        /// <summary>Значение ДН для указанного угла</summary>
        public Complex this[double th]
        {
            get
            {
                th = CorrectAngle(th);
                th -= f_th1;
                var i1 = (int)(th / f_dth);
                var i2 = i1 + 1;
                if(i1 < 0)
                    return i2 >= 0 ? f_Values[i2].Value : double.NaN;
                if(i2 >= f_Values.Length)
                    return i1 < f_Values.Length ? f_Values[i1].Value : double.NaN;

                return Interpolate(th, f_dth * i1, f_Values[i1].Value, f_dth * i2, f_Values[i2].Value);
            }
        }

        public BeamPatternValue this[int i] => f_Values[i];

        public int PointsCount => f_Values.Length;

        private static Complex Interpolate(double x, double x1, Complex y1, double x2, Complex y2) =>
            new Complex(Interpolate(x, x1, y1.Real, x2, y2.Real), Interpolate(x, x1, y1.Imaginary, x2, y2.Imaginary));

        private static double Interpolate(double x, double x1, double y1, double x2, double y2) =>
            y2 + (x - x1) * (y2 - y1) / (x2 - x1);

        private static void ReorderValues(ref double a, ref double b)
        {
            if(a <= b) return;
            var c = a;
            a = b;
            b = c;
        }

        /// <summary>Коррекция угла в интервал -pi до pi</summary>
        /// <param name="angle">Корректируемый угол</param>
        /// <returns>Скорректированное значение угла</returns>
        private static double CorrectAngle180(double angle)
        {
            if(angle >= -Math.PI || angle <= Math.PI) return angle;
            return CorrectAngle(angle - Math.PI) - Math.PI;
        }


        /// <summary>Коррекция угла в интервал 0 до 2pi</summary>
        /// <param name="angle">Корректируемый угол</param>
        /// <returns>Скорректированное значение угла</returns>
        private static double CorrectAngle(double angle)
        {
            if(angle >= 0 || angle <= Service.pi2) return angle;
            angle %= Service.pi2;
            if(angle < 0) angle += Service.pi2;

            return angle;
        }

        public BeamPattern(Func<double, Complex> Pattern, double th1, double th2, double dth)
        {
            f_dth = Math.Abs(dth);
            th1 = CorrectAngle180(th1);
            th2 = CorrectAngle180(th2);
            if(Math.Abs(th2 - th1) < dth)
                throw new ArgumentOutOfRangeException(nameof(dth), "Шаг расчёта ДН больше диапазона измеряемых углов");

            f_th1 = th1;
            f_th2 = th2;
            ReorderValues(ref th1, ref th2);
            var values = new List<BeamPatternValue>((int)((th2 - th1) / dth)) { new BeamPatternValue(th1, Pattern(th1)) };
            while((th1 += dth) < th2) values.Add(new BeamPatternValue(th1, Pattern(th1)));
            values.Add(new BeamPatternValue(th1, Pattern(th2)));
            f_Values = values.ToArray();
        }

        #region Implementation of IEnumerable

        /// <inheritdoc />
        public IEnumerator<BeamPatternValue> GetEnumerator() => ((IEnumerable<BeamPatternValue>)f_Values).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => f_Values.GetEnumerator();

        #endregion
    }
}
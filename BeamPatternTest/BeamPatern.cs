using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace BeamPatternTest
{
    public struct BeamPatternValue
    {
        public double Angle { get; }
        public Complex Value { get; }

        public double Abs => Value.Magnitude;

        public double db => 20 * Math.Log10(Value.Magnitude);

        public double AngleDeg => Angle * 180 / Math.PI;

        public BeamPatternValue(double angle, Complex value)
        {
            Angle = angle;
            Value = value;
        }
    }

    public class BeamPatern : IEnumerable<BeamPatternValue>
    {
        private readonly double f_th1, f_th2, f_dth;
        private readonly BeamPatternValue[] f_Values;

        public int PointsCount => f_Values.Length;

        public BeamPatternValue this[double th]
        {
            get
            {
                th -= f_th1;
                var i = (int)(th / f_dth);
                return f_Values[i];
            }
        }

        private int f_MaxIndex = -2;
        private int MaxIndex
        {
            get
            {
                if(f_MaxIndex > -2) return f_MaxIndex;
                var max = double.NegativeInfinity;
                var max_index = -1;
                for(var i = 0; i < f_Values.Length; i++)
                {
                    var v = f_Values[i];
                    if(v.Angle < -Math.PI / 2 || v.Angle > Math.PI / 2) continue;
                    if(v.Abs <= max) continue;
                    max = v.Abs;
                    max_index = i;
                }
                return f_MaxIndex = max_index;
            }
        }

        public double MaxPosAngle => f_Values[MaxIndex].Angle;
        public double MaxPosAngleDeg => MaxPosAngle * 180 / Math.PI;

        public double LeftBeamEdge07Deg => LeftBeamEdge07 * 180 / Math.PI;
        public double LeftBeamEdge07
        {
            get
            {
                var i = MaxIndex;
                if(i < 0) return double.NaN;
                var max = f_Values[i].Abs;

                while(i >= 0 && f_Values[i].Abs / max > 0.707) i--;
                if(i < 0) return double.NaN;
                return f_Values[i].Angle;
            }
        }

        public double RightBeamEdge07Deg => RightBeamEdge07 * 180 / Math.PI;
        public double RightBeamEdge07
        {
            get
            {
                var i = MaxIndex;
                if(i < 0) return double.NaN;
                var max = f_Values[i].Abs;

                while(i < f_Values.Length && f_Values[i].Abs / max > 0.707) i++;
                if(i >= f_Values.Length) return double.NaN;
                return f_Values[i].Angle;
            }
        }

        public double BeamWidth => RightBeamEdge07 - LeftBeamEdge07;
        public double BeamWidthDeg => BeamWidth * 180 / Math.PI;

        public BeamPatern(Func<double, Complex> F, double th1, double th2, double dth)
        {
            f_th1 = th1;
            f_th2 = th2;
            f_dth = dth;

            var values = new List<BeamPatternValue>();

            values.Add(new BeamPatternValue(th1, F(th1)));
            while((th1 += dth) < th2)
                values.Add(new BeamPatternValue(th1, F(th1)));
            values.Add(new BeamPatternValue(th2, F(th2)));

            f_Values = values.ToArray();
        }

        public IEnumerator<BeamPatternValue> GetEnumerator()
        {
            return ((IEnumerable<BeamPatternValue>)f_Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<BeamPatternValue>)f_Values).GetEnumerator();
        }
    }
}

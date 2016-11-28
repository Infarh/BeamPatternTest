using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;

namespace BeamPatternTest
{
    public class AntennaArray : Antenna
    {
        private Antenna[] f_Elements;
        private double[] f_X;
        private double[] f_A;
        private double[] f_Ph;

        public AntennaArray(XElement ArrayNode)
        {
            if(ArrayNode.Name != "AntennaArray")
                throw new FormatException("Неверный узел XML");

            var elements = new List<Antenna>();
            var x = new List<double>();
            var a = new List<double>();
            var ph = new List<double>();

            foreach(var node in ArrayNode.Descendants())
            {
                var type = (string)node.Attribute("Type");
                var antenna_type = Type.GetType("BeamPatternTest." + type);
                if(antenna_type == null) continue;

                var antenna = (Antenna)Activator.CreateInstance(antenna_type);
                elements.Add(antenna);

                x.Add((double)node.Attribute("x"));
                a.Add((double)node.Attribute("A"));
                ph.Add((double)node.Attribute("Ph"));
            }

            f_Elements = elements.ToArray();
            f_X = x.ToArray();
            f_A = a.ToArray();
            f_Ph = ph.ToArray();
        }

        public override Complex Pattern(double th)
        {
            var K = f_A.Aggregate(0d, (v, V) => v + V);
            var sin_th = Math.Sin(th);

            var S = new Complex();

            for(int i = 0; i < f_Elements.Length; i++)
            {
                var f1 = f_Elements[i].Pattern(th);
                f1 *= f_A[i];
                f1 *= new Complex(Math.Cos(f_Ph[i]), Math.Sin(f_Ph[i]));
                var t = 2 * Math.PI * f_X[i] * sin_th;
                f1 *= new Complex(Math.Cos(t), Math.Sin(-t));
                S += f1;
            }

            return S / K;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;

namespace BeamPatternTest
{
    public class AntennaArray : Antenna
    {
        private readonly Antenna[] f_Elements;
        private readonly double[] f_X;
        private readonly double[] f_A;
        private readonly double[] f_Ph;

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

        /// <inheritdoc />
        public override Complex Pattern(double th)
        {
            var sin_th = Math.Sin(th);

            var s = new Complex();

            for(var i = 0; i < f_Elements.Length; i++)
            {
                var t = Service.pi2 * f_X[i] * sin_th;
                s += f_A[i]
                     * new Complex(Math.Cos(f_Ph[i]), Math.Sin(f_Ph[i])) * f_Elements[i].Pattern(th)
                     * new Complex(Math.Cos(t), Math.Sin(-t));
            }

            return s / f_A.Sum();
        }
    }
}

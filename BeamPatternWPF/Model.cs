using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using BeamPatternTest;
using OxyPlot;

namespace BeamPatternWPF
{
    class Model : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string Name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));

        private LinearAntennaArray antenna;

        /// <summary>Антенная решётка</summary>
        public LinearAntennaArray Antenna
        {
            get { return antenna; }
            set
            {
                if(Equals(antenna, value)) return;
                antenna = value;
                OnPropertyChanged(nameof(Antenna));
            }
        }

        /// <summary>Шаг элементов решётки</summary>
        public double dx
        {
            get { return antenna.dx; }
            set
            {
                if(antenna.dx.Equals(value)) return;
                antenna = new LinearAntennaArray(value, antenna.N, i => new Dipole());
                OnPropertyChanged(nameof(dx));
                OnPropertyChanged(nameof(Antenna));
                CalculateBeam();
            }
        }

        /// <summary>Число элементов решётки</summary>
        public int N
        {
            get { return antenna.N; }
            set
            {
                if(antenna.N.Equals(value)) return;
                antenna = new LinearAntennaArray(antenna.dx, value, i => new Dipole());
                OnPropertyChanged(nameof(N));
                OnPropertyChanged(nameof(Antenna));
                CalculateBeam();
            }
        }

        /// <summary>Диаграмма направленности</summary>
        public DataPoint[] BeamPattern
        {
            get { return f_BeamPattern; }
            private set
            {
                if(Equals(f_BeamPattern, value)) return;
                f_BeamPattern = value;
                OnPropertyChanged(nameof(BeamPattern));
            }
        }

        public Model()
        {
            antenna = new LinearAntennaArray(0.5, 5, i => new Dipole());
            CalculateBeam();
        }

        private void CalculateBeam()
        {
            const int BeamDataLength = 360;
            var beam = new DataPoint[BeamDataLength];
            for(var i = 0; i < BeamDataLength; i++)
            {
                var q = -180.0 + i * 1.0;
                var q_rad = q * Math.PI / 180;
                var f = antenna.Pattern(q_rad).Magnitude;
                var f_db = 20 * Math.Log(Math.Abs(f));
                beam[i] = new DataPoint(q, f_db);
            }
            BeamPattern = beam;
        }

        private Complex[][] f_DataBeam;
        private GenericAntenna f_Antenna0;
        private DataPoint[] f_BeamPattern;

        public void ReadPatternFromFile(string file_name)
        {
            var file = new FileInfo(file_name);
            if(!file.Exists) return;

            var beam = new List<List<Complex>>();
            List<Complex> data_list = null;

            using(var data = file.OpenText())
            {
                data.ReadLine();
                data.ReadLine();

                while(!data.EndOfStream)
                {
                    var values = data.ReadLine().Split(' ')
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => s.Replace('.', ','))
                        .Select(double.Parse)
                        .ToArray();

                    var th = values[0];
                    var phi = values[1];
                    var abs = values[2];
                    var phase = values[4];

                    if(th == 0)
                    {
                        data_list = new List<Complex>();
                        beam.Add(data_list);
                    }
                    data_list.Add(new Complex(abs * Math.Cos(phase), abs * Math.Sin(phase)));
                }
            }

            f_DataBeam = beam.Select(b => b.ToArray()).ToArray();
            f_Antenna0 = new GenericAntenna(f_DataBeam[0].Concat(f_DataBeam[180].Skip(1).Reverse()).ToArray(), 1);
            f_Antenna0.Pattern((15 - 360 - 360) * Math.PI / 180);
        }

        class GenericAntenna : Antenna
        {
            private readonly Complex[] f_BeamData;
            private readonly double f_dTh;

            public GenericAntenna(Complex[] BeamData, double dTh)
            {
                f_BeamData = BeamData;
                f_dTh = dTh;
            }

            private const double pi2 = Math.PI * 2;
            private const double toRad = Math.PI / 180;
            public override Complex Pattern(double th)
            {
                th %= pi2;
                if(th < 0) th += pi2;
                var i = (int)(th / (f_dTh * toRad));
                var th1 = i * f_dTh * toRad;
                var th2 = (i + 1) * f_dTh * toRad;
                var f1 = f_BeamData[i];
                var f2 = f_BeamData[i + 1];
                return Interpolate(th, th1, f1, th2, f2);
            }

            private static Complex Interpolate(double th, double th1, Complex F1, double th2, Complex F2)
            {
                return new Complex(
                    real: Interpolate(th, th1, F1.Real, th2, F2.Real),
                    imaginary: Interpolate(th, th1, F1.Imaginary, th2, F2.Imaginary));
            }

            private static double Interpolate(double th, double th1, double F1, double th2, double F2)
            {
                return F1 + (th - th1) * (F2 - F1) / (th2 - th1);
            }
        }
    }
}

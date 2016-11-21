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

        private void OnPropertyChanged(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }

        private LinearAntennaArray antenna;

        public LinearAntennaArray Antenna => antenna;

        public double dx
        {
            get { return antenna.dx; }
            set
            {
                antenna.dx = value;
                CalculateBeam();
                OnPropertyChanged("dx");
                OnPropertyChanged("Antenna");
                OnPropertyChanged("BeamPattern");
            }
        }

        public int N
        {
            get { return antenna.N; }
            set
            {
                var antenna_dx = antenna.dx;
                antenna = new LinearAntennaArray(antenna_dx, value, i => new Dipole());
                CalculateBeam();
                OnPropertyChanged("N");
                OnPropertyChanged("Antenna");
                OnPropertyChanged("BeamPattern");
            }
        }

        public DataPoint[] BeamPattern { get; private set; }

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
                    var line = data.ReadLine();
                    var values_str = line.Split(' ');
                    var values = values_str
                        .Where(s => s != null && s != "")
                        .Select(s => s.Replace('.', ','))
                        .Select(s => double.Parse(s))
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
                var th1 = th % pi2;
                if(th1 < 0) th1 += pi2;
                var i = (int)(th1 / (f_dTh * toRad));
                return f_BeamData[i];
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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

        public OxyPlot.DataPoint[] BeamPattern { get; private set; }

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

        public void ReadPatternFromFile(string file_name)
        {
            var file = new FileInfo(file_name);
            if (!file.Exists) return;

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BeamPatternTest;

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
                OnPropertyChanged("dx");
                OnPropertyChanged("Antenna");
            }
        }

        public int N
        {
            get { return antenna.N; }
            set
            {
                antenna = new LinearAntennaArray(antenna.dx, N, i => new Dipole());
                OnPropertyChanged("N");
                OnPropertyChanged("Antenna");
            }
        }

        public Model()
        {
            antenna = new LinearAntennaArray(0.5, 5, i => new Dipole());
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using BeamPatternTest;
using OxyPlot;

namespace BeamPatternWPF
{
    /// <summary>Млавная модель логики работы программы</summary>
    class Model : INotifyPropertyChanged
    {
        /// <summary>Событие возникает, когда значение свойства модели изменилось</summary>
        /// <remarks>В параметрах сбоытия передаётся имя свйоства, значение которого изменилось</remarks>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Генерация события изменения значения свойства</summary>
        /// <param name="Name"></param>
        private void OnPropertyChanged(string Name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));

        /// <summary>Исследуемая антенная решётка</summary>
        private LinearAntennaArray f_Antenna;

        /// <summary>Антенная решётка</summary>
        public LinearAntennaArray Antenna
        {
            get { return f_Antenna; }
            set
            {
                if(Equals(f_Antenna, value)) return;
                f_Antenna = value;
                OnPropertyChanged(nameof(Antenna));
                OnPropertyChanged(nameof(Beam));
                CalculateBeam();
            }
        }

        /// <summary>Воссоздать антенную решётку по текущим параметрам модели</summary>
        private void RecreateAntenna() => Antenna = new LinearAntennaArray(f_dx, f_N, i => new Dipole());

        /// <summary>Шаг элементов решётки</summary>
        private double f_dx;
        /// <summary>Шаг элементов решётки</summary>
        public double dx
        {
            get { return f_Antenna.dx; }
            set
            {
                if(f_dx.Equals(value)) return;
                f_dx = value;
                OnPropertyChanged(nameof(dx));
                RecreateAntenna();
            }
        }

        /// <summary>Число элементов решётки</summary>
        private int f_N;
        /// <summary>Число элементов решётки</summary>
        public int N
        {
            get { return f_N; }
            set
            {
                if(f_N.Equals(value)) return;
                f_N = value;
                OnPropertyChanged(nameof(N));
                RecreateAntenna();
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

        public BeamPattern Beam => f_Antenna?.Beam;

        public ICommand LoadBeamDataCommand { get; }
        public ICommand TestCommand { get; }

        public IEnumerable<Complex> Graph { get; private set; }

        public Model()
        {
            LoadBeamDataCommand = new LambdaCommand("Загрузка ДН из файла", OnLoadBeamDataCommandExecutedAsync);
            var phase = 0d;
            TestCommand = new LambdaCommand("Тест", p =>
            {
                Graph = Enumerable
                    .Range(0, 1000)
                    .Select(i => i / 1000d)
                    .Select(x => new Complex(x, Math.Sin(2 * Math.PI * x + phase)));
                phase += Math.PI / 8;
                OnPropertyChanged(nameof(Graph));
            });

            f_Antenna = new LinearAntennaArray(f_dx = 0.5, f_N = 5, i => new Dipole());// { A = LinearAntennaArray.AmplitudeDistribution.CosOnPedistal(0.2) };
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
                var f = f_Antenna.Pattern(q_rad).Magnitude;
                var f_db = 20 * Math.Log(Math.Abs(f));
                beam[i] = new DataPoint(q, f_db);
            }
            BeamPattern = beam;
        }

        private async void OnLoadBeamDataCommandExecutedAsync(object parameter)
        {
            var file_name = parameter as string
                ?? FileDialogEx.Open("Выбор файла с данными ДН", "Текстовые данные (*.txt)|*.txt|Все файлы (*.*)|*.*");
            await ReadPatternFromFileAsync(file_name).ConfigureAwait(false);
        }

        private Complex[][] f_DataBeam;
        private GenericAntenna f_Antenna0;
        private DataPoint[] f_BeamPattern;

        private async Task ReadPatternFromFileAsync(string file_name)
        {
            var file = new FileInfo(file_name);
            if(!file.Exists) return;

            var beam = new List<List<Complex>>();
            List<Complex> data_list = null;

            using(var data = file.OpenText())
            {
                await data.ReadLineAsync();
                await data.ReadLineAsync();

                while(!data.EndOfStream)
                {
                    var values = (await data.ReadLineAsync()).Split(' ')
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => s.Replace('.', ','))
                        .Select(double.Parse)
                        .ToArray();

                    var th = values[0];
                    var phi = values[1];
                    var abs = values[2];
                    var phase = values[4];

                    if(th.Equals(0))
                    {
                        data_list = new List<Complex>();
                        beam.Add(data_list);
                    }
                    data_list.Add(new Complex(abs * Math.Cos(phase), abs * Math.Sin(phase)));
                }
                f_DataBeam = beam.Select(b => b.ToArray()).ToArray();
                f_Antenna0 = new GenericAntenna(f_DataBeam[0].Concat(f_DataBeam[180].Skip(1).Reverse()).ToArray(), 1);
            }
        }
    }
}

using System.Numerics;

namespace BeamPatternTest
{
    /// <summary>Всенаправленная антенна</summary>
    public class Uniform : Antenna
    {
        /// <summary>Диаграмма направленности</summary>
        /// <param name="th">Угол места</param>
        /// <returns>Тождественно = 1</returns>
        public override Complex Pattern(double th) => 1;
    }
}
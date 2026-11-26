namespace FotF.Logic
{
    /// <summary>Источник случайности для Logic: детерминирован от seed (Architecture §2).</summary>
    public interface IRandom
    {
        /// <summary>[0, 1)</summary>
        float NextFloat();
        /// <summary>[min, max)</summary>
        int NextInt(int min, int max);
    }

    public sealed class SeededRandom : IRandom
    {
        readonly System.Random _r;
        public SeededRandom(int seed) { _r = new System.Random(seed); }
        public float NextFloat() => (float)_r.NextDouble();
        public int NextInt(int min, int max) => _r.Next(min, max);
    }
}

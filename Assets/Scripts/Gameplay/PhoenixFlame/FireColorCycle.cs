namespace SoftGames.Gameplay.PhoenixFlame
{
    public enum FireColorId
    {
        Orange = 0,
        Green = 1,
        Blue = 2
    }

    /// <summary>
    /// Plain cycle: Orange → Green → Blue → Orange.
    /// </summary>
    public sealed class FireColorCycle
    {
        public FireColorId Current { get; private set; }

        public FireColorCycle(FireColorId start = FireColorId.Orange)
        {
            Current = start;
        }

        public FireColorId Advance()
        {
            Current = Current switch
            {
                FireColorId.Orange => FireColorId.Green,
                FireColorId.Green => FireColorId.Blue,
                _ => FireColorId.Orange
            };
            return Current;
        }
    }
}

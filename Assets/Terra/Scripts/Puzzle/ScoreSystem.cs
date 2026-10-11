namespace TerraTactix.Puzzle
{
    public sealed class ScoreSystem
    {
        public int Value { get; private set; }

        public void Add(int amount)
        {
            if (amount > 0)
                Value += amount;
        }
    }

    public sealed class ComboSystem
    {
        public int Chain { get; private set; }

        public int Multiplier => Chain < 2 ? 1 : Chain > 10 ? 10 : Chain;

        public void Register()
        {
            Chain++;
        }

        public void Reset()
        {
            Chain = 0;
        }
    }

    public sealed class BattleTimer
    {
        public float Remaining { get; private set; }

        public BattleTimer(float seconds)
        {
            Remaining = seconds < 0f ? 0f : seconds;
        }

        public void Add(float seconds)
        {
            if (seconds > 0f)
                Remaining += seconds;
        }

        public void Tick(float delta)
        {
            if (delta <= 0f || Remaining <= 0f)
                return;
            Remaining -= delta;
            if (Remaining < 0f)
                Remaining = 0f;
        }

        public bool Expired => Remaining <= 0f;
    }

    public sealed class ObjectiveSystem
    {
        readonly int target;

        public ObjectiveSystem(int targetScore)
        {
            target = targetScore < 1 ? 1 : targetScore;
        }

        public bool IsReached(int score)
        {
            return score >= target;
        }
    }
}

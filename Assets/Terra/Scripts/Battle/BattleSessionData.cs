namespace TerraTactix.Battle
{
    public sealed class BattleSessionData
    {
        public string regionId;
        public int campaignIndex;
        public string difficulty;
        public float difficultyRating;
        public int targetScore;
        public float timeLimit;
        public float boardComplexity;
        public float fallInterval;
        public float scoreMultiplier = 1f;
        public int shapeVariety = 11;
        public float startedUnscaled;
        public int goldReward;
        public bool heartSpent;
        public bool resolved;
        public string resultId;
    }
}

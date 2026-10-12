using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.Battle
{
    public static class DifficultyResolver
    {
        public static void Apply(BattleSessionData session)
        {
            var balance = BalanceConfiguration.GetOrCreate();
            var definition = RegionCatalog.Get(session.regionId);
            var depth = definition == null ? 0 : definition.routeDepth;
            var level = Mathf.Clamp(session.campaignIndex - 1, 0, 19);
            var withinMap = level / 19f;

            // Her harita 20 bolumdur. Ilk harita rahat baslar; sonraki haritalar
            // sure ve tahta karmasikligi bakimindan duzenli olarak zorlasir.
            var challenge = Mathf.Clamp01((depth + withinMap) / 10f);
            session.difficulty = challenge < 0.20f ? "Easy"
                : challenge < 0.40f ? "Normal"
                : challenge < 0.62f ? "Hard"
                : challenge < 0.82f ? "VeryHard"
                : "Extreme";
            session.boardComplexity = Mathf.Lerp(0.08f, 1f, challenge);
            session.targetScore = balance.defaultTargetScore + depth * 180 + level * balance.campaignTargetStep;

            var easyTime = Mathf.Max(120f, balance.defaultTimeLimit);
            var hardestTime = Mathf.Max(70f, balance.minTimeLimit);
            session.timeLimit = Mathf.Lerp(easyTime, hardestTime, Mathf.Pow(challenge, 0.9f));
            session.fallInterval = balance.baseFallInterval * Mathf.Lerp(1f, 0.72f, challenge);
            session.goldReward = balance.battleGoldReward + depth * 12 + level * 4;
            session.scoreMultiplier = Multiplier(session.difficulty, balance);
            session.shapeVariety = Variety(session.difficulty, balance);
        }

        static float Multiplier(string difficulty, BalanceConfiguration balance)
        {
            switch (difficulty)
            {
                case "Normal": return balance.normalScoreMultiplier;
                case "Hard": return balance.hardScoreMultiplier;
                case "VeryHard": return balance.veryHardScoreMultiplier;
                case "Extreme": return balance.extremeScoreMultiplier;
                default: return balance.easyScoreMultiplier;
            }
        }

        static int Variety(string difficulty, BalanceConfiguration balance)
        {
            switch (difficulty)
            {
                case "Normal": return balance.normalShapeCount;
                case "Hard": return balance.hardShapeCount;
                case "VeryHard":
                case "Extreme": return balance.lateShapeCount;
                default: return balance.easyShapeCount;
            }
        }
    }
}

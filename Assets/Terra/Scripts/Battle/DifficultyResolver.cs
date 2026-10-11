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
            if (definition != null && definition.id == "baslangic")
            {
                ApplyStarterRamp(session, balance);
                return;
            }

            var depth = definition == null ? 0 : definition.routeDepth;
            var along = Mathf.Max(0, session.campaignIndex - 1);
            var step = depth * 2 + along;
            session.difficulty = step < 6 ? "Easy" : step < 16 ? "Normal" : step < 32 ? "Hard" : step < 48 ? "VeryHard" : "Extreme";
            session.targetScore = balance.defaultTargetScore + depth * 18 + along * balance.campaignTargetStep;
            var shrink = Mathf.Clamp(step * balance.timeShrinkPerStep, 0f, balance.maxTimeShrink);
            session.timeLimit = Mathf.Max(balance.minTimeLimit, balance.defaultTimeLimit * (1f - shrink));
            var speedUp = Mathf.Clamp(depth * 0.004f + along * 0.004f, 0f, 0.3f);
            session.fallInterval = balance.baseFallInterval * (1f - speedUp);
            session.goldReward = balance.battleGoldReward + depth * 3 + along * 5;
            session.scoreMultiplier = Multiplier(session.difficulty, balance);
            session.shapeVariety = Variety(session.difficulty, balance);
        }

        static void ApplyStarterRamp(BattleSessionData session, BalanceConfiguration balance)
        {
            var along = Mathf.Clamp(session.campaignIndex - 1, 0, 19);
            var band = along / 5;
            session.difficulty = band <= 0 ? "Easy" : band == 1 ? "Normal" : band == 2 ? "Hard" : "VeryHard";
            session.targetScore = balance.defaultTargetScore + along * 70;
            var ramp = along / 19f;
            session.timeLimit = Mathf.Max(balance.minTimeLimit, balance.defaultTimeLimit * (1f - 0.28f * ramp));
            session.fallInterval = balance.baseFallInterval * (1f - 0.28f * ramp);
            session.goldReward = balance.battleGoldReward + along * 4;
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

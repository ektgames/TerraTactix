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
            var level = Mathf.Clamp(session.campaignIndex - 1, 0, 19);
            var withinMap = level / 19f;

            // Ada puani temel zorlugu belirler; adanin 20 bolumu de kendi icinde
            // hafifce zorlasir. Boylece oyuncu haritadaki rotasini ozgurce secer.
            session.difficultyRating = definition == null ? 1f : definition.difficultyRating;
            var islandChallenge = Mathf.Clamp01((session.difficultyRating - 1f) / 9f);
            var challenge = Mathf.Clamp01(islandChallenge + withinMap * 0.06f);
            session.difficulty = challenge < 0.20f ? "Easy"
                : challenge < 0.40f ? "Normal"
                : challenge < 0.62f ? "Hard"
                : challenge < 0.82f ? "VeryHard"
                : "Extreme";
            session.boardComplexity = Mathf.Lerp(0.08f, 1f, challenge);
            session.targetScore = balance.defaultTargetScore + Mathf.RoundToInt((session.difficultyRating - 1f) * 220f) + level * balance.campaignTargetStep;

            var easyTime = Mathf.Max(120f, balance.defaultTimeLimit);
            var hardestTime = Mathf.Max(70f, balance.minTimeLimit);
            session.timeLimit = Mathf.Lerp(easyTime, hardestTime, Mathf.Pow(challenge, 0.9f));
            session.fallInterval = balance.baseFallInterval * Mathf.Lerp(1f, 0.72f, challenge);
            session.goldReward = balance.battleGoldReward + Mathf.RoundToInt((session.difficultyRating - 1f) * 12f) + level * 4;
            session.scoreMultiplier = Mathf.Lerp(balance.easyScoreMultiplier, balance.extremeScoreMultiplier, islandChallenge);
            session.shapeVariety = Variety(session.difficulty, balance);
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

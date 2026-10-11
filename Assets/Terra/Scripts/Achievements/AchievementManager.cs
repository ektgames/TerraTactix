using System.Collections.Generic;
using TerraTactix.Core;
using TerraTactix.Data;
using TerraTactix.Missions;
using UnityEngine;

namespace TerraTactix.Achievements
{
    public sealed class AchievementManager : MonoBehaviour
    {
        public static readonly GoalDefinition[] Definitions =
        {
            new GoalDefinition { id = "first-victory", title = "İlk zafer", description = "Bir sefer kazan.", target = 1, rewardType = RewardType.Gold, rewardAmount = 100, track = GoalTrack.BattleWon },
            new GoalDefinition { id = "score-1k", title = "Bin puan", description = "Toplam puanda 1.000'e ulaş.", target = 1000, rewardType = RewardType.Gold, rewardAmount = 40, track = GoalTrack.TotalScore, useHighest = true },
            new GoalDefinition { id = "merges-100", title = "Yüz patlama", description = "100 kez taş patlat.", target = 100, rewardType = RewardType.Gold, rewardAmount = 50, track = GoalTrack.Merge },
            new GoalDefinition { id = "ach-combo-5", title = "5 kat kombo", description = "Bir seferde 5 kombo yap.", target = 5, rewardType = RewardType.Gold, rewardAmount = 60, track = GoalTrack.BestCombo, useHighest = true },
            new GoalDefinition { id = "wins-10", title = "On zafer", description = "10 sefer kazan.", target = 10, rewardType = RewardType.Gold, rewardAmount = 80, track = GoalTrack.BattleWon },
            new GoalDefinition { id = "score-10k", title = "10 bin puan", description = "Toplam puanda 10.000'e ulaş.", target = 10000, rewardType = RewardType.Gold, rewardAmount = 150, track = GoalTrack.TotalScore, useHighest = true },
            new GoalDefinition { id = "spins-10", title = "On çark", description = "Günlük çarkı 10 kez çevir.", target = 10, rewardType = RewardType.Gold, rewardAmount = 200, track = GoalTrack.DailySpin },
            new GoalDefinition { id = "shop-5", title = "Müdavim", description = "Mağazadan 5 kez alışveriş yap.", target = 5, rewardType = RewardType.Gold, rewardAmount = 80, track = GoalTrack.ShopPurchase },
            new GoalDefinition { id = "hard-clear", title = "Zor zafer", description = "Zor veya daha yüksek bir sefer kazan.", target = 1, rewardType = RewardType.Gold, rewardAmount = 75, track = GoalTrack.HardClear },
            new GoalDefinition { id = "combo-10", title = "10 kat kombo", description = "Bir seferde 10 kombo yap.", target = 10, rewardType = RewardType.Gold, rewardAmount = 100, track = GoalTrack.BestCombo, useHighest = true },
            new GoalDefinition { id = "wins-50", title = "Elli zafer", description = "50 sefer kazan.", target = 50, rewardType = RewardType.Gold, rewardAmount = 150, track = GoalTrack.BattleWon },
            new GoalDefinition { id = "extreme-1", title = "Çok zor zafer", description = "Çok zor veya aşırı bir sefer kazan.", target = 1, rewardType = RewardType.Gems, rewardAmount = 15, track = GoalTrack.ExtremeClear },
            new GoalDefinition { id = "merges-500", title = "Beş yüz patlama", description = "500 kez taş patlat.", target = 500, rewardType = RewardType.Gold, rewardAmount = 150, track = GoalTrack.Merge },
            new GoalDefinition { id = "score-100k", title = "100 bin puan", description = "Toplam puanda 100.000'e ulaş.", target = 100000, rewardType = RewardType.Gems, rewardAmount = 30, track = GoalTrack.TotalScore, useHighest = true },
            new GoalDefinition { id = "hard-10", title = "On zor sefer", description = "Zor veya daha yüksek 10 sefer kazan.", target = 10, rewardType = RewardType.Gold, rewardAmount = 200, track = GoalTrack.HardClear },
            new GoalDefinition { id = "score-500k", title = "Yarım milyon", description = "Toplam puanda 500.000'e ulaş.", target = 500000, rewardType = RewardType.Gems, rewardAmount = 40, track = GoalTrack.TotalScore, useHighest = true },
            new GoalDefinition { id = "capture-10", title = "On bölge", description = "10 bölgeye sahip ol.", target = 10, rewardType = RewardType.Gems, rewardAmount = 20, track = GoalTrack.RegionCaptured },
            new GoalDefinition { id = "capture-25", title = "Yirmi beş bölge", description = "25 bölgeye sahip ol.", target = 25, rewardType = RewardType.Gems, rewardAmount = 30, track = GoalTrack.RegionCaptured }
        };

        AchievementProgressData progress;

        void OnEnable()
        {
            GameEvents.BattleWon += OnWin;
            GameEvents.BattleCompleted += OnBattle;
            GameEvents.RegionCaptured += OnCapture;
            GameEvents.DailySpinCompleted += OnSpin;
            GameEvents.MergePerformed += OnMerge;
            GameEvents.ComboReached += OnCombo;
            GameEvents.ShopPurchaseCompleted += OnShop;
        }

        void OnDisable()
        {
            GameEvents.BattleWon -= OnWin;
            GameEvents.BattleCompleted -= OnBattle;
            GameEvents.RegionCaptured -= OnCapture;
            GameEvents.DailySpinCompleted -= OnSpin;
            GameEvents.MergePerformed -= OnMerge;
            GameEvents.ComboReached -= OnCombo;
            GameEvents.ShopPurchaseCompleted -= OnShop;
        }

        public void Bind(AchievementProgressData data)
        {
            progress = data;
            progress.entries ??= new List<ProgressEntry>();
            var app = GameBootstrap.Instance;
            if (app == null || app.Save.Data == null)
                return;

            var changed = false;
            changed |= Apply(GoalTrack.TotalScore, app.Save.Data.player.totalScore, true);
            changed |= Apply(GoalTrack.RegionCaptured, app.Expeditions.CapturedCount(), true);
            changed |= Apply(GoalTrack.DailySpin, app.Save.Data.wheel.totalSpins, true);
            if (changed)
                GameBootstrap.Instance.Save.Save();
        }

        public ProgressEntry Get(string id)
        {
            return Find(id);
        }

        public bool HasReadyReward()
        {
            if (progress?.entries == null)
                return false;

            for (var i = 0; i < Definitions.Length; i++)
            {
                var definition = Definitions[i];
                for (var j = 0; j < progress.entries.Count; j++)
                {
                    var entry = progress.entries[j];
                    if (entry.id == definition.id && !entry.claimed && entry.progress >= definition.target)
                        return true;
                }
            }

            return false;
        }

        public bool TryClaim(string id, out string message)
        {
            var definition = FindDefinition(id);
            var entry = Find(id);
            if (definition == null)
            {
                message = Locale.T("Başarım bulunamadı.", "Achievement not found.");
                return false;
            }

            if (entry.claimed)
            {
                message = Locale.T("Bu başarım zaten alındı.", "This achievement is already claimed.");
                return false;
            }

            if (entry.progress < definition.target)
            {
                message = Locale.T("Başarım henüz bitmedi.", "This achievement is not finished yet.");
                return false;
            }

            entry.claimed = true;
            var applied = GameBootstrap.Instance.Rewards.TryApply("achievement:" + id, definition.rewardType, definition.rewardAmount);
            message = applied ? Locale.T("Başarım ödülü alındı.", "Achievement reward claimed.") : Locale.T("Ödül daha önce verilmiş.", "That reward was already given.");
            return applied;
        }

        void OnWin()
        {
            var changed = Apply(GoalTrack.BattleWon, 1, false);
            var session = GameBootstrap.Instance.Battles.Session;
            if (session != null && (session.difficulty == "Hard" || session.difficulty == "VeryHard" || session.difficulty == "Extreme"))
                changed |= Apply(GoalTrack.HardClear, 1, false);
            if (session != null && (session.difficulty == "VeryHard" || session.difficulty == "Extreme"))
                changed |= Apply(GoalTrack.ExtremeClear, 1, false);
            if (changed)
                GameBootstrap.Instance.Save.Save();
        }

        void OnBattle() => Commit(Apply(GoalTrack.TotalScore, GameBootstrap.Instance.Save.Data.player.totalScore, false));
        void OnCapture(string regionId) => Commit(Apply(GoalTrack.RegionCaptured, 1, false));
        void OnSpin() => Commit(Apply(GoalTrack.DailySpin, 1, false));
        void OnMerge() => Commit(Apply(GoalTrack.Merge, 1, false));
        void OnCombo(int multiplier) => Commit(Apply(GoalTrack.BestCombo, multiplier, false));
        void OnShop() => Commit(Apply(GoalTrack.ShopPurchase, 1, false));

        static void Commit(bool changed)
        {
            if (changed)
                GameBootstrap.Instance.Save.Save();
        }

        bool Apply(GoalTrack track, int amount, bool highest)
        {
            if (progress == null || amount <= 0)
                return false;

            var changed = false;
            for (var i = 0; i < Definitions.Length; i++)
            {
                var definition = Definitions[i];
                if (definition.track != track || Find(definition.id).claimed)
                    continue;
                var entry = Find(definition.id);
                var next = definition.useHighest || highest ? Mathf.Max(entry.progress, amount) : entry.progress + amount;
                if (next == entry.progress)
                    continue;
                entry.progress = next;
                changed = true;
            }

            return changed;
        }

        static GoalDefinition FindDefinition(string id)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].id == id)
                    return Definitions[i];
            }

            return null;
        }

        ProgressEntry Find(string id)
        {
            var entries = progress.entries;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].id == id)
                    return entries[i];
            }

            var created = new ProgressEntry { id = id };
            entries.Add(created);
            return created;
        }
    }
}

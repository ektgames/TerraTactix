using System.Collections.Generic;
using TerraTactix.Core;
using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.Missions
{
    public sealed class GoalDefinition
    {
        public string id;
        public string title;
        public string description;
        public int target;
        public RewardType rewardType;
        public int rewardAmount;
        public GoalTrack track;
        public bool useHighest;
    }

    public enum GoalTrack
    {
        GoldEarned,
        DailySpin,
        BattleWon,
        RegionCaptured,
        Merge,
        TotalScore,
        BestCombo,
        HardClear,
        BattlePlayed,
        GoldSpent,
        ShopPurchase,
        ExtremeClear
    }

    public sealed class MissionManager : MonoBehaviour
    {
        public static readonly GoalDefinition[] Definitions =
        {
            new GoalDefinition { id = "spin-once", title = "Günlük çark", description = "Çarkı bir kez çevir.", target = 1, rewardType = RewardType.Hearts, rewardAmount = 1, track = GoalTrack.DailySpin },
            new GoalDefinition { id = "play-1", title = "İlk adım", description = "Bir sefer oyna.", target = 1, rewardType = RewardType.Gold, rewardAmount = 20, track = GoalTrack.BattlePlayed },
            new GoalDefinition { id = "first-win", title = "İlk sefer", description = "Bir sefer kazan.", target = 1, rewardType = RewardType.Gold, rewardAmount = 50, track = GoalTrack.BattleWon },
            new GoalDefinition { id = "match-5", title = "İlk patlama", description = "5 kez taş patlat.", target = 5, rewardType = RewardType.Gold, rewardAmount = 20, track = GoalTrack.Merge },
            new GoalDefinition { id = "earn-gold", title = "Altın biriktir", description = "100 altın kazan.", target = 100, rewardType = RewardType.Gold, rewardAmount = 25, track = GoalTrack.GoldEarned },
            new GoalDefinition { id = "merge-10", title = "Taş patlat", description = "10 kez taş patlat.", target = 10, rewardType = RewardType.Gold, rewardAmount = 25, track = GoalTrack.Merge },
            new GoalDefinition { id = "shop-1", title = "Çarşıya uğra", description = "Mağazadan bir şey al.", target = 1, rewardType = RewardType.Hearts, rewardAmount = 1, track = GoalTrack.ShopPurchase },
            new GoalDefinition { id = "spend-50", title = "Harcama", description = "50 altın harca.", target = 50, rewardType = RewardType.Gold, rewardAmount = 25, track = GoalTrack.GoldSpent },
            new GoalDefinition { id = "win-3", title = "Üç sefer", description = "3 sefer kazan.", target = 3, rewardType = RewardType.Gold, rewardAmount = 40, track = GoalTrack.BattleWon },
            new GoalDefinition { id = "combo-3", title = "Küçük kombo", description = "Bir seferde 3 kombo yap.", target = 3, rewardType = RewardType.Gold, rewardAmount = 40, track = GoalTrack.BestCombo, useHighest = true },
            new GoalDefinition { id = "gold-300", title = "Kese", description = "300 altın kazan.", target = 300, rewardType = RewardType.Gold, rewardAmount = 50, track = GoalTrack.GoldEarned },
            new GoalDefinition { id = "match-25", title = "Patlama serisi", description = "25 kez taş patlat.", target = 25, rewardType = RewardType.Gold, rewardAmount = 40, track = GoalTrack.Merge },
            new GoalDefinition { id = "win-5", title = "Beş sefer", description = "5 sefer kazan.", target = 5, rewardType = RewardType.Gold, rewardAmount = 60, track = GoalTrack.BattleWon },
            new GoalDefinition { id = "spend-150", title = "Cömert", description = "150 altın harca.", target = 150, rewardType = RewardType.Gold, rewardAmount = 40, track = GoalTrack.GoldSpent },
            new GoalDefinition { id = "combo-5", title = "Kombo", description = "Bir seferde 5 kombo yap.", target = 5, rewardType = RewardType.Hearts, rewardAmount = 1, track = GoalTrack.BestCombo, useHighest = true },
            new GoalDefinition { id = "match-40", title = "Taş yağmuru", description = "40 kez taş patlat.", target = 40, rewardType = RewardType.Gold, rewardAmount = 50, track = GoalTrack.Merge },
            new GoalDefinition { id = "gold-700", title = "Hazine", description = "700 altın kazan.", target = 700, rewardType = RewardType.Gold, rewardAmount = 70, track = GoalTrack.GoldEarned },
            new GoalDefinition { id = "win-8", title = "Sekiz sefer", description = "8 sefer kazan.", target = 8, rewardType = RewardType.Gold, rewardAmount = 80, track = GoalTrack.BattleWon },
            new GoalDefinition { id = "match-70", title = "Büyük patlama", description = "70 kez taş patlat.", target = 70, rewardType = RewardType.Gold, rewardAmount = 70, track = GoalTrack.Merge },
            new GoalDefinition { id = "gold-1200", title = "Sandık", description = "1200 altın kazan.", target = 1200, rewardType = RewardType.Gold, rewardAmount = 100, track = GoalTrack.GoldEarned },
            new GoalDefinition { id = "hard-1", title = "Zor sefer", description = "Zor veya daha yüksek bir sefer kazan.", target = 1, rewardType = RewardType.Hearts, rewardAmount = 1, track = GoalTrack.HardClear },
            new GoalDefinition { id = "win-12", title = "On iki sefer", description = "12 sefer kazan.", target = 12, rewardType = RewardType.Gold, rewardAmount = 120, track = GoalTrack.BattleWon },
            new GoalDefinition { id = "combo-8", title = "Uzun kombo", description = "Bir seferde 8 kombo yap.", target = 8, rewardType = RewardType.Gold, rewardAmount = 100, track = GoalTrack.BestCombo, useHighest = true },
            new GoalDefinition { id = "match-120", title = "Taş fırtınası", description = "120 kez taş patlat.", target = 120, rewardType = RewardType.Hearts, rewardAmount = 2, track = GoalTrack.Merge }
        };

        MissionProgressData progress;

        void OnEnable()
        {
            GameEvents.GoldEarned += OnGold;
            GameEvents.GoldSpent += OnSpent;
            GameEvents.DailySpinCompleted += OnSpin;
            GameEvents.BattleWon += OnWin;
            GameEvents.BattleCompleted += OnPlayed;
            GameEvents.RegionCaptured += OnCapture;
            GameEvents.MergePerformed += OnMerge;
            GameEvents.ComboReached += OnCombo;
            GameEvents.ShopPurchaseCompleted += OnShop;
        }

        void OnDisable()
        {
            GameEvents.GoldEarned -= OnGold;
            GameEvents.GoldSpent -= OnSpent;
            GameEvents.DailySpinCompleted -= OnSpin;
            GameEvents.BattleWon -= OnWin;
            GameEvents.BattleCompleted -= OnPlayed;
            GameEvents.RegionCaptured -= OnCapture;
            GameEvents.MergePerformed -= OnMerge;
            GameEvents.ComboReached -= OnCombo;
            GameEvents.ShopPurchaseCompleted -= OnShop;
        }

        public void Bind(MissionProgressData data)
        {
            progress = data;
            progress.entries ??= new List<ProgressEntry>();
            var today = TimeService.TodayKey;
            if (string.IsNullOrEmpty(progress.lastResetDay))
            {
                progress.lastResetDay = today;
                return;
            }

            if (progress.lastResetDay == today)
                return;

            progress.lastResetDay = today;
            progress.entries.Clear();
            GameBootstrap.Instance.Save.Save();
        }

        public ProgressEntry Get(string id)
        {
            return Find(progress.entries, id);
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
            var entry = Get(id);
            if (definition == null || entry == null)
            {
                message = Locale.T("Görev bulunamadı.", "Mission not found.");
                return false;
            }

            if (entry.claimed)
            {
                message = Locale.T("Bu görev zaten alındı.", "This mission is already claimed.");
                return false;
            }

            if (entry.progress < definition.target)
            {
                message = Locale.T("Görev henüz bitmedi.", "This mission is not finished yet.");
                return false;
            }

            entry.claimed = true;
            var applied = GameBootstrap.Instance.Rewards.TryApply("mission:" + id + ":" + TimeService.TodayKey, definition.rewardType, definition.rewardAmount);
            message = applied ? Locale.T("Görev ödülü alındı.", "Mission reward claimed.") : Locale.T("Ödül daha önce verilmiş.", "That reward was already given.");
            return applied;
        }

        void OnGold(int amount) => Add(GoalTrack.GoldEarned, amount);
        void OnSpent(int amount) => Add(GoalTrack.GoldSpent, amount);
        void OnSpin() => Add(GoalTrack.DailySpin, 1);
        void OnPlayed() => Add(GoalTrack.BattlePlayed, 1);
        void OnCapture(string regionId) => Add(GoalTrack.RegionCaptured, 1);
        void OnMerge() => Add(GoalTrack.Merge, 1);
        void OnCombo(int multiplier) => Add(GoalTrack.BestCombo, multiplier, true);
        void OnShop() => Add(GoalTrack.ShopPurchase, 1);

        void OnWin()
        {
            Add(GoalTrack.BattleWon, 1, false, false);
            var session = GameBootstrap.Instance.Battles.Session;
            if (session != null && (session.difficulty == "Hard" || session.difficulty == "VeryHard" || session.difficulty == "Extreme"))
                Add(GoalTrack.HardClear, 1, false, false);
            GameBootstrap.Instance.Save.Save();
        }

        void Add(GoalTrack track, int amount, bool highest = false, bool save = true)
        {
            if (progress == null || amount <= 0)
                return;

            for (var i = 0; i < Definitions.Length; i++)
            {
                var definition = Definitions[i];
                if (definition.track != track)
                    continue;
                var entry = Find(progress.entries, definition.id);
                if (entry.claimed)
                    continue;
                var next = definition.useHighest || highest ? Mathf.Max(entry.progress, amount) : entry.progress + amount;
                if (next == entry.progress)
                    continue;
                entry.progress = next;
            }

            if (save)
                GameBootstrap.Instance.Save.Save();
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

        static ProgressEntry Find(List<ProgressEntry> entries, string id)
        {
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

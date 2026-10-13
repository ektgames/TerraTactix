using System;
using System.Collections.Generic;

namespace TerraTactix.Data
{
    [Serializable]
    public class GameSave
    {
        public int version = 1;
        public PlayerData player = new PlayerData();
        public EconomyData economy = new EconomyData();
        public WorldProgressData world = new WorldProgressData();
        public ShopProgressData shop = new ShopProgressData();
        public DailyWheelProgressData wheel = new DailyWheelProgressData();
        public MissionProgressData missions = new MissionProgressData();
        public AchievementProgressData achievements = new AchievementProgressData();
        public SettingsData settings = new SettingsData();
        public List<string> rewardTransactionIds = new List<string>();
        public List<BattleSubmission> pendingSubmissions = new List<BattleSubmission>();
        public string resumeScene = string.Empty;
        public ActiveBattleSave activeBattle = new ActiveBattleSave();

        public void Sanitize()
        {
            if (version < 1)
                version = 1;

            player ??= new PlayerData();
            economy ??= new EconomyData();
            world ??= new WorldProgressData();
            shop ??= new ShopProgressData();
            wheel ??= new DailyWheelProgressData();
            missions ??= new MissionProgressData();
            achievements ??= new AchievementProgressData();
            settings ??= new SettingsData();
            rewardTransactionIds ??= new List<string>();
            pendingSubmissions ??= new List<BattleSubmission>();
            activeBattle ??= new ActiveBattleSave();
            if (resumeScene == null)
                resumeScene = string.Empty;
            if (activeBattle.open && string.IsNullOrEmpty(activeBattle.regionId))
                activeBattle.open = false;

            world.regions ??= new List<RegionProgressEntry>();
            shop.levels ??= new List<ShopLevelEntry>();
            missions.entries ??= new List<ProgressEntry>();
            achievements.entries ??= new List<ProgressEntry>();

            if (player.nameChosen)
            {
                player.playerName = PlayerData.CleanName(player.playerName);
                if (player.playerName.Length == 0)
                    player.nameChosen = false;
            }
            else
                player.playerName = string.Empty;
            if (string.IsNullOrEmpty(player.playerId) || player.playerId.Length < 16)
                player.playerId = Guid.NewGuid().ToString("N");
            if (player.level < 1)
                player.level = 1;
            if (player.xpToNext < 1)
                player.xpToNext = 100;

            if (economy.maxHearts < 1)
                economy.maxHearts = 5;
            if (economy.hearts < 0)
                economy.hearts = 0;
            if (economy.hearts > economy.maxHearts)
                economy.hearts = economy.maxHearts;
            if (economy.gold < 0)
                economy.gold = 0;
            if (economy.gems < 0)
                economy.gems = 0;
            if (economy.timeFreezes < 0)
                economy.timeFreezes = 0;

            if (string.IsNullOrEmpty(settings.language))
                settings.language = "tr";
            if (!settings.audioReady)
            {
                settings.musicVolume = 1f;
                settings.soundVolume = 1f;
                settings.audioReady = true;
            }

            settings.musicVolume = UnityEngine.Mathf.Clamp01(settings.musicVolume);
            settings.soundVolume = UnityEngine.Mathf.Clamp01(settings.soundVolume);

            if (version < 2)
            {
                OpenStarterIsland();
                version = 2;
            }

            if (version < 3)
            {
                RemoveAttackSpeedUpgrade();
                version = 3;
            }
        }

        void RemoveAttackSpeedUpgrade()
        {
            if (shop == null || shop.levels == null)
                return;
            for (var i = shop.levels.Count - 1; i >= 0; i--)
            {
                if (shop.levels[i] != null && shop.levels[i].productId == "attack-speed")
                    shop.levels.RemoveAt(i);
            }
        }

        void OpenStarterIsland()
        {
            if (world == null || world.regions == null)
                return;
            for (var i = 0; i < world.regions.Count; i++)
            {
                var entry = world.regions[i];
                if (entry == null || entry.regionId != "baslangic")
                    continue;
                entry.state = RegionState.Available;
                entry.campaignProgress = 0;
                return;
            }
        }
    }

    [Serializable]
    public class ActiveBattleSave
    {
        public bool open;
        public string regionId;
        public int campaignIndex;
        public string difficulty;
        public int goldReward;
        public string resultId;
        public int score;
    }

    [Serializable]
    public class PlayerData
    {
        public const int MaxNameLength = 18;

        public string playerName = string.Empty;
        public string playerId = string.Empty;
        public bool nameChosen;
        public int level = 1;
        public int xp;
        public int xpToNext = 100;
        public int bestScore;
        public int totalScore;

        public static string CleanName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            var builder = new System.Text.StringBuilder(raw.Length);
            for (var i = 0; i < raw.Length; i++)
            {
                if (!char.IsControl(raw[i]))
                    builder.Append(raw[i]);
            }

            var text = builder.ToString().Trim();
            if (text.Length > MaxNameLength)
                text = text.Substring(0, MaxNameLength).Trim();
            return text;
        }

        public static string NameKey(string raw)
        {
            var name = CleanName(raw);
            if (name.Length == 0)
                return string.Empty;
            name = name.Normalize(System.Text.NormalizationForm.FormKC).ToUpperInvariant();
            var builder = new System.Text.StringBuilder(name.Length);
            var pendingSpace = false;
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsWhiteSpace(c))
                {
                    pendingSpace = builder.Length > 0;
                    continue;
                }
                if (pendingSpace)
                    builder.Append(' ');
                pendingSpace = false;
                builder.Append(c == 'İ' || c == 'ı' ? 'I' : c);
            }
            return builder.ToString();
        }
    }

    [Serializable]
    public class EconomyData
    {
        public int gold;
        public int hearts = 5;
        public int maxHearts = 5;
        public int gems;
        public int timeFreezes;
    }

    [Serializable]
    public class RegionProgressEntry
    {
        public string regionId;
        public RegionState state;
        public int campaignProgress;
    }

    [Serializable]
    public class WorldProgressData
    {
        public List<RegionProgressEntry> regions = new List<RegionProgressEntry>();
    }

    [Serializable]
    public class ShopLevelEntry
    {
        public string productId;
        public int level;
    }

    [Serializable]
    public class ShopProgressData
    {
        public List<ShopLevelEntry> levels = new List<ShopLevelEntry>();
    }

    [Serializable]
    public class DailyWheelProgressData
    {
        public string lastSpinDay;
        public int totalSpins;
        public bool hasPending;
        public int pendingSegment;
        public RewardType pendingType;
        public int pendingAmount;
        public float pendingFloatAmount;
        public string pendingTransactionId;
    }

    [Serializable]
    public class ProgressEntry
    {
        public string id;
        public int progress;
        public bool claimed;
    }

    [Serializable]
    public class MissionProgressData
    {
        public string lastResetDay;
        public List<ProgressEntry> entries = new List<ProgressEntry>();
    }

    [Serializable]
    public class AchievementProgressData
    {
        public List<ProgressEntry> entries = new List<ProgressEntry>();
    }

    [Serializable]
    public class SettingsData
    {
        public bool musicEnabled = true;
        public bool soundEnabled = true;
        public float musicVolume = 1f;
        public float soundVolume = 1f;
        public bool audioReady;
        public string language = "tr";
    }

    [Serializable]
    public class BattleSubmission
    {
        public string submissionId;
        public string playerId;
        public int score;
        public string regionId;
        public int campaignIndex;
        public string difficulty;
        public float duration;
        public string timestamp;
        public bool uploaded;
    }
}

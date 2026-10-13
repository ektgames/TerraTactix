using UnityEngine;

namespace TerraTactix.Data
{
    [CreateAssetMenu(fileName = "TerraBalance", menuName = "TerraTactix/Balance")]
    public sealed class BalanceConfiguration : ScriptableObject
    {
        public int startingHearts = 5;
        public int startingMaxHearts = 5;
        public int startingGold;
        public int startingGems;
        public int heartCost = 1;
        public int defaultTargetScore = 1000;
        public float defaultTimeLimit = 90f;
        public float minTimeLimit = 62f;
        public float timeShrinkPerStep = 0.008f;
        public float maxTimeShrink = 0.28f;
        public float easyScoreMultiplier = 1f;
        public float normalScoreMultiplier = 1.1f;
        public float hardScoreMultiplier = 1.25f;
        public float veryHardScoreMultiplier = 1.45f;
        public float extremeScoreMultiplier = 1.65f;
        public int easyShapeCount = 5;
        public int normalShapeCount = 7;
        public int hardShapeCount = 9;
        public int lateShapeCount = 11;
        public float easySpecialChance = 0.04f;
        public float normalSpecialChance = 0.05f;
        public float hardSpecialChance = 0.08f;
        public float veryHardSpecialChance = 0.1f;
        public float extremeSpecialChance = 0.12f;
        public int bombScore = 30;
        public int lightningScore = 36;
        public float timeBonusSeconds = 8f;
        public int gridWidth = 8;
        public int gridHeight = 12;
        public float baseFallInterval = 0.8f;
        public int campaignTargetStep = 40;
        public int battleGoldReward = 50;
        public int captureGoldReward = 150;
        public int winXp = 25;
        public int defeatXp = 8;
        public int dailyChestGold = 40;
        public int placementScore = 8;
        public int mergeScore = 20;
        public int lineClearScore = 120;
        public int hardDropScore = 2;
        public int maxBlockLevel = 4;

        public ShopProductConfig[] products =
        {
            new ShopProductConfig
            {
                id = "heart",
                title = "+1 Can",
                description = "Bir can ekler. Üst sınıra kadar.",
                price = 100,
                amount = 1,
                maxLevel = 0,
                kind = ShopProductKind.Heart
            },
            new ShopProductConfig
            {
                id = "heart-pack",
                title = "Can Paketi",
                description = "Beş can ekler. Üst sınıra kadar.",
                price = 400,
                amount = 5,
                maxLevel = 0,
                kind = ShopProductKind.HeartPack
            },
            new ShopProductConfig
            {
                id = "max-hearts",
                title = "Can Kapasitesi",
                description = "En fazla canı 1 artırır.",
                price = 250,
                amount = 1,
                maxLevel = 5,
                kind = ShopProductKind.MaxHearts
            },
            new ShopProductConfig
            {
                id = "time-freeze",
                title = "Süre Dondurucu",
                description = "Bir adet verir. Savaşta süreyi 10 saniye dondurur.",
                price = 300,
                amount = 1,
                maxLevel = 0,
                kind = ShopProductKind.TimeFreeze
            }
        };

        public static BalanceConfiguration Active { get; private set; }

        public static BalanceConfiguration GetOrCreate()
        {
            if (Active != null)
                return Active;

            Active = Resources.Load<BalanceConfiguration>("TerraBalance");
            if (Active == null)
                Active = CreateInstance<BalanceConfiguration>();
            return Active;
        }
    }

    [System.Serializable]
    public class ShopProductConfig
    {
        public string id;
        public string title;
        public string description;
        public int price;
        public int amount;
        public int maxLevel;
        public ShopProductKind kind;
    }
}

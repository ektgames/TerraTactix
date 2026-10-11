using System;

namespace TerraTactix.Core
{
    public static class GameEvents
    {
        public static event Action<int> GoldEarned;
        public static event Action<int> GoldSpent;
        public static event Action BattleCompleted;
        public static event Action BattleWon;
        public static event Action<string> RegionCaptured;
        public static event Action DailySpinCompleted;
        public static event Action ShopPurchaseCompleted;
        public static event Action MergePerformed;
        public static event Action<int> LineCleared;
        public static event Action<int> ComboReached;

        public static void RaiseGoldEarned(int amount) => GoldEarned?.Invoke(amount);
        public static void RaiseGoldSpent(int amount) => GoldSpent?.Invoke(amount);
        public static void RaiseBattleCompleted() => BattleCompleted?.Invoke();
        public static void RaiseBattleWon() => BattleWon?.Invoke();
        public static void RaiseRegionCaptured(string regionId) => RegionCaptured?.Invoke(regionId);
        public static void RaiseDailySpinCompleted() => DailySpinCompleted?.Invoke();
        public static void RaiseShopPurchaseCompleted() => ShopPurchaseCompleted?.Invoke();
        public static void RaiseMergePerformed() => MergePerformed?.Invoke();
        public static void RaiseLineCleared(int count) => LineCleared?.Invoke(count);
        public static void RaiseComboReached(int combo) => ComboReached?.Invoke(combo);
    }
}

using System;
using TerraTactix.Core;
using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.Economy
{
    public sealed class EconomyManager : MonoBehaviour
    {
        EconomyData data;
        BalanceConfiguration balance;

        public event Action Changed;

        public int Gold => data.gold;
        public int Hearts => data.hearts;
        public int MaxHearts => data.maxHearts;
        public int Gems => data.gems;
        public int TimeFreezes => data.timeFreezes;

        public void Bind(EconomyData economyData)
        {
            data = economyData;
            balance = BalanceConfiguration.GetOrCreate();
            Changed?.Invoke();
        }

        public bool CanPlay()
        {
            return data.hearts >= balance.heartCost;
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0 || data.gold < amount)
                return false;
            data.gold -= amount;
            if (amount > 0)
                Core.GameEvents.RaiseGoldSpent(amount);
            Notify();
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
                return;
            data.gold += amount;
            Core.GameEvents.RaiseGoldEarned(amount);
            Notify();
        }

        public void RefundGold(int amount)
        {
            if (amount <= 0)
                return;
            data.gold += amount;
            Notify();
        }

        public void AddHearts(int amount)
        {
            if (amount <= 0)
                return;
            data.hearts = Mathf.Min(data.maxHearts, data.hearts + amount);
            Notify();
        }

        public void AddGems(int amount)
        {
            if (amount <= 0)
                return;
            data.gems += amount;
            Notify();
        }

        public void AddMaxHearts(int amount)
        {
            if (amount <= 0)
                return;
            data.maxHearts += amount;
            data.hearts = Mathf.Min(data.maxHearts, data.hearts + amount);
            Notify();
        }

        public void AddTimeFreezes(int amount)
        {
            if (amount <= 0)
                return;
            data.timeFreezes += amount;
            Notify();
        }

        public bool TryConsumeTimeFreeze()
        {
            if (data.timeFreezes <= 0)
                return false;
            data.timeFreezes--;
            Notify();
            return true;
        }

        public bool ConsumeHeart()
        {
            if (!CanPlay())
                return false;
            data.hearts -= balance.heartCost;
            Notify();
            return true;
        }

        void Notify()
        {
            Changed?.Invoke();
            GameBootstrap.Instance.Save.Save();
        }
    }
}

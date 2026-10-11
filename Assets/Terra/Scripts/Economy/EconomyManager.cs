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
        public float AttackSpeed => data.attackSpeed;

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

        public void AddAttackSpeed(float amount)
        {
            if (amount <= 0f)
                return;
            data.attackSpeed = Mathf.Min(balance.attackSpeedCap, data.attackSpeed + amount);
            Notify();
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

using TerraTactix.Core;
using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.Economy
{
    public sealed class RewardService : MonoBehaviour
    {
        SaveManager save;
        EconomyManager economy;

        public void Bind(SaveManager saveManager, EconomyManager economyManager)
        {
            save = saveManager;
            economy = economyManager;
        }

        public bool TryApply(string transactionId, RewardType type, int amount, float floatAmount = 0f)
        {
            if (!string.IsNullOrEmpty(transactionId) && save.HasTransaction(transactionId))
                return false;

            if (!string.IsNullOrEmpty(transactionId))
                save.RememberTransaction(transactionId);

            switch (type)
            {
                case RewardType.Gold:
                    economy.AddGold(amount);
                    break;
                case RewardType.Hearts:
                    economy.AddHearts(amount);
                    break;
                case RewardType.Gems:
                    economy.AddGems(amount);
                    break;
                case RewardType.MaxHearts:
                    economy.AddMaxHearts(amount);
                    break;
                case RewardType.TimeFreeze:
                    economy.AddTimeFreezes(amount);
                    break;
                default:
                    Debug.LogError($"RewardService: bilinmeyen ödül tipi {type}.");
                    return false;
            }

            save.Save();
            return true;
        }
    }
}

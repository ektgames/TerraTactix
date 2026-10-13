using TerraTactix.Core;
using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.Economy
{
    public sealed class ShopManager : MonoBehaviour
    {
        ShopProgressData progress;

        public void Bind(ShopProgressData shopProgress)
        {
            progress = shopProgress;
            progress.levels ??= new System.Collections.Generic.List<ShopLevelEntry>();
        }

        public int LevelOf(string productId)
        {
            for (var i = 0; i < progress.levels.Count; i++)
            {
                if (progress.levels[i].productId == productId)
                    return progress.levels[i].level;
            }

            return 0;
        }

        public bool TryPurchase(string productId, out string message)
        {
            var product = Find(productId);
            if (product == null)
            {
                message = Locale.T("Ürün bulunamadı.", "Product not found.");
                return false;
            }

            var app = GameBootstrap.Instance;
            var level = LevelOf(productId);
            if (product.maxLevel > 0 && level >= product.maxLevel)
            {
                message = Locale.T("Bu yükseltme tamamlandı.", "This upgrade is complete.");
                return false;
            }

            if (product.kind == ShopProductKind.Heart || product.kind == ShopProductKind.HeartPack)
            {
                if (app.Economy.Hearts >= app.Economy.MaxHearts)
                {
                    message = Locale.T("Canların zaten dolu.", "Your hearts are already full.");
                    return false;
                }
            }

            if (!app.Economy.TrySpendGold(product.price))
            {
                message = Locale.T("Yeterli altın yok.", "Not enough gold.");
                return false;
            }

            var transactionId = product.maxLevel > 0
                ? $"shop:{product.id}:level:{level + 1}"
                : $"shop:{product.id}:{System.Guid.NewGuid():N}";

            var applied = app.Rewards.TryApply(transactionId, RewardFor(product.kind), product.amount);

            if (!applied)
            {
                app.Economy.RefundGold(product.price);
                message = Locale.T("Bu satın alma zaten işlendi.", "This purchase was already processed.");
                return false;
            }

            if (product.maxLevel > 0)
                SetLevel(productId, level + 1);

            GameEvents.RaiseShopPurchaseCompleted();
            app.Save.Save();
            message = Locale.T($"{product.title} alındı.", $"{Locale.ProductTitle(product.id, product.title)} purchased.");
            return true;
        }

        static RewardType RewardFor(ShopProductKind kind)
        {
            switch (kind)
            {
                case ShopProductKind.MaxHearts:
                    return RewardType.MaxHearts;
                case ShopProductKind.Heart:
                case ShopProductKind.HeartPack:
                    return RewardType.Hearts;
                case ShopProductKind.TimeFreeze:
                    return RewardType.TimeFreeze;
                default:
                    return RewardType.Gold;
            }
        }

        void SetLevel(string productId, int level)
        {
            for (var i = 0; i < progress.levels.Count; i++)
            {
                if (progress.levels[i].productId != productId)
                    continue;
                progress.levels[i].level = level;
                return;
            }

            progress.levels.Add(new ShopLevelEntry { productId = productId, level = level });
        }

        static ShopProductConfig Find(string productId)
        {
            var products = BalanceConfiguration.GetOrCreate().products;
            for (var i = 0; i < products.Length; i++)
            {
                if (products[i].id == productId)
                    return products[i];
            }

            return null;
        }
    }
}

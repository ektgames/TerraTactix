using TerraTactix.Core;
using TerraTactix.Data;
using TMPro;
using UnityEngine;

namespace TerraTactix.UI
{
    public static class ShopView
    {
        public static void Show(Transform parent)
        {
            var blocker = UiFactory.Image(parent, "ShopModal", new Color(0.04f, 0.06f, 0.1f, 0.72f), false, Vector2.zero, Vector2.one);
            GameBootstrap.Instance.State.SetModal(true);
            var card = UiFactory.Image(blocker.transform, "Card", UiTheme.Panel, false, new Vector2(0.20f, 0.06f), new Vector2(0.80f, 0.94f));
            UiFactory.Label(card.transform, Locale.T("Mağaza", "Shop"), 34f, UiTheme.Yellow, new Vector2(0.06f, 0.91f), new Vector2(0.94f, 0.98f));
            var status = UiFactory.Label(card.transform, Status(), 20f, UiTheme.Gold, new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.90f));
            var note = UiFactory.Label(card.transform, string.Empty, 18f, UiTheme.Ink, new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.20f));
            var products = GameBootstrap.Instance.Balance.products;
            for (var i = 0; i < products.Length; i++)
            {
                var yMax = 0.82f - i * 0.155f;
                var yMin = yMax - 0.14f;
                PlaceModalProduct(card.transform, products[i], yMin, yMax, status, note);
            }

            UiFactory.Button(card.transform, "CloseShop", Locale.T("Kapat", "Close"), UiTheme.Yellow, new Vector2(0.28f, 0.03f), new Vector2(0.72f, 0.12f), () =>
            {
                GameBootstrap.Instance.State.SetModal(false);
                UnityEngine.Object.Destroy(blocker.gameObject);
            }, 24f);
        }

        static void PlaceModalProduct(Transform parent, ShopProductConfig product, float yMin, float yMax, TMP_Text status, TMP_Text note)
        {
            var plate = UiFactory.Image(parent, product.id, new Color(0.08f, 0.13f, 0.22f, 1f), false, new Vector2(0.05f, yMin), new Vector2(0.95f, yMax));
            var caption = UiFactory.Label(plate.transform, string.Empty, 20f, UiTheme.Ink, new Vector2(0.03f, 0.08f), new Vector2(0.62f, 0.92f), TextAlignmentOptions.TopLeft);
            caption.enableAutoSizing = true;
            caption.fontSizeMin = 15f;
            caption.fontSizeMax = 22f;
            caption.outlineWidth = 0f;
            caption.overflowMode = TextOverflowModes.Ellipsis;
            var current = product;
            UnityEngine.UI.Button buy = null;
            buy = UiFactory.Button(plate.transform, "Buy" + current.id, Locale.T("Satın al", "Buy"), UiTheme.Yellow, new Vector2(0.64f, 0.18f), new Vector2(0.96f, 0.82f), () =>
            {
                GameBootstrap.Instance.Shop.TryPurchase(current.id, out var message);
                note.text = message;
                status.text = Status();
                RefreshProduct(current, caption, buy);
            }, 20f);
            var buyLabel = buy.GetComponentInChildren<TMP_Text>();
            buyLabel.outlineWidth = 0f;
            RefreshProduct(current, caption, buy);
        }

        public static void Build(Transform parent)
        {
            var sky = UiFactory.Image(parent, "Bg", Color.white, false, Vector2.zero, Vector2.one);
            sky.sprite = UiFactory.Sky;
            sky.type = UnityEngine.UI.Image.Type.Simple;
            sky.raycastTarget = false;
            UiFactory.Header(parent, Locale.T("Mağaza", "Shop"), () => GameBootstrap.Instance.Scenes.Load(SceneNames.MainMenu));
            var status = UiFactory.Label(parent, Status(), 22f, UiTheme.Gold, new Vector2(0.55f, 0.9f), new Vector2(0.98f, 0.98f), TextAlignmentOptions.Right);
            var note = UiFactory.Label(parent, string.Empty, 22f, UiTheme.Ink, new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.1f));
            var products = GameBootstrap.Instance.Balance.products;
            for (var i = 0; i < products.Length; i++)
            {
                var product = products[i];
                var yMax = 0.82f - i * 0.18f;
                var yMin = yMax - 0.16f;
                PlaceProduct(parent, product, yMin, yMax, status, note);
            }
        }

        static void PlaceProduct(Transform parent, ShopProductConfig product, float yMin, float yMax, TMP_Text status, TMP_Text note)
        {
            UiFactory.Image(parent, product.id, UiTheme.Panel, false, new Vector2(0.06f, yMin), new Vector2(0.94f, yMax));
            var caption = UiFactory.Label(parent, string.Empty, 18f, UiTheme.Ink, new Vector2(0.08f, yMin), new Vector2(0.64f, yMax), TextAlignmentOptions.Left);
            var current = product;
            UnityEngine.UI.Button buy = null;
            buy = UiFactory.Button(parent, "Buy" + current.id, Locale.T("Satın al", "Buy"), UiTheme.Yellow, new Vector2(0.66f, yMin + 0.012f), new Vector2(0.92f, yMax - 0.012f), () =>
            {
                GameBootstrap.Instance.Shop.TryPurchase(current.id, out var message);
                note.text = message;
                status.text = Status();
                RefreshProduct(current, caption, buy);
            }, 18f);
            RefreshProduct(current, caption, buy);
        }

        static void RefreshProduct(ShopProductConfig product, TMP_Text caption, UnityEngine.UI.Button buy)
        {
            var level = GameBootstrap.Instance.Shop.LevelOf(product.id);
            var done = product.maxLevel > 0 && level >= product.maxLevel;
            var levelText = product.maxLevel > 0 ? $"  {Locale.T("Seviye", "Level")} {level}/{product.maxLevel}" : string.Empty;
            var price = done ? Locale.T("Tamamlandı", "Complete") : product.price + " " + Locale.T("altın", "gold");
            caption.text = $"{Locale.ProductTitle(product.id, product.title)}{levelText}\n{Locale.ProductBody(product.id, product.description)}\n{price}";
            buy.interactable = !done;
            var buyLabel = buy.GetComponentInChildren<TMP_Text>();
            if (buyLabel != null)
                buyLabel.text = done ? Locale.T("Tamam", "Done") : Locale.T("Satın al", "Buy");
        }

        static string Status()
        {
            var economy = GameBootstrap.Instance.Economy;
            return $"{Locale.T("Altın", "Gold")} {economy.Gold}   {Locale.T("Can", "Hearts")} {economy.Hearts}/{economy.MaxHearts}   {Locale.T("Dondurucu", "Freezes")} {economy.TimeFreezes}";
        }
    }
}

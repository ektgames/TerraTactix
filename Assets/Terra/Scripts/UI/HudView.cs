using TerraTactix.Core;
using TMPro;
using UnityEngine;

namespace TerraTactix.UI
{
    public static class HudView
    {
        public static void Build(Transform parent)
        {
            Build(parent, false);
        }

        public static void Build(Transform parent, bool menu)
        {
            var hearts = Chip(parent, "Hearts", UiTheme.Heart, menu ? new Vector2(0.33f, 0.905f) : new Vector2(0.40f, 0.915f), menu ? new Vector2(0.46f, 0.975f) : new Vector2(0.53f, 0.985f));
            var gold = Chip(parent, "Gold", UiTheme.Gold, menu ? new Vector2(0.47f, 0.905f) : new Vector2(0.54f, 0.915f), menu ? new Vector2(0.60f, 0.975f) : new Vector2(0.67f, 0.985f));
            var gems = Chip(parent, "Gems", UiTheme.Gem, menu ? new Vector2(0.61f, 0.905f) : new Vector2(0.68f, 0.915f), menu ? new Vector2(0.74f, 0.975f) : new Vector2(0.80f, 0.985f));
            var row = hearts.transform.parent.gameObject;
            row.SetActive(false);
            var binder = row.AddComponent<HudBinder>();
            binder.hearts = hearts;
            binder.gold = gold;
            binder.gems = gems;
            row.SetActive(true);

            var plusMin = menu ? new Vector2(0.755f, 0.905f) : new Vector2(0.815f, 0.915f);
            var plusMax = menu ? new Vector2(0.795f, 0.975f) : new Vector2(0.855f, 0.975f);
            var gemMin = menu ? new Vector2(0.80f, 0.905f) : new Vector2(0.86f, 0.915f);
            var gemMax = menu ? new Vector2(0.84f, 0.975f) : new Vector2(0.90f, 0.975f);
            var heartsPlus = UiFactory.Button(parent, "HeartShop", "+", UiTheme.Green, plusMin, plusMax, () => ShopView.Show(parent), 24f);
            var gemsPlus = UiFactory.Button(parent, "GemInfo", "+", UiTheme.Gem, gemMin, gemMax, () =>
            {
                UiFactory.Dialog(parent, Locale.T("Kristal", "Crystals"), Locale.T("Kristaller günlük çark ve görevlerden gelir. Gerçek para ile satış yok.", "Crystals come from the daily wheel and missions. Nothing is sold for real money."), null);
            }, 24f);
            var player = GameBootstrap.Instance.Save.Data.player;
            if (player == null || !player.nameChosen)
            {
                heartsPlus.interactable = false;
                gemsPlus.interactable = false;
            }
        }

        static TMP_Text Chip(Transform parent, string name, Color accent, Vector2 min, Vector2 max)
        {
            var plate = UiFactory.Image(parent, name, new Color(0.07f, 0.11f, 0.18f, 0.88f), false, min, max);
            var dot = UiFactory.Point(plate.transform, "Dot", new Vector2(0.14f, 0.5f), new Vector2(18f, 18f));
            var dotImage = dot.gameObject.AddComponent<UnityEngine.UI.Image>();
            dotImage.sprite = UiFactory.Knob;
            dotImage.color = accent;
            dotImage.raycastTarget = false;
            var label = UiFactory.Label(plate.transform, string.Empty, 20f, accent, new Vector2(0.24f, 0f), new Vector2(0.98f, 1f));
            return label;
        }
    }

    public sealed class HudBinder : MonoBehaviour
    {
        public TMP_Text hearts;
        public TMP_Text gold;
        public TMP_Text gems;

        void OnEnable()
        {
            GameBootstrap.Instance.Economy.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (GameBootstrap.Instance != null)
                GameBootstrap.Instance.Economy.Changed -= Refresh;
        }

        void Refresh()
        {
            var app = GameBootstrap.Instance;
            if (app == null || app.Economy == null || hearts == null)
                return;

            var economy = app.Economy;
            hearts.text = $"{Locale.T("Can", "Hearts")} {economy.Hearts}/{economy.MaxHearts}";
            gold.text = $"{Locale.T("Altın", "Gold")} {economy.Gold}";
            gems.text = $"{Locale.T("Kristal", "Crystals")} {economy.Gems}";
        }
    }
}

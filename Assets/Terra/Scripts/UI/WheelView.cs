using TerraTactix.Core;
using TerraTactix.Wheel;
using TMPro;
using UnityEngine;

namespace TerraTactix.UI
{
    public static class WheelView
    {
        public static void Show(Transform parent)
        {
            var blocker = UiFactory.Image(parent, "WheelModal", new Color(0.04f, 0.06f, 0.1f, 0.72f), false, Vector2.zero, Vector2.one);
            GameBootstrap.Instance.State.SetModal(true);
            var card = UiFactory.Image(blocker.transform, "Card", UiTheme.Panel, false, new Vector2(0.14f, 0.08f), new Vector2(0.86f, 0.92f));
            UiFactory.Label(card.transform, Locale.T("Günlük Çark", "Daily Wheel"), 34f, UiTheme.Yellow, new Vector2(0.06f, 0.90f), new Vector2(0.94f, 0.98f));
            Fill(card.transform, true);
            UiFactory.Button(card.transform, "CloseWheel", Locale.T("Kapat", "Close"), UiTheme.Yellow, new Vector2(0.34f, 0.04f), new Vector2(0.66f, 0.13f), () =>
            {
                GameBootstrap.Instance.State.SetModal(false);
                UnityEngine.Object.Destroy(blocker.gameObject);
            }, 24f);
        }

        public static void Build(Transform parent)
        {
            var sky = UiFactory.Image(parent, "Bg", Color.white, false, Vector2.zero, Vector2.one);
            sky.sprite = UiFactory.Sky;
            sky.type = UnityEngine.UI.Image.Type.Simple;
            sky.raycastTarget = false;
            UiFactory.Header(parent, Locale.T("Günlük Çark", "Daily Wheel"), () => GameBootstrap.Instance.Scenes.Load(SceneNames.MainMenu));
            Fill(parent, false);
        }

        static void Fill(Transform parent, bool modal)
        {
            var wheelSize = modal ? new Vector2(390f, 390f) : new Vector2(460f, 460f);
            var wheel = UiFactory.Point(parent, "Wheel", modal ? new Vector2(0.32f, 0.50f) : new Vector2(0.32f, 0.48f), wheelSize);
            Disc(wheel, "Rim", wheelSize, new Color(0.10f, 0.14f, 0.22f));
            Disc(wheel, "Gold", wheelSize * 0.94f, new Color(0.98f, 0.82f, 0.28f));

            var sliceSize = wheelSize * 0.88f;
            for (var i = 0; i < DailyWheelManager.SegmentCount; i++)
            {
                var segment = DailyWheelManager.Segments[i];
                var slice = UiFactory.Point(wheel, "Slice" + i, new Vector2(0.5f, 0.5f), sliceSize);
                var sliceImage = slice.gameObject.AddComponent<UnityEngine.UI.Image>();
                sliceImage.sprite = UiFactory.Knob;
                sliceImage.type = UnityEngine.UI.Image.Type.Filled;
                sliceImage.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
                sliceImage.fillOrigin = (int)UnityEngine.UI.Image.Origin360.Top;
                sliceImage.fillClockwise = true;
                sliceImage.fillAmount = 1f / DailyWheelManager.SegmentCount;
                sliceImage.color = segment.color;
                sliceImage.raycastTarget = false;
                slice.localRotation = Quaternion.Euler(0f, 0f, 30f + i * 60f);

                var label = UiFactory.Point(wheel, "Seg" + i, new Vector2(0.5f, 0.5f), modal ? new Vector2(108f, 52f) : new Vector2(130f, 60f));
                var degrees = i * 60f;
                label.anchoredPosition = Quaternion.Euler(0f, 0f, degrees) * new Vector3(0f, modal ? 112f : 136f, 0f);
                label.localRotation = Quaternion.Euler(0f, 0f, degrees);
                var text = label.gameObject.AddComponent<TextMeshProUGUI>();
                text.font = UiFactory.Font;
                text.text = PrizeLines(Locale.WheelLabel(segment.label));
                text.fontSize = modal ? 18f : 22f;
                text.fontStyle = FontStyles.Bold;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.white;
                text.outlineWidth = 0.18f;
                text.outlineColor = new Color32(18, 22, 34, 230);
                text.raycastTarget = false;
            }

            Disc(wheel, "Hub", wheelSize * 0.22f, new Color(0.08f, 0.12f, 0.2f));
            var pointer = UiFactory.Point(parent, "Pointer", modal ? new Vector2(0.32f, 0.50f) : new Vector2(0.32f, 0.48f), new Vector2(64f, 48f));
            pointer.anchoredPosition = new Vector2(0f, wheelSize.y * 0.5f + 2f);
            var arrow = pointer.gameObject.AddComponent<TextMeshProUGUI>();
            arrow.font = UiFactory.Font;
            arrow.text = "▼";
            arrow.fontSize = 40f;
            arrow.fontStyle = FontStyles.Bold;
            arrow.alignment = TextAlignmentOptions.Center;
            arrow.color = UiTheme.Yellow;
            arrow.raycastTarget = false;
            var result = UiFactory.Label(parent, string.Empty, modal ? 22f : 28f, UiTheme.Ink, modal ? new Vector2(0.56f, 0.46f) : new Vector2(0.55f, 0.35f), modal ? new Vector2(0.94f, 0.78f) : new Vector2(0.96f, 0.7f));
            var wheelManager = GameBootstrap.Instance.Wheel;
            var spin = UiFactory.Button(parent, "Spin", wheelManager.CanSpin ? Locale.T("Çevir", "Spin") : Locale.T("Bugün kullanıldı", "Used today"), UiTheme.Yellow, modal ? new Vector2(0.58f, 0.24f) : new Vector2(0.62f, 0.16f), modal ? new Vector2(0.92f, 0.38f) : new Vector2(0.9f, 0.3f), () => { }, modal ? 22f : 28f);
            var presenter = parent.gameObject.AddComponent<WheelPresenter>();
            presenter.wheel = wheel;
            presenter.result = result;
            presenter.spinLabel = spin.GetComponentInChildren<TMP_Text>();
            spin.onClick.AddListener(presenter.Spin);

            if (wheelManager.HasPending)
                presenter.Resume();
            else if (!wheelManager.CanSpin)
                result.text = Locale.T("Bugünkü çevirme hakkın doldu.\nYarın tekrar dene.", "Today's spin is used.\nTry again tomorrow.");
            else
                result.text = Locale.T("Ödül çevrilmeden önce seçilir.\nÇark o dilimde durur.", "The reward is chosen before the spin.\nThe wheel stops on that slice.");
        }

        static void Disc(RectTransform parent, string name, Vector2 size, Color color)
        {
            var rect = UiFactory.Point(parent, name, new Vector2(0.5f, 0.5f), size);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = UiFactory.Knob;
            image.color = color;
            image.raycastTarget = false;
        }

        static string PrizeLines(string label)
        {
            if (string.IsNullOrEmpty(label))
                return string.Empty;
            var space = label.IndexOf(' ');
            if (space <= 0)
                return label;
            return label.Substring(0, space) + "\n" + label.Substring(space + 1);
        }
    }

    public sealed class WheelPresenter : MonoBehaviour
    {
        public RectTransform wheel;
        public TMP_Text result;
        public TMP_Text spinLabel;
        bool spinning;
        float elapsed;
        float targetZ;

        public void Spin()
        {
            var manager = GameBootstrap.Instance.Wheel;
            if (spinning || !manager.CanSpin)
                return;
            manager.BeginSpin();
            targetZ = -360f * 4f - manager.PendingSegment * 60f;
            elapsed = 0f;
            spinning = true;
            spinLabel.text = Locale.T("Dönüyor", "Spinning");
            result.text = Locale.T("Ödül seçildi.", "Reward chosen.");
        }

        public void Resume()
        {
            var manager = GameBootstrap.Instance.Wheel;
            targetZ = -360f * 2f - manager.PendingSegment * 60f;
            elapsed = 0f;
            spinning = true;
            spinLabel.text = Locale.T("Dönüyor", "Spinning");
            result.text = Locale.T("Yarım kalan çevirme tamamlanıyor.", "Finishing the interrupted spin.");
        }

        void Update()
        {
            if (!spinning)
                return;
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / 2.4f);
            var eased = 1f - Mathf.Pow(1f - t, 3f);
            wheel.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, targetZ, eased));
            if (t < 1f)
                return;
            spinning = false;
            GameBootstrap.Instance.Wheel.CompletePending(out var label);
            result.text = $"{Locale.T("Ödül", "Reward")}: {Locale.WheelLabel(label)}";
            spinLabel.text = Locale.T("Bugün kullanıldı", "Used today");
        }
    }
}

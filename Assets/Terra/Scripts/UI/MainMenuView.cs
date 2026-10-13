using TerraTactix.Core;
using TerraTactix.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraTactix.UI
{
    public static class MainMenuView
    {
        public static void Build(Transform parent)
        {
            var column = UiFactory.Image(parent, "Side", new Color(0.05f, 0.10f, 0.18f, 0.9f), false, new Vector2(0.014f, 0.03f), new Vector2(0.20f, 0.97f));
            BuildPlayer(column.transform);
            BuildMenu(column.transform);
            HudView.Build(parent, true);
            BuildSide(parent);
            if (AwaitingName())
                ShowNamePrompt(parent);
        }

        static bool AwaitingName()
        {
            var player = GameBootstrap.Instance.Save.Data.player;
            return player == null || !player.nameChosen;
        }

        static void BuildPlayer(Transform column)
        {
            var player = GameBootstrap.Instance.Save.Data.player;
            var ring = UiFactory.Point(column, "AvatarRing", new Vector2(0.2f, 0.86f), new Vector2(92f, 92f));
            var ringImage = ring.gameObject.AddComponent<UnityEngine.UI.Image>();
            ringImage.sprite = UiFactory.Knob;
            ringImage.color = new Color(0.35f, 0.72f, 0.98f);
            ringImage.raycastTarget = false;
            var frame = UiFactory.Point(ring, "AvatarFrame", new Vector2(0.5f, 0.5f), new Vector2(80f, 80f));
            var frameImage = frame.gameObject.AddComponent<UnityEngine.UI.Image>();
            frameImage.sprite = UiFactory.Knob;
            frameImage.color = new Color(0.12f, 0.45f, 0.82f);
            frameImage.raycastTarget = false;
            frame.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = true;
            if (ArtLibrary.Hero != null)
                Painted(frame, "Avatar", ArtLibrary.Hero, new Vector2(0.5f, 0.12f), new Vector2(86f, 180f));
            else
                Blob(frame, "Avatar", new Color(0.95f, 0.72f, 0.45f), new Vector2(0.5f, 0.5f), new Vector2(48f, 48f));
            if (player.nameChosen)
            {
                UiFactory.Label(column, player.playerName, 22f, UiTheme.Ink, new Vector2(0.4f, 0.84f), new Vector2(0.96f, 0.96f), TextAlignmentOptions.Left);
            }
            else
            {
                var nameButton = UiFactory.Button(column, "PlayerName", Locale.T("İsmini yaz", "Enter name"), new Color(1f, 1f, 1f, 0f), new Vector2(0.38f, 0.82f), new Vector2(0.98f, 0.97f), () => ShowNamePrompt(column.parent), 22f);
                var plate = nameButton.GetComponent<Image>();
                plate.color = new Color(1f, 1f, 1f, 0f);
                var nameLabel = nameButton.GetComponentInChildren<TMP_Text>();
                nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
                nameLabel.color = UiTheme.Yellow;
                nameLabel.outlineWidth = 0.12f;
                nameLabel.fontSize = 22f;
                var shine = nameButton.transform.Find("Shine");
                if (shine != null)
                    shine.gameObject.SetActive(false);
                var shadow = column.Find("PlayerNameShadow");
                if (shadow != null)
                    shadow.gameObject.SetActive(false);
            }
            UiFactory.Label(column, $"Lv. {player.level}", 16f, UiTheme.Muted, new Vector2(0.4f, 0.74f), new Vector2(0.96f, 0.84f), TMPro.TextAlignmentOptions.Left);
            var pct = player.xpToNext <= 0 ? 0f : Mathf.Clamp01(player.xp / (float)player.xpToNext);
            UiFactory.Image(column, "XpBg", new Color(0.02f, 0.05f, 0.08f, 1f), false, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.73f));
            UiFactory.Image(column, "Xp", UiTheme.Green, false, new Vector2(0.08f, 0.66f), new Vector2(0.08f + 0.84f * pct, 0.73f));
        }

        static void BuildMenu(Transform column)
        {
            var locked = AwaitingName();
            Row(column, "Missions", Locale.T("Görevler", "Missions"), new Color(0.36f, 0.78f, 0.46f), new Vector2(0.06f, 0.50f), new Vector2(0.94f, 0.62f), () => GoalsModal.Show(column.parent, false), locked ? null : () => GameBootstrap.Instance.Missions.HasReadyReward(), locked);
            Row(column, "Achievements", Locale.T("Başarımlar", "Achievements"), new Color(0.98f, 0.74f, 0.22f), new Vector2(0.06f, 0.36f), new Vector2(0.94f, 0.48f), () => GoalsModal.Show(column.parent, true), locked ? null : () => GameBootstrap.Instance.Achievements.HasReadyReward(), locked);
            Row(column, "Wheel", Locale.T("Günlük Çark", "Daily Wheel"), new Color(0.35f, 0.72f, 0.98f), new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.34f), () => WheelView.Show(column.parent), locked ? null : () => GameBootstrap.Instance.Wheel.CanSpin, locked);
            Row(column, "Shop", Locale.T("Mağaza", "Shop"), new Color(0.64f, 0.42f, 0.96f), new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.20f), () => ShopView.Show(column.parent), null, locked);
        }

        static void Row(Transform parent, string name, string caption, Color accent, Vector2 min, Vector2 max, System.Action click, System.Func<bool> attention, bool locked = false)
        {
            var plate = new Color(
                Mathf.Lerp(0.10f, accent.r, 0.34f),
                Mathf.Lerp(0.14f, accent.g, 0.34f),
                Mathf.Lerp(0.22f, accent.b, 0.34f),
                1f);
            var button = UiFactory.Button(parent, name, caption, plate, min, max, click, 20f);
            button.transition = UnityEngine.UI.Selectable.Transition.None;
            var label = button.GetComponentInChildren<TMPro.TMP_Text>();
            label.fontStyle = TMPro.FontStyles.Bold;
            label.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            label.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            label.overflowMode = TMPro.TextOverflowModes.Overflow;
            label.rectTransform.offsetMin = new Vector2(92f, 4f);
            label.rectTransform.offsetMax = new Vector2(-10f, -4f);
            label.color = Color.white;
            label.outlineWidth = 0.08f;
            label.outlineColor = new Color32(16, 22, 34, 180);
            label.transform.SetAsLastSibling();
            var shine = button.transform.Find("Shine");
            if (shine != null)
            {
                var shineImage = shine.GetComponent<UnityEngine.UI.Image>();
                shineImage.color = new Color(1f, 1f, 1f, 0.16f);
            }

            var ring = UiFactory.Point(button.transform, "Ring", new Vector2(0.10f, 0.5f), new Vector2(36f, 36f));
            var ringImage = ring.gameObject.AddComponent<UnityEngine.UI.Image>();
            ringImage.sprite = UiFactory.Knob;
            ringImage.color = new Color(1f, 1f, 1f, 0.28f);
            ringImage.raycastTarget = false;
            var badge = UiFactory.Point(ring, "Badge", new Vector2(0.5f, 0.5f), new Vector2(30f, 30f));
            var badgeImage = badge.gameObject.AddComponent<UnityEngine.UI.Image>();
            badgeImage.sprite = UiFactory.Knob;
            badgeImage.color = accent;
            badgeImage.raycastTarget = false;
            if (locked)
            {
                Freeze(button);
                return;
            }

            if (attention == null)
                return;

            var shadow = parent.Find(name + "Shadow");
            var pulse = button.gameObject.AddComponent<AttentionPulse>();
            pulse.plate = button.targetGraphic as UnityEngine.UI.Image;
            pulse.badge = badge;
            pulse.shadow = shadow == null ? null : shadow.GetComponent<UnityEngine.UI.Image>();
            pulse.rest = plate;
            pulse.accent = accent;
            pulse.Active = attention;
        }

        static void BuildSide(Transform parent)
        {
            var locked = AwaitingName();
            var mail = UiFactory.Button(parent, "Mail", Locale.T("Mektup", "Mail"), UiTheme.Panel, new Vector2(0.855f, 0.905f), new Vector2(0.925f, 0.975f), () => ShowMail(parent), 16f);
            var settings = UiFactory.Button(parent, "Settings", Locale.T("Ayar", "Settings"), UiTheme.Panel, new Vector2(0.93f, 0.905f), new Vector2(0.988f, 0.975f), () => ShowSettings(parent), 16f);
            if (locked)
            {
                Freeze(mail);
                Freeze(settings);
            }

            var app = GameBootstrap.Instance;
            var next = app.Expeditions.NextPlayable();
            var card = UiFactory.Image(parent, "Chapter", new Color(0.07f, 0.12f, 0.22f, 0.92f), false, new Vector2(0.745f, 0.50f), new Vector2(0.985f, 0.88f));
            if (next == null)
            {
                UiFactory.Label(card.transform, Locale.T("Açık bölge yok", "No open region"), 24f, UiTheme.Ink, new Vector2(0.06f, 0.2f), new Vector2(0.94f, 0.8f));
            }
            else
            {
                var progress = app.Expeditions.GetProgress(next.id);
                var done = progress == null ? 0 : progress.campaignProgress;
                var need = next.requiredCampaigns;
                UiFactory.Label(card.transform, Locale.Place(next.id, next.displayName), 28f, UiTheme.Ink, new Vector2(0.06f, 0.55f), new Vector2(0.94f, 0.92f));
                var filled = need <= 0 ? 0 : Mathf.Clamp(Mathf.RoundToInt(done / (float)need * 5f), 0, 5);
                for (var i = 0; i < 5; i++)
                {
                    var pip = UiFactory.Point(card.transform, "Star" + i, new Vector2(0.16f + i * 0.14f, 0.42f), new Vector2(18f, 18f));
                    var star = pip.gameObject.AddComponent<UnityEngine.UI.Image>();
                    star.sprite = UiFactory.Knob;
                    star.color = i < filled ? UiTheme.Yellow : new Color(1f, 1f, 1f, 0.28f);
                    star.raycastTarget = false;
                }
                UiFactory.Label(card.transform, $"{Locale.Biome(next.biome)}   {Locale.T("Sefer", "Expedition")} {done}/{need}", 18f, UiTheme.Muted, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.32f));
            }

            var offer = UiFactory.Button(parent, "Offer", OfferText(), new Color(0.48f, 0.28f, 0.82f), new Vector2(0.745f, 0.30f), new Vector2(0.985f, 0.47f), () => OpenChest(parent), 22f);
            if (locked)
                Freeze(offer);
            var offerLabel = offer.GetComponentInChildren<TMPro.TMP_Text>();
            var ticker = offer.gameObject.AddComponent<EverySecond>();
            ticker.Tick = () => offerLabel.text = OfferText();

            var play = UiFactory.Button(parent, "Play", Locale.T("Oyna", "Play"), UiTheme.Yellow, new Vector2(0.745f, 0.05f), new Vector2(0.985f, 0.27f), () =>
            {
                if (AwaitingName())
                    return;
                GameBootstrap.Instance.Scenes.Load(SceneNames.WorldMap);
            }, 42f);
            var playLabel = play.GetComponentInChildren<TMPro.TMP_Text>();
            playLabel.color = new Color(0.28f, 0.16f, 0.04f);
            playLabel.outlineWidth = 0f;
            if (locked)
                Freeze(play);
        }

        static void Freeze(Button button)
        {
            button.interactable = false;
            var image = button.targetGraphic as Image;
            if (image != null)
                image.color = new Color(image.color.r * 0.45f, image.color.g * 0.45f, image.color.b * 0.45f, 0.55f);
            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.color = new Color(label.color.r, label.color.g, label.color.b, 0.4f);
        }

        static void ShowNamePrompt(Transform parent)
        {
            if (parent == null || parent.Find("NamePrompt") != null || !AwaitingName())
                return;

            var blocker = UiFactory.Image(parent, "NamePrompt", new Color(0.04f, 0.06f, 0.1f, 0.78f), false, Vector2.zero, Vector2.one);
            GameBootstrap.Instance.State.SetModal(true);
            var card = UiFactory.Image(blocker.transform, "Card", UiTheme.Panel, false, new Vector2(0.30f, 0.26f), new Vector2(0.70f, 0.74f));
            UiFactory.Label(card.transform, Locale.T("İsmini yaz", "Enter your name"), 32f, UiTheme.Yellow, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.94f));
            UiFactory.Label(card.transform, Locale.T("Bu isim bir kez yazılır ve sonra değiştirilemez.\nEn fazla 18 harf.", "This name is written once and cannot be changed.\nUp to 18 letters."), 20f, UiTheme.Muted, new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.76f));

            var field = UiFactory.Image(card.transform, "Field", new Color(0.05f, 0.08f, 0.14f, 1f), false, new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.55f));
            var viewport = UiFactory.Rect(field.transform, "Text Area", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));
            viewport.gameObject.AddComponent<RectMask2D>();
            var placeholderRect = UiFactory.Rect(viewport, "Placeholder", Vector2.zero, Vector2.one);
            var placeholder = placeholderRect.gameObject.AddComponent<TextMeshProUGUI>();
            placeholder.font = UiFactory.Font;
            placeholder.text = Locale.T("İsim", "Name");
            placeholder.fontSize = 26f;
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.color = new Color(0.78f, 0.86f, 0.94f, 0.45f);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.raycastTarget = false;
            var textRect = UiFactory.Rect(viewport, "Text", Vector2.zero, Vector2.one);
            var text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = UiFactory.Font;
            text.fontSize = 28f;
            text.color = UiTheme.Ink;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            var input = field.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = viewport;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.fontAsset = UiFactory.Font;
            input.pointSize = 28f;
            input.characterLimit = PlayerData.MaxNameLength;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.caretColor = Color.white;
            input.customCaretColor = true;
            var focus = field.gameObject.AddComponent<NameFieldFocus>();
            focus.field = input;

            var error = UiFactory.Label(card.transform, string.Empty, 18f, UiTheme.Heart, new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.38f));
            var start = UiFactory.Button(card.transform, "StartName", Locale.T("Başla", "Start"), UiTheme.Yellow, new Vector2(0.12f, 0.16f), new Vector2(0.88f, 0.28f), null, 24f);
            var startLabel = start.GetComponentInChildren<TMP_Text>();
            startLabel.color = new Color(0.28f, 0.16f, 0.04f);
            startLabel.outlineWidth = 0f;
            start.onClick.AddListener(() =>
            {
                if (!GameBootstrap.Instance.Save.TryChooseName(input.text, out var message))
                {
                    error.text = message;
                    return;
                }

                GameBootstrap.Instance.Ui.Show(SceneNames.MainMenu);
            });
            UiFactory.Button(card.transform, "CloseName", Locale.T("Kapat", "Close"), UiTheme.Panel, new Vector2(0.22f, 0.04f), new Vector2(0.78f, 0.14f), () =>
            {
                GameBootstrap.Instance.State.SetModal(false);
                UnityEngine.Object.Destroy(blocker.gameObject);
            }, 22f);
        }

        public static void ShowSettings(Transform parent)
        {
            var blocker = UiFactory.Image(parent, "SettingsModal", new Color(0.04f, 0.06f, 0.1f, 0.72f), false, Vector2.zero, Vector2.one);
            GameBootstrap.Instance.State.SetModal(true);
            var card = UiFactory.Image(blocker.transform, "Card", UiTheme.Panel, false, new Vector2(0.28f, 0.06f), new Vector2(0.72f, 0.94f));
            UiFactory.Label(card.transform, Locale.T("Ayarlar", "Settings"), 34f, UiTheme.Yellow, new Vector2(0.08f, 0.91f), new Vector2(0.92f, 0.98f));

            var audio = GameBootstrap.Instance.Audio;
            var music = UiFactory.Button(card.transform, "Music", audio.MusicEnabled ? Locale.T("Müzik: Açık", "Music: On") : Locale.T("Müzik: Kapalı", "Music: Off"), UiTheme.Green, new Vector2(0.08f, 0.81f), new Vector2(0.92f, 0.89f), null, 22f);
            music.onClick.AddListener(() =>
            {
                audio.SetMusic(!audio.MusicEnabled);
                music.GetComponentInChildren<TMPro.TMP_Text>().text = audio.MusicEnabled ? Locale.T("Müzik: Açık", "Music: On") : Locale.T("Müzik: Kapalı", "Music: Off");
            });
            VolumeRow(card.transform, "MusicVolume", audio.MusicVolume, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.79f), audio.SetMusicVolume);

            var sound = UiFactory.Button(card.transform, "Sound", audio.SoundEnabled ? Locale.T("Ses: Açık", "Sound: On") : Locale.T("Ses: Kapalı", "Sound: Off"), UiTheme.Green, new Vector2(0.08f, 0.61f), new Vector2(0.92f, 0.69f), null, 22f);
            sound.onClick.AddListener(() =>
            {
                audio.SetSound(!audio.SoundEnabled);
                sound.GetComponentInChildren<TMPro.TMP_Text>().text = audio.SoundEnabled ? Locale.T("Ses: Açık", "Sound: On") : Locale.T("Ses: Kapalı", "Sound: Off");
            });
            VolumeRow(card.transform, "SoundVolume", audio.SoundVolume, new Vector2(0.08f, 0.52f), new Vector2(0.92f, 0.59f), audio.SetSoundVolume);

            UiFactory.Label(card.transform, Locale.T("Dil", "Language"), 22f, UiTheme.Muted, new Vector2(0.08f, 0.45f), new Vector2(0.92f, 0.51f));
            UiFactory.Button(card.transform, "LangTr", "Türkçe", Locale.English ? UiTheme.Panel : UiTheme.Green, new Vector2(0.08f, 0.36f), new Vector2(0.48f, 0.44f), () => Locale.SetLanguage("tr"), 22f);
            UiFactory.Button(card.transform, "LangEn", "English", Locale.English ? UiTheme.Green : UiTheme.Panel, new Vector2(0.52f, 0.36f), new Vector2(0.92f, 0.44f), () => Locale.SetLanguage("en"), 22f);

            UiFactory.Button(card.transform, "Ranks", Locale.T("Sıralama", "Leaderboard"), UiTheme.Panel, new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.33f), () =>
            {
                GameBootstrap.Instance.State.SetModal(false);
                GameBootstrap.Instance.Scenes.Load(SceneNames.Leaderboard);
            }, 22f);
            UiFactory.Button(card.transform, "Reset", Locale.T("İlerlemeyi sıfırla", "Reset progress"), UiTheme.Heart, new Vector2(0.08f, 0.15f), new Vector2(0.92f, 0.23f), () =>
            {
                var confirm = UiFactory.Dialog(blocker.transform, Locale.T("Emin misin?", "Are you sure?"), Locale.T("Yerel kayıt yeni oyuna döner.", "The local save returns to a new game."), null);
                UiFactory.Button(confirm, "Yes", Locale.T("Evet, sıfırla", "Yes, reset"), UiTheme.Heart, new Vector2(0.15f, 0.24f), new Vector2(0.85f, 0.42f), () =>
                {
                    GameBootstrap.Instance.ResetProgress();
                }, 22f);
            }, 20f);
            UiFactory.Button(card.transform, "CloseSettings", Locale.T("Kapat", "Close"), UiTheme.Yellow, new Vector2(0.22f, 0.03f), new Vector2(0.78f, 0.12f), () =>
            {
                GameBootstrap.Instance.State.SetModal(false);
                UnityEngine.Object.Destroy(blocker.gameObject);
            }, 24f);
        }

        static void VolumeRow(Transform parent, string name, float value, Vector2 min, Vector2 max, System.Action<float> changed)
        {
            var row = UiFactory.Rect(parent, name, min, max);
            var minus = UiFactory.Button(row, "Minus", "-", UiTheme.Panel, new Vector2(0f, 0.08f), new Vector2(0.14f, 0.92f), null, 26f);
            var plus = UiFactory.Button(row, "Plus", "+", UiTheme.Panel, new Vector2(0.86f, 0.08f), new Vector2(1f, 0.92f), null, 26f);
            var percent = UiFactory.Label(row, Percent(value), 18f, UiTheme.Ink, new Vector2(0.68f, 0f), new Vector2(0.84f, 1f));
            var track = UiFactory.Image(row, "Track", new Color(0.05f, 0.08f, 0.14f, 1f), false, new Vector2(0.16f, 0.22f), new Vector2(0.66f, 0.78f));
            var fill = UiFactory.Image(track.transform, "Fill", UiTheme.Green, false, Vector2.zero, Vector2.one);
            var handleGo = UiFactory.Point(track.transform, "Handle", new Vector2(0.5f, 0.5f), new Vector2(22f, 36f));
            var handleImage = handleGo.gameObject.AddComponent<UnityEngine.UI.Image>();
            handleImage.sprite = UiFactory.Knob;
            handleImage.color = Color.white;
            var slider = track.gameObject.AddComponent<UnityEngine.UI.Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handleGo;
            slider.targetGraphic = handleImage;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(Mathf.Clamp01(value));
            var navigation = slider.navigation;
            navigation.mode = UnityEngine.UI.Navigation.Mode.None;
            slider.navigation = navigation;
            slider.onValueChanged.AddListener(next =>
            {
                percent.text = Percent(next);
                changed(next);
            });
            minus.onClick.AddListener(() => slider.value = Mathf.Clamp01(slider.value - 0.1f));
            plus.onClick.AddListener(() => slider.value = Mathf.Clamp01(slider.value + 0.1f));
        }

        static string Percent(float value)
        {
            return Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
        }

        static void ShowMail(Transform parent)
        {
            UiFactory.Dialog(parent, Locale.T("Mektup", "Mail"), Locale.T("Kaşif notu: Yeşil Kıyılar ilk seferini bekliyor.\n\nÇark notu: Her gün bir ödül hazır. Altın, can, kristal veya süre dondurucu gelebilir.", "Explorer note: Green Shores is waiting for its first expedition.\n\nWheel note: One reward is ready each day. It may be gold, a heart, crystals, or a time freeze."), null);
        }

        static void OpenChest(Transform parent)
        {
            var app = GameBootstrap.Instance;
            var id = "chest:" + TimeService.TodayKey;
            var gold = app.Balance.dailyChestGold;
            var opened = app.Rewards.TryApply(id, RewardType.Gold, gold);
            var message = opened
                ? Locale.T($"Bugünün sandığı açıldı.\n+{gold} altın.", $"Today's chest is open.\n+{gold} gold.")
                : Locale.T("Bugünün sandığı zaten açıldı. Yarın yenilenir.", "Today's chest is already open. It renews tomorrow.");
            UiFactory.Dialog(parent, Locale.T("Özel Sandık", "Special Chest"), message, null);
        }

        static string OfferText()
        {
            var left = TimeService.UntilNextUtcDay;
            return $"{Locale.T("Özel Sandık", "Special Chest")}\n{left.Hours:00}:{left.Minutes:00}:{left.Seconds:00}";
        }

        static void Blob(Transform parent, string name, Color color, Vector2 anchor, Vector2 size)
        {
            Shape(parent, name, UiFactory.Knob, color, anchor, size);
        }

        static void Painted(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 size)
        {
            var rect = UiFactory.Point(parent, name, anchor, size);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        static void Shape(Transform parent, string name, Sprite sprite, Color color, Vector2 anchor, Vector2 size)
        {
            var rect = UiFactory.Point(parent, name, anchor, size);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
        }
    }

    public sealed class NameFieldFocus : MonoBehaviour
    {
        public TMP_InputField field;

        void OnEnable()
        {
            if (field != null)
                field.ActivateInputField();
        }
    }

    public sealed class AttentionPulse : MonoBehaviour
    {
        public UnityEngine.UI.Image plate;
        public RectTransform badge;
        public UnityEngine.UI.Image shadow;
        public Color rest;
        public Color accent;
        public System.Func<bool> Active;
        bool wasOn = true;

        void Update()
        {
            var on = GameBootstrap.Instance != null && Active != null && Active();
            if (!on)
            {
                if (!wasOn)
                    return;
                wasOn = false;
                if (plate != null)
                    plate.color = rest;
                if (badge != null)
                    badge.localScale = Vector3.one;
                if (shadow != null)
                    shadow.color = new Color(0.08f, 0.12f, 0.18f, 0.34f);
                return;
            }

            wasOn = true;
            var wave = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5.2f);
            if (plate != null)
                plate.color = Color.Lerp(rest, Color.Lerp(rest, Color.white, 0.5f), wave);
            if (badge != null)
                badge.localScale = Vector3.one * Mathf.Lerp(1f, 1.18f, wave);
            if (shadow != null)
                shadow.color = new Color(accent.r, accent.g, accent.b, Mathf.Lerp(0.2f, 0.9f, wave));
        }
    }
}

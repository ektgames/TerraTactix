using System;
using System.IO;
using TerraTactix.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace TerraTactix.UI
{
    public static class UiTheme
    {
        public static readonly Color Navy = new Color(0.07f, 0.11f, 0.18f, 1f);
        public static readonly Color Panel = new Color(0.10f, 0.16f, 0.28f, 0.94f);
        public static readonly Color Ink = new Color(0.96f, 0.98f, 1f, 1f);
        public static readonly Color Muted = new Color(0.78f, 0.86f, 0.94f, 1f);
        public static readonly Color Yellow = new Color(0.98f, 0.74f, 0.22f, 1f);
        public static readonly Color Green = new Color(0.29f, 0.75f, 0.46f, 1f);
        public static readonly Color Heart = new Color(0.91f, 0.30f, 0.36f, 1f);
        public static readonly Color Gold = new Color(0.98f, 0.80f, 0.28f, 1f);
        public static readonly Color Gem = new Color(0.64f, 0.42f, 0.96f, 1f);
        public static readonly Color SkyTop = new Color(0.36f, 0.72f, 0.98f, 1f);
        public static readonly Color SkyBottom = new Color(0.78f, 0.93f, 1f, 1f);
        public static readonly Color OceanTop = new Color(0.20f, 0.62f, 0.86f, 1f);
        public static readonly Color OceanBottom = new Color(0.06f, 0.28f, 0.58f, 1f);
    }

    public static class UiFactory
    {
        static Sprite rounded;
        static Sprite knob;
        static Sprite tile;
        static Sprite island;
        static Sprite territory;
        static Sprite cloud;
        static Sprite ocean;
        static Sprite sky;
        static TMP_FontAsset font;
        static TMP_FontAsset cute;
        static bool loggedMissingFont;

        public static Sprite Rounded
        {
            get
            {
                if (rounded == null)
                    rounded = CreateRounded();
                return rounded;
            }
        }

        public static Sprite Knob
        {
            get
            {
                if (knob == null)
                    knob = CreateCircle(64);
                return knob;
            }
        }

        public static Sprite Tile
        {
            get
            {
                if (tile == null)
                    tile = CreateTile();
                return tile;
            }
        }

        public static Sprite Island
        {
            get
            {
                if (island == null)
                    island = CreateIsland();
                return island;
            }
        }

        public static Sprite Territory
        {
            get
            {
                if (territory == null)
                    territory = CreateTerritory();
                return territory;
            }
        }

        public static Sprite Cloud
        {
            get
            {
                if (cloud == null)
                    cloud = CreateCloud();
                return cloud;
            }
        }

        public static Sprite Ocean
        {
            get
            {
                if (ocean != null)
                    return ocean;
                var texture = new Texture2D(1, 64, TextureFormat.RGBA32, false);
                texture.hideFlags = HideFlags.HideAndDontSave;
                for (var y = 0; y < 64; y++)
                    texture.SetPixel(0, y, Color.Lerp(UiTheme.OceanBottom, UiTheme.OceanTop, y / 63f));
                texture.Apply();
                ocean = Sprite.Create(texture, new Rect(0f, 0f, 1f, 64f), new Vector2(0.5f, 0.5f));
                return ocean;
            }
        }

        public static Sprite Sky
        {
            get
            {
                if (sky != null)
                    return sky;
                var texture = new Texture2D(1, 64, TextureFormat.RGBA32, false);
                for (var y = 0; y < 64; y++)
                    texture.SetPixel(0, y, Color.Lerp(UiTheme.SkyBottom, UiTheme.SkyTop, y / 63f));
                texture.Apply();
                sky = Sprite.Create(texture, new Rect(0f, 0f, 1f, 64f), new Vector2(0.5f, 0.5f));
                return sky;
            }
        }

        static Sprite CreateRounded()
        {
            const int size = 64;
            const int radius = 18;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            for (var y = 0; y < size; y++)
            {
                var height = y / (size - 1f);
                var shade = Mathf.Lerp(0.72f, 1f, height);
                if (height > 0.62f)
                    shade = Mathf.Lerp(shade, 1f, (height - 0.62f) / 0.38f);
                for (var x = 0; x < size; x++)
                {
                    var alpha = RoundedAlpha(x, y, size, radius);
                    var gloss = height > 0.72f && x > radius * 0.6f && x < size - radius * 0.6f ? 0.18f : 0f;
                    var value = Mathf.Clamp01(shade + gloss);
                    texture.SetPixel(x, y, new Color(value, value, value, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        static float RoundedAlpha(int x, int y, int size, int radius)
        {
            var px = x + 0.5f;
            var py = y + 0.5f;
            var left = px < radius;
            var right = px > size - radius;
            var bottom = py < radius;
            var top = py > size - radius;
            if (!left && !right && !bottom && !top)
                return 1f;

            var cx = left ? radius : size - radius;
            var cy = bottom ? radius : size - radius;
            if (!(left || right) || !(bottom || top))
                return 1f;

            var dx = px - cx;
            var dy = py - cy;
            var distance = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01(radius - distance + 0.75f);
        }

        static Sprite CreateCircle(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var center = (size - 1) * 0.5f;
            var radius = center;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center;
                    var dy = y - center;
                    var alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 0.75f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite CreateTile()
        {
            const int size = 48;
            const int radius = 12;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var alpha = RoundedAlpha(x, y, size, radius);
                    var shade = 0.78f + (y / (float)(size - 1)) * 0.22f;
                    if (y > size - radius)
                        shade = 1f;
                    if (y < radius * 0.45f)
                        shade *= 0.82f;
                    texture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite CreateIsland()
        {
            const int width = 160;
            const int height = 96;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.42f);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var nx = (x - center.x) / (width * 0.46f);
                    var ny = (y - center.y) / (height * 0.36f);
                    var wobble = 0.08f * Mathf.Sin(x * 0.17f) + 0.06f * Mathf.Sin(y * 0.23f);
                    var distance = nx * nx + ny * ny + wobble;
                    var alpha = Mathf.Clamp01((1.05f - distance) * 4f);
                    var shade = y > center.y ? 1f : 0.86f;
                    texture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.35f), 100f);
        }

        static Sprite CreateTerritory()
        {
            const int size = 160;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = (x - center) / (size * 0.46f);
                    var dy = (y - center) / (size * 0.44f);
                    var angle = Mathf.Atan2(dy, dx);
                    var wobble = 0.07f * Mathf.Sin(angle * 3f) + 0.045f * Mathf.Sin(angle * 5f + 0.8f);
                    var distance = Mathf.Sqrt(dx * dx + dy * dy) - wobble;
                    var alpha = Mathf.Clamp01((1f - distance) * 7f);
                    var shade = Mathf.Lerp(0.78f, 1f, y / (float)(size - 1));
                    texture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite CreateCloud()
        {
            const int width = 180;
            const int height = 80;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var puffs = new[]
            {
                new Vector2(48f, 34f), new Vector2(90f, 46f), new Vector2(132f, 36f)
            };
            var radii = new[] { 26f, 32f, 24f };
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var alpha = 0f;
                    for (var i = 0; i < puffs.Length; i++)
                    {
                        var dx = x - puffs[i].x;
                        var dy = y - puffs[i].y;
                        var distance = Mathf.Sqrt(dx * dx + dy * dy);
                        alpha = Mathf.Max(alpha, Mathf.Clamp01(radii[i] - distance + 1.2f));
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        public static TMP_FontAsset Font
        {
            get
            {
                if (font != null)
                    return font;
                font = TMP_Settings.defaultFontAsset;
                if (font == null)
                    font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (font == null && !loggedMissingFont)
                {
                    loggedMissingFont = true;
                    Debug.LogError("UI: TextMeshPro fontu bulunamadı.");
                }

                return font;
            }
        }

        public static TMP_FontAsset Cute
        {
            get
            {
                if (cute != null)
                    return cute;

                var path = Path.Combine(Application.streamingAssetsPath, "Fonts", "VarelaRound-Regular.ttf");
                if (File.Exists(path))
                    cute = TMP_FontAsset.CreateFontAsset(path, 0, 72, 6, GlyphRenderMode.SDFAA, 512, 512);

                return cute != null ? cute : Font;
            }
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        public static RectTransform Point(Transform parent, string name, Vector2 anchor, Vector2 size)
        {
            var rect = Rect(parent, name, anchor, anchor);
            rect.sizeDelta = size;
            return rect;
        }

        public static UnityEngine.UI.Image Image(Transform parent, string name, Color color, bool circle, Vector2 min, Vector2 max)
        {
            var rect = Rect(parent, name, min, max);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = circle ? Knob : Rounded;
            image.type = circle ? UnityEngine.UI.Image.Type.Simple : UnityEngine.UI.Image.Type.Sliced;
            image.color = color;
            return image;
        }

        public static TMP_Text Label(Transform parent, string value, float size, Color color, Vector2 min, Vector2 max, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var rect = Rect(parent, "Label", min, max);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = Font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.fontStyle = FontStyles.Bold;
            return text;
        }

        public static UnityEngine.UI.Button Button(Transform parent, string name, string caption, Color color, Vector2 min, Vector2 max, Action onClick, float fontSize = 28f)
        {
            var image = Image(parent, name, color, false, min, max);
            var button = image.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.fadeDuration = 0.05f;
            colors.highlightedColor = new Color(1f, 0.96f, 0.86f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f);
            button.colors = colors;
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            var captionLabel = Label(image.transform, caption, fontSize, UiTheme.Ink, Vector2.zero, Vector2.one);
            captionLabel.outlineWidth = 0.14f;
            captionLabel.outlineColor = new Color32(28, 36, 52, 140);
            var shine = Image(image.transform, "Shine", new Color(1f, 1f, 1f, 0.22f), false, new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.86f));
            shine.raycastTarget = false;
            var shadow = Image(parent, name + "Shadow", new Color(0.08f, 0.12f, 0.18f, 0.34f), false, min + new Vector2(0.004f, -0.012f), max + new Vector2(0.004f, -0.012f));
            shadow.raycastTarget = false;
            shadow.transform.SetSiblingIndex(image.transform.GetSiblingIndex());
            var gate = 0f;
            button.onClick.AddListener(() =>
            {
                if (Time.unscaledTime < gate)
                    return;
                gate = Time.unscaledTime + 0.2f;
                if (GameBootstrap.Instance != null)
                    GameBootstrap.Instance.Audio.PlayTap();
                onClick?.Invoke();
            });
            return button;
        }

        public static RectTransform Dialog(Transform parent, string title, string body, Action onClose)
        {
            var blocker = Image(parent, "Modal", new Color(0.04f, 0.06f, 0.1f, 0.72f), false, Vector2.zero, Vector2.one);
            var card = Image(blocker.transform, "Card", UiTheme.Panel, false, new Vector2(0.28f, 0.22f), new Vector2(0.72f, 0.78f));
            var titleLabel = Label(card.transform, title, 36f, UiTheme.Yellow, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.96f));
            titleLabel.gameObject.name = "Title";
            var bodyLabel = Label(card.transform, body, 24f, UiTheme.Ink, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.76f), TextAlignmentOptions.Top);
            bodyLabel.gameObject.name = "Body";
            Button(card.transform, "Close", Locale.T("Kapat", "Close"), UiTheme.Yellow, new Vector2(0.3f, 0.06f), new Vector2(0.7f, 0.2f), () =>
            {
                GameBootstrap.Instance.State.SetModal(false);
                UnityEngine.Object.Destroy(blocker.gameObject);
                onClose?.Invoke();
            }, 26f);
            GameBootstrap.Instance.State.SetModal(true);
            return card.rectTransform;
        }

        public static void Header(Transform parent, string title, Action back)
        {
            Image(parent, "TopBar", new Color(0.06f, 0.1f, 0.16f, 0.55f), false, new Vector2(0f, 0.9f), Vector2.one);
            Button(parent, "Back", Locale.T("Geri", "Back"), UiTheme.Panel, new Vector2(0.02f, 0.915f), new Vector2(0.12f, 0.975f), back, 24f);
            Label(parent, title, 34f, UiTheme.Ink, new Vector2(0.14f, 0.9f), new Vector2(0.7f, 1f), TextAlignmentOptions.Left);
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
    }

    public sealed class EverySecond : MonoBehaviour
    {
        public Action Tick;
        float elapsed;

        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed < 1f)
                return;
            elapsed = 0f;
            Tick?.Invoke();
        }
    }
}

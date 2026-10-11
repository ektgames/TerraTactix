using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TerraTactix.UI
{
    public static class EktIntro
    {
        static bool played;

        public static void Play(MonoBehaviour host, Transform canvas)
        {
            if (played || host == null || canvas == null)
                return;

            played = true;
            host.StartCoroutine(Run(canvas));
        }

        static IEnumerator Run(Transform canvas)
        {
            var intro = new GameObject("EktReklamIntroPanel", typeof(RectTransform));
            intro.transform.SetParent(canvas, false);
            var panel = intro.GetComponent<RectTransform>();
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            intro.transform.SetAsLastSibling();

            var background = intro.AddComponent<Image>();
            background.color = Color.black;
            background.raycastTarget = true;
            var group = intro.AddComponent<CanvasGroup>();
            group.alpha = 1f;

            var barcode = new GameObject("BarcodeContainer", typeof(RectTransform));
            barcode.transform.SetParent(intro.transform, false);
            var barcodeRect = barcode.GetComponent<RectTransform>();
            barcodeRect.anchorMin = new Vector2(0.5f, 0.5f);
            barcodeRect.anchorMax = new Vector2(0.5f, 0.5f);
            barcodeRect.pivot = new Vector2(0.5f, 0.5f);
            barcodeRect.anchoredPosition = new Vector2(0f, 70f);
            barcodeRect.sizeDelta = new Vector2(420f, 90f);
            var barcodeGroup = barcode.AddComponent<CanvasGroup>();

            var pattern = new[] { 2, 4, 1, 1, 3, 2, 5, 1, 2, 4, 1, 3, 2, 1, 5, 2, 3, 1, 4, 2, 1, 3, 2 };
            var total = 0f;
            for (var i = 0; i < pattern.Length; i++)
                total += pattern[i] * 4f + 3f;
            var cursor = -total * 0.5f;
            for (var i = 0; i < pattern.Length; i++)
            {
                var width = pattern[i] * 4f;
                if (i % 2 == 0)
                {
                    var bar = new GameObject("Bar", typeof(RectTransform));
                    bar.transform.SetParent(barcode.transform, false);
                    var barRect = bar.GetComponent<RectTransform>();
                    barRect.anchorMin = new Vector2(0.5f, 0.5f);
                    barRect.anchorMax = new Vector2(0.5f, 0.5f);
                    barRect.pivot = new Vector2(0f, 0.5f);
                    barRect.anchoredPosition = new Vector2(cursor, 12f);
                    barRect.sizeDelta = new Vector2(width, 78f);
                    var barImage = bar.AddComponent<Image>();
                    barImage.color = new Color(0.85f, 0.85f, 0.9f, 1f);
                    barImage.raycastTarget = false;
                }

                cursor += width + 3f;
            }

            var code = Word(barcode.transform, "EKT-7928-GAMES", 22f, new Color(0.6f, 0.6f, 0.65f, 1f), new Vector2(0f, -8f), new Vector2(420f, 28f));
            code.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            code.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            code.rectTransform.pivot = new Vector2(0.5f, 1f);

            var laser = new GameObject("LaserGlow", typeof(RectTransform));
            laser.transform.SetParent(barcode.transform, false);
            var laserRect = laser.GetComponent<RectTransform>();
            laserRect.anchorMin = new Vector2(0.5f, 1f);
            laserRect.anchorMax = new Vector2(0.5f, 1f);
            laserRect.pivot = new Vector2(0.5f, 0.5f);
            laserRect.sizeDelta = new Vector2(460f, 8f);
            var laserImage = laser.AddComponent<Image>();
            laserImage.color = new Color(1f, 0f, 0.1f, 0.35f);
            laserImage.raycastTarget = false;
            var core = new GameObject("LaserCore", typeof(RectTransform));
            core.transform.SetParent(laser.transform, false);
            var coreRect = core.GetComponent<RectTransform>();
            coreRect.anchorMin = Vector2.zero;
            coreRect.anchorMax = Vector2.one;
            coreRect.offsetMin = new Vector2(0f, 2f);
            coreRect.offsetMax = new Vector2(0f, -2f);
            var coreImage = core.AddComponent<Image>();
            coreImage.color = new Color(1f, 0.3f, 0.3f, 1f);
            coreImage.raycastTarget = false;

            var glow = Word(intro.transform, "EKT GAMES", 96f, new Color(0f, 0.7f, 1f, 0f), new Vector2(0f, -36f), new Vector2(900f, 120f));
            var outline = glow.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0.4f, 0.8f, 0.3f);
            outline.effectDistance = new Vector2(3f, 3f);
            var letters = Word(glow.transform, "EKT GAMES", 92f, new Color(1f, 1f, 1f, 0f), Vector2.zero, Vector2.zero);
            letters.rectTransform.anchorMin = Vector2.zero;
            letters.rectTransform.anchorMax = Vector2.one;
            letters.rectTransform.offsetMin = Vector2.zero;
            letters.rectTransform.offsetMax = Vector2.zero;
            var presents = Word(intro.transform, "PRESENTS", 28f, new Color(0.6f, 0.6f, 0.7f, 0f), new Vector2(0f, -130f), new Vector2(520f, 40f));

            var elapsed = 0f;
            var beeped = false;
            while (elapsed < 4.8f)
            {
                elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                intro.transform.SetAsLastSibling();
                if (elapsed > 1.5f && SkipPressed())
                    break;

                if (elapsed < 2f)
                {
                    barcodeGroup.alpha = Mathf.Clamp01((2f - elapsed) / 0.3f);
                    var scan = Mathf.Clamp01(elapsed / 1.5f);
                    laserRect.anchoredPosition = new Vector2(0f, -scan * 78f);
                    if (!beeped && elapsed >= 1.5f)
                    {
                        beeped = true;
                        PlayBeep();
                    }
                }
                else if (barcode.activeSelf)
                    barcode.SetActive(false);

                if (elapsed >= 1.3f)
                {
                    var logo = Mathf.Clamp01((elapsed - 1.3f) / 0.4f);
                    var flicker = elapsed >= 2.1f || Random.value > 0.4f;
                    var pulse = Mathf.PingPong(Time.unscaledTime * 2f, 1f);
                    var neon = flicker ? 0.6f + pulse * 0.4f : 0.05f;
                    glow.color = new Color(0f, 0.7f, 1f, neon * 0.8f * logo);
                    letters.color = new Color(1f, 1f, 1f, logo * (flicker ? 1f : 0.1f));
                    var subtitle = Mathf.Clamp01((elapsed - 1.4f) / 0.6f);
                    presents.color = new Color(0.6f, 0.6f, 0.7f, subtitle);
                }

                if (elapsed >= 4f)
                    group.alpha = Mathf.Clamp01((4.8f - elapsed) / 0.8f);

                yield return null;
            }

            Object.Destroy(intro);
        }

        static TMP_Text Word(Transform parent, string value, float size, Color color, Vector2 position, Vector2 box)
        {
            var go = new GameObject(value, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = box;
            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = UiFactory.Font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        static bool SkipPressed()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
                return true;
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                return true;
            var touches = Touchscreen.current;
            if (touches == null)
                return false;
            for (var i = 0; i < touches.touches.Count; i++)
            {
                if (touches.touches[i].press.wasPressedThisFrame)
                    return true;
            }

            return false;
        }

        static void PlayBeep()
        {
            const int rate = 44100;
            const float duration = 0.08f;
            const float frequency = 1800f;
            var count = (int)(rate * duration);
            var samples = new float[count];
            for (var i = 0; i < count; i++)
            {
                var time = (float)i / rate;
                var fade = 1f - time / duration;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * 0.4f * fade;
            }

            var clip = AudioClip.Create("EktScanBeep", count, 1, rate, false);
            clip.SetData(samples, 0);
            var listener = Object.FindAnyObjectByType<AudioListener>();
            var point = listener != null ? listener.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(clip, point, 0.4f);
        }
    }
}

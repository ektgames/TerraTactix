using System.Collections;
using TerraTactix.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TerraTactix.UI
{
    public sealed class UIManager : MonoBehaviour
    {
        RectTransform canvasRoot;
        RectTransform safeRoot;
        RectTransform screen;
        Image backdrop;
        string shown = SceneNames.MainMenu;
        bool localeQueued;

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Locale.Changed += OnLanguageChanged;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Locale.Changed -= OnLanguageChanged;
        }

        void OnLanguageChanged()
        {
            if (localeQueued || safeRoot == null)
                return;
            localeQueued = true;
            StartCoroutine(RebuildAfterLanguage());
        }

        IEnumerator RebuildAfterLanguage()
        {
            yield return null;
            localeQueued = false;
            if (safeRoot == null)
                yield break;
            var reopen = Locale.ReopenSettings;
            Locale.ConsumeReopen();
            Show(shown);
            if (reopen && shown == SceneNames.MainMenu && screen != null)
                MainMenuView.ShowSettings(screen);
        }

        public void BuildCanvas()
        {
            UiFactory.EnsureEventSystem();
            var canvasObject = new GameObject("TerraCanvas", typeof(RectTransform));
            DontDestroyOnLoad(canvasObject);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            backdrop = UiFactory.Image(canvasObject.transform, "Backdrop", Color.white, false, Vector2.zero, Vector2.one);
            backdrop.sprite = UiFactory.Sky;
            backdrop.type = Image.Type.Simple;
            backdrop.raycastTarget = false;

            var safe = UiFactory.Rect(canvasObject.transform, "Safe", Vector2.zero, Vector2.one);
            safe.gameObject.AddComponent<SafeAreaFit>();
            safeRoot = safe;
            canvasRoot = canvasObject.GetComponent<RectTransform>();
        }

        public void PlayStartupIntro()
        {
            EktIntro.Play(this, canvasRoot);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameBootstrap.Instance.RememberScene(scene.name);
            Show(scene.name);
        }

        public void Show(string sceneName)
        {
            if (safeRoot == null)
                return;

            if (screen != null)
                Destroy(screen.gameObject);

            if (sceneName != SceneNames.MainMenu && GameBootstrap.Instance.Save.Data.player != null && !GameBootstrap.Instance.Save.Data.player.nameChosen)
                sceneName = SceneNames.MainMenu;

            shown = sceneName;
            GameBootstrap.Instance.State.SetModal(false);
            GameBootstrap.Instance.State.SetScene(sceneName);
            GameBootstrap.Instance.Audio.SetSceneMusic(sceneName);
            if (backdrop != null)
                ApplyBackdrop(sceneName);
            screen = UiFactory.Rect(safeRoot, "Screen", Vector2.zero, Vector2.one);

            switch (sceneName)
            {
                case SceneNames.WorldMap:
                    WorldMapView.Build(screen);
                    break;
                case SceneNames.Battle:
                    BattleView.Build(screen);
                    break;
                case SceneNames.Shop:
                    ShopView.Build(screen);
                    break;
                case SceneNames.DailyWheel:
                    WheelView.Build(screen);
                    break;
                case SceneNames.Leaderboard:
                    LeaderboardView.Build(screen);
                    break;
                default:
                    MainMenuView.Build(screen);
                    break;
            }
        }

        void ApplyBackdrop(string sceneName)
        {
            var menu = sceneName == SceneNames.MainMenu ? ArtLibrary.MenuScene : null;
            var cover = backdrop.GetComponent<CoverFit>();
            if (menu != null)
            {
                backdrop.sprite = menu;
                backdrop.color = Color.white;
                backdrop.type = Image.Type.Simple;
                backdrop.preserveAspect = false;
                if (cover == null)
                    cover = backdrop.gameObject.AddComponent<CoverFit>();
                cover.enabled = true;
                cover.Apply();
                return;
            }

            if (cover != null)
                cover.enabled = false;
            var rect = backdrop.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            backdrop.sprite = sceneName == SceneNames.WorldMap ? UiFactory.Ocean : UiFactory.Sky;
            backdrop.color = Color.white;
            backdrop.type = Image.Type.Simple;
            backdrop.preserveAspect = false;
        }
    }

    public sealed class SafeAreaFit : MonoBehaviour
    {
        Rect last;
        RectTransform rect;

        void Awake()
        {
            rect = (RectTransform)transform;
        }

        void OnEnable()
        {
            Apply();
        }

        void Update()
        {
            Apply();
        }

        void Apply()
        {
            if (rect == null || Screen.width <= 0 || Screen.height <= 0)
                return;
            var area = Screen.safeArea;
            if (area == last)
                return;
            last = area;
            var min = area.position;
            var max = area.position + area.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    public sealed class CoverFit : MonoBehaviour
    {
        RectTransform rect;
        Image image;
        Vector2 lastParent;
        Sprite lastSprite;

        void Awake()
        {
            rect = (RectTransform)transform;
            image = GetComponent<Image>();
        }

        void OnEnable()
        {
            lastParent = Vector2.zero;
            lastSprite = null;
            Apply();
        }

        void Update()
        {
            Apply();
        }

        public void Apply()
        {
            if (rect == null)
                rect = (RectTransform)transform;
            if (image == null)
                image = GetComponent<Image>();
            var parent = rect.parent as RectTransform;
            if (parent == null || image == null || image.sprite == null)
                return;

            var size = parent.rect.size;
            if (size.x < 1f || size.y < 1f)
                return;
            if (size == lastParent && image.sprite == lastSprite)
                return;

            var sprite = image.sprite.rect.size;
            if (sprite.x < 1f || sprite.y < 1f)
                return;

            lastParent = size;
            lastSprite = image.sprite;
            var scale = Mathf.Max(size.x / sprite.x, size.y / sprite.y);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = sprite * scale;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
        }
    }
}

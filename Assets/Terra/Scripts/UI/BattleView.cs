using TerraTactix.Core;
using TerraTactix.Data;
using TerraTactix.Puzzle;
using TMPro;
using UnityEngine;

namespace TerraTactix.UI
{
    public static class BattleView
    {
        public static void Build(Transform parent)
        {
            var sky = UiFactory.Image(parent, "Sky", Color.white, false, Vector2.zero, Vector2.one);
            sky.sprite = UiFactory.Sky;
            sky.type = UnityEngine.UI.Image.Type.Simple;
            sky.raycastTarget = false;

            var session = GameBootstrap.Instance.Battles.Session;
            if (session == null)
            {
                UiFactory.Label(parent, Locale.T("BattleManager: BattleSessionData eksik.", "BattleManager: BattleSessionData is missing."), 28f, UiTheme.Ink, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f));
                UiFactory.Button(parent, "Back", Locale.T("Haritaya dön", "Back to map"), UiTheme.Yellow, new Vector2(0.35f, 0.2f), new Vector2(0.65f, 0.32f), () =>
                {
                    GameBootstrap.Instance.Scenes.Load(SceneNames.WorldMap);
                }, 24f);
                return;
            }

            var region = RegionCatalog.Get(session.regionId);
            var regionName = region == null ? session.regionId : Locale.Place(region.id, region.displayName);
            var rim = UiFactory.Image(parent, "BoardRim", new Color(0.98f, 0.82f, 0.32f, 1f), false, new Vector2(0.31f, 0.03f), new Vector2(0.69f, 0.9f));
            rim.raycastTarget = false;
            var board = UiFactory.Image(parent, "Board", new Color(0.05f, 0.09f, 0.16f, 0.98f), false, new Vector2(0.318f, 0.045f), new Vector2(0.682f, 0.885f));
            var frame = parent.gameObject.AddComponent<BoardFrame>();
            frame.Rim = rim.rectTransform;
            frame.Board = board.rectTransform;
            frame.Columns = 8;
            frame.Rows = 12;
            frame.Fit();

            var bar = UiFactory.Image(parent, "TopBar", new Color(0.05f, 0.1f, 0.18f, 0.55f), false, new Vector2(0f, 0.9f), Vector2.one);
            bar.raycastTarget = false;
            Head(parent, "PlayerHead", new Vector2(0.034f, 0.948f), 58f, ArtLibrary.Hero, new Color(1f, 0.82f, 0.28f));
            var score = UiFactory.Label(parent, $"{Locale.T("Sefer", "Expedition")} {session.campaignIndex}", 20f, UiTheme.Ink, new Vector2(0.068f, 0.908f), new Vector2(0.4f, 0.985f), TextAlignmentOptions.Left);
            score.outlineWidth = 0.12f;
            score.textWrappingMode = TextWrappingModes.NoWrap;
            score.overflowMode = TextOverflowModes.Ellipsis;
            var timerPlate = UiFactory.Image(parent, "TimerPlate", new Color(0.07f, 0.12f, 0.2f, 0.94f), false, new Vector2(0.41f, 0.918f), new Vector2(0.57f, 0.978f));
            timerPlate.raycastTarget = false;
            var timerRim = UiFactory.Image(parent, "TimerRim", new Color(0.98f, 0.8f, 0.28f, 0.9f), false, new Vector2(0.408f, 0.912f), new Vector2(0.572f, 0.984f));
            timerRim.raycastTarget = false;
            timerRim.transform.SetSiblingIndex(timerPlate.transform.GetSiblingIndex());
            var timer = UiFactory.Label(parent, "01:30", 32f, UiTheme.Ink, new Vector2(0.41f, 0.918f), new Vector2(0.57f, 0.978f));
            var island = UiFactory.Label(parent, regionName, 24f, UiTheme.Ink, new Vector2(0.58f, 0.91f), new Vector2(0.845f, 0.985f), TextAlignmentOptions.Right);
            island.fontStyle = FontStyles.Bold;
            island.textWrappingMode = TextWrappingModes.NoWrap;
            island.overflowMode = TextOverflowModes.Ellipsis;
            island.outlineWidth = 0.12f;

            var game = parent.gameObject.AddComponent<PuzzleGameManager>();
            UiFactory.Button(parent, "Pause", Locale.T("Duraklat", "Pause"), new Color(0.16f, 0.22f, 0.34f), new Vector2(0.858f, 0.918f), new Vector2(0.985f, 0.982f), game.Pause, 16f);

            var seconds = Mathf.CeilToInt(session.timeLimit);
            var blocker = UiFactory.Image(parent, "Modal", new Color(0.04f, 0.06f, 0.1f, 0.72f), false, Vector2.zero, Vector2.one);
            var prepRim = UiFactory.Image(blocker.transform, "PrepRim", UiTheme.Gold, false, new Vector2(0.292f, 0.078f), new Vector2(0.708f, 0.922f));
            prepRim.raycastTarget = false;
            var card = UiFactory.Image(blocker.transform, "Card", new Color(0.08f, 0.13f, 0.22f, 0.98f), false, new Vector2(0.30f, 0.09f), new Vector2(0.70f, 0.91f));
            var title = UiFactory.Label(card.transform, Locale.T("Sefer hazırlığı", "Expedition prep"), 32f, UiTheme.Yellow, new Vector2(0.08f, 0.905f), new Vector2(0.92f, 0.985f));
            title.fontStyle = FontStyles.Bold;
            PrepRow(card.transform, "Region", Locale.T("Bölge", "Region"), regionName, 0.798f, 0.888f, UiTheme.Ink);
            PrepRow(card.transform, "Index", Locale.T("Sefer", "Expedition"), session.campaignIndex.ToString(), 0.698f, 0.788f, UiTheme.Gold);
            PrepRow(card.transform, "Difficulty", Locale.T("Zorluk", "Difficulty"), Locale.Difficulty(session.difficulty), 0.598f, 0.688f, UiTheme.Yellow);
            PrepRow(card.transform, "Goal", Locale.T("Hedef", "Goal"), Locale.T("Tahtadaki tüm taşlar", "Clear every stone"), 0.498f, 0.588f, UiTheme.Ink);
            PrepRow(card.transform, "Time", Locale.T("Süre", "Time"), $"{seconds / 60:00}:{seconds % 60:00}", 0.398f, 0.488f, UiTheme.Ink);
            var notePlate = UiFactory.Image(card.transform, "HeartNote", new Color(0.28f, 0.10f, 0.12f, 1f), false, new Vector2(0.08f, 0.275f), new Vector2(0.92f, 0.372f));
            notePlate.raycastTarget = false;
            var note = UiFactory.Label(notePlate.transform, Locale.T("Başlayınca 1 can harcanır.\nVazgeçersen can gitmez.", "Starting spends 1 heart.\nLeaving before the start does not."), 18f, UiTheme.Ink, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));
            note.fontStyle = FontStyles.Bold;
            GameBootstrap.Instance.State.SetModal(true);
            UiFactory.Button(card.transform, "Start", Locale.T("Seferi başlat", "Start expedition"), UiTheme.Green, new Vector2(0.1f, 0.145f), new Vector2(0.9f, 0.245f), () =>
            {
                GameBootstrap.Instance.State.SetModal(false);
                UnityEngine.Object.Destroy(blocker.gameObject);
                game.Begin(board.rectTransform, score, timer, null, board.rectTransform);
            }, 24f);
            UiFactory.Button(card.transform, "Leave", Locale.T("Vazgeç", "Leave"), UiTheme.Yellow, new Vector2(0.22f, 0.03f), new Vector2(0.78f, 0.115f), () =>
            {
                GameBootstrap.Instance.State.SetModal(false);
                UnityEngine.Object.Destroy(blocker.gameObject);
                if (GameBootstrap.Instance != null)
                    GameBootstrap.Instance.Scenes.Load(SceneNames.WorldMap);
            }, 22f);
        }

        static void PrepRow(Transform card, string name, string caption, string value, float yMin, float yMax, Color valueColor)
        {
            var plate = UiFactory.Image(card, name, new Color(0.05f, 0.09f, 0.16f, 1f), false, new Vector2(0.08f, yMin), new Vector2(0.92f, yMax));
            plate.raycastTarget = false;
            var cap = UiFactory.Label(plate.transform, caption, 18f, UiTheme.Muted, new Vector2(0.04f, 0f), new Vector2(0.36f, 1f), TextAlignmentOptions.MidlineLeft);
            cap.margin = new Vector4(16f, 0f, 8f, 0f);
            var val = UiFactory.Label(plate.transform, value, 22f, valueColor, new Vector2(0.38f, 0f), new Vector2(0.96f, 1f), TextAlignmentOptions.MidlineRight);
            val.margin = new Vector4(8f, 0f, 16f, 0f);
            val.fontStyle = FontStyles.Bold;
        }

        static void Head(Transform parent, string name, Vector2 anchor, float size, Sprite sprite, Color ringColor)
        {
            var ring = UiFactory.Point(parent, name + "Ring", anchor, new Vector2(size + 10f, size + 10f));
            var ringImage = ring.gameObject.AddComponent<UnityEngine.UI.Image>();
            ringImage.sprite = UiFactory.Knob;
            ringImage.color = ringColor;
            ringImage.raycastTarget = false;
            var frame = UiFactory.Point(ring, "Frame", new Vector2(0.5f, 0.5f), new Vector2(size, size));
            var frameImage = frame.gameObject.AddComponent<UnityEngine.UI.Image>();
            frameImage.sprite = UiFactory.Knob;
            frameImage.color = new Color(0.1f, 0.22f, 0.4f);
            frameImage.raycastTarget = false;
            frame.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = true;
            if (sprite == null)
                return;
            var face = UiFactory.Point(frame, "Face", new Vector2(0.5f, 0.14f), new Vector2(size * 1.15f, size * 2.35f));
            var image = face.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
    }

    public sealed class BoardFrame : MonoBehaviour
    {
        public RectTransform Rim;
        public RectTransform Board;
        public int Columns = 8;
        public int Rows = 12;

        void Start()
        {
            Fit();
        }

        void OnRectTransformDimensionsChange()
        {
            Fit();
        }

        public void Fit()
        {
            if (Board == null)
                return;
            var area = (RectTransform)transform;
            var rect = area.rect;
            if (rect.width < 32f || rect.height < 32f)
                return;
            var hud = rect.height * 0.1f;
            var marginX = Mathf.Max(14f, rect.width * 0.012f);
            var marginY = Mathf.Max(12f, rect.height * 0.015f);
            var x0 = marginX / rect.width;
            var y0 = marginY / rect.height;
            var x1 = 1f - x0;
            var top = 1f - hud / rect.height - 0.012f;
            Place(Board, x0, y0, x1, top);
            if (Rim == null)
                return;
            var px = 6f / rect.width;
            var py = 6f / rect.height;
            Place(Rim, x0 - px, y0 - py, x1 + px, top + py);
        }

        static void Place(RectTransform rect, float x0, float y0, float x1, float y1)
        {
            rect.anchorMin = new Vector2(x0, y0);
            rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}

using TerraTactix.Core;
using TerraTactix.Data;
using TerraTactix.Online;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerraTactix.UI
{
    public static class LeaderboardView
    {
        const float RowPitch = 92f;

        public static void Build(Transform parent)
        {
            var sky = UiFactory.Image(parent, "Bg", Color.white, false, Vector2.zero, Vector2.one);
            sky.sprite = UiFactory.Sky;
            sky.type = Image.Type.Simple;
            sky.raycastTarget = false;
            UiFactory.Header(parent, Locale.T("Sıralama", "Leaderboard"), () => GameBootstrap.Instance.Scenes.Load(SceneNames.MainMenu));

            var status = Cell(parent, Locale.T("Sıralama yükleniyor...", "Loading the leaderboard..."), 22f, UiTheme.Ink, new Vector2(0.06f, 0.845f), new Vector2(0.94f, 0.895f), TextAlignmentOptions.Left);
            Head(parent);

            var content = List(parent);
            var mine = Mine(parent, out var rank, out var name, out var score, out var regions, out var place);
            var note = Cell(parent, string.Empty, 18f, UiTheme.Muted, new Vector2(0.08f, 0.012f), new Vector2(0.92f, 0.055f), TextAlignmentOptions.Left);

            var page = parent.gameObject.AddComponent<LeaderboardPage>();
            page.Open(status, content, rank, name, score, regions, place, note, mine);
        }

        static void Head(Transform parent)
        {
            var bar = UiFactory.Image(parent, "Columns", new Color(0.08f, 0.13f, 0.22f, 0.72f), false, new Vector2(0.05f, 0.785f), new Vector2(0.95f, 0.84f));
            bar.raycastTarget = false;
            Cell(bar.transform, Locale.T("Sıra", "Rank"), 20f, UiTheme.Yellow, new Vector2(0.02f, 0f), new Vector2(0.14f, 1f), TextAlignmentOptions.Center);
            Cell(bar.transform, Locale.T("İsim", "Name"), 20f, UiTheme.Yellow, new Vector2(0.16f, 0f), new Vector2(0.52f, 1f), TextAlignmentOptions.Left);
            Cell(bar.transform, Locale.T("Puan", "Score"), 20f, UiTheme.Yellow, new Vector2(0.54f, 0f), new Vector2(0.74f, 1f), TextAlignmentOptions.Center);
            Cell(bar.transform, Locale.T("Bölge", "Regions"), 20f, UiTheme.Yellow, new Vector2(0.76f, 0f), new Vector2(0.98f, 1f), TextAlignmentOptions.Center);
        }

        static RectTransform Mine(Transform parent, out TMP_Text rank, out TMP_Text name, out TMP_Text score, out TMP_Text regions, out TMP_Text place)
        {
            var rim = UiFactory.Image(parent, "MineRim", UiTheme.Gold, false, new Vector2(0.046f, 0.058f), new Vector2(0.954f, 0.232f));
            rim.raycastTarget = false;
            var card = UiFactory.Image(rim.transform, "Mine", new Color(0.12f, 0.28f, 0.24f, 0.98f), false, new Vector2(0.008f, 0.03f), new Vector2(0.992f, 0.97f));
            card.raycastTarget = false;
            Cell(card.transform, Locale.T("Senin sıran", "Your place"), 18f, UiTheme.Yellow, new Vector2(0.03f, 0.62f), new Vector2(0.55f, 0.95f), TextAlignmentOptions.Left);
            place = Cell(card.transform, string.Empty, 18f, UiTheme.Muted, new Vector2(0.5f, 0.62f), new Vector2(0.97f, 0.95f), TextAlignmentOptions.Right);
            rank = Cell(card.transform, "—", 28f, Color.white, new Vector2(0.02f, 0.08f), new Vector2(0.14f, 0.6f), TextAlignmentOptions.Center);
            name = Cell(card.transform, string.Empty, 28f, Color.white, new Vector2(0.16f, 0.08f), new Vector2(0.52f, 0.6f), TextAlignmentOptions.Left);
            score = Cell(card.transform, "0", 28f, Color.white, new Vector2(0.54f, 0.08f), new Vector2(0.74f, 0.6f), TextAlignmentOptions.Center);
            regions = Cell(card.transform, "0", 28f, Color.white, new Vector2(0.76f, 0.08f), new Vector2(0.98f, 0.6f), TextAlignmentOptions.Center);
            return rim.rectTransform;
        }

        static RectTransform List(Transform parent)
        {
            var viewport = UiFactory.Rect(parent, "List", new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.775f));
            var hit = viewport.gameObject.AddComponent<Image>();
            hit.color = new Color(0.05f, 0.09f, 0.16f, 0.28f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 36f;
            scroll.inertia = true;
            scroll.viewport = viewport;

            var contentGo = new GameObject("Rows", typeof(RectTransform));
            contentGo.transform.SetParent(viewport, false);
            var content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, RowPitch);
            scroll.content = content;
            return content;
        }

        public static void Row(RectTransform content, int index, LeaderboardEntry entry)
        {
            var row = UiFactory.Rect(content, "Row" + index, new Vector2(0f, 1f), new Vector2(1f, 1f));
            row.pivot = new Vector2(0.5f, 1f);
            row.sizeDelta = new Vector2(-16f, RowPitch - 12f);
            row.anchoredPosition = new Vector2(0f, -8f - index * RowPitch);
            var plate = UiFactory.Image(row, "Plate", RowColor(entry), false, Vector2.zero, Vector2.one);
            plate.raycastTarget = false;
            Cell(row, entry.rank.ToString(), 28f, RankColor(entry.rank), new Vector2(0.02f, 0f), new Vector2(0.14f, 1f), TextAlignmentOptions.Center);
            Cell(row, string.IsNullOrEmpty(entry.playerName) ? "—" : entry.playerName, 26f, Color.white, new Vector2(0.16f, 0f), new Vector2(0.52f, 1f), TextAlignmentOptions.Left);
            Cell(row, entry.score.ToString(), 26f, Color.white, new Vector2(0.54f, 0f), new Vector2(0.74f, 1f), TextAlignmentOptions.Center);
            Cell(row, entry.regions.ToString(), 26f, Color.white, new Vector2(0.76f, 0f), new Vector2(0.98f, 1f), TextAlignmentOptions.Center);
        }

        static Color RowColor(LeaderboardEntry entry)
        {
            if (entry.isLocal)
                return new Color(0.16f, 0.36f, 0.3f, 0.98f);
            if (entry.rank == 1)
                return new Color(0.42f, 0.3f, 0.08f, 0.96f);
            if (entry.rank == 2)
                return new Color(0.22f, 0.26f, 0.34f, 0.96f);
            if (entry.rank == 3)
                return new Color(0.36f, 0.2f, 0.12f, 0.96f);
            return new Color(0.1f, 0.16f, 0.28f, 0.94f);
        }

        static Color RankColor(int rank)
        {
            if (rank == 1)
                return UiTheme.Gold;
            if (rank == 2)
                return new Color(0.86f, 0.9f, 0.96f, 1f);
            if (rank == 3)
                return new Color(0.92f, 0.62f, 0.38f, 1f);
            return UiTheme.Yellow;
        }

        static TMP_Text Cell(Transform parent, string value, float size, Color color, Vector2 min, Vector2 max, TextAlignmentOptions align)
        {
            var text = UiFactory.Label(parent, value, size, color, min, max, align);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }
    }

    public sealed class LeaderboardPage : MonoBehaviour
    {
        TMP_Text status;
        RectTransform content;
        TMP_Text rank;
        TMP_Text playerName;
        TMP_Text score;
        TMP_Text regions;
        TMP_Text place;
        TMP_Text note;

        public void Open(TMP_Text statusLabel, RectTransform rows, TMP_Text rankLabel, TMP_Text nameLabel, TMP_Text scoreLabel, TMP_Text regionLabel, TMP_Text placeLabel, TMP_Text noteLabel, RectTransform mine)
        {
            status = statusLabel;
            content = rows;
            rank = rankLabel;
            playerName = nameLabel;
            score = scoreLabel;
            regions = regionLabel;
            place = placeLabel;
            note = noteLabel;
            if (mine != null)
                mine.SetAsLastSibling();
            if (note != null)
                note.transform.SetAsLastSibling();

            var board = GameBootstrap.Instance.Leaderboard;
            if (!board.IsLive)
            {
                Show(board.GetTop(), true, true);
                return;
            }

            board.Refresh((rowsResult, ok) =>
            {
                if (this == null)
                    return;
                Show(rowsResult, ok, false);
            });
        }

        void Show(LeaderboardEntry[] rows, bool ok, bool sample)
        {
            if (status != null)
            {
                if (sample)
                    status.text = Locale.T("Örnek liste. Bu isimler gerçek oyuncu değil.", "Sample list. These names are not real players.");
                else if (!ok)
                    status.text = Locale.T("Canlı liste alınamadı. İnternet gelince yeniden aç.", "The live list could not be loaded. Open it again when you are online.");
                else if (rows == null || rows.Length == 0)
                    status.text = Locale.T("Henüz listede oyuncu yok.", "No players on the list yet.");
                else
                    status.text = Locale.T("İlk 100 oyuncu", "Top 100 players");
            }

            if (content != null)
            {
                for (var i = content.childCount - 1; i >= 0; i--)
                    Destroy(content.GetChild(i).gameObject);
                var count = rows == null ? 0 : rows.Length;
                content.sizeDelta = new Vector2(0f, Mathf.Max(92f, 8f + count * 92f));
                for (var i = 0; i < count; i++)
                    LeaderboardView.Row(content, i, rows[i]);
            }

            var local = GameBootstrap.Instance.Leaderboard.LocalRow();
            if (rank != null)
                rank.text = local.rank > 0 ? local.rank.ToString() : "—";
            if (playerName != null)
                playerName.text = string.IsNullOrEmpty(local.playerName) ? "—" : local.playerName;
            if (score != null)
                score.text = local.score.ToString();
            if (regions != null)
                regions.text = local.regions.ToString();
            if (place != null)
                place.text = local.rank > 0
                    ? Locale.T("Listedesin", "You are on the list")
                    : Locale.T("İlk 100 dışında", "Outside the top 100");
            if (note != null)
                note.text = Pending(sample);
        }

        static string Pending(bool sample)
        {
            if (sample)
                return string.Empty;

            var list = GameBootstrap.Instance.Save.Data.pendingSubmissions;
            if (list == null)
                return string.Empty;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var last = list[i];
                if (last == null || last.uploaded)
                    continue;
                return Locale.T("Son puanın henüz gönderilemedi.", "Your latest score has not been sent yet.");
            }

            return list.Count == 0
                ? string.Empty
                : Locale.T("Puanın sıralamaya yazıldı.", "Your score is on the leaderboard.");
        }
    }
}

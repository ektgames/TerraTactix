using System;
using System.Collections.Generic;
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

            Search(parent, out var searchInput, out var searchButton);
            var status = Cell(parent, Locale.T("Sıralama yükleniyor...", "Loading the leaderboard..."), 20f, UiTheme.Ink, new Vector2(0.06f, 0.845f), new Vector2(0.94f, 0.895f), TextAlignmentOptions.Left);
            Head(parent);

            var content = List(parent, out var scroll);
            var mine = Mine(parent, out var rank, out var name, out var score, out var country, out var place);
            var note = Cell(parent, string.Empty, 18f, UiTheme.Muted, new Vector2(0.08f, 0.012f), new Vector2(0.92f, 0.055f), TextAlignmentOptions.Left);

            var page = parent.gameObject.AddComponent<LeaderboardPage>();
            page.Open(status, content, scroll, rank, name, score, country, place, note, mine);
            searchButton.onClick.AddListener(() =>
            {
                searchInput.DeactivateInputField();
                page.Search(searchInput.text);
            });
            searchInput.onSubmit.AddListener(page.Search);
        }

        static void Search(Transform parent, out TMP_InputField input, out Button button)
        {
            var field = UiFactory.Image(parent, "SearchField", new Color(0.04f, 0.10f, 0.18f, 0.96f), false, new Vector2(0.61f, 0.915f), new Vector2(0.86f, 0.975f));
            field.sprite = UiFactory.Rounded;
            field.type = Image.Type.Sliced;
            var viewport = UiFactory.Rect(field.transform, "TextArea", new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f));
            viewport.gameObject.AddComponent<RectMask2D>();
            var placeholderRect = UiFactory.Rect(viewport, "Placeholder", Vector2.zero, Vector2.one);
            var placeholder = placeholderRect.gameObject.AddComponent<TextMeshProUGUI>();
            placeholder.font = UiFactory.Font;
            placeholder.text = Locale.T("Oyuncu adı", "Player name");
            placeholder.fontSize = 18f;
            placeholder.color = new Color(0.78f, 0.86f, 0.94f, 0.55f);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.raycastTarget = false;
            var textRect = UiFactory.Rect(viewport, "Text", Vector2.zero, Vector2.one);
            var text = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = UiFactory.Font;
            text.fontSize = 18f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            input = field.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = viewport;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.fontAsset = UiFactory.Font;
            input.pointSize = 18f;
            input.characterLimit = PlayerData.MaxNameLength;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.caretColor = Color.white;
            input.customCaretColor = true;
            button = UiFactory.Button(parent, "Search", Locale.T("Ara", "Search"), UiTheme.Green, new Vector2(0.87f, 0.915f), new Vector2(0.98f, 0.975f), null, 18f);
        }

        static void Head(Transform parent)
        {
            var bar = UiFactory.Image(parent, "Columns", new Color(0.08f, 0.13f, 0.22f, 0.72f), false, new Vector2(0.05f, 0.785f), new Vector2(0.95f, 0.84f));
            bar.raycastTarget = false;
            Cell(bar.transform, Locale.T("Sıra", "Rank"), 20f, UiTheme.Yellow, new Vector2(0.02f, 0f), new Vector2(0.14f, 1f), TextAlignmentOptions.Center);
            Cell(bar.transform, Locale.T("İsim", "Name"), 20f, UiTheme.Yellow, new Vector2(0.16f, 0f), new Vector2(0.52f, 1f), TextAlignmentOptions.Left);
            Cell(bar.transform, Locale.T("Puan", "Score"), 20f, UiTheme.Yellow, new Vector2(0.54f, 0f), new Vector2(0.74f, 1f), TextAlignmentOptions.Center);
            Cell(bar.transform, Locale.T("Ülke", "Country"), 20f, UiTheme.Yellow, new Vector2(0.76f, 0f), new Vector2(0.98f, 1f), TextAlignmentOptions.Center);
        }

        static RectTransform Mine(Transform parent, out TMP_Text rank, out TMP_Text name, out TMP_Text score, out TMP_Text country, out TMP_Text place)
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
            country = Cell(card.transform, "—", 22f, Color.white, new Vector2(0.76f, 0.08f), new Vector2(0.98f, 0.6f), TextAlignmentOptions.Center);
            return rim.rectTransform;
        }

        static RectTransform List(Transform parent, out ScrollRect scroll)
        {
            var viewport = UiFactory.Rect(parent, "List", new Vector2(0.05f, 0.25f), new Vector2(0.935f, 0.775f));
            var hit = viewport.gameObject.AddComponent<Image>();
            hit.color = new Color(0.05f, 0.09f, 0.16f, 0.28f);
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
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

            var barImage = UiFactory.Image(parent, "ScrollBar", new Color(0.04f, 0.09f, 0.16f, 0.7f), false, new Vector2(0.943f, 0.25f), new Vector2(0.963f, 0.775f));
            barImage.sprite = UiFactory.Rounded;
            barImage.type = Image.Type.Sliced;
            var handle = UiFactory.Rect(barImage.transform, "Handle", new Vector2(0.1f, 0f), new Vector2(0.9f, 1f));
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.sprite = UiFactory.Rounded;
            handleImage.type = Image.Type.Sliced;
            handleImage.color = new Color(0.98f, 0.74f, 0.22f, 0.96f);
            var scrollbar = barImage.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            return content;
        }

        public static RowView Row(RectTransform content, int poolIndex)
        {
            var row = UiFactory.Rect(content, "VisibleRow" + poolIndex, new Vector2(0f, 1f), new Vector2(1f, 1f));
            row.pivot = new Vector2(0.5f, 1f);
            row.sizeDelta = new Vector2(-16f, RowPitch - 12f);
            var plate = UiFactory.Image(row, "Plate", UiTheme.Panel, false, Vector2.zero, Vector2.one);
            plate.raycastTarget = false;
            return new RowView
            {
                root = row,
                plate = plate,
                rank = Cell(row, string.Empty, 28f, UiTheme.Yellow, new Vector2(0.02f, 0f), new Vector2(0.14f, 1f), TextAlignmentOptions.Center),
                playerName = Cell(row, string.Empty, 26f, Color.white, new Vector2(0.16f, 0f), new Vector2(0.52f, 1f), TextAlignmentOptions.Left),
                score = Cell(row, string.Empty, 26f, Color.white, new Vector2(0.54f, 0f), new Vector2(0.74f, 1f), TextAlignmentOptions.Center),
                country = Cell(row, string.Empty, 20f, Color.white, new Vector2(0.76f, 0f), new Vector2(0.98f, 1f), TextAlignmentOptions.Center)
            };
        }

        public sealed class RowView
        {
            public RectTransform root;
            public Image plate;
            public TMP_Text rank;
            public TMP_Text playerName;
            public TMP_Text score;
            public TMP_Text country;

            public void Bind(int index, LeaderboardEntry entry, bool searched)
            {
                root.gameObject.SetActive(true);
                root.anchoredPosition = new Vector2(0f, -8f - index * RowPitch);
                plate.color = searched ? new Color(0.46f, 0.24f, 0.64f, 0.98f) : RowColor(entry);
                rank.text = entry.rank.ToString();
                rank.color = RankColor(entry.rank);
                playerName.text = string.IsNullOrEmpty(entry.playerName) ? "—" : entry.playerName;
                score.text = entry.score.ToString();
                country.text = PlayerCountry.DisplayName(entry.countryCode);
            }

            public void Hide()
            {
                root.gameObject.SetActive(false);
            }
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
        const int VisibleRowPool = 12;
        TMP_Text status;
        RectTransform content;
        ScrollRect scroll;
        TMP_Text rank;
        TMP_Text playerName;
        TMP_Text score;
        TMP_Text country;
        TMP_Text place;
        TMP_Text note;
        LeaderboardEntry[] entries = new LeaderboardEntry[0];
        readonly List<LeaderboardView.RowView> rowPool = new List<LeaderboardView.RowView>();
        int searchedIndex = -1;
        int firstVisible = -1;
        bool hasLoaded;

        public void Open(TMP_Text statusLabel, RectTransform rows, ScrollRect listScroll, TMP_Text rankLabel, TMP_Text nameLabel, TMP_Text scoreLabel, TMP_Text countryLabel, TMP_Text placeLabel, TMP_Text noteLabel, RectTransform mine)
        {
            status = statusLabel;
            content = rows;
            scroll = listScroll;
            rank = rankLabel;
            playerName = nameLabel;
            score = scoreLabel;
            country = countryLabel;
            place = placeLabel;
            note = noteLabel;
            if (scroll != null)
                scroll.onValueChanged.AddListener(_ => RefreshVisibleRows());
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
            hasLoaded = true;
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
                var count = rows == null ? 0 : rows.Length;
                content.sizeDelta = new Vector2(0f, Mathf.Max(92f, 8f + count * 92f));
                entries = rows ?? new LeaderboardEntry[0];
                searchedIndex = -1;
                firstVisible = -1;
                content.anchoredPosition = Vector2.zero;
                EnsureRowPool();
                RefreshVisibleRows();
            }

            var local = GameBootstrap.Instance.Leaderboard.LocalRow();
            if (rank != null)
                rank.text = local.rank > 0 ? local.rank.ToString() : "—";
            if (playerName != null)
                playerName.text = string.IsNullOrEmpty(local.playerName) ? "—" : local.playerName;
            if (score != null)
                score.text = local.score.ToString();
            if (country != null)
                country.text = PlayerCountry.DisplayName(local.countryCode);
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

        void EnsureRowPool()
        {
            if (content == null)
                return;
            while (rowPool.Count < VisibleRowPool)
                rowPool.Add(LeaderboardView.Row(content, rowPool.Count));
        }

        void RefreshVisibleRows(bool force = false)
        {
            if (content == null || rowPool.Count == 0)
                return;
            var first = Mathf.Max(0, Mathf.FloorToInt(Mathf.Max(0f, content.anchoredPosition.y - 8f) / 92f) - 1);
            if (entries.Length > rowPool.Count)
                first = Mathf.Min(first, entries.Length - rowPool.Count);
            if (!force && first == firstVisible)
                return;
            firstVisible = first;
            for (var i = 0; i < rowPool.Count; i++)
            {
                var entryIndex = first + i;
                if (entryIndex < 0 || entryIndex >= entries.Length || entries[entryIndex] == null)
                    rowPool[i].Hide();
                else
                    rowPool[i].Bind(entryIndex, entries[entryIndex], entryIndex == searchedIndex);
            }
        }

        public void Search(string rawName)
        {
            if (!hasLoaded)
            {
                status.text = Locale.T("Liste yükleniyor, lütfen bekle.", "The list is loading, please wait.");
                return;
            }
            var query = string.IsNullOrWhiteSpace(rawName) ? string.Empty : rawName.Trim();
            if (query.Length == 0)
            {
                status.text = Locale.T("Aramak için oyuncu adı yaz.", "Enter a player name to search.");
                return;
            }

            var found = -1;
            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null && string.Equals(entries[i].playerName, query, StringComparison.OrdinalIgnoreCase))
                {
                    found = i;
                    break;
                }
            }
            if (found < 0)
            {
                for (var i = 0; i < entries.Length; i++)
                {
                    if (entries[i] != null && !string.IsNullOrEmpty(entries[i].playerName) && entries[i].playerName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = i;
                        break;
                    }
                }
            }

            searchedIndex = found;
            if (found < 0)
            {
                status.text = Locale.T("Oyuncu ilk 100 içinde bulunamadı.", "Player was not found in the top 100.");
                RefreshVisibleRows(true);
                return;
            }

            var entry = entries[found];
            status.text = $"#{entry.rank}  {entry.playerName}  •  {entry.score} {Locale.T("puan", "points")}";
            ScrollTo(found);
        }

        void ScrollTo(int index)
        {
            if (content == null || scroll == null || scroll.viewport == null)
                return;
            Canvas.ForceUpdateCanvases();
            var maxY = Mathf.Max(0f, content.rect.height - scroll.viewport.rect.height);
            var targetY = Mathf.Clamp(index * 92f, 0f, maxY);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetY);
            scroll.velocity = Vector2.zero;
            RefreshVisibleRows(true);
        }

        void OnDestroy()
        {
            if (scroll != null)
                scroll.onValueChanged.RemoveAllListeners();
        }
    }
}

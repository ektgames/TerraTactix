using System.Collections.Generic;
using TerraTactix.Core;
using TerraTactix.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace TerraTactix.UI
{
    public static class WorldMapView
    {
        public static void Build(Transform parent)
        {
            var ocean = UiFactory.Image(parent, "Ocean", Color.white, false, Vector2.zero, Vector2.one);
            ocean.sprite = UiFactory.Ocean;
            ocean.type = Image.Type.Simple;
            ocean.raycastTarget = false;
            UiFactory.Button(parent, "Back", Locale.T("Geri", "Back"), UiTheme.Panel, new Vector2(0.02f, 0.915f), new Vector2(0.12f, 0.975f), () =>
            {
                GameBootstrap.Instance.Scenes.Load(SceneNames.MainMenu);
            }, 24f);
            UiFactory.Label(parent, Locale.T("Dünya Haritası", "World Map"), 32f, UiTheme.Ink, new Vector2(0.13f, 0.9f), new Vector2(0.42f, 1f), TextAlignmentOptions.Left);
            HudView.Build(parent);

            var map = UiFactory.Rect(parent, "Map", new Vector2(0.02f, 0.06f), new Vector2(0.68f, 0.88f));
            var mapHit = map.gameObject.AddComponent<Image>();
            mapHit.color = new Color(0f, 0f, 0f, 0f);
            map.gameObject.AddComponent<RectMask2D>();
            var scroll = map.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            scroll.scrollSensitivity = 0f;
            scroll.viewport = map;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(map, false);
            var content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(0f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.sizeDelta = new Vector2(MapNavigator.MapWidth, MapNavigator.MapHeight);
            scroll.content = content;

            var info = UiFactory.Image(parent, "Info", UiTheme.Panel, false, new Vector2(0.70f, 0.06f), new Vector2(0.98f, 0.88f));
            var title = UiFactory.Label(info.transform, string.Empty, 26f, UiTheme.Ink, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.96f));
            var body = UiFactory.Label(info.transform, string.Empty, 20f, UiTheme.Muted, new Vector2(0.06f, 0.34f), new Vector2(0.94f, 0.76f), TextAlignmentOptions.Top);
            var action = UiFactory.Button(info.transform, "Explore", Locale.T("Keşfet", "Explore"), UiTheme.Green, new Vector2(0.1f, 0.16f), new Vector2(0.9f, 0.3f), () => { }, 26f);
            UiFactory.Label(info.transform, Locale.T("Sisli topraklar kapalı. Komşunu fethedince açılır.", "Fogged lands stay closed. They open when you capture a neighbor."), 16f, UiTheme.Muted, new Vector2(0.06f, 0.02f), new Vector2(0.94f, 0.14f));
            var captured = GameBootstrap.Instance.Expeditions.CapturedCount();
            UiFactory.Label(parent, Locale.T($"{captured}/{RegionCatalog.All.Length} bölge fethedildi. Kaydır, tekerlek veya iki parmakla yakınlaştır.", $"{captured}/{RegionCatalog.All.Length} regions captured. Drag, scroll, or pinch to zoom."), 16f, UiTheme.Ink, new Vector2(0.02f, 0.01f), new Vector2(0.68f, 0.055f), TextAlignmentOptions.Left);

            var controller = parent.gameObject.AddComponent<WorldMapController>();
            controller.title = title;
            controller.body = body;
            controller.actionLabel = action.GetComponentInChildren<TMP_Text>();
            action.onClick.AddListener(controller.Explore);

            var navigator = map.gameObject.AddComponent<MapNavigator>();
            navigator.scroll = scroll;
            navigator.content = content;
            navigator.SetZoom(0.72f);

            MapBoard.Build();
            var sea = UiFactory.Image(content, "Sea", Color.white, false, Vector2.zero, Vector2.one);
            sea.sprite = UiFactory.Ocean;
            sea.type = Image.Type.Simple;
            sea.raycastTarget = false;
            var width = MapBoard.Width;
            var height = MapBoard.Height;
            content.sizeDelta = new Vector2(width, height);
            sea.rectTransform.anchorMin = Vector2.zero;
            sea.rectTransform.anchorMax = Vector2.one;
            sea.rectTransform.offsetMin = Vector2.zero;
            sea.rectTransform.offsetMax = Vector2.zero;

            PlaceFogRim(content);
            for (var i = 0; i < RegionCatalog.All.Length; i++)
                PlaceTerritory(content, controller, RegionCatalog.All[i]);
            var labels = Piece(content, "Labels", Vector2.zero, 0f, true);
            labels.anchorMin = Vector2.zero;
            labels.anchorMax = Vector2.one;
            labels.offsetMin = Vector2.zero;
            labels.offsetMax = Vector2.zero;
            labels.SetAsLastSibling();
            for (var i = 0; i < RegionCatalog.All.Length; i++)
                PlaceLabel(labels, RegionCatalog.All[i]);

            var start = RegionCatalog.Get("baslangic");
            var focusId = start != null ? start.id : RegionCatalog.All[0].id;
            var playable = GameBootstrap.Instance.Expeditions.NextPlayable();
            if (playable != null && MapBoard.Clear.Count > 3 && MapBoard.Clear.Contains(playable.id))
                focusId = playable.id;
            navigator.Focus(MapBoard.Normalized(focusId));
            controller.Select(focusId);
            var battles = GameBootstrap.Instance.Battles;
            if (battles.TakeFreshResult(out var last) && last.captured && battles.Session != null)
                controller.Celebrate(battles.Session.regionId);
            var opened = GameBootstrap.Instance.Expeditions.TakeJustUnlocked();
            for (var n = 0; n < opened.Count; n++)
                controller.Sparkle(opened[n]);
        }

        static void PlaceFogRim(Transform content)
        {
            var sprite = ArtLibrary.FogBank != null ? ArtLibrary.FogBank : ArtLibrary.CloudA;
            if (sprite == null)
                sprite = UiFactory.Cloud;
            var spots = new[]
            {
                new Vector2(MapBoard.Width * 0.48f, -30f),
                new Vector2(MapBoard.Width * 0.22f, -MapBoard.Height + 40f),
                new Vector2(MapBoard.Width * 0.72f, -MapBoard.Height + 10f),
                new Vector2(20f, -MapBoard.Height * 0.48f),
                new Vector2(MapBoard.Width - 20f, -MapBoard.Height * 0.38f)
            };
            for (var i = 0; i < spots.Length; i++)
            {
                var cloud = Piece(content, "FogRim", spots[i], 820f, true);
                cloud.sizeDelta = new Vector2(860f, 460f);
                var image = cloud.gameObject.AddComponent<Image>();
                image.sprite = sprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.color = new Color(1f, 1f, 1f, 0.96f);
            }
        }

        static void PlaceTerritory(Transform content, WorldMapController controller, RegionDefinition definition)
        {
            if (!MapBoard.Cells.TryGetValue(definition.id, out var cell))
                return;

            var clear = MapBoard.Clear.Contains(definition.id);
            var edge = MapBoard.Edge.Contains(definition.id);
            if (!clear && !edge)
                return;

            var land = Piece(content, definition.id, MapBoard.Position(cell, definition.id), MapBoard.Tile, true);
            var painted = ArtLibrary.Land(definition.biome);
            var image = land.gameObject.AddComponent<Image>();
            image.sprite = painted != null ? painted : UiFactory.Territory;
            image.preserveAspect = painted != null;
            image.color = painted != null ? Color.white : definition.tint;
            var button = land.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            var id = definition.id;
            button.onClick.AddListener(() => controller.Select(id));
            controller.Register(id, land);
            var ring = Piece(content, id + "-ring", land.anchoredPosition, MapBoard.Tile * 1.07f, true);
            var ringImage = ring.gameObject.AddComponent<Image>();
            ringImage.sprite = image.sprite;
            ringImage.preserveAspect = image.preserveAspect;
            ringImage.color = new Color(1f, 0.86f, 0.12f, 1f);
            ringImage.raycastTarget = false;
            ring.SetSiblingIndex(land.GetSiblingIndex());
            controller.RegisterOutline(id, ring.gameObject);
            if (clear)
                return;

            var mist = Piece(land, "Mist", new Vector2(0f, land.sizeDelta.y * 0.22f), 220f, false);
            mist.sizeDelta = new Vector2(280f, 150f);
            var mistImage = mist.gameObject.AddComponent<Image>();
            mistImage.sprite = ArtLibrary.CloudA != null ? ArtLibrary.CloudA : UiFactory.Cloud;
            mistImage.preserveAspect = true;
            mistImage.color = new Color(1f, 1f, 1f, 0.55f);
            mistImage.raycastTarget = false;
        }

        static void PlaceLabel(RectTransform labels, RegionDefinition definition)
        {
            if (!MapBoard.Cells.TryGetValue(definition.id, out var cell))
                return;
            if (!MapBoard.Clear.Contains(definition.id) && !MapBoard.Edge.Contains(definition.id))
                return;

            var banner = Piece(labels, definition.id + "-label", MapBoard.Position(cell, definition.id) + new Vector2(0f, 36f), 220f, true);
            banner.sizeDelta = new Vector2(230f, 52f);
            var plate = banner.gameObject.AddComponent<Image>();
            plate.sprite = UiFactory.Rounded;
            plate.type = Image.Type.Sliced;
            plate.raycastTarget = false;
            plate.color = new Color(0.05f, 0.09f, 0.18f, 0.94f);
            var label = UiFactory.Label(banner, Locale.Place(definition.id, definition.displayName), 24f, Color.white, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));
            label.font = UiFactory.Cute;
            label.fontStyle = FontStyles.Bold;
            label.outlineWidth = 0.22f;
            label.outlineColor = new Color32(0, 0, 0, 220);
        }

        static RectTransform Piece(Transform parent, string name, Vector2 position, float size, bool onMap)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            var anchor = onMap ? new Vector2(0f, 1f) : new Vector2(0.5f, 0.5f);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = position;
            return rect;
        }

        public static string StateLabel(RegionState state)
        {
            switch (state)
            {
                case RegionState.Captured: return Locale.T("Fethedildi", "Captured");
                case RegionState.Available: return Locale.T("Mevcut", "Open");
                case RegionState.Attacking: return Locale.T("Saldırı", "Attack");
                case RegionState.PartiallyCaptured: return Locale.T("Kısmi", "Partial");
                default: return Locale.T("Kilitli", "Locked");
            }
        }
    }

    public sealed class WorldMapController : MonoBehaviour
    {
        public TMP_Text title;
        public TMP_Text body;
        public TMP_Text actionLabel;
        string selectedId;
        readonly Dictionary<string, RectTransform> islands = new Dictionary<string, RectTransform>();
        readonly Dictionary<string, GameObject> outlines = new Dictionary<string, GameObject>();

        public void Register(string regionId, RectTransform island)
        {
            islands[regionId] = island;
        }

        public void RegisterOutline(string regionId, GameObject outline)
        {
            outlines[regionId] = outline;
            outline.SetActive(false);
        }

        public void Select(string regionId)
        {
            selectedId = regionId;
            foreach (var pair in outlines)
                pair.Value.SetActive(pair.Key == regionId);
            var definition = RegionCatalog.Get(regionId);
            var progress = GameBootstrap.Instance.Expeditions.GetProgress(regionId);
            if (definition == null || progress == null)
                return;

            title.text = Locale.Place(definition.id, definition.displayName);
            if (MapBoard.Edge != null && MapBoard.Edge.Contains(regionId))
            {
                body.text = Locale.T("Bu toprak sisin altında.\nKomşu bölge fethedilince görünür olur.", "This land is under the fog.\nIt appears when a neighbor is captured.");
                actionLabel.text = Locale.T("Sis", "Fog");
                return;
            }

            body.text = $"{Locale.Biome(definition.biome)}\n{Locale.T("Sefer", "Expedition")} {progress.campaignProgress}/{definition.requiredCampaigns}\n{Locale.T("Durum", "Status")}: {WorldMapView.StateLabel(progress.state)}\n{Locale.T("Keşfet 1 can harcar.", "Explore spends 1 heart.")}";
            var canAttack = progress.state == RegionState.Available || progress.state == RegionState.PartiallyCaptured || progress.state == RegionState.Attacking;
            actionLabel.text = canAttack ? Locale.T("Keşfet", "Explore") : WorldMapView.StateLabel(progress.state);
        }

        public void Explore()
        {
            var definition = RegionCatalog.Get(selectedId);
            var progress = GameBootstrap.Instance.Expeditions.GetProgress(selectedId);
            if (definition == null || progress == null)
                return;
            if (MapBoard.Edge != null && MapBoard.Edge.Contains(selectedId))
            {
                UiFactory.Dialog(transform, Locale.Place(definition.id, definition.displayName), Locale.T("Burası sisin altında. Önce bitişik toprağı fethet.", "This place is under the fog. Capture a neighboring land first."), null);
                return;
            }

            if (progress.state != RegionState.Available && progress.state != RegionState.PartiallyCaptured && progress.state != RegionState.Attacking)
            {
                UiFactory.Dialog(transform, Locale.Place(definition.id, definition.displayName), Locale.T("Bu bölge şu an saldırıya açık değil.", "This region is not open to attack right now."), null);
                return;
            }

            if (!GameBootstrap.Instance.Economy.CanPlay())
            {
                UiFactory.Dialog(transform, Locale.T("Can yok", "No hearts"), Locale.T("Savaşa girmek 1 can ister. Mağazadan veya günlük çarktan can al.", "Entering battle costs 1 heart. Get hearts from the shop or the daily wheel."), null);
                return;
            }

            GameBootstrap.Instance.Battles.Prepare(definition);
            GameBootstrap.Instance.Scenes.Load(SceneNames.Battle);
        }

        public void Celebrate(string regionId)
        {
            if (!islands.TryGetValue(regionId, out var island))
                return;
            var pulse = island.gameObject.AddComponent<IslandPulse>();
            pulse.RestScale = 1f;
        }

        public void Sparkle(string regionId)
        {
            if (!islands.TryGetValue(regionId, out var island))
                return;
            var go = new GameObject("Sparkle", typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(island.parent, false);
            rect.anchorMin = island.anchorMin;
            rect.anchorMax = island.anchorMax;
            rect.pivot = island.pivot;
            rect.sizeDelta = island.sizeDelta * 0.42f;
            rect.anchoredPosition = island.anchoredPosition + new Vector2(0f, island.sizeDelta.y * 0.42f);
            rect.SetSiblingIndex(island.GetSiblingIndex() + 1);
            var image = go.AddComponent<Image>();
            image.sprite = UiFactory.Knob;
            image.color = new Color(1f, 0.95f, 0.55f, 0.95f);
            image.raycastTarget = false;
            go.AddComponent<UnlockSparkle>();
        }
    }

    public sealed class IslandPulse : MonoBehaviour
    {
        public float RestScale = 1f;
        float time;

        void Update()
        {
            time += Time.unscaledDeltaTime;
            var wave = RestScale + 0.14f * Mathf.Sin(time * 9f);
            transform.localScale = new Vector3(wave, wave, 1f);
            if (time < 1.5f)
                return;
            transform.localScale = new Vector3(RestScale, RestScale, 1f);
            Destroy(this);
        }
    }

    public sealed class UnlockSparkle : MonoBehaviour
    {
        float time;
        Image image;

        void Awake()
        {
            image = GetComponent<Image>();
        }

        void Update()
        {
            time += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(time / 1.1f);
            transform.localScale = Vector3.one * (1f + t * 1.4f);
            if (image != null)
            {
                var color = image.color;
                color.a = 1f - t;
                image.color = color;
            }

            if (t >= 1f)
                Destroy(gameObject);
        }
    }

    static class MapBoard
    {
        public const float Tile = 520f;
        const float StepX = 430f;
        const float StepY = 340f;
        const float Pad = 460f;
        static Vector2 shift;

        public static Dictionary<string, Vector2Int> Cells;
        public static HashSet<string> Clear;
        public static HashSet<string> Edge;
        public static float Width;
        public static float Height;

        public static void Build()
        {
            Cells = PlaceCells();
            Clear = new HashSet<string>();
            Edge = new HashSet<string>();
            for (var i = 0; i < RegionCatalog.All.Length; i++)
            {
                var id = RegionCatalog.All[i].id;
                var progress = GameBootstrap.Instance.Expeditions.GetProgress(id);
                var state = progress == null ? RegionState.Locked : progress.state;
                if (state == RegionState.Captured || state == RegionState.Available || state == RegionState.PartiallyCaptured || state == RegionState.Attacking)
                    Clear.Add(id);
            }

            for (var i = 0; i < RegionCatalog.All.Length; i++)
            {
                var id = RegionCatalog.All[i].id;
                if (!Clear.Contains(id))
                    Edge.Add(id);
            }

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            var any = false;
            for (var i = 0; i < RegionCatalog.All.Length; i++)
            {
                var id = RegionCatalog.All[i].id;
                if (!Clear.Contains(id) && !Edge.Contains(id))
                    continue;
                if (!Cells.TryGetValue(id, out var cell))
                    continue;
                var raw = Raw(cell, id);
                min = Vector2.Min(min, raw);
                max = Vector2.Max(max, raw);
                any = true;
            }

            if (!any)
            {
                shift = Vector2.zero;
                Width = 1800f;
                Height = 1400f;
                return;
            }

            shift = new Vector2(min.x - Pad, max.y + Pad);
            Width = (max.x - min.x) + Tile + Pad * 2f;
            Height = (max.y - min.y) + Tile + Pad * 2f;
        }

        public static Vector2 Position(Vector2Int cell, string id)
        {
            var raw = Raw(cell, id);
            return new Vector2(raw.x - shift.x, raw.y - shift.y);
        }

        static Vector2 Raw(Vector2Int cell, string id)
        {
            var hash = 17;
            for (var i = 0; i < id.Length; i++)
                hash = hash * 31 + id[i];
            hash &= 0x7fffffff;
            var stagger = (cell.y & 1) == 1 ? StepX * 0.46f : 0f;
            var wobbleX = (hash % 80) - 40f;
            var wobbleY = ((hash / 80) % 60) - 30f;
            return new Vector2(cell.x * StepX + stagger + wobbleX, -(cell.y * StepY) + wobbleY);
        }

        public static Vector2 Normalized(string id)
        {
            if (Cells == null || !Cells.TryGetValue(id, out var cell) || Width <= 1f || Height <= 1f)
                return new Vector2(0.2f, 0.8f);
            var pos = Position(cell, id);
            return new Vector2(Mathf.Clamp01(pos.x / Width), Mathf.Clamp01(1f + pos.y / Height));
        }

        static Dictionary<string, Vector2Int> PlaceCells()
        {
            var links = new Dictionary<string, List<string>>();
            for (var i = 0; i < RegionCatalog.All.Length; i++)
            {
                var id = RegionCatalog.All[i].id;
                if (!links.ContainsKey(id))
                    links[id] = new List<string>();
                var next = RegionCatalog.All[i].nextRegionIds;
                if (next == null)
                    continue;
                for (var n = 0; n < next.Length; n++)
                {
                    var other = next[n];
                    if (string.IsNullOrEmpty(other) || RegionCatalog.Get(other) == null)
                        continue;
                    if (!links.ContainsKey(other))
                        links[other] = new List<string>();
                    if (!links[id].Contains(other))
                        links[id].Add(other);
                    if (!links[other].Contains(id))
                        links[other].Add(id);
                }
            }

            var cells = new Dictionary<string, Vector2Int>();
            var used = new HashSet<Vector2Int>();
            var start = RegionCatalog.Get("baslangic");
            var startId = start != null ? start.id : RegionCatalog.All[0].id;
            cells[startId] = Vector2Int.zero;
            used.Add(Vector2Int.zero);
            var queue = new Queue<string>();
            queue.Enqueue(startId);
            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (!links.TryGetValue(id, out var neighbors))
                    continue;
                var at = cells[id];
                for (var i = 0; i < neighbors.Count; i++)
                {
                    var next = neighbors[i];
                    if (cells.ContainsKey(next))
                        continue;
                    var spot = NearestFree(at, used);
                    cells[next] = spot;
                    used.Add(spot);
                    queue.Enqueue(next);
                }
            }

            for (var i = 0; i < RegionCatalog.All.Length; i++)
            {
                var id = RegionCatalog.All[i].id;
                if (cells.ContainsKey(id))
                    continue;
                var spot = NearestFree(Vector2Int.zero, used);
                cells[id] = spot;
                used.Add(spot);
            }

            return cells;
        }

        static Vector2Int NearestFree(Vector2Int at, HashSet<Vector2Int> used)
        {
            for (var radius = 1; radius <= 16; radius++)
            {
                var best = new Vector2Int(-1, -1);
                var bestScore = int.MaxValue;
                for (var y = -radius; y <= radius; y++)
                {
                    for (var x = -radius; x <= radius; x++)
                    {
                        if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != radius)
                            continue;
                        var cell = new Vector2Int(at.x + x, at.y + y);
                        if (cell.x < 0 || cell.y < 0 || used.Contains(cell))
                            continue;
                        var score = cell.x + cell.y * 2 + radius;
                        if (score >= bestScore)
                            continue;
                        bestScore = score;
                        best = cell;
                    }
                }

                if (best.x >= 0)
                    return best;
            }

            var scan = Vector2Int.zero;
            while (used.Contains(scan))
                scan.x++;
            return scan;
        }
    }

    public sealed class MapNavigator : MonoBehaviour
    {
        public const float MapWidth = 4800f;
        public const float MapHeight = 5200f;

        public ScrollRect scroll;
        public RectTransform content;
        float zoom = 1f;
        float pinchDistance;

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        void Update()
        {
            var touches = Touch.activeTouches;
            if (touches.Count >= 2)
            {
                scroll.enabled = false;
                var distance = Vector2.Distance(touches[0].screenPosition, touches[1].screenPosition);
                if (pinchDistance > 1f)
                    SetZoom(zoom * (distance / pinchDistance));
                pinchDistance = distance;
                return;
            }

            pinchDistance = 0f;
            if (!scroll.enabled)
                scroll.enabled = true;

            var mouse = Mouse.current;
            if (mouse == null)
                return;
            var wheel = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(wheel) > 0.01f)
                SetZoom(zoom * (wheel > 0f ? 1.08f : 0.92f));
        }

        public void Focus(Vector2 anchor)
        {
            Canvas.ForceUpdateCanvases();
            scroll.horizontalNormalizedPosition = Mathf.Clamp01(anchor.x);
            scroll.verticalNormalizedPosition = Mathf.Clamp01(anchor.y);
        }

        public void SetZoom(float value)
        {
            zoom = Mathf.Clamp(value, 0.28f, 1.6f);
            if (content != null)
                content.localScale = new Vector3(zoom, zoom, 1f);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace TerraTactix.Data
{
    public sealed class RegionDefinition
    {
        public string id;
        public string displayName;
        public string biome;
        public int requiredCampaigns;
        public RegionState startingState;
        public string[] nextRegionIds;
        public Vector2 mapAnchor;
        public Color tint;
        public int routeDepth;
    }

    public static class RegionCatalog
    {
        const int Columns = 10;
        const int Rows = 10;

        static readonly string[] NamePrefix =
        {
            "Pırıltı", "Yumuşak", "Sessiz", "Minik", "Parlak", "Tatlı", "Uçuk", "Serin", "Ilık", "Neşeli",
            "Puslu", "Berrak", "Kıvrık", "Turuncu", "Mavi", "İnce", "Yuvarlak", "Gizli", "Hafif", "Şeker"
        };

        static readonly string[] NamePlace =
        {
            "Koy", "Tepe", "Ada", "Vadi", "Körfez"
        };

        public static readonly RegionDefinition[] All = Build();

        static RegionDefinition[] Build()
        {
            var grid = new RegionDefinition[Columns, Rows];
            var links = new Dictionary<string, List<string>>();
            var starters = new[]
            {
                Place(grid, links, "baslangic", "Başlangıç Adası", "Çayır", 20, RegionState.Available, new[] { "yesil" }, 1, 3, 0, new Color(0.45f, 0.78f, 0.38f)),
                Place(grid, links, "yesil", "Yeşil Kıyılar", "Orman", 20, RegionState.Available, new[] { "kuzey", "col" }, 3, 5, 1, new Color(0.25f, 0.62f, 0.32f)),
                Place(grid, links, "kuzey", "Kuzey Ormanları", "Orman", 20, RegionState.Locked, new[] { "kutup" }, 5, 7, 2, new Color(0.20f, 0.55f, 0.34f)),
                Place(grid, links, "col", "Çöl Kıyısı", "Çöl", 20, RegionState.Locked, new[] { "lav" }, 6, 3, 2, new Color(0.86f, 0.72f, 0.32f)),
                Place(grid, links, "kutup", "Kutup Uçları", "Buz", 20, RegionState.Locked, new[] { "gizem" }, 2, 7, 3, new Color(0.75f, 0.90f, 0.95f)),
                Place(grid, links, "lav", "Lav Diyarı", "Volkan", 20, RegionState.Locked, new[] { "kristal" }, 7, 6, 3, new Color(0.85f, 0.35f, 0.22f)),
                Place(grid, links, "kristal", "Kristal Alanı", "Kristal", 20, RegionState.Locked, new string[0], 4, 2, 4, new Color(0.62f, 0.40f, 0.92f)),
                Place(grid, links, "gizem", "Gizemli Ada", "Gizem", 20, RegionState.Locked, new string[0], 7, 1, 4, new Color(0.45f, 0.55f, 0.72f))
            };

            var order = new List<RegionDefinition>(Columns * Rows);
            order.AddRange(starters);
            var queue = new Queue<RegionDefinition>();
            for (var i = 0; i < starters.Length; i++)
                queue.Enqueue(starters[i]);

            var serial = 0;
            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();
                var cell = CellOf(parent.mapAnchor);
                var budget = IsStarter(parent.id) ? 1 : 99;
                var added = 0;
                for (var dir = 0; dir < 4 && added < budget; dir++)
                {
                    var next = Neighbor(cell, dir);
                    if (next.x < 0 || next.y < 0 || next.x >= Columns || next.y >= Rows || grid[next.x, next.y] != null)
                        continue;

                    var biome = BiomeFor(next.x, next.y);
                    var depth = parent.routeDepth + 1;
                    var child = Place(grid, links, "k" + next.x + "x" + next.y, GeneratedName(serial), biome, RequiredFor(depth), RegionState.Locked, new string[0], next.x, next.y, depth, TintFor(biome));
                    links[parent.id].Add(child.id);
                    order.Add(child);
                    queue.Enqueue(child);
                    serial++;
                    added++;
                }
            }

            for (var i = 0; i < order.Count; i++)
                order[i].nextRegionIds = links[order[i].id].ToArray();

            return order.ToArray();
        }

        static bool IsStarter(string id)
        {
            return id == "baslangic" || id == "yesil" || id == "kuzey" || id == "col" || id == "kutup" || id == "lav" || id == "kristal" || id == "gizem";
        }

        static int RequiredFor(int depth)
        {
            return 20;
        }

        static string GeneratedName(int serial)
        {
            return NamePrefix[serial % NamePrefix.Length] + " " + NamePlace[serial / NamePrefix.Length];
        }

        static string BiomeFor(int col, int row)
        {
            if (row >= 8)
                return "Buz";
            if (col >= 8)
                return "Volkan";
            if (col <= 1)
                return "Çayır";
            if (row <= 1)
                return "Kristal";
            if (col >= 6 && row <= 4)
                return "Çöl";
            if (row >= 6)
                return "Orman";
            if ((col + row) % 6 == 0)
                return "Zehir";
            return "Ada";
        }

        static Color TintFor(string biome)
        {
            switch (biome)
            {
                case "Çayır": return new Color(0.45f, 0.78f, 0.38f);
                case "Orman": return new Color(0.22f, 0.58f, 0.32f);
                case "Çöl": return new Color(0.86f, 0.72f, 0.32f);
                case "Buz": return new Color(0.75f, 0.90f, 0.95f);
                case "Volkan": return new Color(0.85f, 0.35f, 0.22f);
                case "Kristal": return new Color(0.62f, 0.40f, 0.92f);
                case "Zehir": return new Color(0.55f, 0.78f, 0.28f);
                default: return new Color(0.35f, 0.72f, 0.78f);
            }
        }

        static RegionDefinition Place(RegionDefinition[,] grid, Dictionary<string, List<string>> links, string id, string displayName, string biome, int required, RegionState state, string[] next, int col, int row, int depth, Color tint)
        {
            var definition = Region(id, displayName, biome, required, state, next, CellAnchor(col, row), tint);
            definition.routeDepth = depth;
            grid[col, row] = definition;
            links[id] = new List<string>(next);
            return definition;
        }

        static Vector2Int CellOf(Vector2 anchor)
        {
            return new Vector2Int(Mathf.Clamp(Mathf.FloorToInt(anchor.x * Columns), 0, Columns - 1), Mathf.Clamp(Mathf.FloorToInt(anchor.y * Rows), 0, Rows - 1));
        }

        static Vector2Int Neighbor(Vector2Int cell, int dir)
        {
            switch (dir)
            {
                case 0: return new Vector2Int(cell.x - 1, cell.y);
                case 1: return new Vector2Int(cell.x, cell.y - 1);
                case 2: return new Vector2Int(cell.x, cell.y + 1);
                default: return new Vector2Int(cell.x + 1, cell.y);
            }
        }

        static Vector2 CellAnchor(int col, int row)
        {
            return new Vector2((col + 0.5f) / Columns, (row + 0.5f) / Rows);
        }

        public static RegionDefinition Get(string id)
        {
            for (var i = 0; i < All.Length; i++)
            {
                if (All[i].id == id)
                    return All[i];
            }

            return null;
        }

        static RegionDefinition Region(string id, string displayName, string biome, int required, RegionState state, string[] next, Vector2 anchor, Color tint)
        {
            return new RegionDefinition
            {
                id = id,
                displayName = displayName,
                biome = biome,
                requiredCampaigns = required,
                startingState = state,
                nextRegionIds = next,
                mapAnchor = anchor,
                tint = tint
            };
        }

        public static List<RegionProgressEntry> CreateInitialProgress()
        {
            var list = new List<RegionProgressEntry>(All.Length);
            for (var i = 0; i < All.Length; i++)
            {
                var def = All[i];
                list.Add(new RegionProgressEntry
                {
                    regionId = def.id,
                    state = def.startingState,
                    campaignProgress = def.startingState == RegionState.Captured ? def.requiredCampaigns : 0
                });
            }

            return list;
        }
    }
}

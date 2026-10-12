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
        public float difficultyRating;
    }

    public static class RegionCatalog
    {
        const int Columns = 10;
        const int Rows = 10;

        static readonly string[] FictionalRoots =
        {
            "Avelin", "Belora", "Cerya", "Doreli", "Elvara", "Fenya", "Gavera", "Heluna", "Ilyora", "Javera",
            "Kirela", "Lumera", "Mireva", "Noreli", "Oryana", "Pelira", "Ravena", "Selya", "Tavira", "Umera",
            "Velya", "Yunera", "Zorela", "Arvila", "Esmira"
        };

        static readonly Vector2Int[] LegacySpecialCells =
        {
            new Vector2Int(1, 3), new Vector2Int(3, 5), new Vector2Int(5, 7), new Vector2Int(6, 3),
            new Vector2Int(2, 7), new Vector2Int(7, 6), new Vector2Int(4, 2), new Vector2Int(7, 1)
        };

        public static readonly RegionDefinition[] All = Build();

        static RegionDefinition[] Build()
        {
            var ids = LegacyIds();
            var usedIds = new HashSet<string>();
            var pinnedIds = new HashSet<string> { "baslangic", "yesil", "kuzey", "col", "kutup", "lav", "kristal", "gizem" };
            var biomeCounts = new Dictionary<string, int>();
            var grid = new RegionDefinition[Columns, Rows];
            var cursor = 0;

            for (var y = 0; y < Rows; y++)
            {
                for (var x = 0; x < Columns; x++)
                {
                    var id = PinnedId(x, y);
                    if (string.IsNullOrEmpty(id))
                    {
                        while (cursor < ids.Count && (usedIds.Contains(ids[cursor]) || pinnedIds.Contains(ids[cursor])))
                            cursor++;
                        id = ids[cursor++];
                    }

                    usedIds.Add(id);
                    var biome = BiomeFor(x, y);
                    biomeCounts.TryGetValue(biome, out var biomeIndex);
                    biomeCounts[biome] = biomeIndex + 1;
                    var rating = DifficultyFor(biome, x, y);
                    grid[x, y] = new RegionDefinition
                    {
                        id = id,
                        displayName = id == "baslangic" ? "Lunarya Adası" : NameFor(biome, biomeIndex),
                        biome = biome,
                        requiredCampaigns = 20,
                        startingState = id == "baslangic" ? RegionState.Available : RegionState.Locked,
                        mapAnchor = CellAnchor(x, y),
                        tint = TintFor(biome),
                        routeDepth = Mathf.RoundToInt(rating),
                        difficultyRating = rating
                    };
                }
            }

            var result = new List<RegionDefinition>(Columns * Rows);
            for (var y = 0; y < Rows; y++)
            {
                for (var x = 0; x < Columns; x++)
                {
                    var links = new List<string>(8);
                    for (var offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        for (var offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            if (offsetX != 0 || offsetY != 0)
                                AddLink(grid, links, x + offsetX, y + offsetY);
                        }
                    }
                    grid[x, y].nextRegionIds = links.ToArray();
                    result.Add(grid[x, y]);
                }
            }

            return result.ToArray();
        }

        static List<string> LegacyIds()
        {
            var ids = new List<string>(Columns * Rows)
            {
                "baslangic", "yesil", "kuzey", "col", "kutup", "lav", "kristal", "gizem"
            };
            for (var y = 0; y < Rows; y++)
            {
                for (var x = 0; x < Columns; x++)
                {
                    var oldCell = new Vector2Int(x, y);
                    var special = false;
                    for (var i = 0; i < LegacySpecialCells.Length; i++)
                    {
                        if (LegacySpecialCells[i] == oldCell)
                        {
                            special = true;
                            break;
                        }
                    }
                    if (!special)
                        ids.Add("k" + x + "x" + y);
                }
            }
            return ids;
        }

        static string PinnedId(int x, int y)
        {
            if (x == 0 && y == 9) return "baslangic";
            if (x == 1 && y == 9) return "yesil";
            if (x == 0 && y == 0) return "kutup";
            if (x == 1 && y == 0) return "kuzey";
            if (x == 9 && y == 9) return "lav";
            if (x == 5 && y == 8) return "kristal";
            if (x == 6 && y == 3) return "gizem";
            if (x == 8 && y == 3) return "col";
            return null;
        }

        static void AddLink(RegionDefinition[,] grid, List<string> links, int x, int y)
        {
            if (x >= 0 && x < Columns && y >= 0 && y < Rows)
                links.Add(grid[x, y].id);
        }

        static string BiomeFor(int x, int y)
        {
            if (x == 9)
                return "Volkan";
            if (x <= 3 && y <= 2)
                return "Buz";
            if (x <= 2 && y >= 6)
                return (x + y) % 3 == 0 ? "Orman" : "Çayır";
            if (x <= 4 && y >= 6)
                return "Koy";
            if (x >= 5 && x <= 7 && y >= 5)
                return "Kristal";
            if (x >= 7 && y <= 4)
                return "Çöl";
            if (x <= 3)
                return "Orman";
            if (y <= 3)
                return "Gizem";
            return (x + y) % 2 == 0 ? "Zehir" : "Ada";
        }

        static float DifficultyFor(string biome, int x, int y)
        {
            float rating;
            switch (biome)
            {
                case "Çayır":
                case "Orman": rating = 1f + (x + (9 - y)) * 0.22f; break;
                case "Koy": rating = 2.6f + (x - 3) * 0.45f + (9 - y) * 0.18f; break;
                case "Buz": rating = 4f + x * 0.22f + y * 0.18f; break;
                case "Kristal": rating = 7.1f + (x - 5) * 0.45f + (9 - y) * 0.12f; break;
                case "Volkan": rating = 9.1f + y * 0.1f; break;
                case "Çöl": rating = 6f + (x - 7) * 0.38f + y * 0.18f; break;
                case "Gizem":
                case "Zehir": rating = 6.2f + x * 0.18f + y * 0.12f; break;
                default: rating = 4.4f + x * 0.24f + (9 - y) * 0.1f; break;
            }
            return Mathf.Round(Mathf.Clamp(rating, 1f, 10f) * 10f) / 10f;
        }

        static string NameFor(string biome, int index)
        {
            var root = FictionalRoots[index % FictionalRoots.Length];
            switch (biome)
            {
                case "Çayır": return root + new[] { " Çayırları", " Yoncalığı", " Bahçesi", " Ovası" }[index % 4];
                case "Orman": return root + new[] { " Korusu", " Ormanı", " Filizliği", " Ağaçlığı" }[index % 4];
                case "Koy": return root + new[] { " Koyu", " Körfezi", " Lagünü", " Sahili" }[index % 4];
                case "Buz": return root + new[] { " Buzluğu", " Ayazı", " Kartepesi", " Don Denizi" }[index % 4];
                case "Kristal": return root + new[] { " Kristalleri", " Prizma Adası", " Işıktaşı", " Parıltı Ovası" }[index % 4];
                case "Volkan": return root + new[] { " Yanardağı", " Lav Çanağı", " Köz Adası", " Ateş Yarığı" }[index % 4];
                case "Çöl": return root + new[] { " Kumluğu", " Güneş Çölü", " Kum Denizi", " Sıcaklığı" }[index % 4];
                case "Zehir": return root + new[] { " Sisliği", " İksir Bataklığı", " Yeşil Dumanı", " Yankısı" }[index % 4];
                case "Gizem": return root + new[] { " Rüyası", " Gizli Adası", " Fısıltısı", " Bilmece Vadisi" }[index % 4];
                default: return root + new[] { " Adası", " Tepesi", " Vadisi", " Kıyısı" }[index % 4];
            }
        }

        static Vector2 CellAnchor(int col, int row)
        {
            return new Vector2((col + 0.5f) / Columns, (row + 0.5f) / Rows);
        }

        static Color TintFor(string biome)
        {
            switch (biome)
            {
                case "Çayır": return new Color(0.45f, 0.78f, 0.38f);
                case "Orman": return new Color(0.22f, 0.58f, 0.32f);
                case "Koy": return new Color(0.32f, 0.76f, 0.82f);
                case "Çöl": return new Color(0.86f, 0.72f, 0.32f);
                case "Buz": return new Color(0.75f, 0.90f, 0.95f);
                case "Volkan": return new Color(0.85f, 0.35f, 0.22f);
                case "Kristal": return new Color(0.62f, 0.40f, 0.92f);
                case "Zehir": return new Color(0.55f, 0.78f, 0.28f);
                case "Gizem": return new Color(0.45f, 0.55f, 0.72f);
                default: return new Color(0.35f, 0.72f, 0.78f);
            }
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

        public static List<RegionProgressEntry> CreateInitialProgress()
        {
            var list = new List<RegionProgressEntry>(All.Length);
            for (var i = 0; i < All.Length; i++)
            {
                var definition = All[i];
                list.Add(new RegionProgressEntry
                {
                    regionId = definition.id,
                    state = definition.startingState,
                    campaignProgress = 0
                });
            }
            return list;
        }
    }
}

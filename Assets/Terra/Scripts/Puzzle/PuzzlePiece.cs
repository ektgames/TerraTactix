using UnityEngine;

namespace TerraTactix.Puzzle
{
    public struct PuzzleCell
    {
        public bool occupied;
        public int level;
        public int typeId;
        public int special;
    }

    public sealed class PuzzlePiece
    {
        public int typeId;
        public int level;
        public Vector2Int[] cells;
        public SpecialKind special;

        public PuzzlePiece(int typeId, int level, Vector2Int[] cells, SpecialKind special = SpecialKind.None)
        {
            this.typeId = typeId;
            this.level = level;
            this.cells = cells;
            this.special = special;
        }

        public PuzzlePiece Rotated()
        {
            var next = new Vector2Int[cells.Length];
            var min = new Vector2Int(int.MaxValue, int.MaxValue);
            for (var i = 0; i < cells.Length; i++)
            {
                next[i] = new Vector2Int(cells[i].y, -cells[i].x);
                if (next[i].x < min.x)
                    min.x = next[i].x;
                if (next[i].y < min.y)
                    min.y = next[i].y;
            }

            for (var i = 0; i < next.Length; i++)
                next[i] -= min;

            return new PuzzlePiece(typeId, level, next, special);
        }
    }

    public static class PieceCatalog
    {
        public static readonly Vector2Int[][] Shapes =
        {
            new[] { new Vector2Int(0, 0) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new[] { new Vector2Int(0, 0), new Vector2Int(0, 1) },
            new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2) },
            new[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(0, 2) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) }
        };

        public static readonly Color[] Colors =
        {
            new Color(0.25f, 0.78f, 0.95f),
            new Color(0.98f, 0.82f, 0.25f),
            new Color(0.64f, 0.42f, 0.96f),
            new Color(0.35f, 0.80f, 0.45f),
            new Color(0.92f, 0.36f, 0.38f),
            new Color(0.30f, 0.48f, 0.95f)
        };

        public static Color ColorFor(int typeId, int level)
        {
            var color = Colors[Mathf.Abs(typeId) % Colors.Length];
            var lift = Mathf.Clamp01((level - 1) * 0.16f);
            return Color.Lerp(color, Color.white, lift);
        }
    }
}

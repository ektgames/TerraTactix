using UnityEngine;

namespace TerraTactix.Puzzle
{
    public sealed class PuzzleGrid
    {
        readonly PuzzleCell[,] cells;

        public int Width { get; }
        public int Height { get; }

        public PuzzleGrid(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new PuzzleCell[width, height];
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public PuzzleCell Get(int x, int y)
        {
            return IsInside(x, y) ? cells[x, y] : default;
        }

        public void Set(int x, int y, PuzzleCell cell)
        {
            if (!IsInside(x, y))
                return;
            cells[x, y] = cell;
        }

        public void Clear(int x, int y)
        {
            if (!IsInside(x, y))
                return;
            cells[x, y] = default;
        }

        public bool CanPlace(PuzzlePiece piece, Vector2Int origin)
        {
            var offsets = piece.cells;
            for (var i = 0; i < offsets.Length; i++)
            {
                var x = origin.x + offsets[i].x;
                var y = origin.y + offsets[i].y;
                if (!IsInside(x, y) || cells[x, y].occupied)
                    return false;
            }

            return true;
        }

        public void Place(PuzzlePiece piece, Vector2Int origin)
        {
            var offsets = piece.cells;
            for (var i = 0; i < offsets.Length; i++)
            {
                Set(origin.x + offsets[i].x, origin.y + offsets[i].y, new PuzzleCell
                {
                    occupied = true,
                    level = piece.level,
                    typeId = piece.typeId,
                    special = i == 0 ? (int)piece.special : 0
                });
            }
        }

        public void Collapse()
        {
            for (var x = 0; x < Width; x++)
            {
                var write = 0;
                for (var y = 0; y < Height; y++)
                {
                    if (!cells[x, y].occupied)
                        continue;
                    if (write != y)
                    {
                        cells[x, write] = cells[x, y];
                        cells[x, y] = default;
                    }

                    write++;
                }
            }
        }
    }
}

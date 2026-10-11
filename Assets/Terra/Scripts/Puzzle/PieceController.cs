using UnityEngine;

namespace TerraTactix.Puzzle
{
    public sealed class PieceController
    {
        public PuzzlePiece Piece { get; private set; }
        public Vector2Int Origin { get; private set; }

        public void Set(PuzzlePiece piece, Vector2Int origin)
        {
            Piece = piece;
            Origin = origin;
        }

        public bool TryMove(PuzzleGrid grid, Vector2Int delta)
        {
            var next = Origin + delta;
            if (Piece == null || !grid.CanPlace(Piece, next))
                return false;
            Origin = next;
            return true;
        }

        public bool TryRotate(PuzzleGrid grid)
        {
            if (Piece == null)
                return false;

            var rotated = Piece.Rotated();
            var kicks = new[]
            {
                Vector2Int.zero,
                Vector2Int.right,
                Vector2Int.left,
                new Vector2Int(2, 0),
                new Vector2Int(-2, 0),
                Vector2Int.up
            };

            for (var i = 0; i < kicks.Length; i++)
            {
                var origin = Origin + kicks[i];
                if (!grid.CanPlace(rotated, origin))
                    continue;
                Piece = rotated;
                Origin = origin;
                return true;
            }

            return false;
        }

        public int HardDrop(PuzzleGrid grid)
        {
            var dropped = 0;
            while (TryMove(grid, Vector2Int.down))
                dropped++;
            return dropped;
        }

        public Vector2Int GhostOrigin(PuzzleGrid grid)
        {
            return GhostPieceController.DropOrigin(grid, Piece, Origin);
        }
    }

    public static class GhostPieceController
    {
        public static Vector2Int DropOrigin(PuzzleGrid grid, PuzzlePiece piece, Vector2Int origin)
        {
            if (piece == null)
                return origin;

            var cursor = origin;
            while (grid.CanPlace(piece, cursor + Vector2Int.down))
                cursor += Vector2Int.down;
            return cursor;
        }
    }
}

using System.Collections.Generic;
using TerraTactix.Core;

namespace TerraTactix.Puzzle
{
    public sealed class PieceSpawner
    {
        readonly Queue<PuzzlePiece> queue = new Queue<PuzzlePiece>();
        readonly int variety;
        readonly float specialChance;

        public PieceSpawner(int shapeVariety, float specialChance)
        {
            var total = PieceCatalog.Shapes.Length;
            variety = shapeVariety < 1 ? total : shapeVariety > total ? total : shapeVariety;
            this.specialChance = specialChance < 0f ? 0f : specialChance;
            while (queue.Count < 4)
                Refill();
        }

        public PuzzlePiece Pop()
        {
            if (queue.Count < 4)
                Refill();
            return queue.Dequeue();
        }

        public PuzzlePiece Preview(int index)
        {
            if (queue.Count < 4)
                Refill();
            var copy = queue.ToArray();
            if (index < 0 || index >= copy.Length)
                return null;
            return copy[index];
        }

        void Refill()
        {
            var order = new int[variety];
            for (var i = 0; i < order.Length; i++)
                order[i] = i;

            for (var i = order.Length - 1; i > 0; i--)
            {
                var swap = RngService.Range(0, i + 1);
                var temp = order[i];
                order[i] = order[swap];
                order[swap] = temp;
            }

            for (var i = 0; i < order.Length; i++)
            {
                var shape = PieceCatalog.Shapes[order[i]];
                var cells = new UnityEngine.Vector2Int[shape.Length];
                for (var c = 0; c < shape.Length; c++)
                    cells[c] = shape[c];
                var special = SpecialPieceSystem.Roll(specialChance);
                queue.Enqueue(new PuzzlePiece(RngService.Range(0, PieceCatalog.Colors.Length), 1, cells, special));
            }
        }
    }
}

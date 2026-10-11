namespace TerraTactix.Puzzle
{
    public sealed class MergeSystem
    {
        readonly int maxLevel;

        public MergeSystem(int maxLevel)
        {
            this.maxLevel = maxLevel < 2 ? 2 : maxLevel;
        }

        public bool TryMergeOnce(PuzzleGrid grid, out int newLevel, out int mergedX, out int mergedY)
        {
            newLevel = 0;
            mergedX = 0;
            mergedY = 0;
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var cell = grid.Get(x, y);
                    if (!cell.occupied || cell.level >= maxLevel)
                        continue;

                    if (TryNeighbor(grid, x, y, x + 1, y, cell, out newLevel)
                        || TryNeighbor(grid, x, y, x - 1, y, cell, out newLevel)
                        || TryNeighbor(grid, x, y, x, y + 1, cell, out newLevel)
                        || TryNeighbor(grid, x, y, x, y - 1, cell, out newLevel))
                    {
                        mergedX = x;
                        mergedY = y;
                        return true;
                    }
                }
            }

            return false;
        }

        static bool TryNeighbor(PuzzleGrid grid, int x, int y, int nx, int ny, PuzzleCell cell, out int newLevel)
        {
            newLevel = 0;
            var other = grid.Get(nx, ny);
            if (!other.occupied || other.level != cell.level || other.typeId != cell.typeId)
                return false;
            if (cell.special != 0 || other.special != 0)
                return false;

            cell.level++;
            grid.Set(x, y, cell);
            grid.Clear(nx, ny);
            newLevel = cell.level;
            return true;
        }
    }
}

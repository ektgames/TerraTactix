namespace TerraTactix.Puzzle
{
    public sealed class LineClearSystem
    {
        public int ClearFullRows(PuzzleGrid grid)
        {
            var cleared = 0;
            for (var y = 0; y < grid.Height; y++)
            {
                var full = true;
                for (var x = 0; x < grid.Width; x++)
                {
                    if (grid.Get(x, y).occupied)
                        continue;
                    full = false;
                    break;
                }

                if (!full)
                    continue;

                for (var x = 0; x < grid.Width; x++)
                    grid.Clear(x, y);
                cleared++;
            }

            return cleared;
        }
    }
}

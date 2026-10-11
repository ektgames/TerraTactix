using System.Collections.Generic;
using TerraTactix.Core;
using TerraTactix.Data;

namespace TerraTactix.Puzzle
{
    public enum SpecialKind
    {
        None = 0,
        Bomb = 1,
        Lightning = 2,
        TimeBonus = 3
    }

    public static class SpecialPieceSystem
    {
        public struct Result
        {
            public int score;
            public float time;
            public string label;
        }

        public static float Chance(string difficulty, BalanceConfiguration balance)
        {
            switch (difficulty)
            {
                case "Normal": return balance.normalSpecialChance;
                case "Hard": return balance.hardSpecialChance;
                case "VeryHard": return balance.veryHardSpecialChance;
                case "Extreme": return balance.extremeSpecialChance;
                default: return balance.easySpecialChance;
            }
        }

        public static SpecialKind Roll(float chance)
        {
            if (chance <= 0f)
                return SpecialKind.None;
            var gate = (int)(chance * 1000f);
            if (gate < 1 || RngService.Range(0, 1000) >= gate)
                return SpecialKind.None;
            return (SpecialKind)RngService.Range(1, 4);
        }

        public static Result Apply(PuzzleGrid grid, BalanceConfiguration balance)
        {
            var found = new List<PuzzleCell>();
            var xs = new List<int>();
            var ys = new List<int>();
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var cell = grid.Get(x, y);
                    if (!cell.occupied || cell.special == (int)SpecialKind.None)
                        continue;
                    found.Add(cell);
                    xs.Add(x);
                    ys.Add(y);
                }
            }

            var result = new Result();
            for (var i = 0; i < found.Count; i++)
            {
                var current = grid.Get(xs[i], ys[i]);
                if (!current.occupied || current.special != found[i].special)
                    continue;
                ApplyOne(grid, balance, xs[i], ys[i], current, ref result);
            }

            return result;
        }

        public static string Mark(int special, int level)
        {
            switch ((SpecialKind)special)
            {
                case SpecialKind.Bomb: return "B";
                case SpecialKind.Lightning: return "Ş";
                case SpecialKind.TimeBonus: return "+";
                default: return level.ToString();
            }
        }

        static void ApplyOne(PuzzleGrid grid, BalanceConfiguration balance, int x, int y, PuzzleCell cell, ref Result result)
        {
            switch ((SpecialKind)cell.special)
            {
                case SpecialKind.Bomb:
                    result.label = "Bomba";
                    ClearArea(grid, x, y, balance.bombScore, ref result);
                    break;
                case SpecialKind.Lightning:
                    result.label = "Şimşek";
                    ClearType(grid, cell.typeId, balance.lightningScore, ref result);
                    break;
                case SpecialKind.TimeBonus:
                    result.label = "Süre";
                    result.time += balance.timeBonusSeconds;
                    grid.Clear(x, y);
                    break;
            }
        }

        static void ClearArea(PuzzleGrid grid, int x, int y, int score, ref Result result)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    var cell = grid.Get(x + dx, y + dy);
                    if (!cell.occupied)
                        continue;
                    grid.Clear(x + dx, y + dy);
                    result.score += score;
                }
            }
        }

        static void ClearType(PuzzleGrid grid, int typeId, int score, ref Result result)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var cell = grid.Get(x, y);
                    if (!cell.occupied || cell.typeId != typeId)
                        continue;
                    grid.Clear(x, y);
                    result.score += score;
                }
            }
        }
    }
}

using TerraTactix.Core;

namespace TerraTactix.Core
{
    public static class RngService
    {
        static System.Random random = new System.Random();

        public static int Range(int minInclusive, int maxExclusive)
        {
            return random.Next(minInclusive, maxExclusive);
        }
    }
}

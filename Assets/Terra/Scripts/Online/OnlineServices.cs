using TerraTactix.Core;

namespace TerraTactix.Online
{
    public interface IPlayerService
    {
        string PlayerId { get; }
        string DisplayName { get; }
        bool IsMock { get; }
    }

    public interface ICloudSaveService
    {
        bool IsAvailable { get; }
    }

    public interface ILeaderboardService
    {
        LeaderboardEntry[] GetTop();
    }

    public sealed class LeaderboardEntry
    {
        public int rank;
        public string playerName;
        public int score;
        public int regions;
        public bool isLocal;
        public bool isMock;
    }

    public sealed class MockPlayerService : IPlayerService
    {
        public string PlayerId
        {
            get
            {
                var app = GameBootstrap.Instance;
                if (app == null || app.Save == null || app.Save.Data == null || app.Save.Data.player == null)
                    return "local-player";
                var id = app.Save.Data.player.playerId;
                return string.IsNullOrEmpty(id) ? "local-player" : id;
            }
        }
        public bool IsMock => true;

        public string DisplayName
        {
            get
            {
                var app = GameBootstrap.Instance;
                if (app == null || app.Save.Data == null || app.Save.Data.player == null || !app.Save.Data.player.nameChosen)
                    return string.Empty;
                return app.Save.Data.player.playerName;
            }
        }
    }

    public sealed class MockCloudSaveService : ICloudSaveService
    {
        public bool IsAvailable => false;
    }

    public sealed class MockLeaderboardService : ILeaderboardService
    {
        public LeaderboardEntry[] GetTop()
        {
            return new[]
            {
                Entry(1, "Örnek Kaşif", 18400, 12),
                Entry(2, "Örnek Gezgin", 14250, 9),
                Entry(3, "Örnek Pilot", 11000, 7),
                Entry(4, "Örnek Bahçıvan", 8600, 5),
                Entry(5, "Örnek Fener", 6400, 4)
            };
        }

        static LeaderboardEntry Entry(int rank, string playerName, int score, int regions)
        {
            return new LeaderboardEntry
            {
                rank = rank,
                playerName = playerName,
                score = score,
                regions = regions,
                isMock = true
            };
        }
    }
}

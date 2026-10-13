using System;
using System.Globalization;
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
        public string countryCode;
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
                Entry(1, "Örnek Kaşif", 18400, 12, "TR"),
                Entry(2, "Örnek Gezgin", 14250, 9, "DE"),
                Entry(3, "Örnek Pilot", 11000, 7, "GB"),
                Entry(4, "Örnek Bahçıvan", 8600, 5, "FR"),
                Entry(5, "Örnek Fener", 6400, 4, "US")
            };
        }

        static LeaderboardEntry Entry(int rank, string playerName, int score, int regions, string countryCode)
        {
            return new LeaderboardEntry
            {
                rank = rank,
                playerName = playerName,
                score = score,
                regions = regions,
                countryCode = countryCode,
                isMock = true
            };
        }
    }

    public static class PlayerCountry
    {
        public static string CurrentCode()
        {
            try
            {
                var code = RegionInfo.CurrentRegion.TwoLetterISORegionName;
                return CleanCode(code);
            }
            catch (Exception)
            {
                return "XX";
            }
        }

        public static string DisplayName(string rawCode)
        {
            var code = CleanCode(rawCode);
            if (code == "XX")
                return Locale.T("Bilinmiyor", "Unknown");

            if (!Locale.English)
            {
                switch (code)
                {
                    case "TR": return "Türkiye";
                    case "US": return "ABD";
                    case "GB": return "Birleşik Krallık";
                    case "DE": return "Almanya";
                    case "FR": return "Fransa";
                    case "IT": return "İtalya";
                    case "ES": return "İspanya";
                    case "NL": return "Hollanda";
                    case "BE": return "Belçika";
                    case "AT": return "Avusturya";
                    case "CH": return "İsviçre";
                    case "SE": return "İsveç";
                    case "NO": return "Norveç";
                    case "DK": return "Danimarka";
                    case "FI": return "Finlandiya";
                    case "PL": return "Polonya";
                    case "GR": return "Yunanistan";
                    case "RU": return "Rusya";
                    case "UA": return "Ukrayna";
                    case "AZ": return "Azerbaycan";
                    case "CA": return "Kanada";
                    case "BR": return "Brezilya";
                    case "MX": return "Meksika";
                    case "AR": return "Arjantin";
                    case "AU": return "Avustralya";
                    case "NZ": return "Yeni Zelanda";
                    case "JP": return "Japonya";
                    case "KR": return "Güney Kore";
                    case "CN": return "Çin";
                    case "IN": return "Hindistan";
                    case "ID": return "Endonezya";
                    case "SA": return "Suudi Arabistan";
                    case "AE": return "Birleşik Arap Emirlikleri";
                    case "EG": return "Mısır";
                    case "ZA": return "Güney Afrika";
                }
            }

            try
            {
                var region = new RegionInfo(code);
                return Locale.English ? region.EnglishName : region.NativeName;
            }
            catch (ArgumentException)
            {
                return code;
            }
        }

        public static string CleanCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "XX";
            code = code.Trim().ToUpperInvariant();
            if (code.Length != 2 || !char.IsLetter(code[0]) || !char.IsLetter(code[1]))
                return "XX";
            return code;
        }
    }
}

using System;

namespace TerraTactix.Core
{
    public static class Locale
    {
        public static event Action Changed;

        public static bool English { get; private set; }
        public static bool ReopenSettings { get; private set; }

        public static void Bind(string language)
        {
            English = language == "en";
        }

        public static void SetLanguage(string language)
        {
            var english = language == "en";
            if (English == english)
                return;

            English = english;
            var save = GameBootstrap.Instance.Save;
            save.Data.settings.language = english ? "en" : "tr";
            save.Save();
            ReopenSettings = true;
            Changed?.Invoke();
        }

        public static void ConsumeReopen()
        {
            ReopenSettings = false;
        }

        public static string T(string turkish, string english)
        {
            return English ? english : turkish;
        }

        public static string Place(string id, string turkish)
        {
            if (!English)
                return turkish;

            switch (id)
            {
                case "baslangic": return "Starting Island";
                case "yesil": return "Green Shores";
                case "kuzey": return "Northern Forests";
                case "col": return "Desert Coast";
                case "kutup": return "Polar Reach";
                case "lav": return "Lava Realm";
                case "kristal": return "Crystal Field";
                case "gizem": return "Mystery Island";
            }

            var space = turkish == null ? -1 : turkish.IndexOf(' ');
            if (space <= 0)
                return turkish;

            var prefix = Word(turkish.Substring(0, space));
            var place = Word(turkish.Substring(space + 1));
            return prefix + " " + place;
        }

        public static string Biome(string biome)
        {
            if (!English)
                return biome;

            switch (biome)
            {
                case "Çayır": return "Grassland";
                case "Orman": return "Forest";
                case "Koy": return "Cove";
                case "Çöl": return "Desert";
                case "Buz": return "Ice";
                case "Volkan": return "Volcanic";
                case "Kristal": return "Crystal";
                case "Zehir": return "Toxic";
                case "Ada": return "Island";
                case "Gizem": return "Mystery";
                default: return biome;
            }
        }

        public static string Difficulty(string id)
        {
            switch (id)
            {
                case "Easy": return T("Kolay", "Easy");
                case "Normal": return T("Normal", "Normal");
                case "Hard": return T("Zor", "Hard");
                case "VeryHard": return T("Çok zor", "Very Hard");
                case "Extreme": return T("Aşırı", "Extreme");
                default: return id;
            }
        }

        public static string GoalTitle(string id, string fallback)
        {
            if (!English)
                return fallback;

            switch (id)
            {
                case "earn-gold": return "Collect gold";
                case "spin-once": return "Daily wheel";
                case "first-win": return "First expedition";
                case "merge-10": return "Pop stones";
                case "play-1": return "First step";
                case "match-5": return "First pop";
                case "shop-1": return "Visit the shop";
                case "spend-50": return "Spend a little";
                case "win-3": return "Three expeditions";
                case "combo-3": return "Small combo";
                case "gold-300": return "Purse";
                case "match-25": return "Pop streak";
                case "win-5": return "Five expeditions";
                case "spend-150": return "Generous";
                case "combo-5": return "Combo";
                case "ach-combo-5": return "5x combo";
                case "match-40": return "Stone rain";
                case "gold-700": return "Treasure";
                case "win-8": return "Eight expeditions";
                case "match-70": return "Big pop";
                case "gold-1200": return "Chest";
                case "hard-1": return "Hard expedition";
                case "win-12": return "Twelve expeditions";
                case "combo-8": return "Long combo";
                case "match-120": return "Stone storm";
                case "first-victory": return "First victory";
                case "score-1k": return "One thousand points";
                case "wins-10": return "Ten victories";
                case "shop-5": return "Regular";
                case "wins-50": return "Fifty victories";
                case "extreme-1": return "Very hard victory";
                case "merges-500": return "Five hundred pops";
                case "hard-10": return "Ten hard expeditions";
                case "score-500k": return "Half a million";
                case "capture-25": return "Twenty-five regions";
                case "capture-10": return "Ten regions";
                case "score-10k": return "10 thousand points";
                case "score-100k": return "100 thousand points";
                case "merges-100": return "One hundred pops";
                case "combo-10": return "10x combo";
                case "hard-clear": return "Hard victory";
                case "spins-10": return "Ten spins";
                default: return fallback;
            }
        }

        public static string GoalBody(string id, string fallback)
        {
            if (!English)
                return fallback;

            switch (id)
            {
                case "earn-gold": return "Earn 100 gold.";
                case "spin-once": return "Spin the wheel once.";
                case "first-win": return "Win an expedition.";
                case "merge-10": return "Pop stones 10 times.";
                case "play-1": return "Play one expedition.";
                case "match-5": return "Pop stones 5 times.";
                case "shop-1": return "Buy something from the shop.";
                case "spend-50": return "Spend 50 gold.";
                case "win-3": return "Win 3 expeditions.";
                case "combo-3": return "Reach a 3x combo in one expedition.";
                case "gold-300": return "Earn 300 gold.";
                case "match-25": return "Pop stones 25 times.";
                case "win-5": return "Win 5 expeditions.";
                case "spend-150": return "Spend 150 gold.";
                case "combo-5": return "Reach a 5x combo in one expedition.";
                case "ach-combo-5": return "Reach a 5x combo in one expedition.";
                case "match-40": return "Pop stones 40 times.";
                case "gold-700": return "Earn 700 gold.";
                case "win-8": return "Win 8 expeditions.";
                case "match-70": return "Pop stones 70 times.";
                case "gold-1200": return "Earn 1,200 gold.";
                case "hard-1": return "Win a Hard or higher expedition.";
                case "win-12": return "Win 12 expeditions.";
                case "combo-8": return "Reach an 8x combo in one expedition.";
                case "match-120": return "Pop stones 120 times.";
                case "first-victory": return "Win an expedition.";
                case "score-1k": return "Reach 1,000 total points.";
                case "wins-10": return "Win 10 expeditions.";
                case "shop-5": return "Buy from the shop 5 times.";
                case "wins-50": return "Win 50 expeditions.";
                case "extreme-1": return "Win a Very Hard or Extreme expedition.";
                case "merges-500": return "Pop stones 500 times.";
                case "hard-10": return "Win 10 Hard or higher expeditions.";
                case "score-500k": return "Reach 500,000 total points.";
                case "capture-25": return "Own 25 regions.";
                case "capture-10": return "Own 10 regions.";
                case "score-10k": return "Reach 10,000 total points.";
                case "score-100k": return "Reach 100,000 total points.";
                case "merges-100": return "Pop stones 100 times.";
                case "combo-10": return "Reach a 10x combo in one expedition.";
                case "hard-clear": return "Win a Hard or higher expedition.";
                case "spins-10": return "Spin the daily wheel 10 times.";
                default: return fallback;
            }
        }

        public static string ProductTitle(string id, string fallback)
        {
            if (!English)
                return fallback;

            switch (id)
            {
                case "heart": return "+1 Heart";
                case "heart-pack": return "Heart Pack";
                case "max-hearts": return "Heart Capacity";
                case "time-freeze": return "Time Freeze";
                default: return fallback;
            }
        }

        public static string ProductBody(string id, string fallback)
        {
            if (!English)
                return fallback;

            switch (id)
            {
                case "heart": return "Adds one heart, up to the cap.";
                case "heart-pack": return "Adds five hearts, up to the cap.";
                case "max-hearts": return "Raises the heart cap by 1.";
                case "time-freeze": return "Freezes the battle timer for 10 seconds.";
                default: return fallback;
            }
        }

        public static string WheelLabel(string label)
        {
            if (!English || string.IsNullOrEmpty(label))
                return label;

            return label.Replace("Altın", "Gold").Replace("Kristal", "Crystals").Replace("Can", "Heart").Replace("Dondurucu", "Freeze");
        }

        static string Word(string turkish)
        {
            switch (turkish)
            {
                case "Pırıltı": return "Sparkle";
                case "Yumuşak": return "Soft";
                case "Sessiz": return "Quiet";
                case "Minik": return "Tiny";
                case "Parlak": return "Bright";
                case "Tatlı": return "Sweet";
                case "Uçuk": return "Pale";
                case "Serin": return "Cool";
                case "Ilık": return "Warm";
                case "Neşeli": return "Merry";
                case "Puslu": return "Misty";
                case "Berrak": return "Clear";
                case "Kıvrık": return "Curved";
                case "Turuncu": return "Orange";
                case "Mavi": return "Blue";
                case "İnce": return "Thin";
                case "Yuvarlak": return "Round";
                case "Gizli": return "Hidden";
                case "Hafif": return "Light";
                case "Şeker": return "Sugar";
                case "Koy": return "Cove";
                case "Tepe": return "Hill";
                case "Ada": return "Isle";
                case "Vadi": return "Valley";
                case "Körfez": return "Bay";
                default: return turkish;
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TerraTactix.UI
{
    public static class ArtLibrary
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Hero => Load("hero.png");

        public static Sprite Pal => Load("pal.png");

        public static Sprite Guardian => Load("guardian.png");

        public static Sprite CloudA => Load("cloud-a.png");

        public static Sprite CloudB => Load("cloud-b.png");

        public static Sprite FogBank => Load("fog-bank.png", 0.5f);

        public static Sprite MenuScene => Load("menu-scene.jpg", 0.5f);

        public static Sprite Land(string biome)
        {
            switch (biome)
            {
                case "Orman": return Load("land-orman.png", 0.46f);
                case "Çöl": return Load("land-col.png", 0.46f);
                case "Buz": return Load("land-buz.png", 0.46f);
                case "Volkan": return Load("land-volkan.png", 0.46f);
                case "Kristal": return Load("land-kristal.png", 0.46f);
                case "Zehir": return Load("land-zehir.png", 0.46f);
                case "Gizem": return Load("land-gizem.png", 0.46f);
                case "Ada": return Load("land-ada.png", 0.46f);
                default: return Load("land-cayir.png", 0.46f);
            }
        }

        public static Sprite Gem(int typeId)
        {
            var index = Mathf.Abs(typeId) % 6;
            return Load("gem-" + index + ".png", 0.5f);
        }

        public static Sprite ForBiome(string biome)
        {
            switch (biome)
            {
                case "Orman": return Load("island-orman.png");
                case "Çöl": return Load("island-col.png");
                case "Buz": return Load("island-buz.png");
                case "Volkan": return Load("island-volkan.png");
                case "Kristal": return Load("island-kristal.png");
                case "Zehir": return Load("island-zehir.png");
                case "Gizem": return Load("island-gizem.png");
                case "Ada": return Load("island-ada.png");
                default: return Load("island-cayir.png");
            }
        }

        static Sprite Load(string file, float pivotY = 0.42f)
        {
            if (cache.TryGetValue(file, out var cached))
                return cached;

            var path = Path.Combine(Application.streamingAssetsPath, "Art", file);
            if (!File.Exists(path))
            {
                Debug.LogWarning("ArtLibrary: eksik çizim " + path);
                return null;
            }

            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes))
            {
                Debug.LogWarning("ArtLibrary: okunamadı " + path);
                return null;
            }

            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, pivotY), 100f);
            cache[file] = sprite;
            return sprite;
        }
    }
}

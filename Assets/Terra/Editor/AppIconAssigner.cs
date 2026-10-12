using UnityEditor;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif
using UnityEditor.Build;
using UnityEngine;

namespace TerraTactix.EditorTools
{
    [InitializeOnLoad]
    static class AppIconAssigner
    {
        const string IconPath = "Assets/Terra/Art/AppIcon.png";

        static int waits;
        static bool prepared;

        static AppIconAssigner()
        {
            EditorApplication.delayCall += Assign;
        }

        static void Assign()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Later();
                return;
            }

            var importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer == null)
            {
                Later();
                return;
            }

            if (!prepared)
            {
                var changed = false;
                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    changed = true;
                }

                if (importer.npotScale != TextureImporterNPOTScale.None)
                {
                    importer.npotScale = TextureImporterNPOTScale.None;
                    changed = true;
                }

                prepared = true;
                if (changed)
                {
                    importer.SaveAndReimport();
                    EditorApplication.delayCall += Assign;
                    return;
                }
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (tex == null)
            {
                Later();
                return;
            }

            var changedIcons = false;
            changedIcons |= Apply(NamedBuildTarget.Unknown, IconKind.Any, tex);
            changedIcons |= Apply(NamedBuildTarget.Standalone, IconKind.Application, tex);
            changedIcons |= Apply(NamedBuildTarget.Android, IconKind.Application, tex);
            changedIcons |= Apply(NamedBuildTarget.iOS, IconKind.Application, tex);
            changedIcons |= Apply(NamedBuildTarget.iOS, IconKind.Store, tex);
#if UNITY_ANDROID
            changedIcons |= ApplyPlatform(NamedBuildTarget.Android, AndroidPlatformIconKind.Round, tex, false);
            changedIcons |= ApplyPlatform(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive, tex, true);
#endif

            if (changedIcons)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("TerraTactix app icon set.");
            }
        }

        static bool Apply(NamedBuildTarget target, IconKind kind, Texture2D tex)
        {
            var sizes = PlayerSettings.GetIconSizes(target, kind);
            if (sizes == null || sizes.Length == 0)
                return false;

            var current = PlayerSettings.GetIcons(target, kind);
            if (current != null && current.Length == sizes.Length)
            {
                var same = true;
                for (var i = 0; i < current.Length; i++)
                {
                    if (current[i] != tex)
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                    return false;
            }

            var icons = new Texture2D[sizes.Length];
            for (var i = 0; i < icons.Length; i++)
                icons[i] = tex;

            PlayerSettings.SetIcons(target, icons, kind);
            return true;
        }

        static bool ApplyPlatform(NamedBuildTarget target, PlatformIconKind kind, Texture2D tex, bool layered)
        {
            var icons = PlayerSettings.GetPlatformIcons(target, kind);
            if (icons == null || icons.Length == 0)
                return false;

            var same = true;
            foreach (var icon in icons)
            {
                var layers = icon.GetTextures();
                if (layered)
                {
                    if (layers == null || layers.Length < 2 || layers[0] != tex || layers[1] != tex)
                        same = false;
                }
                else if (layers == null || layers.Length == 0 || layers[0] != tex)
                {
                    same = false;
                }
            }

            if (same)
                return false;

            foreach (var icon in icons)
            {
                if (layered)
                    icon.SetTextures(new[] { tex, tex });
                else
                    icon.SetTexture(tex);
            }

            PlayerSettings.SetPlatformIcons(target, kind, icons);
            return true;
        }

        static void Later()
        {
            if (waits++ > 40)
            {
                Debug.LogWarning("TerraTactix app icon: AppIcon.png was not imported.");
                return;
            }

            EditorApplication.delayCall += Assign;
        }
    }
}

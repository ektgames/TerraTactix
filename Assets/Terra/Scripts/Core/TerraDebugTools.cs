#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

namespace TerraTactix.Core
{
    public sealed class TerraDebugTools : MonoBehaviour
    {
        bool open;

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f1Key.wasPressedThisFrame)
                open = !open;
        }

        void OnGUI()
        {
            if (!open || GameBootstrap.Instance == null)
                return;

            GUILayout.BeginArea(new Rect(16f, 16f, 280f, 280f), GUI.skin.box);
            GUILayout.Label(Locale.T("Geliştirme araçları", "Development tools"));
            var economy = GameBootstrap.Instance.Economy;
            if (GUILayout.Button(Locale.T("+500 altın", "+500 gold")))
                economy.AddGold(500);
            if (GUILayout.Button("+1 can"))
                economy.AddHearts(1);
            if (GUILayout.Button(Locale.T("Açık seferi tamamla", "Complete open expedition")))
                SkipExpedition();
            if (GUILayout.Button("Kapat"))
                open = false;
            GUILayout.EndArea();
        }

        static void SkipExpedition()
        {
            var region = GameBootstrap.Instance.Expeditions.NextPlayable();
            if (region == null)
            {
                Debug.Log("[Terra] Atlanacak açık bölge yok.");
                return;
            }

            GameBootstrap.Instance.Expeditions.RegisterVictory(region.id);
            Debug.Log("[Terra] Geliştirme: " + region.displayName + " seferi tamamlandı.");
        }
    }
}
#endif

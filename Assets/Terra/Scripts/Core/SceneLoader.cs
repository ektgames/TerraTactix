using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerraTactix.Core
{
    public sealed class SceneLoader : MonoBehaviour
    {
        public bool Load(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("SceneLoader: sahne adı boş.");
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"SceneLoader: '{sceneName}' build listesinde yok.");
                return false;
            }

            var save = GameBootstrap.Instance == null ? null : GameBootstrap.Instance.Save;
            if (save != null && save.Data != null && save.Data.player != null && !save.Data.player.nameChosen && sceneName != SceneNames.MainMenu)
            {
                Debug.Log("[Terra] İsim yazılmadan oyun açılmaz.");
                return false;
            }

            if (SceneManager.GetActiveScene().name == sceneName)
                return false;

            SceneManager.LoadScene(sceneName);
            return true;
        }
    }
}

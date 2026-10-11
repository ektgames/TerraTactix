using UnityEngine;

namespace TerraTactix.Core
{
    public sealed class GameStateManager : MonoBehaviour
    {
        public string CurrentScene { get; private set; }
        public bool ModalOpen { get; private set; }

        public void SetScene(string sceneName)
        {
            CurrentScene = sceneName;
        }

        public void SetModal(bool open)
        {
            ModalOpen = open;
        }
    }
}

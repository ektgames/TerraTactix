using UnityEngine;

namespace TerraTactix.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public void ResetProgress()
        {
            GameBootstrap.Instance.ResetProgress();
        }
    }
}

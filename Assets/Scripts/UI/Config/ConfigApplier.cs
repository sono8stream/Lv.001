using UnityEngine;

namespace UI.Config
{
    class ConfigApplier : MonoBehaviour
    {
        private void Awake()
        {
            int frameRate = 30;
            Application.targetFrameRate = frameRate;
        }
    }
}

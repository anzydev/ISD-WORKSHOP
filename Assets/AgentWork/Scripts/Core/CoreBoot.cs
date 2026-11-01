using UnityEngine;

namespace CoinCatcher.Core
{
    public static class CoreBoot
    {
        public static void Ensure()
        {
            if (GameManager.I == null)
            {
                var go = new GameObject("GameRoot");
                go.AddComponent<GameManager>();
            }
            if (Audio.AudioManager.I == null)
            {
                var go = new GameObject("AudioRoot");
                go.AddComponent<Audio.AudioManager>();
            }
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinCatcher.Core
{
    public enum Difficulty { Easy, Medium, Hard }

    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public Difficulty Difficulty { get; private set; } = Difficulty.Medium;
        public float MusicVolume { get; private set; } = 0.55f;
        public float SfxVolume { get; private set; } = 0.9f;

        public const string MenuScene = "Menu";
        public const string ArenaScene = "Arena";

        void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
            Difficulty = (Difficulty)PlayerPrefs.GetInt("cc_difficulty", 1);
            MusicVolume = PlayerPrefs.GetFloat("cc_music", 0.55f);
            SfxVolume = PlayerPrefs.GetFloat("cc_sfx", 0.9f);
        }

        public void SetDifficulty(Difficulty d)
        {
            Difficulty = d;
            PlayerPrefs.SetInt("cc_difficulty", (int)d);
            PlayerPrefs.Save();
        }

        public void SetMusicVolume(float v)
        {
            MusicVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat("cc_music", MusicVolume);
            if (Audio.AudioManager.I != null) Audio.AudioManager.I.ApplyVolumes();
        }

        public void SetSfxVolume(float v)
        {
            SfxVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat("cc_sfx", SfxVolume);
        }

        public void PlayGame() { SceneManager.LoadScene(ArenaScene); }
        public void GoMenu() { SceneManager.LoadScene(MenuScene); }
        public void QuitGame() { Application.Quit(); }
    }
}

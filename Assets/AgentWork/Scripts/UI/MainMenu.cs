using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using CoinCatcher.Core;
using CoinCatcher.Audio;

namespace CoinCatcher.UI
{
    public class MainMenu : MonoBehaviour
    {
        Text difficultyLabel;
        GameObject settingsPanel;
        GameObject howToPanel;
        Text settingsHeader;

        void Start()
        {
            CoreBoot.Ensure();
            BuildUI();
            AudioManager.I?.PlayMusic("music_menu");
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                if (settingsPanel != null && settingsPanel.activeSelf) { settingsPanel.SetActive(false); AudioManager.I?.Sfx("ui_back", 0.7f); }
                else if (howToPanel != null && howToPanel.activeSelf) { howToPanel.SetActive(false); AudioManager.I?.Sfx("ui_back", 0.7f); }
            }
        }

        void BuildUI()
        {
            var canvas = UIKit.MakeCanvas(transform, "MenuCanvas", 50);
            var root = canvas.transform;

            var title = UIKit.MakeText(root, "Title", "ISD-WORKSHOP-2", 120, UIKit.Gold, TextAnchor.MiddleCenter);
            UIKit.Place(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -210f), new Vector2(1400f, 150f));

            float y = -40f;
            float gap = 92f;

            UIKit.MakeButton(root, "PLAY", new Vector2(0f, y), new Vector2(460f, 78f), UIKit.Gold, () =>
            {
                AudioManager.I?.Sfx("ui_click", 0.9f);
                GameManager.I.PlayGame();
            });

            var diffBtn = UIKit.MakeButton(root, "DIFFICULTY: MEDIUM", new Vector2(0f, y - gap),
                new Vector2(460f, 78f), UIKit.NeonCyan, () =>
            {
                int next = ((int)GameManager.I.Difficulty + 1) % 3;
                GameManager.I.SetDifficulty((Difficulty)next);
                UpdateDifficultyLabel();
                AudioManager.I?.Sfx("ui_click", 0.8f);
            }, 24);
            difficultyLabel = diffBtn.GetComponentInChildren<Text>();
            UpdateDifficultyLabel();

            UIKit.MakeButton(root, "SETTINGS", new Vector2(0f, y - gap * 2f), new Vector2(460f, 78f),
                UIKit.SoftWhite, () =>
            {
                settingsPanel.SetActive(true);
                AudioManager.I?.Sfx("ui_click", 0.8f);
            });

            UIKit.MakeButton(root, "HOW TO PLAY", new Vector2(0f, y - gap * 3f), new Vector2(460f, 78f),
                new Color(0.75f, 0.7f, 1f), () =>
            {
                howToPanel.SetActive(true);
                AudioManager.I?.Sfx("ui_click", 0.8f);
            });

            UIKit.MakeButton(root, "QUIT", new Vector2(0f, y - gap * 4f), new Vector2(460f, 78f),
                new Color(1f, 0.45f, 0.45f), () =>
            {
                AudioManager.I?.Sfx("ui_back", 0.9f);
                GameManager.I.QuitGame();
            });

            BuildSettings(root);
            BuildHowTo(root);
        }

        void UpdateDifficultyLabel()
        {
            if (difficultyLabel != null)
                difficultyLabel.text = "DIFFICULTY: " + DifficultySettings.Label(GameManager.I.Difficulty);
        }

        void BuildSettings(Transform root)
        {
            var panel = UIKit.MakeRect(root, "SettingsPanel");
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            settingsPanel = panel.gameObject;
            UIKit.Fill(panel, "Dim", new Color(0.02f, 0.01f, 0.05f, 1f), 0.85f);

            var box = UIKit.MakeRect(panel, "Box");
            box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
            box.pivot = new Vector2(0.5f, 0.5f);
            box.anchoredPosition = Vector2.zero;
            box.sizeDelta = new Vector2(860f, 560f);
            box.gameObject.AddComponent<Image>().color = UIKit.Panel;

            settingsHeader = UIKit.MakeText(box, "Header", "SETTINGS", 54, UIKit.NeonCyan, TextAnchor.MiddleCenter);
            UIKit.Place(settingsHeader, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -52f), new Vector2(760f, 66f));

            var musicLabel = UIKit.MakeText(box, "MusicLabel", "MUSIC", 26, UIKit.SoftWhite, TextAnchor.MiddleLeft);
            UIKit.Place(musicLabel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-170f, -160f), new Vector2(380f, 40f));

            UIKit.MakeSlider(box, new Vector2(60f, -160f), new Vector2(420f, 40f), GameManager.I.MusicVolume,
                UIKit.NeonPink, v => { GameManager.I.SetMusicVolume(v); });

            var sfxLabel = UIKit.MakeText(box, "SfxLabel", "SFX", 26, UIKit.SoftWhite, TextAnchor.MiddleLeft);
            UIKit.Place(sfxLabel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-170f, -250f), new Vector2(380f, 40f));

            UIKit.MakeSlider(box, new Vector2(60f, -250f), new Vector2(420f, 40f), GameManager.I.SfxVolume,
                UIKit.NeonCyan, v =>
                {
                    GameManager.I.SetSfxVolume(v);
                    AudioManager.I?.Sfx("coin_sfx_1", 0.5f);
                });

            var hint = UIKit.MakeText(box, "Hint", "DIFFICULTY IS ON THE MAIN MENU", 16,
                new Color(0.7f, 0.65f, 0.9f, 0.8f), TextAnchor.MiddleCenter);
            UIKit.Place(hint, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 150f), new Vector2(700f, 26f));

            UIKit.MakeButton(box, "CLOSE", new Vector2(0f, -175f), new Vector2(300f, 66f), UIKit.SoftWhite, () =>
            {
                settingsPanel.SetActive(false);
                AudioManager.I?.Sfx("ui_back", 0.7f);
            });

            settingsPanel.SetActive(false);
        }

        void BuildHowTo(Transform root)
        {
            var panel = UIKit.MakeRect(root, "HowToPanel");
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            howToPanel = panel.gameObject;
            UIKit.Fill(panel, "Dim", new Color(0.02f, 0.01f, 0.05f, 1f), 0.85f);

            var box = UIKit.MakeRect(panel, "Box");
            box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
            box.pivot = new Vector2(0.5f, 0.5f);
            box.anchoredPosition = Vector2.zero;
            box.sizeDelta = new Vector2(1020f, 740f);
            box.gameObject.AddComponent<Image>().color = UIKit.Panel;

            var header = UIKit.MakeText(box, "Header", "HOW TO PLAY", 54, UIKit.Gold, TextAnchor.MiddleCenter);
            UIKit.Place(header, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -56f), new Vector2(900f, 66f));

            var body = UIKit.MakeText(box, "Body",
                "THE COIN CANNON FIRES AT RANDOM.\n" +
                "GRAB MORE COINS THAN EVERYONE ELSE.\n" +
                "LOWEST SCORE EACH ROUND IS ELIMINATED!\n" +
                "3 ROUNDS. ONE WINNER.\n\n" +
                "GOLD COINS = 5 POINTS\n" +
                "TIES GO TO SUDDEN DEATH\n\n" +
                "WASD / ARROWS  -  MOVE\n" +
                "SPACE  -  JUMP\n" +
                "SHIFT  -  DASH",
                25, UIKit.SoftWhite, TextAnchor.UpperCenter);
            UIKit.Place(body, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -140f), new Vector2(940f, 470f));
            body.lineSpacing = 1.4f;

            UIKit.MakeButton(box, "GOT IT", new Vector2(0f, -292f), new Vector2(300f, 66f), UIKit.NeonCyan, () =>
            {
                howToPanel.SetActive(false);
                AudioManager.I?.Sfx("ui_back", 0.7f);
            });

            howToPanel.SetActive(false);
        }
    }
}

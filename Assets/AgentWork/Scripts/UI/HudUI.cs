using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoinCatcher.Core;
using CoinCatcher.Player;

namespace CoinCatcher.UI
{
    public class HudUI : MonoBehaviour
    {
        Text[] nameTexts;
        Text[] scoreTexts;
        GameObject[] columns;
        Text roundText;
        Text timerText;
        Text announceText;
        Text hintText;
        RectTransform announceRt;
        Coroutine announceCo;
        GameObject resultsPanel;
        Text resultsHeader;
        Text resultsBody;
        List<CatcherBase> players;

        void Awake()
        {
            Build();
        }

        public void Setup(List<CatcherBase> ps)
        {
            players = ps;
            for (int i = 0; i < 4; i++)
            {
                bool on = i < ps.Count;
                columns[i].SetActive(on);
                if (!on) continue;
                nameTexts[i].text = ps[i].displayName;
                nameTexts[i].color = ps[i].Color;
                scoreTexts[i].color = ps[i].Color;
            }
            RefreshScores();
            if (hintText != null)
            {
                hintText.canvasRenderer.SetAlpha(1f);
                hintText.CrossFadeAlpha(0f, 6f, false);
            }
        }

        void Build()
        {
            var canvas = UIKit.MakeCanvas(transform, "HUD_Canvas", 100);
            var root = canvas.transform;

            nameTexts = new Text[4];
            scoreTexts = new Text[4];
            columns = new GameObject[4];

            for (int i = 0; i < 4; i++)
            {
                var col = UIKit.MakeRect(root, "PlayerCol_" + i);
                col.anchorMin = col.anchorMax = new Vector2(0f, 1f);
                col.pivot = new Vector2(0f, 1f);
                col.anchoredPosition = new Vector2(24f + i * 472f, -18f);
                col.sizeDelta = new Vector2(452f, 120f);

                nameTexts[i] = UIKit.MakeText(col, "Name", "--", 22, UIKit.SoftWhite, TextAnchor.MiddleCenter);
                UIKit.Place(nameTexts[i], new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, 0f), new Vector2(440f, 32f));

                scoreTexts[i] = UIKit.MakeText(col, "Score", "0", 46, Color.white, TextAnchor.MiddleCenter);
                UIKit.Place(scoreTexts[i], new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -32f), new Vector2(440f, 62f));

                columns[i] = col.gameObject;
                columns[i].SetActive(false);
            }

            roundText = UIKit.MakeText(root, "Round", "GET READY", 30, new Color(0.8f, 0.75f, 1f), TextAnchor.MiddleCenter);
            UIKit.Place(roundText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -150f), new Vector2(900f, 40f));

            timerText = UIKit.MakeText(root, "Timer", "1:15", 56, UIKit.Gold, TextAnchor.MiddleCenter);
            UIKit.Place(timerText, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -206f), new Vector2(700f, 72f));

            hintText = UIKit.MakeText(root, "Hint", "WASD / ARROWS  MOVE      SPACE  JUMP      SHIFT  DASH",
                19, new Color(0.85f, 0.82f, 1f, 0.8f), TextAnchor.MiddleCenter);
            UIKit.Place(hintText, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 26f), new Vector2(1400f, 30f));

            announceText = UIKit.MakeText(root, "Announce", "", 92, UIKit.Gold, TextAnchor.MiddleCenter);
            announceRt = UIKit.Place(announceText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(1720f, 320f)).rectTransform;
            announceText.color = new Color(UIKit.Gold.r, UIKit.Gold.g, UIKit.Gold.b, 0f);

            BuildResults(canvas.transform);
        }

        void BuildResults(Transform root)
        {
            var panel = UIKit.MakeRect(root, "ResultsPanel");
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            resultsPanel = panel.gameObject;
            UIKit.Fill(panel, "Dim", new Color(0.02f, 0.01f, 0.05f, 1f), 0.86f);

            var box = UIKit.MakeRect(panel, "Box");
            box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
            box.pivot = new Vector2(0.5f, 0.5f);
            box.anchoredPosition = new Vector2(0f, 10f);
            box.sizeDelta = new Vector2(980f, 640f);
            var boxImg = box.gameObject.AddComponent<Image>();
            boxImg.color = UIKit.Panel;

            resultsHeader = UIKit.MakeText(box, "Header", "GAME OVER", 64, UIKit.Gold, TextAnchor.MiddleCenter);
            UIKit.Place(resultsHeader, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -56f), new Vector2(900f, 84f));

            resultsBody = UIKit.MakeText(box, "Body", "", 30, UIKit.SoftWhite, TextAnchor.UpperCenter);
            UIKit.Place(resultsBody, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -156f), new Vector2(860f, 330f));
            resultsBody.lineSpacing = 1.35f;

            UIKit.MakeButton(box, "REMATCH", new Vector2(-220f, -260f), new Vector2(340f, 86f), UIKit.NeonCyan,
                () => { Audio.AudioManager.I?.Sfx("ui_click", 0.8f); GameManager.I.PlayGame(); });
            UIKit.MakeButton(box, "MENU", new Vector2(220f, -260f), new Vector2(340f, 86f), UIKit.NeonPink,
                () => { Audio.AudioManager.I?.Sfx("ui_back", 0.8f); GameManager.I.GoMenu(); });

            resultsPanel.SetActive(false);
        }

        public void ShowRound(int current, int total)
        {
            if (roundText == null) return;
            roundText.text = current <= 0 ? "GET READY" : "ROUND " + current + " / " + total;
        }

        public void ShowTimer(bool on)
        {
            if (timerText != null) timerText.gameObject.SetActive(on);
        }

        public void SetTimer(float seconds)
        {
            if (timerText == null) return;
            int s = Mathf.CeilToInt(seconds);
            if (s >= 60) timerText.text = (s / 60) + ":" + (s % 60).ToString("00");
            else timerText.text = "0:" + s.ToString("00");
            timerText.color = s <= 10 ? new Color(1f, 0.35f, 0.35f) : UIKit.Gold;
        }

        public void RefreshScores()
        {
            if (players == null) return;
            for (int i = 0; i < players.Count && i < 4; i++)
                scoreTexts[i].text = players[i].RoundScore.ToString();
        }

        public void PulsePlayer(int index)
        {
            if (index < 0 || index >= 4 || !columns[index].activeSelf) return;
            StartCoroutine(Pulse(scoreTexts[index].rectTransform));
        }

        IEnumerator Pulse(RectTransform rt)
        {
            float t = 0f;
            while (t < 0.26f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.26f);
                rt.localScale = Vector3.one * (1f + 0.22f * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        public void ShowAnnounce(string text, Color color, float duration)
        {
            if (announceCo != null) StopCoroutine(announceCo);
            announceCo = StartCoroutine(AnnounceRoutine(text, color, duration));
        }

        IEnumerator AnnounceRoutine(string text, Color color, float duration)
        {
            announceText.text = text;
            announceRt.localScale = Vector3.one * 0.55f;
            float t = 0f;
            while (t < 0.16f)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / 0.16f);
                announceText.color = new Color(color.r, color.g, color.b, k);
                announceRt.localScale = Vector3.one * Mathf.Lerp(0.55f, 1.06f, k);
                yield return null;
            }
            announceRt.localScale = Vector3.one;
            announceText.color = color;
            yield return new WaitForSeconds(Mathf.Max(0.05f, duration - 0.42f));
            float t2 = 0f;
            while (t2 < 0.26f)
            {
                t2 += Time.deltaTime;
                announceText.color = new Color(color.r, color.g, color.b, 1f - t2 / 0.26f);
                yield return null;
            }
            announceText.color = new Color(color.r, color.g, color.b, 0f);
            announceCo = null;
        }

        public void ShowResults(List<CatcherBase> ranked, bool humanWon)
        {
            if (resultsPanel == null) return;
            resultsPanel.SetActive(true);
            resultsHeader.text = humanWon ? "YOU WIN!" : "GAME OVER";
            resultsHeader.color = humanWon ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.35f, 0.4f);

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < ranked.Count; i++)
            {
                var p = ranked[i];
                sb.Append(i + 1).Append(".").Append(p.displayName.PadLeft(7)).Append("   ")
                  .Append(p.TotalScore.ToString().PadLeft(3)).Append(" PTS   ")
                  .Append(p.alive ? "SURVIVED" : "OUT").Append("\n");
            }
            sb.Append("\nDIFFICULTY: ").Append(DifficultySettings.Label(GameManager.I.Difficulty));
            resultsBody.text = sb.ToString();
        }
    }
}

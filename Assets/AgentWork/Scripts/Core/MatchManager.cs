using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoinCatcher.Audio;
using CoinCatcher.Bots;
using CoinCatcher.Gameplay;
using CoinCatcher.Player;
using CoinCatcher.UI;

namespace CoinCatcher.Core
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager I { get; private set; }

        [Header("Setup")]
        public GameObject catcherPrefab;
        public Transform[] spawnPoints;
        public CoinCannon cannon;
        public HudUI hud;
        public CameraRig cameraRig;

        [Header("Rules")]
        public int roundsToPlay = 3;
        public float roundDuration = 75f;
        public float overtimeDuration = 15f;
        public string[] botNames = { "VEX", "NYX", "RAX" };
        public Color[] botColors =
        {
            new Color(0.35f, 0.9f, 1f),
            new Color(1f, 0.35f, 0.75f),
            new Color(0.55f, 1f, 0.35f)
        };
        public Color humanColor = new Color(1f, 0.9f, 0.35f);

        public readonly List<CatcherBase> players = new List<CatcherBase>();
        public CatcherBase Human { get; private set; }
        public int Round { get; private set; }
        public bool RoundActive { get; private set; }

        DiffTuning tune;
        float timeLeft;

        void Awake()
        {
            if (I != null) { Destroy(this); return; }
            I = this;
        }

        void Start()
        {
            CoreBoot.Ensure();
            tune = DifficultySettings.Get(GameManager.I.Difficulty);
            SpawnPlayers();
            if (hud != null) hud.Setup(players);
            if (cameraRig != null && Human != null) cameraRig.Target = Human.transform;
            AudioManager.I?.PlayMusic("music_match");
            StartCoroutine(RunMatch());
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }

        void SpawnPlayers()
        {
            for (int i = 0; i < spawnPoints.Length && i < 4; i++)
            {
                var go = Instantiate(catcherPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
                go.name = i == 0 ? "Catcher_You" : "Catcher_" + botNames[i - 1];
                if (i == 0)
                {
                    var extraBot = go.GetComponent<BotController>();
                    if (extraBot != null) Destroy(extraBot);
                    var human = go.GetComponent<CatcherController>();
                    human.playerIndex = 0;
                    human.displayName = "YOU";
                    human.color = humanColor;
                    Human = human;
                    players.Add(human);
                    ApplyColor(go, humanColor);
                }
                else
                {
                    var extraHuman = go.GetComponent<CatcherController>();
                    if (extraHuman != null) Destroy(extraHuman);
                    var bot = go.GetComponent<BotController>();
                    bot.playerIndex = i;
                    bot.displayName = botNames[i - 1];
                    bot.color = botColors[i - 1];
                    bot.Configure(GameManager.I.Difficulty);
                    players.Add(bot);
                    ApplyColor(go, botColors[i - 1]);
                }
            }
        }

        public static void ApplyColor(GameObject go, Color c)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mpb = new MaterialPropertyBlock();
                r.GetPropertyBlock(mpb);
                mpb.SetColor("_BaseColor", c);
                r.SetPropertyBlock(mpb);
            }
        }

        IEnumerator RunMatch()
        {
            if (hud != null)
            {
                hud.ShowAnnounce("COIN MAYHEM", new Color(1f, 0.85f, 0.2f), 1.7f);
                hud.ShowRound(0, roundsToPlay);
            }
            yield return new WaitForSeconds(1.9f);

            for (Round = 1; Round <= roundsToPlay; Round++)
            {
                if (AliveCount() < 2) break;

                foreach (var p in players) p.RoundScore = 0;
                for (int i = 0; i < players.Count && i < spawnPoints.Length; i++)
                {
                    if (players[i].alive && players[i].MatchGate == false)
                        players[i].ResetForMatch(spawnPoints[i].position, spawnPoints[i].rotation);
                }
                hud?.RefreshScores();
                hud?.ShowRound(Round, roundsToPlay);
                hud?.ShowAnnounce("ROUND " + Round, new Color(0.6f, 0.9f, 1f), 1.2f);
                AudioManager.I?.Sfx("round_start_jingle", 0.8f);
                yield return new WaitForSeconds(1.5f);
                yield return Countdown();

                if (cannon != null)
                {
                    cannon.interval = Mathf.Max(0.55f, tune.cannonInterval);
                    cannon.goldChance = tune.goldChance;
                    cannon.burstMin = tune.burstMin;
                    cannon.burstMax = tune.burstMax;
                    cannon.active = true;
                }

                timeLeft = roundDuration;
                RoundActive = true;
                foreach (var p in players) p.MatchGate = p.alive;
                hud?.ShowTimer(true);

                bool tensionStarted = false;
                while (timeLeft > 0f && AliveCount() > 1)
                {
                    timeLeft -= Time.deltaTime;
                    hud?.SetTimer(Mathf.Max(0f, timeLeft));
                    if (!tensionStarted && timeLeft <= 12f)
                    {
                        tensionStarted = true;
                        AudioManager.I?.PlayMusic("music_tension", 1.2f);
                    }
                    yield return null;
                }

                RoundActive = false;
                foreach (var p in players) p.MatchGate = false;
                if (cannon != null) cannon.active = false;
                hud?.ShowTimer(false);
                if (AliveCount() < 2) break;

                hud?.ShowAnnounce("TIME UP!", new Color(1f, 0.5f, 0.3f), 1.2f);
                AudioManager.I?.Sfx("round_end_alarm", 0.8f);
                AudioManager.I?.PlayMusic("music_match", 1f);
                yield return new WaitForSeconds(1.5f);

                yield return ResolveElimination();

                if (Human != null && !Human.alive) break;
                yield return new WaitForSeconds(1f);
            }

            yield return EndMatch();
        }

        IEnumerator Countdown()
        {
            for (int i = 3; i >= 1; i--)
            {
                hud?.ShowAnnounce(i.ToString(), new Color(1f, 0.9f, 0.3f), 0.5f);
                AudioManager.I?.Sfx("target_beep", 0.7f);
                yield return new WaitForSeconds(0.55f);
            }
            hud?.ShowAnnounce("GO!", new Color(0.4f, 1f, 0.5f), 0.55f);
            AudioManager.I?.Sfx("fanfare_small", 0.6f);
            yield return new WaitForSeconds(0.55f);
        }

        IEnumerator ResolveElimination()
        {
            var alive = players.Where(p => p.alive).ToList();
            int min = alive.Min(p => p.RoundScore);
            var lowest = alive.Where(p => p.RoundScore == min).ToList();
            int overtime = 0;

            while (lowest.Count > 1 && overtime < 3)
            {
                overtime++;
                hud?.ShowAnnounce("SUDDEN DEATH!", new Color(1f, 0.3f, 0.35f), 1.6f);
                AudioManager.I?.Sfx("elim_sting", 0.85f);
                AudioManager.I?.PlayMusic("music_tension", 0.7f);

                if (cannon != null)
                {
                    cannon.interval = 0.35f;
                    cannon.burstMin = 2;
                    cannon.burstMax = 4;
                    cannon.active = true;
                }

                timeLeft = overtimeDuration;
                RoundActive = true;
                foreach (var p in players) p.MatchGate = p.alive;
                hud?.ShowTimer(true);

                while (timeLeft > 0f && lowest.Count > 1)
                {
                    timeLeft -= Time.deltaTime;
                    hud?.SetTimer(Mathf.Max(0f, timeLeft));
                    min = alive.Min(p => p.RoundScore);
                    lowest = alive.Where(p => p.RoundScore == min).ToList();
                    yield return null;
                }

                RoundActive = false;
                foreach (var p in players) p.MatchGate = false;
                if (cannon != null) cannon.active = false;
                hud?.ShowTimer(false);

                if (lowest.Count > 1 && overtime >= 3)
                    lowest = new List<CatcherBase> { lowest[Random.Range(0, lowest.Count)] };
            }

            if (lowest.Count == 0) yield break;
            var gone = lowest[0];
            gone.Eliminate();
            hud?.RefreshScores();
            hud?.ShowAnnounce(gone.displayName + " ELIMINATED", gone.Color, 2f);
            AudioManager.I?.Sfx("elim_sting", 1f);
            yield return new WaitForSeconds(1.4f);
        }

        IEnumerator EndMatch()
        {
            RoundActive = false;
            foreach (var p in players) p.MatchGate = false;
            if (cannon != null) cannon.active = false;
            hud?.ShowTimer(false);

            var ranked = players.OrderByDescending(p => p.alive)
                                .ThenByDescending(p => p.TotalScore)
                                .ToList();
            bool humanWon = Human != null && Human.alive && ranked[0] == Human;

            hud?.ShowAnnounce(humanWon ? "YOU WIN!" : "GAME OVER",
                humanWon ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f), 2f);
            AudioManager.I?.Sfx(humanWon ? "fanfare_win" : "elim_sting", 1f);
            AudioManager.I?.PlayMusic("music_menu", 1f);
            yield return new WaitForSeconds(2.4f);
            hud?.ShowResults(ranked, humanWon);
        }

        public void NotifyScore(CatcherBase who, Coin coin)
        {
            hud?.RefreshScores();
            hud?.PulsePlayer(who.playerIndex);
            if (who == Human && cameraRig != null) cameraRig.Shake(coin.isGold ? 0.22f : 0.07f);
        }

        int AliveCount()
        {
            int n = 0;
            for (int i = 0; i < players.Count; i++) if (players[i].alive) n++;
            return n;
        }
    }
}

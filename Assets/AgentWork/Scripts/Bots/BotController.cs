using UnityEngine;
using UnityEngine.AI;
using CoinCatcher.Core;
using CoinCatcher.Gameplay;
using CoinCatcher.Player;

namespace CoinCatcher.Bots
{
    public class BotController : CatcherBase
    {
        NavMeshAgent agent;
        Animator anim;
        DiffTuning tune;
        float decideTimer;
        float pauseTimer;
        Coin target;
        bool configured;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponentInChildren<Animator>();
            if (agent != null) agent.enabled = false;
        }

        public void Configure(Difficulty difficulty)
        {
            tune = DifficultySettings.Get(difficulty);
            if (agent != null)
            {
                agent.speed = tune.botSpeed;
                agent.enabled = true;
                configured = true;
            }
        }

        void Update()
        {
            if (!configured) return;

            if (anim != null && agent != null)
            {
                float spd = agent.enabled ? agent.velocity.magnitude : 0f;
                anim.SetFloat("Speed", spd > 0.15f ? Mathf.Clamp01(spd / 4f) : 0f);
            }

            if (!alive || !MatchGate || !agent.enabled || !agent.isOnNavMesh) return;

            decideTimer -= Time.deltaTime;
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                if (agent.isOnNavMesh) agent.isStopped = true;
                return;
            }
            if (agent.isOnNavMesh) agent.isStopped = false;

            if (decideTimer <= 0f)
            {
                decideTimer = tune.botReaction * Random.Range(0.8f, 1.25f);
                Decide();
            }
        }

        void Decide()
        {
            if (Random.value < tune.botIdleChance)
            {
                pauseTimer = Random.Range(0.35f, 1.1f);
                return;
            }

            Coin best = null;
            float bestVal = float.MaxValue;
            for (int i = 0; i < Coin.All.Count; i++)
            {
                var c = Coin.All[i];
                if (c == null) continue;
                float d = Vector3.Distance(transform.position, c.transform.position);
                float score = d + Random.Range(0f, tune.botNoise) - (c.isGold ? 2.5f : 0f);
                if (score < bestVal) { bestVal = score; best = c; }
            }

            target = best;
            if (best != null)
            {
                agent.SetDestination(best.transform.position);
            }
            else
            {
                var rnd = transform.position + Random.insideUnitSphere * 8f;
                rnd.y = 0f;
                if (NavMesh.SamplePosition(rnd, out var hit, 4f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
            }
        }

        public override void PlayPickupAnim()
        {
            if (anim != null) anim.SetTrigger("PickUp");
        }

        public override void SetMoveBlend(float v) { }

        public override void Eliminate()
        {
            alive = false;
            MatchGate = false;
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
            if (anim != null) anim.SetTrigger("Die");
        }

        public override void ResetForMatch(Vector3 pos, Quaternion rot)
        {
            alive = true;
            RoundScore = 0;
            MatchGate = false;
            if (agent != null)
            {
                if (!agent.enabled) agent.enabled = true;
                if (NavMesh.SamplePosition(pos, out var hit, 4f, NavMesh.AllAreas))
                    agent.Warp(hit.position);
            }
            transform.rotation = rot;
        }
    }
}

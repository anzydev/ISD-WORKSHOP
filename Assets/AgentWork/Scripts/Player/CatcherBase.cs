using UnityEngine;
using CoinCatcher.Gameplay;

namespace CoinCatcher.Player
{
    public abstract class CatcherBase : MonoBehaviour, ICoinCollector
    {
        public int playerIndex;
        public string displayName = "PLAYER";
        public Color color = Color.white;
        public bool alive = true;
        public bool MatchGate;
        public int RoundScore { get; set; }
        public int TotalScore { get; set; }

        public bool CanCollect => alive && MatchGate;
        public Transform Transform => transform;
        public Color Color => color;

        public virtual void CollectCoin(Coin coin)
        {
            RoundScore += coin.value;
            TotalScore += coin.value;
            PlayPickupAnim();
            Core.MatchManager.I?.NotifyScore(this, coin);
        }

        public abstract void PlayPickupAnim();
        public abstract void SetMoveBlend(float normalizedSpeed);
        public abstract void Eliminate();
        public abstract void ResetForMatch(Vector3 pos, Quaternion rot);
    }
}

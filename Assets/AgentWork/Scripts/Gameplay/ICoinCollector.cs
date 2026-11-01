using UnityEngine;

namespace CoinCatcher.Gameplay
{
    public interface ICoinCollector
    {
        bool CanCollect { get; }
        int RoundScore { get; }
        void CollectCoin(Coin coin);
    }
}

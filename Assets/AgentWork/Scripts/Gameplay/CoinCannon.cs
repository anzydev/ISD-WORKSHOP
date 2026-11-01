using System.Collections;
using UnityEngine;

namespace CoinCatcher.Gameplay
{
    public class CoinCannon : MonoBehaviour
    {
        public GameObject coinPrefab;
        public GameObject markerPrefab;
        public Transform barrel;
        public Transform muzzle;

        public float interval = 1.15f;
        public int burstMin = 1;
        public int burstMax = 3;
        public float goldChance = 0.12f;
        public float arenaRadius = 9.5f;
        public float minRadius = 3.2f;
        public bool active = true;

        float timer;

        void Update()
        {
            if (!active) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = interval * Random.Range(0.75f, 1.25f);
            StartCoroutine(Volley());
        }

        IEnumerator Volley()
        {
            int burst = Random.Range(burstMin, burstMax + 1);
            Audio.AudioManager.I?.Sfx("volley_whistle", 0.45f);
            for (int i = 0; i < burst; i++)
            {
                if (!active) yield break;
                var target = RandomPoint();
                if (markerPrefab != null)
                {
                    var marker = Instantiate(markerPrefab, target + Vector3.up * 0.04f, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                    Destroy(marker, 0.7f);
                }
                if (barrel != null)
                {
                    var flat = new Vector3(target.x - barrel.position.x, 0f, target.z - barrel.position.z);
                    if (flat.sqrMagnitude > 0.01f)
                        barrel.rotation = Quaternion.Slerp(barrel.rotation, Quaternion.LookRotation(flat), 0.5f);
                }
                yield return new WaitForSeconds(0.35f);
                Fire(target);
                yield return new WaitForSeconds(0.13f);
            }
        }

        void Fire(Vector3 target)
        {
            if (coinPrefab == null || muzzle == null) return;
            var go = Instantiate(coinPrefab, muzzle.position, Random.rotation);
            var coin = go.GetComponent<Coin>();
            bool gold = Random.value < goldChance;
            coin.value = gold ? 5 : 1;
            coin.isGold = gold;
            coin.Launch(muzzle.position, target);
            Audio.AudioManager.I?.Sfx("cannon_fire_" + Random.Range(1, 3), 0.85f);
        }

        Vector3 RandomPoint()
        {
            float a = Random.Range(0f, Mathf.PI * 2f);
            float r = Mathf.Lerp(minRadius, arenaRadius, Mathf.Sqrt(Random.value));
            return new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
        }
    }
}

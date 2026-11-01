using System.Collections.Generic;
using UnityEngine;

namespace CoinCatcher.Gameplay
{
    public class Coin : MonoBehaviour
    {
        public static readonly List<Coin> All = new List<Coin>();

        public int value = 1;
        public bool isGold;

        const float Gravity = 14f;
        const float Lifetime = 14f;
        const float LandHeight = 0.22f;

        Vector3 velocity;
        Vector3 baseScale;
        bool landed;
        bool collected;
        float born;
        Renderer rend;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        void Awake()
        {
            rend = GetComponentInChildren<Renderer>();
            baseScale = transform.localScale;
        }

        void OnEnable() { All.Add(this); born = Time.time; }
        void OnDisable() { All.Remove(this); }

        public void Launch(Vector3 from, Vector3 to)
        {
            transform.position = from;
            velocity = (to - from) / 1.1f + Vector3.up * (0.5f * Gravity * 1.1f);
            landed = false;
            if (isGold)
            {
                baseScale *= 1.3f;
                transform.localScale = baseScale;
                if (rend != null)
                {
                    var mpb = new MaterialPropertyBlock();
                    rend.GetPropertyBlock(mpb);
                    mpb.SetColor(BaseColorId, new Color(1f, 0.9f, 0.35f, 1f));
                    rend.SetPropertyBlock(mpb);
                }
            }
        }

        void Update()
        {
            float age = Time.time - born;
            if (!landed)
            {
                velocity.y -= Gravity * Time.deltaTime;
                transform.position += velocity * Time.deltaTime;
                transform.Rotate(Vector3.up, 540f * Time.deltaTime, Space.World);
                if (transform.position.y <= LandHeight)
                {
                    landed = true;
                    transform.position = new Vector3(transform.position.x, LandHeight, transform.position.z);
                }
            }
            else
            {
                transform.position += Vector3.up * (Mathf.Sin(age * 2.6f) * 0.35f * Time.deltaTime);
                transform.Rotate(Vector3.up, 150f * Time.deltaTime, Space.World);
            }

            if (age > Lifetime) { Destroy(gameObject); return; }
            if (age > Lifetime - 2.5f)
            {
                float s = Mathf.PingPong(age * 6f, 1f);
                transform.localScale = baseScale * (0.7f + 0.3f * s);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (collected) return;
            var c = other.GetComponentInParent<ICoinCollector>();
            if (c == null || !c.CanCollect) return;
            collected = true;
            c.CollectCoin(this);
            Audio.AudioManager.I?.Sfx(isGold ? "coin_gold_" + Random.Range(1, 4) : "coin_sfx_" + Random.Range(1, 5),
                isGold ? 0.95f : 0.6f);
            FloatingText.Spawn(transform.position + Vector3.up * 0.45f, "+" + value,
                isGold ? new Color(1f, 0.85f, 0.2f) : new Color(0.85f, 0.95f, 1f));
            Destroy(gameObject);
        }
    }
}

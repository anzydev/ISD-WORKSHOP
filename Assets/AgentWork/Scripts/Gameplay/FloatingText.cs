using UnityEngine;

namespace CoinCatcher.Gameplay
{
    public class FloatingText : MonoBehaviour
    {
        TextMesh tm;
        float born;
        Vector3 vel;
        Color color;

        public static void Spawn(Vector3 pos, string text, Color color)
        {
            var go = new GameObject("Popup");
            go.transform.position = pos;
            var ft = go.AddComponent<FloatingText>();
            ft.Init(text, color);
        }

        void Init(string text, Color c)
        {
            color = c;
            tm = gameObject.AddComponent<TextMesh>();
            tm.text = text;
            tm.font = Resources.Load<Font>("Fonts/PressStart2P");
            tm.fontSize = 64;
            tm.characterSize = 0.045f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = c;
            var mr = GetComponent<MeshRenderer>();
            if (tm.font != null) mr.sharedMaterial = tm.font.material;
            born = Time.time;
            vel = new Vector3(Random.Range(-0.35f, 0.35f), 1.7f, Random.Range(-0.35f, 0.35f));
        }

        void Update()
        {
            float t = Time.time - born;
            transform.position += vel * Time.deltaTime;
            vel.y -= 3.2f * Time.deltaTime;
            var cam = Camera.main;
            if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            if (tm != null)
            {
                var c = color;
                c.a = Mathf.Clamp01(1f - t / 0.95f);
                tm.color = c;
            }
            transform.localScale = Vector3.one * Mathf.Lerp(1.25f, 0.75f, t / 0.95f);
            if (t > 0.95f) Destroy(gameObject);
        }
    }
}

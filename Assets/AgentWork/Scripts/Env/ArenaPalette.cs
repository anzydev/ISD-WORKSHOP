using UnityEngine;

namespace CoinCatcher.Env
{
    public class ArenaPalette : MonoBehaviour
    {
        public Color floorColor = new Color(0.18f, 0.14f, 0.3f);
        public Color wallColor = new Color(0.1f, 0.075f, 0.19f);
        public Color padColor = new Color(0.55f, 0.32f, 0.85f);

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        void Start()
        {
            Apply();
        }

        public void Apply()
        {
            var mpb = new MaterialPropertyBlock();
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                Color c;
                if (r.name.StartsWith("Floor")) c = floorColor;
                else if (r.name.StartsWith("wall") || r.name.StartsWith("Corner")) c = wallColor;
                else if (UnderPad(r.transform)) c = padColor;
                else continue;

                r.GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, c);
                r.SetPropertyBlock(mpb);
            }
        }

        bool UnderPad(Transform t)
        {
            while (t != null && t != transform)
            {
                if (t.name.StartsWith("SpawnPad")) return true;
                t = t.parent;
            }
            return false;
        }
    }
}

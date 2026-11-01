using UnityEngine;

namespace CoinCatcher.UI
{
    public class CameraRig : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 10.5f, -11f);
        public Vector3 LookOffset = new Vector3(0f, 1.2f, 0f);
        public float followLerp = 4f;
        public float rotateLerp = 6f;
        public float arenaClamp = 10f;

        float shake;

        void LateUpdate()
        {
            if (Target == null) return;

            var desired = Target.position + Offset;
            desired.x = Mathf.Clamp(desired.x, -arenaClamp, arenaClamp);
            desired.z = Mathf.Clamp(desired.z, -arenaClamp - 2f, arenaClamp + 2f);
            transform.position = Vector3.Lerp(transform.position, desired, followLerp * Time.deltaTime);

            var look = Target.position + LookOffset;
            var rot = Quaternion.LookRotation(look - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotateLerp * Time.deltaTime);

            if (shake > 0.0005f)
            {
                transform.position += Random.insideUnitSphere * shake;
                shake = Mathf.Max(0f, shake - 0.6f * Time.deltaTime);
            }
        }

        public void Shake(float amount)
        {
            shake = Mathf.Max(shake, amount);
        }
    }
}

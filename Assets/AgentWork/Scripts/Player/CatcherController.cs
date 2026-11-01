using UnityEngine;
using UnityEngine.InputSystem;

namespace CoinCatcher.Player
{
    public class CatcherController : CatcherBase
    {
        public float moveSpeed = 4.6f;
        public float dashSpeed = 10.5f;
        public float dashDuration = 0.18f;
        public float dashCooldown = 1.1f;
        public float jumpVelocity = 5.2f;

        CharacterController cc;
        Animator anim;
        Vector3 vertVel;
        float dashTime;
        float dashCd;
        float stepTimer;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            anim = GetComponentInChildren<Animator>();
        }

        void Update()
        {
            float blend = 0f;

            if (alive && MatchGate)
            {
                var kb = Keyboard.current;
                Vector2 input = Vector2.zero;
                if (kb != null)
                {
                    if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
                    if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;

                    if (kb.spaceKey.wasPressedThisFrame && cc.isGrounded)
                    {
                        vertVel.y = jumpVelocity;
                        Audio.AudioManager.I?.Sfx("jump_up", 0.35f);
                    }
                    bool shift = kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame;
                    if (shift && dashCd <= 0f)
                    {
                        dashTime = dashDuration;
                        dashCd = dashCooldown;
                        Audio.AudioManager.I?.Sfx("dash_sfx", 0.55f);
                    }
                }

                dashTime -= Time.deltaTime;
                dashCd -= Time.deltaTime;

                var cam = Camera.main;
                Vector3 fwd = cam != null ? Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up) : Vector3.forward;
                if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward; else fwd.Normalize();
                Vector3 right = cam != null ? Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized : Vector3.right;

                Vector3 move = fwd * input.y + right * input.x;
                if (move.sqrMagnitude > 1f) move.Normalize();

                float speed = dashTime > 0f ? dashSpeed : moveSpeed;
                if (move.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 16f * Time.deltaTime);
                    blend = dashTime > 0f ? 1f : 0.62f;
                    stepTimer -= Time.deltaTime;
                    if (stepTimer <= 0f && cc.isGrounded)
                    {
                        stepTimer = 0.3f;
                        Audio.AudioManager.I?.Sfx(Random.value < 0.5f ? "foot_step_a" : "foot_step_b", 0.16f);
                    }
                }

                vertVel.y -= 20f * Time.deltaTime;
                if (cc.isGrounded && vertVel.y < 0f) vertVel.y = -1f;
                cc.Move((move * speed + Vector3.up * vertVel.y) * Time.deltaTime);
            }

            SetMoveBlend(blend);
        }

        public override void PlayPickupAnim()
        {
            if (anim != null) anim.SetTrigger("PickUp");
        }

        public override void SetMoveBlend(float v)
        {
            if (anim != null) anim.SetFloat("Speed", v);
        }

        public override void Eliminate()
        {
            alive = false;
            MatchGate = false;
            if (anim != null) anim.SetTrigger("Die");
        }

        public override void ResetForMatch(Vector3 pos, Quaternion rot)
        {
            alive = true;
            RoundScore = 0;
            MatchGate = false;
            if (cc != null)
            {
                cc.enabled = false;
                transform.SetPositionAndRotation(pos, rot);
                cc.enabled = true;
            }
            else
            {
                transform.SetPositionAndRotation(pos, rot);
            }
        }
    }
}

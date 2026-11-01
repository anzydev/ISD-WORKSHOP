using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoinCatcher.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        AudioSource musicA, musicB, active, idle;
        AudioSource sfx;
        readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
        string currentMusic = "";
        Coroutine fadeCo;

        void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
            musicA = gameObject.AddComponent<AudioSource>();
            musicB = gameObject.AddComponent<AudioSource>();
            sfx = gameObject.AddComponent<AudioSource>();
            foreach (var s in new[] { musicA, musicB })
            {
                s.loop = true;
                s.playOnAwake = false;
                s.volume = 0f;
            }
            sfx.playOnAwake = false;
            active = musicA;
            idle = musicB;
        }

        public void ApplyVolumes()
        {
            if (active != null && active.isPlaying)
                active.volume = Core.GameManager.I != null ? Core.GameManager.I.MusicVolume : 0.55f;
        }

        public void PlayMusic(string name, float fade = 1f)
        {
            if (currentMusic == name) return;
            var clip = Load("Audio/Music/" + name);
            if (clip == null) return;
            currentMusic = name;
            var next = idle;
            idle = active;
            active = next;
            active.clip = clip;
            active.volume = 0f;
            active.Play();
            if (fadeCo != null) StopCoroutine(fadeCo);
            fadeCo = StartCoroutine(Crossfade(fade));
        }

        IEnumerator Crossfade(float dur)
        {
            float target = Core.GameManager.I != null ? Core.GameManager.I.MusicVolume : 0.55f;
            float startActive = active.volume;
            float startIdle = idle.volume;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                active.volume = Mathf.Lerp(startActive, target, k);
                idle.volume = Mathf.Lerp(startIdle, 0f, k);
                yield return null;
            }
            active.volume = target;
            idle.Stop();
            idle.volume = 0f;
            fadeCo = null;
        }

        public void StopMusic()
        {
            currentMusic = "";
            if (active != null) active.Stop();
            if (idle != null) idle.Stop();
        }

        public void Sfx(string name, float vol = 1f, float pitchVar = 0.06f)
        {
            var clip = Load("Audio/SFX/" + name);
            if (clip == null) return;
            float master = Core.GameManager.I != null ? Core.GameManager.I.SfxVolume : 0.9f;
            sfx.pitch = 1f + Random.Range(-pitchVar, pitchVar);
            sfx.PlayOneShot(clip, vol * master);
        }

        AudioClip Load(string path)
        {
            if (cache.TryGetValue(path, out var c)) return c;
            var clip = Resources.Load<AudioClip>(path);
            cache[path] = clip;
            return clip;
        }
    }
}

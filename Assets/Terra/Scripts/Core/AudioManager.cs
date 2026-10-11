using TerraTactix.Data;
using UnityEngine;

namespace TerraTactix.Core
{
    public sealed class AudioManager : MonoBehaviour
    {
        public bool MusicEnabled { get; private set; } = true;
        public bool SoundEnabled { get; private set; } = true;
        public float MusicVolume { get; private set; } = 1f;
        public float SoundVolume { get; private set; } = 1f;

        AudioSource music;
        AudioSource effects;
        AudioClip musicClip;
        AudioClip tapClip;
        AudioClip mergeClip;
        AudioClip clearClip;
        AudioClip comboLowClip;
        AudioClip comboMidClip;
        AudioClip comboHighClip;

        void OnEnable()
        {
            Ensure();
            GameEvents.MergePerformed += OnMerge;
            GameEvents.LineCleared += OnClear;
            GameEvents.BattleWon += OnWin;
        }

        void OnDisable()
        {
            GameEvents.MergePerformed -= OnMerge;
            GameEvents.LineCleared -= OnClear;
            GameEvents.BattleWon -= OnWin;
        }

        public void Apply(SettingsData settings)
        {
            Ensure();
            if (settings == null)
                return;
            MusicEnabled = settings.musicEnabled;
            SoundEnabled = settings.soundEnabled;
            MusicVolume = Mathf.Clamp01(settings.musicVolume);
            SoundVolume = Mathf.Clamp01(settings.soundVolume);
            ApplyMusicState();
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            var save = GameBootstrap.Instance.Save.Data;
            save.settings.musicVolume = MusicVolume;
            GameBootstrap.Instance.Save.Save();
            ApplyMusicState();
        }

        public void SetSoundVolume(float volume)
        {
            SoundVolume = Mathf.Clamp01(volume);
            var save = GameBootstrap.Instance.Save.Data;
            save.settings.soundVolume = SoundVolume;
            GameBootstrap.Instance.Save.Save();
        }

        public void SetMusic(bool enabled)
        {
            MusicEnabled = enabled;
            var save = GameBootstrap.Instance.Save.Data;
            save.settings.musicEnabled = enabled;
            GameBootstrap.Instance.Save.Save();
            ApplyMusicState();
        }

        public void SetSound(bool enabled)
        {
            SoundEnabled = enabled;
            var save = GameBootstrap.Instance.Save.Data;
            save.settings.soundEnabled = enabled;
            GameBootstrap.Instance.Save.Save();
        }

        public void PlayTap()
        {
            Play(tapClip, 0.35f);
        }

        public void PlayCombo(int tier)
        {
            Ensure();
            var clip = tier >= 20 ? comboHighClip : tier >= 15 ? comboMidClip : comboLowClip;
            Play(clip, tier >= 20 ? 0.82f : tier >= 10 ? 0.7f : 0.58f);
            if (SoundEnabled)
                HapticFeedback.Pulse(tier >= 15);
        }

        void OnMerge()
        {
            Play(mergeClip, 0.45f);
            if (SoundEnabled)
                HapticFeedback.Pulse(false);
        }

        void OnClear(int count)
        {
            Play(clearClip, 0.55f);
            if (SoundEnabled)
                HapticFeedback.Pulse(true);
        }
        void OnWin() => Play(clearClip, 0.7f);

        void Ensure()
        {
            if (music != null)
                return;

            musicClip = Tone(new[] { 523.25f, 659.25f, 783.99f, 659.25f }, 0.35f, 0.08f);
            tapClip = Tone(new[] { 880f }, 0.08f, 0.2f);
            mergeClip = Tone(new[] { 660f, 880f }, 0.07f, 0.18f);
            clearClip = Tone(new[] { 523f, 659f, 784f }, 0.09f, 0.16f);
            comboLowClip = Tone(new[] { 659f, 880f }, 0.1f, 0.2f);
            comboMidClip = Tone(new[] { 523f, 659f, 784f, 1046f }, 0.09f, 0.2f);
            comboHighClip = Tone(new[] { 523f, 659f, 784f, 1046f, 1318f }, 0.08f, 0.22f);

            music = gameObject.AddComponent<AudioSource>();
            music.clip = musicClip;
            music.loop = true;
            music.volume = 0.18f * MusicVolume;
            music.playOnAwake = false;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            effects.volume = 0.55f;
            ApplyMusicState();
        }

        void ApplyMusicState()
        {
            if (music == null)
                return;
            music.volume = 0.18f * MusicVolume;
            if (MusicEnabled)
            {
                if (!music.isPlaying)
                    music.Play();
            }
            else if (music.isPlaying)
            {
                music.Stop();
            }
        }

        void Play(AudioClip clip, float volume)
        {
            if (!SoundEnabled || clip == null || effects == null)
                return;
            effects.PlayOneShot(clip, volume * SoundVolume);
        }

        static AudioClip Tone(float[] notes, float noteLength, float amplitude)
        {
            const int rate = 22050;
            var samplesPerNote = Mathf.Max(1, Mathf.RoundToInt(rate * noteLength));
            var data = new float[samplesPerNote * notes.Length];
            for (var n = 0; n < notes.Length; n++)
            {
                for (var i = 0; i < samplesPerNote; i++)
                {
                    var t = i / (float)rate;
                    var envelope = 1f - i / (float)samplesPerNote;
                    data[n * samplesPerNote + i] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * amplitude * envelope;
                }
            }

            var clip = AudioClip.Create("terra-tone", data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}

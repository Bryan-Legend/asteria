using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework;

namespace HD
{
    public static class Audio
    {
        static SoundEffectInstance playingSongInstance;
        static SoundEffectInstance fadingSongInstance;
        static DateTime fadingStart;

        public static string PlayingSong;

        public static Dictionary<Sound, SoundEffect> Sounds;
        public static Dictionary<Sound, float> SoundDefaultVolumes;
        public static Dictionary<string, SoundEffect> Songs = new Dictionary<string, SoundEffect>();

        internal static void LoadContent(ContentManager content)
        {
            Utility.LogMessage("Loading audio.");
            Sounds = new Dictionary<Sound, SoundEffect>();
            SoundDefaultVolumes = new Dictionary<Sound, float>();

            foreach (Sound sound in Utility.GetEnumValues<Sound>()) {
                float volume;
                var soundEffect = Utility.LoadSoundEffect(sound.ToString(), out volume);
                Sounds[sound] = soundEffect;
                SoundDefaultVolumes[sound] = volume;
            }

            foreach (var enemyType in EnemyBase.Types) {
                enemyType.SpawnSound = Utility.LoadSoundEffect(enemyType.Name + "Spawn", out enemyType.SpawnSoundVolume);
                enemyType.AttackSound = Utility.LoadSoundEffect(enemyType.Name + "Attack", out enemyType.AttackSoundVolume);
                enemyType.DamageSound = Utility.LoadSoundEffect(enemyType.Name + "Damage", out enemyType.DamageSoundVolume);
                enemyType.DieSound = Utility.LoadSoundEffect(enemyType.Name + "Die", out enemyType.DieSoundVolume);
            }

            Songs["Above Ground"] = Utility.LoadSong("Above Ground");
            Songs["Battle"] = Utility.LoadSong("Battle");
            Songs["Botanica"] = Utility.LoadSong("Botanica");
            Songs["Credits"] = Utility.LoadSong("Credits");
            Songs["Krawnix Lair"] = Utility.LoadSong("Krawnix Lair");
            Songs["Start"] = Utility.LoadSong("Start");
            Songs["Underground"] = Utility.LoadSong("Underground");
            Songs["Boss Fight"] = Utility.LoadSong("Boss Fight");
        }

        const double fadeLengthSeconds = 5;

        public static void UpdateMusic(GameTime gameTime)
        {
            if (fadingSongInstance != null) {
                var fadeTime = DateTime.UtcNow - fadingStart;
                var fadePosition = fadeTime.TotalSeconds / fadeLengthSeconds;

                //Utility.LogMessage(fadePosition.ToString());

                if (fadePosition > 1) {
                    fadingSongInstance.Stop();
                    fadingSongInstance.Dispose();
                    fadingSongInstance = null;
                    if (playingSongInstance != null)
                        playingSongInstance.Volume = GameSettings.Default.MusicVolume;
                } else {
                    fadingSongInstance.Volume = GameSettings.Default.MusicVolume * (1 - (float)fadePosition);
                    if (playingSongInstance != null)
                        playingSongInstance.Volume = GameSettings.Default.MusicVolume * (float)fadePosition;
                }
            }

            if (Main.Map == null) {
                PlaySong("Menu & Skyrealm");
            } else {
                if (Main.Player != null && Main.Player.IsFightingBoss()) {
                    PlaySong("Boss Fight");
                } else {
                    if (Main.Map.Music == "Overworld") {
                        if (Main.Player.Position.Y <= Main.Map.SeaLevelInPixels) {
                            PlaySong("Above Ground");
                        } else {
                            if (Main.Player.Position.Y > Main.Map.SeaLevelInPixels + 1000) {
                                PlaySong("Underground");
                            } else {
                                if (PlayingSong != "Above Ground" && PlayingSong != "Underground")
                                    PlaySong("Above Ground");
                            }
                        }
                    } else {
                        if (Main.Map.Music != null) {
                            PlaySong(Main.Map.Music);
                        }
                    }
                }
            }
        }

        internal static void PlaySong(string songName)
        {
            if (PlayingSong != songName) {
                if (!Songs.ContainsKey(songName))
                    return;

                var song = Songs[songName];
                if (song == null || song.IsDisposed)
                    return;

                if (playingSongInstance != null)
                    StopSong();

                playingSongInstance = song.CreateInstance();
                playingSongInstance.IsLooped = true;
                playingSongInstance.Play();
                playingSongInstance.Volume = 0;
                PlayingSong = songName;

                //Utility.LogMessage("Music set to " + songName);
            }

            if (playingSongInstance != null && playingSongInstance.Volume != GameSettings.Default.MusicVolume && fadingSongInstance == null)
                playingSongInstance.Volume = GameSettings.Default.MusicVolume;
        }

        internal static void StopSong()
        {
            if (playingSongInstance != null) {
                if (fadingSongInstance != null) {
                    fadingSongInstance.Stop();
                    fadingSongInstance.Dispose();
                }

                fadingSongInstance = playingSongInstance;
                fadingStart = DateTime.UtcNow;

                PlayingSong = null;
                playingSongInstance = null;
            }
        }

        internal static void PlaySound(SoundEvent soundEvent)
        {
            if (soundEvent.EnemyTypeId != 0) {
                var enemyType = EnemyBase.Get(soundEvent.EnemyTypeId);

                if (enemyType.SoundName != null)
                    enemyType = EnemyBase.Get(enemyType.SoundName);

                switch (soundEvent.Sound) {
                    case Sound.EnemySpawn:
                        PlaySound(enemyType.SpawnSound, enemyType.SpawnSoundVolume);
                        return;
                    case Sound.EnemyAttack:
                        PlaySound(enemyType.AttackSound, enemyType.AttackSoundVolume);
                        return;
                    case Sound.EnemyDamage:
                        PlaySound(enemyType.DamageSound, enemyType.DamageSoundVolume);
                        return;
                    case Sound.EnemyDie:
                        PlaySound(enemyType.DieSound, enemyType.DieSoundVolume);
                        return;
                }
            }
            PlaySound(soundEvent.Sound, soundEvent.Volume);
        }

        internal static void PlaySound(SoundEffect sound, float volume = 1)
        {
            if (sound != null) {
                if (volume > 1)
                    volume = 1;

                sound.Play(volume * GameSettings.Default.SoundEffectVolume, 0, 0);
            }
        }

        internal static void PlaySound(Sound soundId, float volume = 0)
        {
            if (Sounds.ContainsKey(soundId)) {
                var sound = Sounds[soundId];
                if (sound != null) {
                    if (volume == 0)
                        volume = SoundDefaultVolumes[soundId];
                    if (volume > 1)
                        volume = 1;

                    sound.Play(volume * GameSettings.Default.SoundEffectVolume, 0, 0);
                } else
                    Utility.LogMessage("Sound " + soundId + " not found.");
            }
        }

        internal static SoundEffectInstance PlayLoopedSound(Sound soundId, float volume = 0)
        {
            if (Sounds.ContainsKey(soundId)) {
                var sound = Sounds[soundId];
                if (sound != null) {
                    if (volume == 0)
                        volume = SoundDefaultVolumes[soundId];
                    if (volume > 1)
                        volume = 1;

                    var result = sound.CreateInstance();
                    result.Volume = volume * GameSettings.Default.SoundEffectVolume;
                    result.IsLooped = true;
                    result.Play();
                    return result;
                } else
                    Utility.LogMessage("Sound " + soundId + " not found.");
            }
            return null;
        }

        internal static void PlaySoundEvents(List<SoundEvent> sounds)
        {
            lock (sounds) {
                foreach (var soundEvent in sounds)
                    Audio.PlaySound(soundEvent);
                sounds.Clear();
            }
        }

        internal static void PlaySoundEvents(SoundEvent[] sounds)
        {
            foreach (var soundEvent in sounds)
                Audio.PlaySound(soundEvent);
        }
    }
}
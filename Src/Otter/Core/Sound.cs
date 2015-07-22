using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Audio;
using System.IO;

namespace Otter {
    /// <summary>
    /// Class used to play a sound from a file or an IO Stream. Sounds are cached if loaded from a file.
    /// </summary>
    public class Sound {

        #region Static Fields

        /// <summary>
        /// The global volume of all sounds.
        /// </summary>
        public static float GlobalVolume = 1f;

        #endregion

        #region Private Fields

        SFML.Audio.Sound sound;
        SoundBuffer buffer;

        #endregion

        #region Public Fields

        /// <summary>
        /// The local volume of this sound.
        /// </summary>
        public float Volume = 1f;

        #endregion

        #region Public Properties

        /// <summary>
        /// Adjust the pitch of the sound. Default value is 1.
        /// </summary>
        public float Pitch {
            set { sound.Pitch = value; }
            get { return sound.Pitch; }
        }

        /// <summary>
        /// The playback offset of the sound in milliseconds.
        /// </summary>
        public int Offset {
            set { sound.PlayingOffset = new TimeSpan(0, 0, 0, 0, value); }
            get { return sound.PlayingOffset.Milliseconds; }
        }

        /// <summary>
        /// Determines if the sound should loop or not.
        /// </summary>
        public bool Loop {
            set { sound.Loop = value; }
            get { return sound.Loop; }
        }

        /// <summary>
        /// The duration of the sound in milliseconds.
        /// </summary>
        public int Duration {
            get { return (int)sound.SoundBuffer.Duration; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Load a new sound from a filepath. If this file has been used before it will be loaded from the cache.
        /// </summary>
        /// <param name="source"></param>
        public Sound(string source, bool loop = false) {
            buffer = Sounds.Load(source);
            sound = new SFML.Audio.Sound(buffer);
            Loop = loop;
            sound.RelativeToListener = false;
        }

        /// <summary>
        /// Load a new sound from an IO Stream.
        /// </summary>
        /// <param name="stream"></param>
        public Sound(Stream stream) {
            buffer = new SoundBuffer(stream);
            sound = new SFML.Audio.Sound(buffer);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Play the sound.
        /// </summary>
        public void Play() {
            sound.Volume = Util.Clamp(GlobalVolume * Volume, 0f, 1f) * 100f;
            sound.Play();
        }

        /// <summary>
        /// Stop the sound.
        /// </summary>
        public void Stop() {
            sound.Stop();
        }

        /// <summary>
        /// Pause the sound.
        /// </summary>
        public void Pause() {
            sound.Pause();
        }

        #endregion
        
    }
}

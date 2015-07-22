using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Audio;
using System.IO;

namespace Otter {

    #region Sounds

    /// <summary>
    /// Class that manages the cache of sounds.
    /// </summary>
    class Sounds {
        static Dictionary<string, SoundBuffer> sounds = new Dictionary<string, SoundBuffer>();

        public static SoundBuffer Load(string source) {
            if (!File.Exists(source)) throw new FileNotFoundException(source + " not found.");
            if (sounds.ContainsKey(source)) {
                return sounds[source];
            }
            sounds.Add(source, new SoundBuffer(source));
            return sounds[source];
        }
    }

    #endregion

    #region Fonts

    /// <summary>
    /// Class that manages the cache of fonts.
    /// </summary>
    class Fonts {
        static Dictionary<string, Font> fonts = new Dictionary<string, Font>();
        static Dictionary<Stream, Font> fontsStreamed = new Dictionary<Stream, Font>();

        public static Font DefaultFont {
            get {
                if (defaultFont == null) {
                    defaultFont = Load(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Otter.CONSOLA.TTF"));
                }
                return defaultFont;
            }
        }
        static Font defaultFont;

        public static Font Load(string source) {
            if (!File.Exists(source)) throw new FileNotFoundException(source + " not found.");
            if (fonts.ContainsKey(source)) {
                return fonts[source];
            }
            fonts.Add(source, new Font(source));
            return fonts[source];
        }

        public static Font Load(Stream stream) {
            if (fontsStreamed.ContainsKey(stream)) {
                return fontsStreamed[stream];
            }
            fontsStreamed.Add(stream, new Font(stream));
            return Load(stream);
        }
    }

    #endregion

    /// <summary>
    /// Class that manages the cache of Textures.
    /// </summary>
    public class Textures {

        #region Static Fields

        static Dictionary<string, SFML.Graphics.Texture> textures = new Dictionary<string, SFML.Graphics.Texture>();
        static Dictionary<Stream, SFML.Graphics.Texture> texturesStreamed = new Dictionary<Stream, SFML.Graphics.Texture>();

        #endregion

        #region Static Methods

        /// <summary>
        /// This doesn't really work right now.  Textures in images wont update
        /// if you do this.
        /// </summary>
        /// <param name="source"></param>
        public static void Reload(string source) {
            textures.Remove(source);
            Load(source);
        }

        /// <summary>
        /// This doesn't work right now.  Textures in images wont update if you
        /// do this.
        /// </summary>
        public static void ReloadAll() {
            var keys = textures.Keys;
            textures.Clear();
            foreach (var k in keys) {
                Load(k);
            }
        }

        /// <summary>
        /// Tests to see if a file exists using multiple sources.  This also checks to see if
        /// the zip file for the game exists and contains the file.
        /// </summary>
        /// <param name="source">The filepath.</param>
        /// <returns>True if the file exists.</returns>
        public static bool Exists(string source) {
            if (source == null) {
                return false;
            }

            if (File.Exists(source)) {
                return true;
            }

            return false;
        }

        #endregion

        #region Internal

        internal static SFML.Graphics.Texture Load(string source) {
            if (!File.Exists(source)) throw new FileNotFoundException("Texture path " + source + " not found.");
            if (textures.ContainsKey(source)) {
                return textures[source];
            }
            textures.Add(source, new SFML.Graphics.Texture(source));
            return textures[source];
        }

        internal static SFML.Graphics.Texture Load(Stream stream) {
            if (texturesStreamed.ContainsKey(stream)) {
                return texturesStreamed[stream];
            }
            texturesStreamed.Add(stream, new SFML.Graphics.Texture(stream));

            return texturesStreamed[stream];
        }

        #endregion

    }
}

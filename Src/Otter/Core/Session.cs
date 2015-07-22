using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;

namespace Otter {
    /// <summary>
    /// Class that represents a player session.  Use this for maintaining and using information about
    /// a player.  For example a two player game might have two sessions, one for each player, each with
    /// their own controls configured and save data.
    /// </summary>
    public class Session {

        #region Static Fields

        static private int nextSessionId = 0;

        static string defaultPath;

        #endregion

        #region Public Fields

        /// <summary>
        /// The string to use when delimiting key data in data exports.
        /// </summary>
        public string KeyDelim = "::OTTERK::";

        /// <summary>
        /// The string to use when delimiting value data in data exports.
        /// </summary>
        public string ValueDelim = "::OTTERV::";

        /// <summary>
        /// The phrase to use as a salt when encrypting the data exports.
        /// </summary>
        public string EncryptionSalt = "otter";

        /// <summary>
        /// The name of this session. This is important as it will determine the name of save data
        /// files and you can also find a session by name.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The controller to use for this Session.
        /// </summary>
        public Controller Controller = new Controller();

        /// <summary>
        /// The guide to salt the data string.  {S} is the salt, {D} is the data.
        /// It is recommended to change this from the default for your game, but
        /// only if you really care about hacking save data.
        /// </summary>
        public static string SaltGuide = "{S}{D}{S}";

        #endregion

        #region Public Properties

        /// <summary>
        /// The Id of this session in the Game.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The game that manages this session.
        /// </summary>
        public Game Game { get; internal set; }

        /// <summary>
        /// The data to store in this Session.  Must be converted to string to store for the file
        /// exports and imports.
        /// </summary>
        public Dictionary<string, string> Data { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new Session.
        /// </summary>
        /// <param name="game">The Game that the session is tied to.</param>
        public Session(Game game) {
            Data = new Dictionary<string, string>();
            Game = game;

            Id = nextSessionId;
            nextSessionId++;

            defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the full path of a file associated with this session.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string DataPath(string filename) {
            if (!Directory.Exists(defaultPath + Game.GameFolder)) {
                Directory.CreateDirectory(defaultPath + Game.GameFolder);
            }
            return defaultPath + "/" + Game.GameFolder + "/" + Name + "." + filename + ".dat";
        }

        /// <summary>
        /// Verifies that the data has a valid hash.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool VerifyData(string data) {
            string[] split = Regex.Split(data, ":");

            if (split.Length != 2) return false;

            string dataToHash = SaltGuide;
            dataToHash = dataToHash.Replace("{S}", EncryptionSalt);
            dataToHash = dataToHash.Replace("{D}", split[1]);

            string hash = Util.MD5Hash(dataToHash);

            if (hash == split[0]) return true;

            return false;
        }

        /// <summary>
        /// Save this session's data to a file.
        /// </summary>
        /// <param name="filename">The file to save to.</param>
        public void SaveData(string filename = "data") {
            string filepath = DataPath(filename);

            string str = Util.CompressString(Util.DictionaryToString(Data, KeyDelim, ValueDelim));

            string dataToHash = SaltGuide;
            dataToHash = dataToHash.Replace("{S}", EncryptionSalt);
            dataToHash = dataToHash.Replace("{D}", str);

            str = Util.MD5Hash(dataToHash) + ":" + str;

            File.WriteAllText(filepath, str);
        }

        /// <summary>
        /// Load this session's data from a file.
        /// </summary>
        /// <param name="filename">The file to load from.</param>
        public void LoadData(string filename = "data") {
            string filepath = DataPath(filename);

            if (!File.Exists(filepath)) return;

            string loaded = File.ReadAllText(filepath);

            if (VerifyData(loaded)) {
                string[] split = Regex.Split(loaded, ":");
                loaded = Util.DecompressString(split[1]);
                Data = Util.StringToDictionary(loaded, KeyDelim, ValueDelim);
            }
            else {
                Util.Log("Data load failed: corrupt or modified data.");
            }
        }

        /// <summary>
        /// Returns data from the session.  If there is no key, return the onNull value.
        /// </summary>
        /// <param name="key">The key!</param>
        /// <param name="onNull">What to return if there's no key found.  This is added to the data.</param>
        /// <returns>The value of the key, or onNull if that key does not exist in the data.</returns>
        public string GetData(string key, string onNull = "") {
            if (Data.ContainsKey(key)) {
                return Data[key];
            }
            else {
                Data.Add(key, onNull);
                return onNull;
            }
        }

        /// <summary>
        /// Set the data in the session.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The data.</param>
        public void SetData(string key, string value) {
            if (!Data.ContainsKey(key)) {
                Data[key] = value;
            }
            Data[key] = value;
        }

        #endregion

        #region Internal

        internal void Update() {
            if (Controller != null) {
                Controller.UpdateFirst();
            }
        }

        #endregion
        
    }
}

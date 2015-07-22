using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Otter {
    /// <summary>
    /// Class used for animations in Spritemap.
    /// </summary>
    public class Anim {

        #region Static Methods

        /// <summary>
        /// Returns an array of numbers from min to max.  Useful for passing in arguments for long animations.
        /// </summary>
        /// <param name="min">The start of the animation (includes this number.)</param>
        /// <param name="max">The end of the animation (includes this number.)</param>
        /// <returns>The array of ints representing an animation.</returns>
        public static int[] FramesRange(int min, int max) {
            int[] f = new int[max - min + 1];
            for (int i = min; i <= max; i++) {
                f[i - min] = i;
            }
            return f;
        }

        #endregion

        #region Private Fields

        bool pingPong;
        int loopBack = 0;
        float timer;
        float delay;
        int direction;
        int repeatsCounted = 0;

        int currentFrame;

        #endregion

        #region Public Fields

        public Action OnComplete;

        public bool Active;

        #endregion

        #region Public Properties

        /// <summary>
        /// The overall playback speed of the animation.
        /// </summary>
        public float PlaybackSpeed { get; private set; }

        /// <summary>
        /// The repeat count of the animation.
        /// </summary>
        public int RepeatCount { get; private set; }

        /// <summary>
        /// The frames used in the animation.
        /// </summary>
        public List<int> Frames { get; private set; }

        /// <summary>
        /// The frame delays used in the animation.
        /// </summary>
        public List<float> FrameDelays { get; private set; }

        /// <summary>
        /// The total number of frames in this animation.
        /// </summary>
        public int FrameCount {
            get { return Frames.Count; }
        }

        /// <summary>
        /// The current frame of the animation.
        /// </summary>
        public int CurrentFrame {
            get { return Frames[currentFrame]; }
        }

        public int CurrentFrameIndex {
            get { return currentFrame; }
            set { currentFrame = value; }
        }

        /// <summary>
        /// The total duration of the animation.
        /// </summary>
        public float TotalDuration {
            get {
                float delay = 0;
                foreach (float d in FrameDelays) {
                    delay += d;
                }
                return delay;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new Anim with an array of ints for frames, and an array of floats for frameDelays.
        /// </summary>
        /// <param name="frames">The frames from the sprite sheet to display.</param>
        /// <param name="frameDelays">The time that each frame should be displayed.</param>
        public Anim(int[] frames, float[] frameDelays = null) {
            Initialize(frames, frameDelays);
        }

        /// <summary>
        /// Creates a new Anim with a string of ints for frames, and a string of floats for frameDelays.
        /// </summary>
        /// <param name="frames">A string of frames separated by a delim character.  Example: "0,1,2,3,4,5"</param>
        /// <param name="frameDelays">A string of floats separated by a delim character.  Example: "0.5f,1,0.5f,1"</param>
        /// <param name="delim">The string of characters to parse the string by.  Default is ","</param>
        public Anim(string frames, string frameDelays, string delim = ",") {
            string[] frameParts = Regex.Split(frames.Replace(" ", ""), delim);
            string[] frameDelaysParts = Regex.Split(frameDelays, delim);

            int[] framesint = new int[frameParts.Length];

            for (int i = 0; i < frameParts.Length; i++) {
                framesint[i] = int.Parse(frameParts[i]);
            }

            float[] framedelaysfloat = new float[frameDelaysParts.Length];

            for (int i = 0; i < frameDelaysParts.Length; i++) {
                framedelaysfloat[i] = float.Parse(frameDelaysParts[i]);
            }

            Initialize(framesint, framedelaysfloat);
        }

        #endregion

        #region Private Methods

        void Initialize(int[] frames, float[] frameDelays = null) {
            Frames = new List<int>();
            FrameDelays = new List<float>();

            for (int i = 0; i < frames.Length; i++) {
                Frames.Add(frames[i]);
                if (frameDelays != null) {
                    if (i >= frameDelays.Length) {
                        FrameDelays.Add(frameDelays[i % frameDelays.Length]);
                    }
                    else {
                        FrameDelays.Add(frameDelays[i]);
                    }
                }
                else {
                    FrameDelays.Add(1);
                }
            }

            RepeatCount = -1;
            timer = 0;
            delay = 0;
            direction = 1;
            PlaybackSpeed = 1;

            Active = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines how many times this animation loops.  -1 for infinite.
        /// </summary>
        /// <param name="times">How many times the animation should repeat.</param>
        /// <returns>The anim object.</returns>
        public Anim Repeat(int times = -1) {
            RepeatCount = times;
            return this;
        }

        /// <summary>
        /// Disables repeating.  Animations default to repeat on.
        /// </summary>
        /// <returns>The anim object.</returns>
        public Anim NoRepeat() {
            RepeatCount = 0;
            return this;
        }

        /// <summary>
        /// Determines if the animation will repeat by going back and forth between the start and end.
        /// </summary>
        /// <param name="pingpong">True for yes, false for no no no.</param>
        /// <returns>The anim object.</returns>
        public Anim PingPong(bool pingpong = true) {
            pingPong = pingpong;
            return this;
        }

        /// <summary>
        /// Determines the playback speed of the animation.  1 = 1 frame.
        /// </summary>
        /// <param name="speed">The new speed.</param>
        /// <returns>The anim object.</returns>
        public Anim Speed(float speed) {
            PlaybackSpeed = speed;
            return this;
        }

        /// <summary>
        /// Determines which frame the animation will loop back to when it repeats.
        /// </summary>
        /// <param name="frame">The frame to loop back to (from 0 to frame count - 1)</param>
        /// <returns>The anim object.</returns>
        public Anim LoopBackTo(int frame = 0) {
            loopBack = frame;
            return this;
        }

        /// <summary>
        /// Stops the animation and returns it to the first frame.
        /// </summary>
        /// <returns>The anim object.</returns>
        public Anim Stop() {
            Active = false;
            currentFrame = 0;
            return this;
        }

        /// <summary>
        /// Resets the animation back to frame 0 but does not stop it.
        /// </summary>
        /// <returns>The anim object.</returns>
        public Anim Reset() {
            timer = 0;
            currentFrame = 0;
            return this;
        }

        /// <summary>
        /// Updates the Anim object.  Handled by the Spritemap usually.  If this doesn't run the animation will not play.
        /// </summary>
        /// <param name="t">The elapsed time.</param>
        public void Update(float t = 1f) {
            if (Active) {
                timer += PlaybackSpeed * Game.Instance.DeltaTime * t;
            }

            delay = FrameDelays[currentFrame];

            while (timer >= delay) {
                timer -= delay;
                currentFrame += direction;

                if (currentFrame == Frames.Count) {
                    if (repeatsCounted < RepeatCount || RepeatCount < 0) {
                        repeatsCounted++;
                        if (pingPong) {
                            direction *= -1;
                            currentFrame = Frames.Count - 2;
                        }
                        else {
                            currentFrame = loopBack;
                        }
                    }
                    else {
                        if (pingPong) {
                            direction *= -1;
                            currentFrame = Frames.Count - 2;
                        }
                        else {
                            if (OnComplete != null) {
                                OnComplete();
                            }
                            Stop();
                            currentFrame = Frames.Count - 1;
                        }
                    }
                }

                if (currentFrame < loopBack) {
                    if (pingPong) {
                        if (repeatsCounted < RepeatCount || RepeatCount < 0) {
                            repeatsCounted++;
                            direction *= -1;
                            currentFrame = loopBack + 1;
                        }
                        else {
                            if (OnComplete != null) {
                                OnComplete();
                            }
                            Stop();
                        }
                    }
                }
            }
        }

        #endregion

    }
}
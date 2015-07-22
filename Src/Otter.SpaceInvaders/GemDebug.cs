using System;

namespace Otter.SpaceInvaders
{
    public class GemDebug
    {
        private static bool _showHitboxes;

        /// <summary>
        /// Set to True to show hitboxes.
        /// The logic must be implemented in the entity.
        /// </summary>
        public static bool ShowHitboxes
        {
            get { return _showHitboxes; }
            set
            {
                _showHitboxes = value;
                Util.Watch("gemdebug::showhitboxes", _showHitboxes);
            }
        }

        /// <summary>
        /// Command to show hitboxes.
        /// Debugger.Instance.RegisterCommand("showhitboxes", "Toggles the visibility of hitboxes.", GemDebug.CmdHitboxes, CommandType.Bool);
        /// </summary>
        /// <param name="a"></param>
        public static void CmdHitboxes(params string[] a)
        {
            if (a.Length == 0)
            {
                ShowHitboxes = !ShowHitboxes;
                Util.Log("Toggled hitboxes to " + ShowHitboxes);
                return;
            }

            Util.Log("Hitbox visibility set to " + a[0]);
            Util.ShowDebugger();
            ShowHitboxes = bool.Parse(a[0]);
        }

        /// <summary>
        /// Command to change the window resolution, and fullscreen status.
        /// Debugger.Instance.RegisterCommand("setwindow", "Sets the window resolution. -width -height -fullscreen", GemDebug.CmdResolutionChange, CommandType.Int, CommandType.Int, CommandType.Bool);
        /// </summary>
        /// <param name="a"></param>
        public static void CmdResolutionChange(params string[] a)
        {
            if (int.Parse(a[0]) == 0 || int.Parse(a[1]) == 0)
                throw new ArgumentOutOfRangeException((int.Parse(a[0]) == 0) ? "first argument" : "second argument", "Cannot set resolution to 0 pixels");

            Util.Log("Setting resolution to " + a[0] + " pixels wide and " + a[1] + " pixels tall. Fullscreen: " + a[2]);
            Game.Instance.SetWindow(int.Parse(a[0]), int.Parse(a[1]), bool.Parse(a[2]));
            Util.ShowDebugger();
        }

        /// <summary>
        /// Command to change the window scale.
        /// Debugger.Instance.RegisterCommand("setwinscale", "Sets the window scale.", GemDebug.CmdResolutionScale, CommandType.Float);
        /// </summary>
        /// <param name="a"></param>
        public static void CmdResolutionScale(params string[] a)
        {
            if (float.Parse(a[0]) == 0.0f)
                throw new ArgumentOutOfRangeException("float", "Cannot set resolution to 0 pixels.");

            Util.Log("Setting window scale to " + a[0] + ".");
            Game.Instance.SetWindowScale(float.Parse(a[0]));
            Util.ShowDebugger();
        }
    }
}
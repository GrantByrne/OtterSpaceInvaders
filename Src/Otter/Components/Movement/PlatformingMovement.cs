using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otter {
    /// <summary>
    /// Movement Component that adds platforming movement behavior to an Entity.  This is built for
    /// fixed framerate applications.
    /// </summary>
    public class PlatformingMovement : Movement {

        #region Private Fields

        int jumpBuffer = 0;
        int ledgeBuffer = 0;
        List<Speed> Speeds = new List<Speed>();

        #endregion

        #region Public Fields

        /// <summary>
        /// The main input speed of the platforming movement.
        /// </summary>
        public Speed Speed;

        /// <summary>
        /// Any extra speed applied (from boosts, dashes, springs, conveyors, etc)
        /// </summary>
        public Speed ExtraSpeed;

        /// <summary>
        /// The target speed that the input speed will approach (used for Axis input)
        /// </summary>
        public Speed TargetSpeed;

        /// <summary>
        /// The acceleration applied from gravity.
        /// </summary>
        public float Gravity;

        /// <summary>
        /// The multiplication factor on the applied gravity.
        /// </summary>
        public float GravityMultiplier = 1;

        /// <summary>
        /// The burst of speed applied when jumping.
        /// </summary>
        public float JumpStrength = 1500;

        /// <summary>
        /// If the object is currently on the ground (Y+1 overlaps the ground.)
        /// </summary>
        public bool OnGround = true;

        /// <summary>
        /// How many jumps are left to use.
        /// </summary>
        public int JumpsLeft = 0;

        /// <summary>
        /// The maximum number of jumps each time the object touches the ground.
        /// </summary>
        public int JumpsMax = 1;

        /// <summary>
        /// The maximum amount of frames to buffer jump input for the next available moment to jump.
        /// </summary>
        public int JumpBufferMax = 4;

        /// <summary>
        /// The maximum number of frames to allow the object to jump after leaving the ground.
        /// </summary>
        public int LedgeBufferMax = 4;

        /// <summary>
        /// Determines if the object is capable of jumping.
        /// </summary>
        public bool JumpEnabled = true;

        /// <summary>
        /// Determines if a double jump should add jump strength to the current jump, or set the Y
        /// speed to the jump speed.  For example, if true then an object traveling downward will
        /// jump up at full jump strength, if false it will jump at it's downward speed minus jump
        /// strenght.
        /// </summary>
        public bool HardDoubleJump = true;

        /// <summary>
        /// How much to dampen the Y speed when the object releases the jump button in the air.
        /// </summary>
        public float JumpDampening = 0.75f;

        /// <summary>
        /// If the object is in the air because it jumped (instead of falling off a ledge, etc)
        /// </summary>
        public bool HasJumped = false;

        /// <summary>
        /// Determines if the object can control its jump height by releasing jump while in the air.
        /// </summary>
        public bool VariableJumpHeight = true;

        /// <summary>
        /// Determines if the movement should listen to the Axis for input.
        /// </summary>
        public bool UseAxis = true;

        /// <summary>
        /// Determines if the movement should have gravity applied to it.
        /// </summary>
        public bool ApplyGravity = true;

        /// <summary>
        /// The Button used for the jumping input.
        /// </summary>
        public Button JumpButton;

        /// <summary>
        /// The axis used for movement input.
        /// </summary>
        public Axis Axis;

        /// <summary>
        /// An action that is triggered on a successful jump.
        /// </summary>
        public Action OnJump;

        /// <summary>
        /// The dictionary of acceleration values.
        /// </summary>
        public Dictionary<AccelType, int> Acceleration = new Dictionary<AccelType, int>();

        /// <summary>
        /// The default acceleration value to use if none are set.
        /// </summary>
        public static int DefaultAccleration = 150;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current acceleration value.
        /// </summary>
        public int CurrentAccel { get; protected set; }

        /// <summary>
        /// If the Collider is currently against a wall on the left.
        /// </summary>
        public bool AgainstWallLeft { get; private set; }

        /// <summary>
        /// If the Collider is currently against a wall on the right.
        /// </summary>
        public bool AgainstWallRight { get; private set; }

        /// <summary>
        /// If the Collider is currently against a ceiling above it.
        /// </summary>
        public bool AgainstCeiling { get; private set; }

        /// <summary>
        /// True for one update after the object has jumped.
        /// </summary>
        public bool JustJumped { get; private set; }

        /// <summary>
        /// The total X speed.
        /// </summary>
        public float SumSpeedX {
            get {
                float r = 0;
                foreach (var s in Speeds) {
                    r += s.X;
                }
                return r;
            }
        }

        /// <summary>
        /// The total Y speed.
        /// </summary>
        public float SumSpeedY {
            get {
                float r = 0;
                foreach (var s in Speeds) {
                    r += s.Y;
                }
                return r;
            }
        }
        
        #endregion

        #region Constructors

        /// <summary>
        /// Create a new PlatformingMovement.
        /// </summary>
        /// <param name="xSpeedMax">The maximum X input speed.</param>
        /// <param name="ySpeedMax">The maximum Y speed from jumping and gravity.</param>
        /// <param name="gravity">The acceleration caused by gravity.</param>
        public PlatformingMovement(float xSpeedMax, float ySpeedMax, float gravity) {
            Speed = new Speed(xSpeedMax, ySpeedMax);
            ExtraSpeed = new Speed(xSpeedMax, ySpeedMax);

            Speeds.Add(Speed);
            Speeds.Add(ExtraSpeed);

            TargetSpeed = new Speed(xSpeedMax, ySpeedMax);

            Gravity = gravity;

            JustJumped = false;

            Acceleration.Add(AccelType.Ground, DefaultAccleration);
            Acceleration.Add(AccelType.Air, DefaultAccleration / 4);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the movement.
        /// </summary>
        public override void Update() {
            base.Update();

            JustJumped = false;

            OnGround = Collider.Collide(Entity.X, Entity.Y + 1, CollidesWith) != null;
            AgainstWallLeft = Collider.Collide(Entity.X - 1, Entity.Y, CollidesWith) != null;
            AgainstWallRight = Collider.Collide(Entity.X + 1, Entity.Y, CollidesWith) != null;
            AgainstCeiling = Collider.Collide(Entity.X, Entity.Y - 1, CollidesWith) != null;

            if (OnGround) {
                HasJumped = false;
                JumpsLeft = JumpsMax;
                ledgeBuffer = LedgeBufferMax;
            }

            if (!OnGround) {
                ledgeBuffer = (int)Util.Approach(ledgeBuffer, 0, 1);
            }

            jumpBuffer = (int)Util.Approach(jumpBuffer, 0, 1);
            if (JumpEnabled) {
                if (JumpButton.Pressed) {
                    jumpBuffer = JumpBufferMax;
                }
                if (JumpButton.Up) {
                    jumpBuffer = 0;
                }

                if (jumpBuffer > 0) {
                    if (JumpsLeft > 0) {
                        if (OnJump != null) {
                            OnJump();
                        }
                        if (HardDoubleJump) {
                            Speed.Y = -JumpStrength;
                        }
                        else {
                            Speed.Y -= JumpStrength;
                        }
                        HasJumped = true;
                        JustJumped = true;
                        JumpsLeft--;
                        jumpBuffer = 0;
                    }
                }
            }

            if (!OnGround && Speed.Y < 0) {
                if (HasJumped) {
                    if (!JumpButton.Down && VariableJumpHeight) {
                        Speed.Y *= JumpDampening;
                    }
                }
            }

            if (!OnGround && !HasJumped && ledgeBuffer == 0) JumpsLeft = JumpsMax - 1;

            if (OnGround) {
                CurrentAccel = Acceleration[AccelType.Ground];
            }
            else {
                CurrentAccel = Acceleration[AccelType.Air];
            }

            if (UseAxis) {
                TargetSpeed.X = Axis.X * TargetSpeed.MaxX;

                Speed.X = Util.Approach(Speed.X, TargetSpeed.X, CurrentAccel);

                if (Speed.X < 0 && AgainstWallLeft) {
                    Speed.X = 0;
                }
                if (Speed.X > 0 && AgainstWallRight) {
                    Speed.X = 0;
                }
            }

            if (ApplyGravity) {
                if (!OnGround)
                    Speed.Y += Gravity * GravityMultiplier;
            }

            MoveXY((int)SumSpeedX, (int)SumSpeedY, Collider);
        }

        public override void MoveCollideX(Collider collider) {
            base.MoveCollideX(collider);
            Speed.X = 0;
            ExtraSpeed.X = 0;
        }

        public override void MoveCollideY(Collider collider) {
            base.MoveCollideY(collider);
            if (SumSpeedY > 0) OnGround = true;
            Speed.Y = 0;
            ExtraSpeed.Y = 0;
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// The different acceleration types.
    /// </summary>
    public enum AccelType {
        Ground,
        Air
    }

    #endregion
}

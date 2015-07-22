using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Otter {
    /// <summary>
    /// Component that runs a basic state machine.
    /// </summary>
    public class StateMachine : Component {

        #region Private Fields

        Dictionary<int, State> states = new Dictionary<int, State>();
        Dictionary<State, int> invertedstates = new Dictionary<State, int>();

        Dictionary<int, Dictionary<int, Action>> transitions = new Dictionary<int, Dictionary<int, Action>>();

        int nextStateId = 0;

        #endregion

        #region Private Properties

        State s {
            get {
                if (CurrentState == -1) {
                    return null;
                }
                return states[CurrentState];
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The id of the current state.
        /// </summary>
        public int CurrentState { get; private set; }

        /// <summary>
        /// Get the current State, or change it.
        /// </summary>
        public int State {
            get { return CurrentState; }
            set { ChangeState(value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new StateMachine.
        /// </summary>
        public StateMachine() {
            CurrentState = -1;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a State.
        /// </summary>
        /// <param name="state">The State to add.</param>
        /// <returns>The int id of the added State.</returns>
        public int AddState(State state) {
            states.Add(nextStateId, state);
            invertedstates.Add(state, nextStateId);
            state.Id = nextStateId;
            state.Machine = this;
            nextStateId++;
            return nextStateId - 1;
        }

        /// <summary>
        /// Add a State with a specific id.
        /// </summary>
        /// <param name="state">The State to add.</param>
        /// <param name="id">The id for the State.</param>
        /// <returns>The int id of the added State.</returns>
        public int AddState(State state, int id) {
            states.Add(id, state);
            invertedstates.Add(state, id);
            state.Machine = this;
            return id;
        }

        /// <summary>
        /// Add a transition callback for when going from one state to another.
        /// </summary>
        /// <param name="fromState">The State that is ending.</param>
        /// <param name="toState">The State that is starting.</param>
        /// <param name="function">The Action to run when the machine goes from the fromState to the toState.</param>
        public void AddTransition(int fromState, int toState, Action function) {
            if (transitions[fromState] == null) {
                transitions.Add(fromState, new Dictionary<int, Action>());
            }
            transitions[fromState].Add(toState, function);
        }

        /// <summary>
        /// Add a state and automatically grab functions by their name.
        /// </summary>
        /// <param name="names">The common name in all the functions, like "Walking" in "UpdateWalking" "EnterWalking" and "ExitWalking"</param>
        /// <returns>The id to refer to the state.</returns>
        public void AddState(params string[] names) {
            foreach (var name in names) {
                if (Entity == null) throw new ArgumentException("State Machine must be Added to an Entity first!");

                var state = new State();

                //Using reflection to find all the appropriate functions!
                MethodInfo mi;
                mi = Entity.GetType().GetMethod("Enter" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (mi != null) {
                    state.OnEnter = (Action)Delegate.CreateDelegate(typeof(Action), Entity, mi);
                }

                mi = Entity.GetType().GetMethod("Update" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (mi != null) {
                    state.OnUpdate = (Action)Delegate.CreateDelegate(typeof(Action), Entity, mi);
                }

                mi = Entity.GetType().GetMethod("Exit" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (mi != null) {
                    state.OnExit = (Action)Delegate.CreateDelegate(typeof(Action), Entity, mi);
                }

                //Using reflection to assign the id to the right property
                FieldInfo fi;
                fi = Entity.GetType().GetField("state" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                fi.SetValue(Entity, nextStateId);

                AddState(state);
            }
        }

        /// <summary>
        /// Add a black State.
        /// </summary>
        /// <returns>The int id of the added State.</returns>
        public int AddState() {
            return AddState(new State());
        }

        /// <summary>
        /// Add a new State from three Actions.
        /// </summary>
        /// <param name="onEnter">The Action to run when entering this state.</param>
        /// <param name="onUpdate">The Action to run when updating this state.</param>
        /// <param name="onExit">The Action to run when exiting this state.</param>
        /// <returns>The int id of the added State.</returns>
        public int AddState(Action onEnter, Action onUpdate, Action onExit) {
            return AddState(new State(onEnter, onUpdate, onExit));
        }

        /// <summary>
        /// Add a new State with just an update Action.
        /// </summary>
        /// <param name="onUpdate">The Action to run when updating this state.</param>
        /// <returns>The int id of the added State.</returns>
        public int AddState(Action onUpdate) {
            return AddState(new State(onUpdate));
        }

        /// <summary>
        /// Add a new state from three Actions with a specific id.
        /// </summary>
        /// <param name="onEnter">The Action to run when entering this state.</param>
        /// <param name="onUpdate">The Action to run when updating this state.</param>
        /// <param name="onExit">The Action to run when exiting this state.</param>
        /// <param name="id">The id for the State.</param>
        /// <returns>The int id of the added State.</returns>
        public int AddState(Action onEnter, Action onUpdate, Action onExit, int id) {
            return AddState(new State(onEnter, onUpdate, onExit), id);
        }

        /// <summary>
        /// Add a new state with just an update Action with a specific id.
        /// </summary>
        /// <param name="onUpdate">The Action to run when updating this state.</param>
        /// <param name="id">The id for the State.</param>
        /// <returns>The int id of the added State.</returns>
        public int AddState(Action onUpdate, int id) {
            return AddState(new State(onUpdate), id);
        }

        /// <summary>
        /// Update the StateMachine.
        /// </summary>
        public override void Update() {
            base.Update();
            if (CurrentState == -1) return;

            if (states.ContainsKey(CurrentState)) {
                s.Update();
            }
        }

        /// <summary>
        /// Change the state.  Exit will be called on the current state, then Enter on the new state.
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(int state) {
            if (CurrentState == state) return;

            Timer = 0;

            if (states.ContainsKey(state)) {
                if (s != null) {
                    s.Exit();
                }
                CurrentState = state;
                if (s == null) throw new NullReferenceException("Next state is null.");
                s.Enter();

                if (transitions.ContainsKey(state)) {
                    if (transitions[state].ContainsKey(s.Id)) {
                        transitions[state][s.Id]();
                    }
                }
            }
        }

        /// <summary>
        /// Change the state by reference to the State object.  Only works if the State is already registered.
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(State state) {
            if (invertedstates.ContainsKey(state)) {
                ChangeState(invertedstates[state]);
            }
        }

        #endregion
  
    }

    /// <summary>
    /// State machine that uses a specific type.  This is really meant for using an enum as your list of states.
    /// If an enum is used, the state machine will automatically populate the states using methods in the parent
    /// Entity that match the name of the enum values.
    /// </summary>
    /// <example>
    /// Say you have an enum named State, and it has the value "Walking"
    /// When the state machine is added to the Entity, it will match any methods named:
    /// EnterWalking
    /// UpdateWalking
    /// ExitWalking
    /// And use those to build the states.  This saves a lot of boilerplate set up code.
    /// </example>
    /// <typeparam name="TState">An enum of states.</typeparam>
    public class StateMachine<TState> : Component {

        #region Private Fields

        Dictionary<TState, State> states = new Dictionary<TState, State>();

        Dictionary<TState, Dictionary<TState, Action>> transitions = new Dictionary<TState, Dictionary<TState, Action>>();

        bool firstChange = true;

        #endregion

        #region Private Properties

        State s {
            get {
                if (!states.ContainsKey(CurrentState)) {
                    return null;
                }
                return states[CurrentState];
            }
        }

        #endregion

        #region Public Fields

        /// <summary>
        /// Determines if the StateMachine will autopopulate its states based off of the values of the Enum.
        /// </summary>
        public bool AutoPopulate = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current state.
        /// </summary>
        public TState CurrentState { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new StateMachine.
        /// </summary>
        public StateMachine() {
        }

        #endregion

        #region Public Methods

        public override void Added() {
            base.Added();

            if (AutoPopulate) {
                if (typeof(TState).IsEnum) {
                    foreach (TState value in Enum.GetValues(typeof(TState))) {
                        AddState(value);
                    }
                }
            }
        }

        /// <summary>
        /// Change the state.  Exit will be called on the current state followed by Enter on the new state.
        /// </summary>
        /// <param name="state">The state to change to.</param>
        public void ChangeState(TState state) {
            if (!firstChange) {
                if (states.ContainsKey(CurrentState)) {
                    if (states[CurrentState] == states[state]) return;
                }
            }

            Timer = 0;

            var fromState = CurrentState;

            if (states.ContainsKey(state)) {
                if (s != null && !firstChange) {
                    s.Exit();
                }
                CurrentState = state;
                if (s == null) throw new NullReferenceException("Next state is null.");
                s.Enter();

                if (transitions.ContainsKey(fromState)) {
                    if (transitions[fromState].ContainsKey(state)) {
                        transitions[fromState][state]();
                    }
                }
            }

            if (firstChange) {
                firstChange = false;
            }
        }

        /// <summary>
        /// Update the State Machine.
        /// </summary>
        public override void Update() {
            base.Update();
            if (states.ContainsKey(CurrentState)) {
                s.Update();
            }
        }

        /// <summary>
        /// Add a transition callback for when going from one state to another.
        /// </summary>
        /// <param name="fromState">The State that is ending.</param>
        /// <param name="toState">The State that is starting.</param>
        /// <param name="function">The Action to run when the machine goes from the fromState to the toState.</param>
        public void AddTransition(TState fromState, TState toState, Action function) {
            if (transitions[fromState] == null) {
                transitions.Add(fromState, new Dictionary<TState, Action>());
            }
            transitions[fromState].Add(toState, function);
        }

        /// <summary>
        /// Add a state with three Actions.
        /// </summary>
        /// <param name="key">The key to reference the State with.</param>
        /// <param name="onEnter">The method to call when entering this state.</param>
        /// <param name="onUpdate">The method to call when updating this state.</param>
        /// <param name="onExit">The method to call when exiting this state.</param>
        public void AddState(TState key, Action onEnter, Action onUpdate, Action onExit) {
            states.Add(key, new State(onEnter, onUpdate, onExit));
        }

        /// <summary>
        /// Add a state with just an update Action.
        /// </summary>
        /// <param name="key">The key to reference the State with.</param>
        /// <param name="onUpdate">The method to call when updating this state.</param>
        public void AddState(TState key, Action onUpdate) {
            states.Add(key, new State(onUpdate));
        }

        /// <summary>
        /// Add a state.
        /// </summary>
        /// <param name="key">The key to reference the State with.</param>
        /// <param name="value">The State to add.</param>
        public void AddState(TState key, State value) {
            states.Add(key, value);
        }

        /// <summary>
        /// Add a state using reflection to retrieve the approriate methods on the Entity.
        /// For example, a key with a value of "Idle" will retrieve the methods "EnterIdle" "UpdateIdle" and "ExitIdle" automatically.
        /// </summary>
        /// <param name="key">The key to reference the State with.</param>
        public void AddState(TState key) {
            var state = new State();
            var name = key.ToString();
            //Using reflection to find all the appropriate functions!
            MethodInfo mi;
            mi = Entity.GetType().GetMethod("Enter" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (mi == null) Entity.GetType().GetMethod("Enter" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (mi != null) {
                state.OnEnter = (Action)Delegate.CreateDelegate(typeof(Action), Entity, mi);
            }

            mi = Entity.GetType().GetMethod("Update" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (mi == null) Entity.GetType().GetMethod("Update" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (mi != null) {
                state.OnUpdate = (Action)Delegate.CreateDelegate(typeof(Action), Entity, mi);
            }

            mi = Entity.GetType().GetMethod("Exit" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (mi == null) Entity.GetType().GetMethod("Exit" + name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (mi != null) {
                state.OnExit = (Action)Delegate.CreateDelegate(typeof(Action), Entity, mi);
            }

            states.Add(key, state);
        }

        #endregion

    }

    /// <summary>
    /// Used in StateMachine. Contains functions for enter, update, and exit.
    /// </summary>
    public class State {

        #region Public Fields

        /// <summary>
        /// The method to call when entering this state.
        /// </summary>
        public Action OnEnter = delegate { };

        /// <summary>
        /// The method to call when updating this state.
        /// </summary>
        public Action OnUpdate = delegate { };

        /// <summary>
        /// The method to call when exiting this state.
        /// </summary>
        public Action OnExit = delegate { };

        #endregion

        #region Public Properties

        /// <summary>
        /// The Id that this state has been assigned.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The StateMachine that owns this State.
        /// </summary>
        public StateMachine Machine { get; internal set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new State with three Actions.
        /// </summary>
        /// <param name="onEnter">The method to call when entering this state.</param>
        /// <param name="onUpdate">The method to call when updating this state.</param>
        /// <param name="onExit">The method to call when exiting this state.</param>
        public State(Action onEnter = null, Action onUpdate = null, Action onExit = null) {
            Functions(onEnter, onUpdate, onExit);
        }
         
        /// <summary>
        /// Create a new State with just an update Action.
        /// </summary>
        /// <param name="onUpdate">The method to call when updating this state.</param>
        public State(Action onUpdate) : this(null, onUpdate) { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set all three of the methods for enter, update, and exit.
        /// </summary>
        /// <param name="onEnter">The method to call when entering this state.</param>
        /// <param name="onUpdate">The method to call when updating this state.</param>
        /// <param name="onExit">The method to call when exiting this state.</param>
        public void Functions(Action onEnter, Action onUpdate, Action onExit) {
            if (onEnter != null) {
                OnEnter = onEnter;
            }
            if (onUpdate != null) {
                OnUpdate = onUpdate;
            }
            if (onExit != null) {
                OnExit = onExit;
            }
        }

        /// <summary>
        /// Call OnUpdate.
        /// </summary>
        public void Update() {
            OnUpdate();
        }

        /// <summary>
        /// Call OnEnter.
        /// </summary>
        public void Enter() {
            OnEnter();
        }

        /// <summary>
        /// Call OnExit.
        /// </summary>
        public void Exit() {
            OnExit();
        }

        #endregion
        
    }
}

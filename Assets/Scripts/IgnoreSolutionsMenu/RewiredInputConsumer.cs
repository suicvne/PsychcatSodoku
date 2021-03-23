using System;
using System.Collections;
using IgnoreSolutions.GenInput.Interfaces;
using UnityEngine;

#if REWIRED
using Rewired;
namespace IgnoreSolutions.GenInput
{
    ///<summary>
    /// Consumes input from Rewired and passes them into our ScriptableInputObject
    ///</summary>
    [RequireComponent(typeof(Rewired.InputManager))]
    public class RewiredInputConsumer : MonoBehaviour, IInputConsumer
    {
        public delegate void InputConsumerSetup();
        public InputConsumerSetup InputConsumerFullySetup;

        [SerializeField]
        bool TranslatingInput = false;

        [SerializeField]
        ScriptableInputObject ScriptableInputManager;
        private InputManager _RewiredInputManager;

        [SerializeField]
        protected CanvasGroup _TouchControlsParent = null;
        public CanvasGroup p_TouchControlsParent
        {
            get => _TouchControlsParent;
            set => _TouchControlsParent = value;
        }

        [SerializeField]
        protected float _TimeSinceLastTouchInput = 0f;
        public float p_TimeSinceLastTouchInput
        {
            get => _TimeSinceLastTouchInput;
            set => _TimeSinceLastTouchInput = value;
        }


        private Player _P1Controller;
        private Player _P2Controller;

        internal Player _GetCurrentPlayer()
        {
            return _P1Controller;
        }

        public void InputSwitchActivePlayer(object newPlayer)
        {
            



            Debug.Log($"!!!!!! Input Consumer {newPlayer}");
        }

        private Player _GetCurrentPlayer(DORCursor cursorInstance)
        {
            // If P2 doesn't have a controller, P2 may be controlled via P1's input method.
            // TODO: This should probably be changed once the input remapping is fully reimplemented.
            if (cursorInstance.GameManager.CurrentPlayerTurn == CardOwnership.Player2 && _P2Controller == null) return _P2Controller;

            return cursorInstance.GameManager.CurrentPlayerTurn == 0 ? _P1Controller : _P2Controller;
        }

        public void SetTouchControlsEnabled(bool enabled)
        {
            if (p_TouchControlsParent == null) return;
            if (enabled)
            {
                p_TouchControlsParent.alpha = 1.0f;
                p_TouchControlsParent.blocksRaycasts = true;
            }
            else
            {
                p_TouchControlsParent.alpha = 0.0f;
                p_TouchControlsParent.blocksRaycasts = false;
            }
        }

        public void BeginTouchControlTimeout() => StartCoroutine(TouchControlsTimeoutRoutine());

        public IEnumerator TouchControlsTimeoutRoutine()
        {
            for (float f = 1.0f; f > 0.0f; f -= 1.0f * Time.deltaTime)
            {
                p_TouchControlsParent.alpha = f;
                yield return null;
            }

            p_TouchControlsParent.alpha = 0f;
            SetTouchControlsEnabled(false);
            yield return null;
        }

        [SerializeField] bool EnableReassigningControllerToP1 = false;
        private bool HaveWarnedNoActivePlayer = false;

        public void CheckControllerInput(int activePlayer)
        {
            // Resolving current player.
            // Could just be default (1) or if we're passed a duel cursor, we'll get it based on the current player's turn.
            Player activeRewiredPlayer = _GetCurrentPlayer();
            if (activeRewiredPlayer == null)
            {
                if (HaveWarnedNoActivePlayer == false)
                { Debug.Log($"No active player"); HaveWarnedNoActivePlayer = true; }
                

                return;
            }

            CheckInput(0, _P1Controller, _P1Controller.GetButtonDown);
            CheckInput(1, _P2Controller, _P2Controller.GetButtonDown);
        }

        void CheckInput(int playerIndex, Player activeRewiredPlayer, Func<string, bool> inputCheckFunction)
        {

            bool _u, _d, _l, _r;
            _u = inputCheckFunction("DUp");
            _l = inputCheckFunction("DLeft");
            _r = inputCheckFunction("DRight");
            _d = inputCheckFunction("DDown");

            if (_r) ScriptableInputManager.PerformMovement(ControlType.Right, playerIndex);
            else if (_l) ScriptableInputManager.PerformMovement(ControlType.Left, playerIndex);

            if (_u) ScriptableInputManager.PerformMovement(ControlType.Up, playerIndex);
            else if (_d) ScriptableInputManager.PerformMovement(ControlType.Down, playerIndex);

            if (inputCheckFunction("Select")) ScriptableInputManager.PerformMovement(ControlType.Select, playerIndex);
            if (inputCheckFunction("Cancel")) ScriptableInputManager.PerformMovement(ControlType.Cancel, playerIndex);
            if (inputCheckFunction("Summon")) ScriptableInputManager.PerformMovement(ControlType.Summon, playerIndex);
            if (inputCheckFunction("Information")) ScriptableInputManager.PerformMovement(ControlType.Information, playerIndex);
            if (inputCheckFunction("Flip")) ScriptableInputManager.PerformMovement(ControlType.Flip, playerIndex);
            if (inputCheckFunction("Position")) ScriptableInputManager.PerformMovement(ControlType.ChangePosition, playerIndex);
            if (inputCheckFunction("EndTurn")) ScriptableInputManager.PerformMovement(ControlType.EndTurn, playerIndex);
            // TODO: DID THIS FUCK UP INPUT INTO THE GRAVEYARD???

            bool surrenderBeingHeld = activeRewiredPlayer.GetButtonTimedPress("Surrender", .2f);
            bool graveyardButtonPressed = inputCheckFunction("Graveyard");
            if(surrenderBeingHeld == false && graveyardButtonPressed)
            {
                ScriptableInputManager.PerformMovement(ControlType.ShowGraveyard, playerIndex);
            }
            else if(surrenderBeingHeld)
            {
                ScriptableInputManager.PerformMovement(ControlType.Surrender, playerIndex);
            }


            
            if (activeRewiredPlayer.GetButton("ShowChat")) ScriptableInputManager.PerformMovement(ControlType.ShowChat, playerIndex);

            //if (!activeRewiredPlayer.GetButtonTimedPress("Surrender", 0.1f) && inputCheckFunction("Graveyard")) ScriptableInputManager.PerformMovement(ControlType.ShowGraveyard, playerIndex);
            //if (activeRewiredPlayer.GetButtonLongPressDown("Surrender")) ScriptableInputManager.PerformMovement(ControlType.Surrender, playerIndex);
        }

        void Start()
        {
            _RewiredInputManager = GetComponent<Rewired.InputManager>();
            if (_RewiredInputManager == null) throw new System.Exception("No Input Manager attached.");

            InputConsumerFullySetup?.Invoke();

            TranslatingInput = true;
            ScriptableInputManager.AllowInput = true;


            if (_P1Controller == null || _P2Controller == null)
            {
                _P1Controller = ReInput.players.GetPlayer(0);
                _P2Controller = ReInput.players.GetPlayer(1);

                _P1Controller.controllers.ControllerAddedEvent += P1Controllers_ControllerAddedEvent;
            }
        }

        bool p1ReassignedControllerOnce = false;

        private void P1Controllers_ControllerAddedEvent(ControllerAssignmentChangedEventArgs args)
        {
            Debug.Log($"[RewiredInputConsumer] P1 added {args.controller.name} ({args.controller.type}) to its controllers.");
            if (args.controller.type != ControllerType.Joystick) return; // skip if this isn't a Joystick

            if(args.controller.name.Contains("Guitar"))
            {
                Debug.Log($"[RewiredInputConsumer] Removing guitar as assigned joystick type.");
                _P1Controller.controllers.RemoveController(args.controller);
                return;
            }

            if (args.player.controllers.hasKeyboard
                && p1ReassignedControllerOnce == false
                && EnableReassigningControllerToP1 == false)
            {
                p1ReassignedControllerOnce = true;
                // Get the Joystick from ReInput
                Joystick joystick = ReInput.controllers.GetJoystick(args.controller.id);
                if (joystick == null) return;

                Debug.Log($"[RewiredInputConsumer] Re-assigning joystick to P2.");

                _P1Controller.controllers.ClearControllersOfType<Joystick>();
                _P1Controller.controllers.hasKeyboard = true;

                _P2Controller.controllers.hasKeyboard = false;
                _P2Controller.controllers.AddController(joystick, true);
            }
            else if(p1ReassignedControllerOnce && EnableReassigningControllerToP1)
            {
                Debug.Log($"[RewiredInputConsumer] Not re-assigning joystick to P2 because it's already been done once.");
            }
        }

        void Update()
        {
            if (_RewiredInputManager != null && ScriptableInputManager.AllowInput)
            {
                TranslatingInput = true;
                ScriptableInputManager.PerformUpdate(Time.deltaTime, this);
            }
        }
    }
}
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.GenInput.Interfaces;
using UnityEngine;

namespace IgnoreSolutions.GenInput
{
    public enum PsychSudokuInput
    {
        Up,Down,
        Left,Right,
        Select,Cancel
    }

    public abstract class InputConsumer : MonoBehaviour, IInputConsumer
    {
        [SerializeField] StringToEnumMap<PsychSudokuInput> _StringToEnumMap;

        #region Touch Controls
        public CanvasGroup p_TouchControlsParent
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public float p_TimeSinceLastTouchInput
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public virtual void BeginTouchControlTimeout()
        {
            throw new System.NotImplementedException();
        }

        public virtual void SetTouchControlsEnabled(bool enabled)
        {
            throw new System.NotImplementedException();
        }

        public virtual IEnumerator TouchControlsTimeoutRoutine()
        {
            throw new System.NotImplementedException();
        }
        #endregion



        public virtual void CheckControllerInput(int activePlayer)
        {
            // Check input for Player 0
            HandleCheckInput(0, Input.GetButtonDown);
        }

        public void HandleCheckInput(int playerIndex,
            Func<string, bool> inputCheckFunction)
        {
            foreach(var map in _StringToEnumMap.GetList())
            {
                if(inputCheckFunction(map._StringName))
                {
                    // TODO: Broadcast event
                }
            }
        }
    }
}
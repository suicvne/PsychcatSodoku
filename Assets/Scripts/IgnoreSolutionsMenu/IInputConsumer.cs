using System.Collections;
using UnityEngine;

namespace IgnoreSolutions.GenInput.Interfaces
{
    // Defines an interface to represent a class that consumes
    // input.
    public interface IInputConsumer
    {
        /// <summary>
        /// The CanvasGroup that represents 
        /// </summary>
        CanvasGroup p_TouchControlsParent { get; set; }

        /// <summary>
        /// The time since the last input via touch controls
        /// </summary>
        float p_TimeSinceLastTouchInput { get; set; }

        void SetTouchControlsEnabled(bool enabled);
        void BeginTouchControlTimeout();

        void CheckControllerInput(int activePlayer);
        IEnumerator TouchControlsTimeoutRoutine();
    }
}
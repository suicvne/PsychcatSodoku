using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    /// <summary>
    /// A class to handle pausing the game and
    /// exiting back to the main menu in the even the player wishes.
    ///
    /// This class' entire purpose is *just* UI. It will receive
    /// the events from the PauseGameHandler.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PauseMenuScripts : MonoBehaviour
    {
        CanvasGroup _ThisCanvasGroup;
        private bool _AnimationInProgress = false;

        private void Awake()
        {
            _ThisCanvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnGamePaused(bool wasPaused, bool isCurrentlyPaused)
        {
            if (isCurrentlyPaused) Debug.Log($"[PauseMenuScripts] Showing pause menu.");
            else Debug.Log($"[PauseMenuScripts] Hiding pause menu.");

            SetVisibleState(isCurrentlyPaused);
        }

        public void SetVisibleState(bool visibleState)
        {
            if (_AnimationInProgress) return;

            StartCoroutine(AnimateFade(visibleState));
        }

        /// <summary>
        /// Animates the fading in and out of the Pause Menu based
        /// on the visibleState variable.
        ///
        /// This is dependent on the panel you want to animate having a
        /// CanvasGroup attached to it as it fades the alpha value in and out.
        /// The CanvasGroup's .interactable and .blocksRaycasts values are then
        /// set depending on the visibleState parameter.
        /// </summary>
        /// <param name="visibleState">
        /// Should the pause menu be visible or not?
        ///
        /// If true, the alpha value will go from 0f -> 1.0f
        /// indicating the panel is fading in.
        ///
        /// If false, the alpha value will go from 1.0f -> 0f
        /// indicating the panel is already fading in.
        /// </param>
        /// <returns>Coroutine State</returns>
        IEnumerator AnimateFade(bool visibleState)
        {
            _AnimationInProgress = true;
            for (float f = 0f; f <= 1.0f; f += 1.0f * Time.fixedDeltaTime)
            {
                float u_f = visibleState ? f : 1.0f - f;
                _ThisCanvasGroup.alpha = u_f;
                _ThisCanvasGroup.interactable = visibleState;
                _ThisCanvasGroup.blocksRaycasts = visibleState;

                yield return null;
            }

            // Validate the final state since floating point
            // calculations are always unreliable.
            _ThisCanvasGroup.alpha = visibleState ? 1.0f : 0.0f;
            _ThisCanvasGroup.interactable = visibleState;
            _ThisCanvasGroup.blocksRaycasts = visibleState;

            _AnimationInProgress = false;

            yield return null;
        }
    }
}
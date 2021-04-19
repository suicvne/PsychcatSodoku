using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.UI
{
    public abstract class IAnimatableCanvasGroup : MonoBehaviour
    {
        protected internal CanvasGroup _ThisCanvasGroup;
        protected internal bool _AnimationInProgress = false;
        protected internal bool _IsVisible = false;

        public virtual void Start()
        {
            if(_ThisCanvasGroup != null)
            {
                _ThisCanvasGroup.alpha = 0.0f;
                _ThisCanvasGroup.interactable = false;
                _ThisCanvasGroup.blocksRaycasts = false;
            }
        }

        public virtual void SetCanvasGroupVisiblity(bool visibility)
        {
            if (_AnimationInProgress) return;

            StartCoroutine(AnimateCanvasGroupVisibilityChange(visibility));
        }

        protected IEnumerator AnimateCanvasGroupVisibilityChange(bool visibleState)
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
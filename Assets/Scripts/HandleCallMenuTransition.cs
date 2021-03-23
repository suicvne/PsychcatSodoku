using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    public class HandleCallMenuTransition : MonoBehaviour
    {
        [SerializeField] CanvasGroup _MyCanvasGroup;
        [SerializeField] CanvasGroup[] _GroupToTransition;
        [SerializeField] MenuActions _MenuActions;
        public int _TransitionIndex = 0;

        public void SetTransitionIndex(int index)
        {
            if (_MenuActions._TransitionInProgress) return;
            if (index < 0 || index > _GroupToTransition.Length - 1) return;

            _TransitionIndex = index;
        }

        public void TransitionFromMyGroupToNext()
        {
            if (_MenuActions._TransitionInProgress) return;

            if(_MenuActions != null &&
                _MyCanvasGroup != null &&
                _GroupToTransition != null &&
                _TransitionIndex >= 0 &&
                _TransitionIndex < _GroupToTransition.Length)
            {
                _MenuActions.TransitionCanvasGroupToOther(_MyCanvasGroup, _GroupToTransition[_TransitionIndex]);
            }
        }

        public void TransitionFromNextGroupToMyGroup()
        {
            if (_MenuActions._TransitionInProgress) return;

            if (_MenuActions != null &&
                _MyCanvasGroup != null &&
                _GroupToTransition != null &&
                _TransitionIndex >= 0 &&
                _TransitionIndex < _GroupToTransition.Length)
            {
                _MenuActions.TransitionCanvasGroupToOther(_GroupToTransition[_TransitionIndex], _MyCanvasGroup);
            }
        }    
    }
}
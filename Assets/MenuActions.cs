using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IgnoreSolutions.PsychSodoku
{
    [Serializable]
    public struct MenuActionsSetup
    {
        public CanvasGroup _PreviousGroup;
        public CanvasGroup MyCanvasGroup;
        public CanvasGroup NextCanvasGroup;
    }

    /// <summary>
    /// Handles interactions between buttons and menus.
    /// </summary>
    public class MenuActions : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SwitchScene _Transitions;

        [Header("Canvas Groups")]
        [SerializeField] List<MenuActionsSetup> _MenuActions = new List<MenuActionsSetup>();

        [SerializeField] int _CurrentIndex = 0;

        int _nextIndex = -1;

        private void Start()
        {
            DisableAllGroups();
        }

        private void OnEnable()
        {
            _Transitions.OnFadeCompleted += _Transitions_OnFadeCompleted;
        }

        private void OnDisable()
        {
            _Transitions.OnFadeCompleted -= _Transitions_OnFadeCompleted;
        }

        private void _Transitions_OnFadeCompleted(bool wasPrevious = false)
        {
            if(_nextIndex == -1)
            {
                Debug.Log($"Warning: Undefined behaviour. Next Index was equal to {_nextIndex}");
                return;
            }

            _CurrentIndex = _nextIndex;
            _nextIndex = -1;
        }

        void DisableAllGroups()
        {
            foreach(var menu in _MenuActions)
            {
                menu.MyCanvasGroup.alpha = 0f;
                menu.MyCanvasGroup.interactable = false;
                menu.MyCanvasGroup.blocksRaycasts = true;
            }

            _MenuActions[_CurrentIndex].MyCanvasGroup.alpha = 1.0f;
            _MenuActions[_CurrentIndex].MyCanvasGroup.blocksRaycasts = true;
            _MenuActions[_CurrentIndex].MyCanvasGroup.interactable = true;
        }

        public int GetCanvasGroupIndexInList(CanvasGroup cg)
        {
            for(int i = 0; i < _MenuActions.Count; i++)
            {
                if (_MenuActions[i].MyCanvasGroup == cg) return i;
            }

            return -1;
        }

        public void IncrementGroup(int dir)
        {
            if (dir == 0) return;
            if (_Transitions._AnimationInProgress) return;

            CanvasGroup _curGroup = _MenuActions[_CurrentIndex].MyCanvasGroup;
            CanvasGroup _nextGroup = _MenuActions[_CurrentIndex].NextCanvasGroup;
            CanvasGroup _previousGroup = _MenuActions[_CurrentIndex]._PreviousGroup;

            if(dir > 0) // go to next
            {
                if(_nextGroup == null)
                {
                    Debug.Log($"_NextGroup on _MenuActions index {_CurrentIndex} is null.", gameObject);
                    return;
                }

                int n = GetCanvasGroupIndexInList(_previousGroup);
                if (n == -1)
                {
                    Debug.LogError($"Unable to transition to previous scene because we don't know what index in the list the previous group is.", gameObject);
                }
                else _nextIndex = n;

                _Transitions.StartFadeTransition(_nextGroup, _curGroup);
            }
            else if(dir < 0) // go to previous
            {
                if(_previousGroup == null)
                {
                    Debug.Log($"_PreviousGroup on _MenuActions index {_CurrentIndex} was null. Transitioning to first available CanvasGroup instead.",
                        gameObject);

                    _Transitions.StartFadeTransition(_MenuActions[_CurrentIndex].MyCanvasGroup,
                        _curGroup);
                    return;
                }

                int n = GetCanvasGroupIndexInList(_previousGroup);
                if (n == -1)
                {
                    Debug.LogError($"Unable to transition to previous scene because we don't know what index in the list the previous group is.", gameObject);
                }
                else _nextIndex = n;


                _Transitions.StartFadeTransition(_previousGroup, _curGroup);
            }
        }

    }
}
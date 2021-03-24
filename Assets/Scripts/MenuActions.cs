using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IgnoreSolutions.PsychSodoku
{
    /// <summary>
    /// Handles interactions between buttons and menus.
    /// </summary>
    public class MenuActions : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SwitchScene _Transitions;
        [SerializeField] public bool _TransitionInProgress = false;


        public void TransitionCanvasGroupToOther(CanvasGroup cur, CanvasGroup next)
        {
            if (_Transitions == null || _TransitionInProgress == true) return;

            _Transitions.StartFadeTransition(next, cur);
        }
    }
}
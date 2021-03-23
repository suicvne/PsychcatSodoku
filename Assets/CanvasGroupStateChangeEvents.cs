using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IgnoreSolutions.PsychSodoku
{
    public class CanvasGroupStateChangeEvents : MonoBehaviour
    {
        [SerializeField] UnityEvent _OnCanvasGroupBecameVisible;
        [SerializeField] UnityEvent _OnCanvasGroupBecameInvisible;

        public void ExecuteOnVisible() => _OnCanvasGroupBecameVisible?.Invoke();
        public void ExecuteOnInvisible() => _OnCanvasGroupBecameInvisible?.Invoke();
    }
}
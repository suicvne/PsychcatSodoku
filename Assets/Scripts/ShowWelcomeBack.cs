using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    public class ShowWelcomeBack : MonoBehaviour
    {
        [SerializeField] Animator _ThisAnimator;

        SuspendGameState _SuspendGame;
        TestChangeRectTransByOrientation _RectTransOrient;

        private void Start()
        {
            if (_ThisAnimator == null) _ThisAnimator = GetComponent<Animator>();
            if (_RectTransOrient == null) _RectTransOrient = GetComponentInChildren<TestChangeRectTransByOrientation>();
            if (_RectTransOrient == null) Debug.LogWarning($"[ShowWelcomeBack] No TestChangeRectTransByOrientation in children.");

            if (_SuspendGame == null)
            {
                _SuspendGame = FindObjectOfType<SuspendGameState>();

                if(_SuspendGame != null
                    && _SuspendGame.EnabledAndWorking
                    && _SuspendGame.HadValidStateOnStart)
                {
                    if(_RectTransOrient != null
                        && _RectTransOrient.GetOrientation == OrientationCanvasSwap.Orientation.Portrait)
                    {
                        _ThisAnimator.ResetTrigger("Show_Portrait");
                        _ThisAnimator.SetTrigger("Show_Portrait");
                    }
                    else
                    {
                        _ThisAnimator.ResetTrigger("Show");
                        _ThisAnimator.SetTrigger("Show");
                    }
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    public class ShowWelcomeBack : MonoBehaviour
    {
        [SerializeField] Animator _ThisAnimator;

        SuspendGameState _SuspendGame;

        private void Start()
        {
            if (_ThisAnimator == null) _ThisAnimator = GetComponent<Animator>();

            if(_SuspendGame == null)
            {
                _SuspendGame = FindObjectOfType<SuspendGameState>();

                if(_SuspendGame != null
                    && _SuspendGame.EnabledAndWorking
                    && _SuspendGame.HadValidStateOnStart)
                {
                    _ThisAnimator.ResetTrigger("Show");
                    _ThisAnimator.SetTrigger("Show");
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    [RequireComponent(typeof(PauseGameHandler))]
    public class PauseInput : MonoBehaviour
    {
        [SerializeField] bool _Enable = true;
        [SerializeField] float _PauseReEnableDelay = 0.5f;
        PauseGameHandler _PauseGameHandler;

        private void Awake()
        {
            _PauseGameHandler = GetComponent<PauseGameHandler>();
            if (_PauseGameHandler == null) _Enable = false;
        }

        public void CallPause()
        {
            _PauseGameHandler.IsGamePaused = !_PauseGameHandler.IsGamePaused;
            _Enable = false;
            Invoke(nameof(Delayed_ReEnable), _PauseReEnableDelay);
        }

        void Delayed_ReEnable() => _Enable = true;

        private void FixedUpdate()
        {
            if(_Enable)
            {
                if(Input.GetKeyDown(KeyCode.Escape)
                    || Input.GetKeyDown(KeyCode.Menu))
                {
                    CallPause();
                }
            }
        }
    }
}
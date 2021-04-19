using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IgnoreSolutions.PsychSodoku
{
    public class PauseGameHandler : MonoBehaviour
    {
        [SerializeField] bool _IsGamePaused;

        public bool IsGamePaused
        {
            get => _IsGamePaused;
            set
            {
                if(value != _IsGamePaused)
                {
                    OnGamePauseChanged?.Invoke(_IsGamePaused, value);
                    _IsGamePaused = value;
                }
            }
        }

        [Header("Game Paused (oldState, newState)")]
        public UnityEvent<bool, bool> OnGamePauseChanged;
    }
}
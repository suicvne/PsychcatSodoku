using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    public class GameTimeManager : MonoBehaviour
    {
        [SerializeField] public long StartTime;
        [SerializeField] public long CompleteTime;

        private void Start()
        {
            StartTime = DateTime.Now.Ticks;
        }

        public void ResetTimer()
        {
            StartTime = DateTime.Now.Ticks;
        }

        public TimeSpan GetPlayTime()
        {
            CompleteTime = DateTime.Now.Ticks;

            return new TimeSpan(CompleteTime - StartTime);
        }
    }
}
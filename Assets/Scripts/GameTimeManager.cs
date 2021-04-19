using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    public class GameTimeManager : Singleton<GameTimeManager>
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

        public TimeSpan PeekPlayTime() => new TimeSpan((DateTime.Now.Ticks - StartTime));

        public TimeSpan GetPlayTime()
        {
            CompleteTime = DateTime.Now.Ticks;

            return new TimeSpan((CompleteTime - StartTime));
        }
    }
}
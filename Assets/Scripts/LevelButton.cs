using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.Sodoku;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IgnoreSolutions.PsychSodoku
{
    public class LevelButton : Button
    {
        LevelData _MyLevel;
        public UnityEvent _OnSelect;

        public delegate void OnButtonClicked();
        public event OnButtonClicked ButtonClicked;

        protected override void Awake()
        {
            onClick.AddListener(() => ButtonClicked?.Invoke());
        }

        public void SetMyLevel(LevelData _level) => _MyLevel = _level;

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            _OnSelect?.Invoke();
        }

        public void SetSelected(TestOccupyLotsOfButtons me, bool redirect = false)
        {
            me.SetSelectedLevel(_MyLevel, this, redirect);
        }

        public void ThisButtonClicked()
        {
            Debug.Log($"I was clicked! Might be time to show the next button.");
        }

        
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IgnoreSolutions.PsychSodoku
{
    public class CongratsCanvasAnimEvents : MonoBehaviour
    {
        [SerializeField] CanvasGroup _ParentCanvasGroup;
        [SerializeField] CongratulationsScreenController _CongratsCanvas;

        public void VerifyAlpha()
        {
            if (_ParentCanvasGroup != null)
            {
                _ParentCanvasGroup.alpha = 1.0f;
                _ParentCanvasGroup.blocksRaycasts = true;
                _ParentCanvasGroup.interactable = true;
            }
        }

        public void NotifyFadeInComplete()
        {
            if(Application.isPlaying)
                _CongratsCanvas.SetNextButtonSelected();
        }

        public void NotifyFadeOutComplete()
        {
            if (Application.isPlaying)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
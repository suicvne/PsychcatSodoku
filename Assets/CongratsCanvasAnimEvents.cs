using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IgnoreSolutions.PsychSodoku
{
    public class CongratsCanvasAnimEvents : MonoBehaviour
    {
        [SerializeField] CongratulationsScreenController _CongratsCanvas;

        public void NotifyFadeInComplete()
        {
            _CongratsCanvas.SetNextButtonSelected();
        }

        public void NotifyFadeOutComplete()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
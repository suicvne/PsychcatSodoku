using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IgnoreSolutions.PsychSodoku
{
    public class ControlNumberScreenByKeyboard : MonoBehaviour
    {
        public bool _AcceptInput = false;
        public ModifyShaderOffset _MainGame;
        public GameObject _FirstSelected;
        public GameObject _LastSelected = null;

        bool inProgress = false;

        public void RedirectInput()
        {
            if (inProgress) return;

            inProgress = true;
            StartCoroutine(WTF());
        }

        IEnumerator WTF()
        {
            if (_LastSelected != null)
            {
                _AcceptInput = true;
                _MainGame._DirectInputToNumberSelect = true;

                Debug.Log($"Setting input to {_LastSelected}", _LastSelected);
                EventSystem.current.SetSelectedGameObject(_LastSelected);
            }
            else
            {
                _AcceptInput = true;
                _MainGame._DirectInputToNumberSelect = true;
                yield return null;
                Debug.Log($"Setting input to {_FirstSelected} Direct: {_MainGame._DirectInputToNumberSelect}", _FirstSelected);
                Debug.Log($"Event Systme: {EventSystem.current}", EventSystem.current);
                EventSystem.current.SetSelectedGameObject(null);
                yield return null;
                EventSystem.current.SetSelectedGameObject(_FirstSelected, new BaseEventData(EventSystem.current));
                yield return null;
                Debug.Log($"Event Systme: {EventSystem.current} Direct: {_MainGame._DirectInputToNumberSelect}", EventSystem.current);

                Button b = null;
                if ((b = _FirstSelected.GetComponent<Button>()) != null)
                    b.Select();

            }
        }

        public void CancelInput()
        {
            Debug.Log($"Cancel Input.");
            _AcceptInput = false;
            _MainGame._DirectInputToNumberSelect = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void Update()
        {
        }
    }
}
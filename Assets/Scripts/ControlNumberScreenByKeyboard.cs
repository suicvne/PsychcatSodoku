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
        [SerializeField] private CanvasGroup _SelectionFadeGroup;

        public bool inProgress = false;

        public void RedirectInput()
        {
            if (inProgress)
            {
                Debug.Log($"Redirect already in progress.");
                return;
            }
            

            inProgress = true;
            StartCoroutine(WTF());
        }

        private WaitForFixedUpdate _WaitFixedUpdate = new WaitForFixedUpdate();

        IEnumerator WTF()
        {
            if (_SelectionFadeGroup != null)
            {
                for (float f = 0f; f <= 1.0f; f += 4.0f * Time.fixedDeltaTime)
                {
                    _SelectionFadeGroup.alpha = f;
                    yield return _WaitFixedUpdate;
                }

                _SelectionFadeGroup.alpha = 1.0f;
                _SelectionFadeGroup.interactable = false;
                _SelectionFadeGroup.blocksRaycasts = true;
            }
            
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

            inProgress = false;
            _MainGame._DontUpdate = false;
        }

        public void CancelInput()
        {
            if (inProgress == true) return;
            
            Debug.Log($"Cancel Input.");
            _AcceptInput = false;
            _MainGame._DirectInputToNumberSelect = false;
            EventSystem.current.SetSelectedGameObject(null);
            StartCoroutine((DoCancel()));
        }

        IEnumerator DoCancel()
        {
            inProgress = true;
            if (_SelectionFadeGroup != null)
            {
                for (float f = 0f; f <= 1.0f; f += 1.0f * Time.fixedDeltaTime)
                {
                    float i_f = 1.0f - f;
                    _SelectionFadeGroup.alpha = i_f;
                    yield return _WaitFixedUpdate;
                }

                _SelectionFadeGroup.alpha = 0.0f;
                _SelectionFadeGroup.interactable = false;
                _SelectionFadeGroup.blocksRaycasts = false;
            }

            inProgress = false;
            _MainGame._DontUpdate = false;
        }

        private void Update()
        {
        }
    }
}
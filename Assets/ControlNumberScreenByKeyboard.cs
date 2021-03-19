using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IgnoreSolutions.PsychSodoku
{
    public class ControlNumberScreenByKeyboard : MonoBehaviour
    {
        public bool _AcceptInput = false;
        public ModifyShaderOffset _MainGame;
        public GameObject _FirstSelected;
        public GameObject _LastSelected = null;

        public void RedirectInput()
        {
            if (_LastSelected != null)
            {
                EventSystem.current.SetSelectedGameObject(_LastSelected);
            }
            else EventSystem.current.SetSelectedGameObject(_FirstSelected);

            Debug.Log($"Setting input to {_FirstSelected}", _FirstSelected);
            _AcceptInput = true;
            _MainGame._DirectInputToNumberSelect = true;
        }

        public void CancelInput()
        {
            _AcceptInput = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void Update()
        {
            //float v = Input.GetAxis("Vertical"), h = Input.GetAxis("Horizontal");

            //if(h > .35f)
            //{

            //}
            //else if(h < -.35f)
            //{

            //}

            //if(v > .35f)
            //{

            //}
            //else if(v < -.35f)
            //{

            //}
        }
    }
}
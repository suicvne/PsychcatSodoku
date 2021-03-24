using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IgnoreSolutions.PsychSodoku
{
    [RequireComponent(typeof(StandaloneInputModule))]
    public class ForceActivateWithGamepad : MonoBehaviour
    {
        public delegate void ConnectedJoystickCountChanged(int oldJoysticks, int newJoysticks);

        public event ConnectedJoystickCountChanged OnConnectedJoystickCountChanged ;
        
        [SerializeField] bool _ForceAnyway;
        StandaloneInputModule _UnityInputModule;
        ManageFirstSelected _FirstSelected;

        private void Start()
        {
            Debug.Log($"[ForceActivateWithGamepad] Awake");
            _UnityInputModule = GetComponent<StandaloneInputModule>();
            _FirstSelected = GetComponent<ManageFirstSelected>();

            if ((Application.isPlaying && _UnityInputModule != null) || _ForceAnyway)
            {
                Debug.Log($"[ForceActivateWithGamepad] Playing, has InputModule");
                if ((Application.isMobilePlatform && Input.GetJoystickNames().Length > 0) || _ForceAnyway)
                {
                    Debug.Log($"[ForceActivateWithGamepad] Gamepad Enabled");
                    _UnityInputModule.forceModuleActive = true;
                    Debug.Log($"Gamepad Enabled on mobile platform.!");
                }
                else
                {
                    Debug.Log($"[ForceActivateWithGamepad] {Input.GetJoystickNames().Length} joysticks enabled.");
                }
            }
        }
        
        int _LastJoystickCount = 0;

        private void LateUpdate()
        {
            if (Input.GetJoystickNames().Length != _LastJoystickCount)
            {
                Debug.Log($"Connected joystick count changed from {_LastJoystickCount} to {Input.GetJoystickNames().Length}");
                OnConnectedJoystickCountChanged?.Invoke(_LastJoystickCount, Input.GetJoystickNames().Length);
                _LastJoystickCount = Input.GetJoystickNames().Length;
            }
        
            if (Input.GetAxis("Horizontal") != 0f && EventSystem.current.currentSelectedGameObject == null)
            {
                //_FirstSelected.SetSelectedGameObject(_FirstSelected.)
                Debug.Log($"Horizontal `{EventSystem.current.currentSelectedGameObject}`");
            }
            if (Input.GetAxis("Vertical") > 0f) Debug.Log($"Vertical `{EventSystem.current.currentSelectedGameObject}`");
            //if (Input.GetKeyDown(KeyCode.LeftArrow)) Debug.Log($"Left Arrow");
            //if (Input.GetKeyDown(KeyCode.RightArrow)) Debug.Log($"Right Arrow");
            //if (Input.GetKeyDown(KeyCode.UpArrow)) Debug.Log($"Up Arrow");
            //if (Input.GetKeyDown(KeyCode.DownArrow)) Debug.Log($"Down Arrow");
            //if (Input.GetKeyDown(KeyCode.JoystickButton0)) Debug.Log($"Centre Click");
            //if (Input.GetKeyDown(KeyCode.Escape)) Debug.Log($"Back Button");
        }
    }
}
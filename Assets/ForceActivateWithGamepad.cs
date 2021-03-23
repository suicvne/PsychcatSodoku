using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IgnoreSolutions.PsychSodoku
{
    [RequireComponent(typeof(StandaloneInputModule))]
    public class ForceActivateWithGamepad : MonoBehaviour
    {
        StandaloneInputModule _UnityInputModule;

        private void Awake()
        {
            _UnityInputModule = GetComponent<StandaloneInputModule>();
            if(Application.isPlaying && _UnityInputModule != null)
            {
                if(Application.isMobilePlatform && Input.GetJoystickNames().Length > 0)
                {
                    _UnityInputModule.forceModuleActive = true;
                    Debug.Log($"Gamepad Enabled on mobile platform.!");
                }
            }
        }
    }
}
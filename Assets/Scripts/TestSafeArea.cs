using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public class TestSafeArea : MonoBehaviour
    {
        RectTransform _ThisTransform;
        [SerializeField] Vector2 _ScreenSafeSize;

        private void OnEnable()
        {
            _ThisTransform = GetComponent<RectTransform>();

            Debug.Log($"Screen Safe Area: {Screen.safeArea.ToString()}");
            _ThisTransform.sizeDelta = Screen.safeArea.size;
            _ScreenSafeSize = Screen.safeArea.size;
            //_ThisTransform.transform.position = Vector3.zero;
        }

        private void LateUpdate()
        {
            if(_ThisTransform != null) _ThisTransform.sizeDelta = Screen.safeArea.size;
        }

    }
}
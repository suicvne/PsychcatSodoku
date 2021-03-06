using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    [System.Serializable]
    public struct RectTransformProperties
    {
        public Vector2 _SizeDelta;
        public Vector2 _AnchoredPosition;
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class TestGatherRectTransProps : MonoBehaviour
    {
        RectTransform _ThisRectTransform;

        private void OnEnable()
        {
            _ThisRectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            
        }
    }
}
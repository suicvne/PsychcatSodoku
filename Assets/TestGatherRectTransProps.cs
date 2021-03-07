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
        public Vector2 _AnchorMin;
        public Vector2 _AnchorMax;
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class TestGatherRectTransProps : MonoBehaviour
    {
        [SerializeField] public RectTransformProperties _Properties;
        RectTransform _ThisRectTransform;

        private void OnEnable()
        {
            _ThisRectTransform = GetComponent<RectTransform>();
            _Properties = new RectTransformProperties();
        }

        private void LateUpdate()
        {
            _Properties._SizeDelta = _ThisRectTransform.sizeDelta;
            _Properties._AnchoredPosition = _ThisRectTransform.anchoredPosition;
            _Properties._AnchorMin = _ThisRectTransform.anchorMin;
            _Properties._AnchorMax = _ThisRectTransform.anchorMax;
        }
    }
}
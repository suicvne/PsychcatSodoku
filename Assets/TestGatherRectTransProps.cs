using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    [System.Serializable]
    public struct RectTransformProperties
    {
        public string name;
        public Vector2 _ReferenceAspectRatio;
        public Vector2 _SizeDelta;
        public Vector2 _AnchoredPosition;
        public Vector2 _AnchorMin;
        public Vector2 _AnchorMax;
        public Vector3 _LocalScale;
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
            int r = DetectResolutionChange.gcd(Screen.width, Screen.height);
            _Properties.name = _Properties._ReferenceAspectRatio.ToString();
            _Properties._ReferenceAspectRatio = new Vector2(Screen.width / r, Screen.height / r);
            _Properties._SizeDelta = _ThisRectTransform.sizeDelta;
            _Properties._AnchoredPosition = _ThisRectTransform.anchoredPosition;
            _Properties._AnchorMin = _ThisRectTransform.anchorMin;
            _Properties._AnchorMax = _ThisRectTransform.anchorMax;
            _Properties._LocalScale = _ThisRectTransform.localScale;
        }
    }
}
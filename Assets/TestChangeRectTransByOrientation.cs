using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public class TestChangeRectTransByOrientation : MonoBehaviour
    {
        [System.Flags]
        public enum PropertiesToCopy
        {
            SizeDelta = 1,
            AnchoredPosition = 2,
            AnchorMin = 3,
            AnchorMax = 4
        }

        [SerializeField] PropertiesToCopy _PropsToCopy = PropertiesToCopy.SizeDelta | PropertiesToCopy.AnchoredPosition | PropertiesToCopy.AnchorMin | PropertiesToCopy.AnchorMax;
        [SerializeField] bool ShouldWork = false;
        [SerializeField] bool TestFillMode = false;
        [SerializeField] Orientation _CurrentOrientation;
        [SerializeField] RectTransformProperties _Portrait;
        [SerializeField] RectTransformProperties _Landscape;

        RectTransform _ThisRectTransform;
        TestGatherRectTransProps _TestGatherProps;
        Camera _camera;
        Vector2 _LastResolution = new Vector2();
        Vector2 _CurrentResolution = new Vector2();

        public enum Orientation
        {
            Portrait,
            Landscape
        }

        private void OnEnable()
        {
            _camera = Camera.main;
            _ThisRectTransform = GetComponent<RectTransform>();
            _TestGatherProps = GetComponent<TestGatherRectTransProps>();
        }

        private void LateUpdate()
        {
            _CurrentResolution.Set(Screen.width, Screen.height);
            if (_CurrentResolution != _LastResolution)
            {
                ResolutionChanged();
                _LastResolution = _CurrentResolution;
            }

            if(TestFillMode && _TestGatherProps != null)
            {
                if(_CurrentOrientation == Orientation.Landscape)
                {
                    _Landscape = _TestGatherProps._Properties;
                }
                else
                {
                    _Portrait = _TestGatherProps._Properties;
                }
            }
        }

        public void ResolutionChanged()
        {
            

            if (_camera != null)
            {
                if (Screen.width < Screen.height) // w < h. Portrait aspect.
                {
                    //Debug.Log($"13");
                    //camera.orthographicSize = 13;
                    _CurrentOrientation = Orientation.Portrait;
                }
                else if (Screen.width > Screen.height) // w > h. Landscape aspect.
                {
                    //Debug.Log($"7");
                    //camera.orthographicSize = 6;
                    _CurrentOrientation = Orientation.Landscape;
                }
                else // w == h. Dunno yet.
                {
                    Debug.Log($"Idk (11)");
                    _camera.orthographicSize = 11;
                }

                if (ShouldWork == false) return;
                //EnableForOrientation(_CurrentOrientation);
                RectTransformProperties orientationProps = _CurrentOrientation == Orientation.Landscape ? _Landscape : _Portrait;

                if(_PropsToCopy.HasFlag(PropertiesToCopy.SizeDelta)) _ThisRectTransform.sizeDelta = orientationProps._SizeDelta;
                if(_PropsToCopy.HasFlag(PropertiesToCopy.AnchoredPosition)) _ThisRectTransform.anchoredPosition = orientationProps._AnchoredPosition;
                if(_PropsToCopy.HasFlag(PropertiesToCopy.AnchorMin)) _ThisRectTransform.anchorMin = orientationProps._AnchorMin;
                if (_PropsToCopy.HasFlag(PropertiesToCopy.AnchorMax)) _ThisRectTransform.anchorMax = orientationProps._AnchorMax;
            }
        }
    }
}
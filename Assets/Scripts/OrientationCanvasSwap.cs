using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    [System.Serializable]
    public struct CanvasOrientationLookups
    {
        [SerializeField] public string name;
        [SerializeField] public Canvas _PortraitResolution;
        [SerializeField] public Canvas _LandscapeResolution;
    }
    [ExecuteInEditMode]
    public class OrientationCanvasSwap : MonoBehaviour
    {
        [SerializeField] Orientation _CurrentOrientation;
        [SerializeField] List<CanvasOrientationLookups> _CanvasLookups = new List<CanvasOrientationLookups>();
        new Camera camera;
        Vector2 _LastResolution = new Vector2();
        Vector2 _CurrentResolution = new Vector2();

        public enum Orientation
        {
            Portrait,
            Landscape
        }

        private void LateUpdate()
        {
            _CurrentResolution.Set(Screen.width, Screen.height);
            if(_CurrentResolution != _LastResolution)
            {
                ResolutionChanged();
                _LastResolution = _CurrentResolution;
            }
        }

        private void EnableForOrientation(Orientation orient)
        {
            foreach(var col in _CanvasLookups)
            {
                if(orient == Orientation.Landscape)
                {
                    col._LandscapeResolution.enabled = true;
                    col._PortraitResolution.enabled = false;
                }
                else
                {
                    col._LandscapeResolution.enabled = false;
                    col._PortraitResolution.enabled = true;
                }
            }
        }

        private void Awake()
        {
            camera = Camera.main;
            _LastResolution.Set(Screen.width, Screen.height);
            _CurrentResolution = _LastResolution;
        }

        public void ResolutionChanged()
        {
            if (camera != null)
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
                    camera.orthographicSize = 11;
                }

                EnableForOrientation(_CurrentOrientation);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static IgnoreSolutions.PsychSodoku.OrientationCanvasSwap;

namespace IgnoreSolutions.PsychSodoku
{
    [ExecuteInEditMode]
    public class DetectResolutionChange : MonoBehaviour
    {
        [SerializeField] UnityEvent<Orientation, Vector2> OnResolutionChange;
        [SerializeField] Orientation _CurrentOrientation;
        [SerializeField] Vector2 _CurrentAspectRatio;

        Vector2 _CurrentResolution = new Vector2(), _LastResolution = new Vector2();
        Camera _camera;

        private void OnEnable()
        {
            _camera = Camera.main;
        }

        public static int gcd(int a, int b)
        {
            if (b == 0) return a;
            return gcd(b, a % b);
        }

        int[] arc(int val, int lim)
        {
            var lower = new int[] { 0, 1 };
            var upper = new int[] { 1, 0 };

            int steps = 0;
            while(true)
            {
                var mediant = new int[] { lower[0] + upper[0], lower[1] + upper[1] };

                if(val * mediant[1] > mediant[0])
                {
                    if(lim < mediant[1]) { return upper; }
                    lower = mediant;
                }
                else if(val * mediant[1] == mediant[0])
                {
                    if(lim >= mediant[1])
                    {
                        return mediant;
                    }
                    if(lower[1] < upper[1])
                    {
                        return lower;
                    }
                    return upper;
                }
                else
                {
                    if(lim < mediant[1])
                    {
                        return lower;
                    }
                    upper = mediant;
                }

                steps = steps + 1;

                if(steps > 5000)
                {
                    Debug.LogError($"Took more than 500 steps");
                    break;
                }
            }

            return new int[] { 0, 0 };
        }

        public void ResolutionChanged()
        {
            if (_camera != null)
            {
                Debug.Log($"[DetectResolutionChange] Resolution Changed! {_LastResolution} -> {_CurrentResolution}");
                if (Screen.width < Screen.height) // w < h. Portrait aspect.
                {
                    _CurrentOrientation = Orientation.Portrait;
                }
                else if (Screen.width > Screen.height) // w > h. Landscape aspect.
                {
                    _CurrentOrientation = Orientation.Landscape;
                }
                else // w == h. Dunno yet.
                {
                    //_camera.orthographicSize = 11;
                }

                //int r = gcd(Screen.width, Screen.height);
                //_CurrentAspectRatio.Set(Screen.width / r, Screen.height / r);
                int[] _arc = arc(Screen.width / Screen.height, 50);
                _CurrentAspectRatio.Set(_arc[0], _arc[1]);

                if (Application.isPlaying) OnResolutionChange?.Invoke(_CurrentOrientation, _CurrentResolution);
            }
        }

        private void LateUpdate()
        {
            _CurrentResolution.Set(Screen.width, Screen.height);
            if (_CurrentResolution != _LastResolution)
            {
                ResolutionChanged();
                _LastResolution = _CurrentResolution;
            }
        }
    }
}
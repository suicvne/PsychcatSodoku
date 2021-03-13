using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku.Editor
{
    public static class TextureColorMenus
    {
        [MenuItem("Ignore Solutions/Test Texture Color Distribution")]
        public static void TestTextureColorDistribution()
        {
            TextureColorDistribution tcd = ScriptableObject.CreateInstance<TextureColorDistribution>();

            tcd.titleContent.text = "Texture Color Distribution";

            if(Selection.activeObject != null && Selection.activeObject is Texture2D)
            {
                tcd.SetTexture((Texture2D)Selection.activeObject);
            }
            tcd.ShowUtility();
        }
    }

    [Serializable]
    struct ColorCodeAndPixelFrequency
    {
        public Color _ColorCode;
        public int _PixelFrequency;
    }

    class TextureColorDistribution : EditorWindow
    {
        Texture2D _TextureToSort;
        Texture2D _potentialNewTexture;

        Dictionary<Color32, int> _Frequency = new Dictionary<Color32, int>();
        List<KeyValuePair<Color32, int>> _FrequencyList = new List<KeyValuePair<Color32, int>>();

        bool IsColorClose(Color32 a, Color32 b, int diff)
        {
            int _dr, _dg, _db;

            _dr = a.r - b.r;
            _dg = a.g - b.g;
            _db = a.b - b.b;

            return (_dr <= diff && _dg <= diff && _db <= diff);
        }

        Color? IsColorCloseInList(ref List<KeyValuePair<Color32, int>> list, Color32 c)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if(IsColorClose(list[i].Key, c, 20))
                {
                    return list[i].Key;
                }
            }

            return null;
        }

        Texture2D ScaleTexture(Texture2D tex)
        {
            Texture2D scaled = new Texture2D(128, 128, TextureFormat.RGBA32, true);
            Graphics.ConvertTexture(tex, scaled);
            return scaled;
        }

        RenderTextureFormat SupportedRenderTextureFormat()
        {
            for(int i = 0; i < (int)RenderTextureFormat.R16; i++)
            {
                bool supports = SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)i);
                Debug.Log($"Supports {(RenderTextureFormat)i}?: {supports}");

                if (supports) return (RenderTextureFormat)i;
            }

            return RenderTextureFormat.Default;
        }

        void SortTexture()
        {
            if(_TextureToSort != null)
            {
                var supportedFormat = SupportedRenderTextureFormat();
                //Graphics.ConvertTexture(_TextureToSort, new Texture())
                Color32[] colors = _TextureToSort.GetPixels32();

                for (int i = 0; i < colors.Length; i++)
                {
                    int existingInd = FindColorInList(colors[i]);
                    if(existingInd != -1) 
                    {
                        int existingCount = _FrequencyList[existingInd].Value;
                        _FrequencyList[existingInd] = new KeyValuePair<Color32, int>(colors[i], existingCount + 1);
                    }
                    else
                    {
                        Color? closeColor = IsColorCloseInList(ref _FrequencyList, colors[i]);
                        //Color? closeColor = IsColorCloseInList(ref colors, colors[i]);
                        if (closeColor == null)
                        {
                            _FrequencyList.Add(new KeyValuePair<Color32, int>(colors[i], 1));
                        }
                        else
                        {
                            int pInd = FindColorInList(closeColor.Value);
                            if(pInd != -1)
                            {
                                int existingCount = _FrequencyList[pInd].Value;
                                _FrequencyList[pInd] = new KeyValuePair<Color32, int>(closeColor.Value, existingCount + 1);
                            }
                            else
                            {
                                _FrequencyList.Add(new KeyValuePair<Color32, int>(closeColor.Value, 1));
                            }
                        }
                    }
                }

                _FrequencyList = _FrequencyList.OrderBy(x => x.Value).ToList();
                //_FrequencyList = _Frequency.OrderBy(x => x.Value).ToList();
            }
        }

        int FindColorInList(Color c)
        {
            for(int i = 0; i < _FrequencyList.Count; i++)
            {
                if (_FrequencyList[i].Key == c) return i;
            }

            return -1;
        }

        public void SetTexture(Texture2D t)
        {
            _TextureToSort = t;
            _potentialNewTexture = t;

            SortTexture();
        }

        Vector2 _scrollPos = Vector2.zero;

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            _potentialNewTexture = (Texture2D)EditorGUILayout.ObjectField(_potentialNewTexture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();

            if (_potentialNewTexture != _TextureToSort)
            {
                _TextureToSort = _potentialNewTexture;
                SortTexture();
            }


            if(_FrequencyList.Count > 0)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                //for(int i = 0; i < _FrequencyList.Count; i++)
                for(int i = _FrequencyList.Count - 1; i >= 0; i--)
                {
                    var kvp = _FrequencyList[i];
                    EditorGUILayout.ColorField(new GUIContent($"Frequency: {kvp.Value}"), kvp.Key, false, false, false);
                    //EditorGUILayout.LabelField($"Color: {kvp.Key}; Frequency: {kvp.Value}");
                }
                EditorGUILayout.EndScrollView();
            }
            
        }
    }
}
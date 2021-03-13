using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IgnoreSolutions.Sodoku;
using UnityEditor;
using UnityEngine;
using IgnoreSolutions.PsychSodoku.Extensions;

namespace IgnoreSolutions.PsychSodoku.Editor
{
    public static class MassGenerateBoardsMenus
    {
        [MenuItem("Ignore Solutions/Mass Generate Boards from Images in Folder!")]
        public static void ShowMassGenerateBoards()
        {
            // TODO:

            if (Selection.activeObject == null) Debug.LogError($"Select a Folder!");
            string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());

            if (Directory.Exists(path))
            {
                var mgbf = ScriptableObject.CreateInstance<MassGenerateBoardsFromFolder>();
                mgbf.titleContent.text = $"Mass Generate Boards from Images";
                mgbf.SetBoardImagesFolder(path);
                mgbf.ShowUtility();
            }
            else Debug.LogError($"Please select a **FOLDER**!!!!!!");
        }
    }

    class MassGenerateBoardsFromFolder : EditorWindow
    {
        string _BoardImagesFolder;
        string _BoardOutputFolder = "Assets/Resources/Boards";

        List<string> _BoardImages;
        List<LevelData> _GeneratedLevelData;

        protected internal void SetBoardImagesFolder(string path)
        {
            _BoardImagesFolder = path;
        }

        private void VerifyOutputFolderExists()
        {
            if(!Directory.Exists(_BoardOutputFolder))
            {
                Directory.CreateDirectory(_BoardOutputFolder);
            }
        }

        private void OccupyBoardImagesArray()
        {
            string[] filesInImagesFolder = Directory.GetFiles(_BoardImagesFolder);

            for(int i = 0; i < filesInImagesFolder.Length; i++)
            {
                // Skip meta
                if (filesInImagesFolder[i].ToLower().EndsWith(".meta")) continue;
                if (filesInImagesFolder[i].ToLower().EndsWith(".jpeg") ||
                    filesInImagesFolder[i].ToLower().EndsWith(".jpg") ||
                    filesInImagesFolder[i].ToLower().EndsWith(".png"))
                {

                    _BoardImages.Add(filesInImagesFolder[i]);
                }
            }
        }

        private void OnFocus()
        {
            if (_BoardImages == null) _BoardImages = new List<string>();
            if (_BoardImages.Count == 0) OccupyBoardImagesArray();

            VerifyOutputFolderExists();
        }

        bool _beginGenerating = false;
        bool _writeToDisk = false;
        bool _checkForLevelList = false;
        Vector2 _scrollPos = Vector2.zero;
        System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
        long lastMsPuzzleGeneration = -1;
        long lastMsWriteToDisk = -1;

        private void DoPuzzleGeneration()
        {
            if(_GeneratedLevelData == null) _GeneratedLevelData = new List<LevelData>();
            if (_GeneratedLevelData.Count > 0) _GeneratedLevelData.Clear();

            _stopwatch.Start();

            foreach (var imagePath in _BoardImages)
            {
                string imgGUID = AssetDatabase.AssetPathToGUID(imagePath);

                int lastPath = imagePath.LastIndexOf('/')+1;
                int lastExtDot = imagePath.LastIndexOf('.');

                string indexName = imagePath.Substring(lastPath, lastExtDot - lastPath);

                LevelData testNewLevelData = ScriptableObject.CreateInstance<LevelData>();
                testNewLevelData.SetTilesetTexture(AssetDatabase.LoadAssetAtPath<Texture>(imagePath));
                testNewLevelData.name = $"Level_{indexName}";
                testNewLevelData.GenerateBoard();

                _GeneratedLevelData.Add(testNewLevelData);
            }

            _GeneratedLevelData =
                _GeneratedLevelData.OrderBy(
                    x => IgnoreSolutions.PsychSodoku.Extensions.Extensions.PadNumbers(x.name)
                ).ToList();

            _stopwatch.Stop();
            lastMsPuzzleGeneration = _stopwatch.ElapsedMilliseconds;
        }

        private void WriteGeneratedPuzzlesToDisk(bool overwrite = false)
        {
            if(_GeneratedLevelData == null || _GeneratedLevelData.Count == 0)
            {
                Debug.LogError($"No Generated Level Data.....what are you trying to do? o.o");
                return;
            }

            _stopwatch.Reset();
            _stopwatch.Start();

            for(int i = 0; i < _GeneratedLevelData.Count; i++)
            {
                LevelData generatedLevelData = _GeneratedLevelData[i];
                string savePath = Path.Combine(_BoardOutputFolder, generatedLevelData.name + ".asset");
                string absoluteSavePath = Path.Combine(Environment.CurrentDirectory, savePath);

                if(File.Exists(absoluteSavePath))
                {
                    if (overwrite == false)
                    {
                        Debug.LogError($"ERROR: unable to write Level Data at {savePath} because the file already exists.");
                        _GeneratedLevelData[i] = AssetDatabase.LoadAssetAtPath<LevelData>(savePath);
                        continue;
                    }
                    else File.Delete(savePath);
                }

                AssetDatabase.CreateAsset(generatedLevelData, savePath);

                _GeneratedLevelData[i] = AssetDatabase.LoadAssetAtPath<LevelData>(savePath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CheckForLevelList();

            _stopwatch.Stop();
            lastMsWriteToDisk = _stopwatch.ElapsedMilliseconds;
        }

        private void CheckForLevelList()
        {
            string[] potentialPaths = AssetDatabase.FindAssets("LevelList");
            if(potentialPaths.Length > 0)
            {
                LevelList list = null;
                string path = null;
                foreach(var potentialPath in potentialPaths)
                {
                    string pathFromGuid = AssetDatabase.GUIDToAssetPath(potentialPath);
                    list = AssetDatabase.LoadAssetAtPath<LevelList>(pathFromGuid);
                    if (list != null)
                    {
                        path = pathFromGuid;
                        break;
                    }
                }

                if (list == null) { Debug.LogWarning($"No level list found."); return; }
                Debug.Log($"Found LevelList at {path}. Name: {list.name}");
                list.AddLevelsToList(_GeneratedLevelData.ToArray());
            }
            else
            {
                Debug.LogWarning($"No level list found.");
            }
        }

        private void OnGUI()
        {
            if (_BoardImages == null || _BoardImages.Count == 0)
            {
                EditorGUILayout.LabelField($"No valid files in '{_BoardImagesFolder}'");
            }
            else
            {
                EditorGUILayout.LabelField($"Boards to Generate: {_BoardImages.Count}");
                EditorGUILayout.LabelField($"Output Directory: {_BoardOutputFolder}");
                bool outputExists = Directory.Exists(_BoardOutputFolder);
                EditorGUILayout.LabelField($"Output Dir Exists: {outputExists}");

                if(outputExists)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Generate puzzles in memory");
                    _beginGenerating = EditorGUILayout.Toggle(_beginGenerating);
                    EditorGUILayout.EndHorizontal();

                    if (_beginGenerating)
                    {
                        _beginGenerating = false;
                        DoPuzzleGeneration();
                        Repaint();
                    }

                    if(lastMsPuzzleGeneration != -1)
                    {
                        EditorGUILayout.LabelField($"Generated {_GeneratedLevelData.Count} boards in {lastMsPuzzleGeneration} ms");
                    }
                }

                if(_GeneratedLevelData != null && _GeneratedLevelData.Count > 0)
                {
                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                    for(int i = 0; i < _GeneratedLevelData.Count; i++)
                    {
                        _GeneratedLevelData[i] = (LevelData)EditorGUILayout.ObjectField(_GeneratedLevelData[i], typeof(LevelData), false);
                    }
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Write to Disk");
                    _writeToDisk = EditorGUILayout.Toggle(_writeToDisk);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Check for Level List");
                    _checkForLevelList = EditorGUILayout.Toggle(_checkForLevelList);
                    EditorGUILayout.EndHorizontal();

                    if(_writeToDisk)
                    {
                        _writeToDisk = false;
                        WriteGeneratedPuzzlesToDisk();
                    }

                    if(_checkForLevelList)
                    {
                        _checkForLevelList = false;
                        CheckForLevelList();
                    }

                    if (lastMsWriteToDisk != -1)
                    {
                        EditorGUILayout.LabelField($"Wrote {_GeneratedLevelData.Count} boards to disk in {lastMsWriteToDisk} ms");
                    }
                }

                
            }
        }
    }
}
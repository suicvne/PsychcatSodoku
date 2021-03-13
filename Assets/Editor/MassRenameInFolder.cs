

using System.Collections.Generic;
using System.IO;
using IgnoreSolutions.PsychSodoku.Extensions;
using UnityEditor;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku.Editor
{
    public static class MassRenameMenus
    {
        [MenuItem("Ignore Solutions/Mass Rename Files in Selected Folder")]
        public static void ShowMassRenameInFolder()
        {
            var selection = Selection.activeObject;
            if (selection == null) Debug.LogError($"Please select a folder!");
            string path = AssetDatabase.GetAssetPath(selection.GetInstanceID());

            if (Directory.Exists(path))
            {
                var mrf = ScriptableObject.CreateInstance<MassRenameInFolder>();
                mrf.SetDirectory(path);
                mrf.titleContent.text = $"Mass Rename in `{path}`";
                mrf.ShowModal();
            }
            else Debug.LogError($"Please select a FOLDER!!!!!");
        }
    }

    [System.Serializable]
    public class UnityFileInfo
    {
        public string ProposedRename;

        public string PrimaryFilePath;
        public Texture2D PreviewTexture;
        public string MetadataFilePath
        {
            get => PrimaryFilePath + ".meta";
        }

    }

    class MassRenameInFolder : EditorWindow
    {
        private List<UnityFileInfo> _FileInfo;
        private string _Directory;
        private bool _Shuffle = false;
        private bool _DoRename = false;

        public void SetDirectory(string dir)
        {
            _Directory = dir;
        }

        void OccupyFileInfoList()
        {
            string[] filesInDir = Directory.GetFiles(_Directory);

            int count = 0;
            foreach(string filePath in filesInDir)
            {
                if(filePath.ToLower().EndsWith(".png")
                    || filePath.ToLower().EndsWith(".jpg")
                    || filePath.ToLower().EndsWith(".jpeg"))
                {
                    //bool hasMeta = File.Exists(filePath + ".meta");

                    _FileInfo.Add(new UnityFileInfo
                    {
                        ProposedRename = filePath.Substring(0, filePath.LastIndexOf('/') + 1) + $"{count}" + filePath.Substring(filePath.LastIndexOf('.')).ToLower(),
                        PrimaryFilePath = filePath,
                        PreviewTexture = AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D)))
                    });
                    count++;
                }
            }
        }

        private void OnFocus()
        {
            if(_FileInfo == null || _FileInfo.Count == 0)
            {
                _FileInfo = new List<UnityFileInfo>();
                
            }

            if(_FileInfo.Count == 0)
            {
                OccupyFileInfoList();
            }
        }

        Vector2 scrollPos = Vector2.zero;

        private void RenameFilesAndMeta()
        {
            int idx = 0;
            foreach(var file in _FileInfo)
            {
                if(File.Exists(file.PrimaryFilePath) && File.Exists(file.MetadataFilePath))
                {
                    string newFileNameNoExt = file.PrimaryFilePath.Substring(0, file.PrimaryFilePath.LastIndexOf('/') + 1) + $"{idx}";
                    string ext = file.PrimaryFilePath.Substring(file.PrimaryFilePath.LastIndexOf('.')).ToLower();
                    ext = ext == "jpeg" ? "jpg" : ext;

                    // Move meta file first.
                    File.Move(file.MetadataFilePath, newFileNameNoExt + ext + ".meta");
                    // Move main file.
                    File.Move(file.PrimaryFilePath, newFileNameNoExt + ext);

                    Debug.Log($"File {idx}\nPrimary: {file.PrimaryFilePath} -> {newFileNameNoExt + ext}\nMeta: {file.MetadataFilePath} -> {newFileNameNoExt + ext + ".meta"}");
                    idx++;
                }
            }
        }

        private void OnGUI()
        {
            if (_FileInfo != null && _FileInfo.Count > 0)
            {
                EditorGUILayout.LabelField($"Shuffle List");
                _Shuffle = EditorGUILayout.Toggle(_Shuffle);
                GUILayout.Space(10f);

                EditorGUILayout.LabelField($"Do Rename");
                _DoRename = EditorGUILayout.Toggle(_DoRename);
                GUILayout.Space(10f);


                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                int idx = 0;
                foreach(var pathInfo in _FileInfo)
                {
                    EditorGUILayout.LabelField($"File {idx}");

                    //GUILayout.Label(pathInfo.PreviewTexture);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Path: {pathInfo.PrimaryFilePath}");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Meta: {pathInfo.MetadataFilePath}");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Proposed Rename: {pathInfo.ProposedRename}");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Meta Exists: {File.Exists(pathInfo.MetadataFilePath)}");
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(15f);
                    idx++;
                    //EditorGUILayout.ObjectField(pathInfo, typeof(UnityFileInfo), false);
                }
                EditorGUILayout.EndScrollView();

                if(_Shuffle)
                {
                    _Shuffle = false;
                    _FileInfo.Shuffle();
                }

                if(_DoRename)
                {
                    _DoRename = false;
                    RenameFilesAndMeta();
                    _FileInfo.Clear();
                    OccupyFileInfoList();
                }
            }
            else
            {
                EditorGUILayout.LabelField($"No files in the given Directory....");
            }
        }
    }
}
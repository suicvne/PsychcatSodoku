

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
            if (string.IsNullOrEmpty(_Directory))
            {
                return;
            }

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
            renameTotal = _FileInfo.Count;
            renameIdx = 0;
            foreach(var file in _FileInfo)
            {
                if(File.Exists(file.PrimaryFilePath) && File.Exists(file.MetadataFilePath))
                {
                    Debug.Log($"File {idx}");
                    string newFileNameNoExt = file.PrimaryFilePath.Substring(0, file.PrimaryFilePath.LastIndexOf('/') + 1) + $"{idx}";
                    string ext = file.PrimaryFilePath.Substring(file.PrimaryFilePath.LastIndexOf('.')).ToLower();
                    ext = ext == "jpeg" ? "jpg" : ext;

                    if(File.Exists(newFileNameNoExt + ext + ".meta"))
                    {
                        Debug.LogWarning($"ERROR: meta already exists at '{newFileNameNoExt + ext + ".meta"}'");
                        File.Delete(newFileNameNoExt + ext + ".meta");
                    }

                    if(File.Exists(newFileNameNoExt + ext))
                    {
                        Debug.LogWarning($"ERROR: File/image already exists at '{newFileNameNoExt + ext}'");
                        File.Delete(newFileNameNoExt + ext + ".meta");
                    }

                    // Move meta file first.
                    Debug.Log($"{file.MetadataFilePath} -> {newFileNameNoExt + ext + ".meta"}");
                    File.Move(file.MetadataFilePath, newFileNameNoExt + ext + ".meta");
                    // Move main file.
                    Debug.Log($"{file.PrimaryFilePath} -> {newFileNameNoExt + ext}");
                    File.Move(file.PrimaryFilePath, newFileNameNoExt + ext);

                    Debug.Log($"File {idx}\nPrimary: {file.PrimaryFilePath} -> {newFileNameNoExt + ext}\nMeta: {file.MetadataFilePath} -> {newFileNameNoExt + ext + ".meta"}");
                    idx++;
                    renameIdx += 2; // 2, 1 meta, 1 file.
                }
            }
        }

        bool midRename = false;
        int renameIdx = -1;
        int renameTotal = -1;

        private void OnGUI()
        {
            if (_FileInfo != null && _FileInfo.Count > 0 && midRename == false)
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
                    midRename = true;
                    RenameFilesAndMeta();
                    _FileInfo.Clear();
                    OccupyFileInfoList();
                    midRename = false;
                }
            }
            else if(midRename)
            {
                EditorGUILayout.LabelField($"Renaming.....{renameIdx} / {renameTotal} ({(float)renameIdx / (float)renameTotal}%");
                Repaint();
            }
            else
            {
                EditorGUILayout.LabelField($"No files in the given Directory....");
            }
        }
    }
}
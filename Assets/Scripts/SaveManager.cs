using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IgnoreSolutions
{
    public abstract class SaveManager<T> : Singleton<SaveManager<T>>
    {
        [SerializeField] internal int _SelectedSave;
        [SerializeField] internal T[] _LoadedSaves;

        public delegate void SavesRead();
        public delegate void SelectedSaveChanged(T _SelectedSave);

        public event SavesRead OnSavesRead;
        public event SelectedSaveChanged OnSelectedSaveChange;

        public VersionNumber _ClientVersionNumber;

        internal static string ApplicationSavePath
        {
            get
            {
                if (Application.isMobilePlatform || Application.isConsolePlatform)
                    return Application.persistentDataPath;
                else
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PsychCatSudoku");
            }
        }

        public T GetCurrentSave()
        {
            if (_SelectedSave != -1) return _LoadedSaves[_SelectedSave];
            return default(T);
        }

        public T SetCurrentSaveIndex(int index)
        {
            if (index > _LoadedSaves.Length) return default(T);

            _SelectedSave = index;
            OnSelectedSaveChange?.Invoke(_LoadedSaves[index]);

            return _LoadedSaves[index];
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (!Directory.Exists(SaveManager<T>.ApplicationSavePath))
            {
                Debug.Log($"Application Save Path `{ApplicationSavePath}` does not exist. Creating.");
                Directory.CreateDirectory(ApplicationSavePath);
            }
            else Debug.Log($"Save Path: `{ApplicationSavePath}`");

            ReloadTrackedSaves();
        }

        public virtual void ClearTrackedSaves()
        {
            if(_LoadedSaves != null && _LoadedSaves.Length > 0)
                _LoadedSaves = null;
        }

        public abstract void ReloadTrackedSaves();
    }
}

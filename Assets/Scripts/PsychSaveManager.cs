using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IgnoreSolutions.Sodoku;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    public class PsychSaveManager : SaveManager<PsychSudokuSave>
    {
        [SerializeField] LevelList _LevelList;

        public override void ReloadTrackedSaves()
        {
            ClearTrackedSaves();
            Debug.Log($"[PsychSaveManager] Save Format Version: {_ClientVersionNumber.ToString()} {_ClientVersionNumber.ToLong()}");

            var save = PsychSudokuSave.ReadSaveFromJSON();

            if(save != null)
            {
                Debug.Log($"[PsychSaveManager] Loaded save: {save}");
                _LoadedSaves = new PsychSudokuSave[1];
                _LoadedSaves[0] = save;
                SetCurrentSaveIndex(0);
            }
            else
            {
                Debug.Log($"[PsychSaveManager] No saves found to load.");
                save = PsychSudokuSave.Default(_LevelList);
                bool success = PsychSudokuSave.WriteSaveJSON(save);
                if (success == false) Debug.LogError($"Failed to write save to JSON. Unknown reasons why.");
                else ReloadTrackedSaves();
            }
        }
    }
}
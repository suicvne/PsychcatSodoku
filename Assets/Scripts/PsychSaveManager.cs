using System;
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

        public LevelList GetLevelList { get => _LevelList; }

        public delegate void SaveHasSaveState(PsychSudokuSave save);
        public event SaveHasSaveState OnCurSaveHasSaveState;


        public override void ReloadTrackedSaves()
        {
            ClearTrackedSaves();
            Debug.Log($"[PsychSaveManager {gameObject.name}] Save Format Version: {_ClientVersionNumber.ToString()} {_ClientVersionNumber.ToLong()}");

            //var save = PsychSudokuSave.ReadSaveFromJSON();
            var jsonSave = PsychSudokuSave.ReadSaveFromJSON();
            var save = PsychSudokuSaveSerializer.ReadSudokuSave(Path.Combine(ApplicationSavePath, "state.txt"), _LevelList);

            if(save == null && jsonSave != null)
            {
                Debug.Log($"[PsychSaveManager] Upgrading JSON save to new save format.");
                PsychSudokuSaveSerializer.WriteSudokuSave(jsonSave, Path.Combine(ApplicationSavePath, "state.txt"));
                File.Move(Path.Combine(ApplicationSavePath, "state.json"), Path.Combine(ApplicationSavePath, "state.json.bak"));
            }
            else if (save != null)
            {
                Debug.Log($"[PsychSaveManager {gameObject.name}] Loaded save: {save}. Last Completed Level Index: {save._LastCompletedLevel}");
                _LoadedSaves = new PsychSudokuSave[1];
                _LoadedSaves[0] = save;
                SetCurrentSaveIndex(0);

                if(GetCurrentSave()._SaveStateInformation._IsValidSaveState)
                {
                    Debug.Log($"[PsychSaveManager {gameObject.name}] TODO!!! Restore the game's save state. Level: {GetCurrentSave()._SaveStateInformation._LastLevelIndex}");
                    OnCurSaveHasSaveState?.Invoke(GetCurrentSave());
                }
            }
            else
            {
                try
                {
                    Debug.Log($"[PsychSaveManager {gameObject.name}] No saves found to load.");
                    save = PsychSudokuSave.Default(_LevelList);
                    bool success = PsychSudokuSave.WriteSave(save);

                    // TODO: Test AOT deserialization and THEN re-enable this.
                    if (success == false)
                    {
                        Debug.LogError($"Failed to write save to JSON. Unknown reasons why.");

                    }
                    else ReloadTrackedSaves();
                }
                catch(Exception ex)
                {
                    Debug.LogError($"[PsychSaveManager] Exception while attempting to write save: {ex.Message}\n\n{ex.StackTrace}\nEND\n");
                }
            }
        }

        public void SetParametersInjestToNextLevel(PsychSudokuSave save, PlayDifficulty difficulty)
        {
            var parameterInjest = FindObjectOfType<SudokuParametersInjest>();
            if (parameterInjest == null) throw new System.Exception("[PsychSaveManager] Could not find parameter injest to setup game screen parameters.");

            if (save._LastCompletedLevel == -1)
            {
                parameterInjest.SetSudokuParameters(0, _LevelList.GetLevelList()[0], difficulty);
            }
            else
            {
                if (save._LastCompletedLevel == _LevelList.GetLevelList().Count - 1) // TODO: Last level already completed?
                {
                    Debug.Log($"TODO: Handle case where all levels up to last are completed.");
                }
                else
                {
                    parameterInjest.SetSudokuParameters(save._LastCompletedLevel + 1,
                        _LevelList.GetLevelList()[save._LastCompletedLevel + 1],
                        difficulty);
                }
            }
        }

        
    }
}
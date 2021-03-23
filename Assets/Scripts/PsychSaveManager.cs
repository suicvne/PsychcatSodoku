using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IgnoreSolutions.Sodoku;
using UnityEngine;
using static ModifyShaderOffset;

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
                Debug.Log($"[PsychSaveManager] Loaded save: {save}. Last Completed Level Index: {save._LastCompletedLevel}");
                _LoadedSaves = new PsychSudokuSave[1];
                _LoadedSaves[0] = save;
                SetCurrentSaveIndex(0);

                if(GetCurrentSave()._SaveStateInformation._IsValidSaveState)
                {
                    Debug.Log($"[PsychSaveManager ReloadTrackedSaves] TODO!!! Restore the game's save state. Level: {GetCurrentSave()._SaveStateInformation._LastLevelIndex}");
                }
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

        private void OnApplicationFocus(bool focus)
        {
            
        }

        private void OnApplicationPause(bool pause)
        {
            
        }

        private void OnApplicationQuit()
        {
            
        }
    }
}
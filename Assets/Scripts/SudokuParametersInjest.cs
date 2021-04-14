using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.Sodoku;
using UnityEngine;
using static ModifyShaderOffset;

namespace IgnoreSolutions.PsychSodoku
{
    public class SudokuParametersInjest : Singleton<SudokuParametersInjest>
    {
        [SerializeField] LevelList _LevelList;
        [SerializeField] LevelData _LevelToLoad;
        [SerializeField] int _LevelIndex;
        [SerializeField] PlayDifficulty _Difficulty;
        [SerializeField] bool _ShouldRestoreFromCurSaveState = false;

        private void Awake()
        {
            if (PsychSaveManager.InstanceNull() == false)
            {
                ((PsychSaveManager)PsychSaveManager.p_Instance).OnCurSaveHasSaveState += SudokuParametersInjest_OnCurSaveHasSaveState;
            }
            else Debug.LogWarning($"[SudokuParametersInjest] No PsychSaveManager instance yet.");

            DontDestroyOnLoad(gameObject);
        }

        private void SudokuParametersInjest_OnCurSaveHasSaveState(PsychSudokuSave save)
        {
            // Save has already been verified up to this point.
            Debug.Log($"[SudokuParametersInjest] Setting up to use Save State Information from Current Save. Will injest.");

            SetSudokuParameters(save._SaveStateInformation._LastLevelIndex,
                _LevelList.GetLevelList()[save._SaveStateInformation._LastLevelIndex],
                save._SaveStateInformation._LastLevelDifficulty);
            SetShouldRestoreFromSaveState(true);
        }

        public void SetSudokuParameters(int levelIndex,
            LevelData level,
            PlayDifficulty difficulty)
        {
            _LevelIndex = levelIndex;
            _LevelToLoad = level;
            _Difficulty = difficulty;
        }

        public bool GetShouldRestoreFromSaveState() => _ShouldRestoreFromCurSaveState;
        public bool SetShouldRestoreFromSaveState(bool shouldRestore) => _ShouldRestoreFromCurSaveState = shouldRestore;

        public LevelList GetLevelList() => _LevelList;
        public int GetLevelIndex() => _LevelIndex;
        public LevelData GetLevel() => _LevelToLoad;
        public PlayDifficulty GetDifficulty() => _Difficulty;
        
    }
}
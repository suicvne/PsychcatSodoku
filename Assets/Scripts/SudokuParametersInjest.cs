using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.Sodoku;
using UnityEngine;
using static ModifyShaderOffset;

namespace IgnoreSolutions.PsychSodoku
{
    /// <summary>
    /// A small singleton responsible for ingesting the
    /// Main Menu's setup parameters and setting up the grid
    /// accordingly. 
    /// </summary>
    public class SudokuParametersInjest : Singleton<SudokuParametersInjest>
    {
        [Header("External References")]
        [SerializeField] SwitchScene _SceneSwitcher;

        [Header("Sudoku Board Setup")]
        [SerializeField] LevelList _LevelList;
        [SerializeField] LevelData _LevelToLoad;
        [SerializeField] int _LevelIndex;
        [SerializeField] PlayDifficulty _Difficulty;
        [SerializeField] bool _ShouldRestoreFromCurSaveState = false;


        /// <summary>
        /// Script Execution Order guarantees that
        /// this occurs AFTER the SaveManager is instantiated.
        /// </summary>
        private void Awake()
        {
            Debug.Log($"[SudokuParametersInjest] Awake");
            if(_SceneSwitcher == null)
            {
                Debug.Log($"[SudokuParametersInjest] _SceneSwitcher was null.");
                _SceneSwitcher = FindObjectOfType<SwitchScene>();
                if (_SceneSwitcher == null) Debug.LogError($"[SudokuParametersInjest] _SceneSwitcher could not be found.");
            }

            if (PsychSaveManager.InstanceNull() == false
                && PsychSaveManager.p_Instance.GetCurrentSave()._SaveStateInformation._IsValidSaveState)
            {
                SudokuParametersInjest_OnCurSaveHasSaveState(PsychSaveManager.p_Instance.GetCurrentSave());
            }
            else Debug.LogWarning($"[SudokuParametersInjest] No PsychSaveManager instance yet.");

            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// The event handler for when PsychSaveManager notifies
        /// us that our current save has save state information.
        ///
        /// At this point, ParametersInjest will Set the parameters
        /// and notify the SceneManager that we want to load our last scene.
        ///
        /// From there, the Board script (Currently "ModifyShaderOffset") will
        /// read the save state information, load it onto the board,
        /// and resume the Game Timer in GameTimeManager. (TODO/WIP)
        /// </summary>
        /// <param name="save">The loaded save that has the save state info.</param>
        private void SudokuParametersInjest_OnCurSaveHasSaveState(PsychSudokuSave save)
        {
            // Save has already been verified up to this point.
            Debug.Log($"[SudokuParametersInjest] Setting up to use Save State Information from Current Save. Will injest.");

            SetSudokuParameters(save._SaveStateInformation._LastLevelIndex,
                _LevelList.GetLevelList()[save._SaveStateInformation._LastLevelIndex],
                save._SaveStateInformation._LastLevelDifficulty);
            SetShouldRestoreFromSaveState(true);

            _SceneSwitcher.ChangeScene(1);
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
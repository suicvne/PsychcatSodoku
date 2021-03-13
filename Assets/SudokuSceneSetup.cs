using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.Sodoku;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ModifyShaderOffset;

namespace IgnoreSolutions.PsychSodoku
{
    public class SudokuSceneSetup : MonoBehaviour
    {
        [SerializeField] PlayDifficulty _BoardDifficulty;
        [SerializeField] LevelList _MasterLevelList;
        [SerializeField] int _LevelToLoadFromList = 0;
        [SerializeField] ModifyShaderOffset _GameBoard;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"OnLevelFinishedLoad: Scene: {scene.name}, Mode: {mode}");

            // TODO: Update our GameBoard object with what it needs to start
            if(_GameBoard != null)
            {
                _GameBoard.SetLevelInformation(_MasterLevelList.GetLevelList()[_LevelToLoadFromList], _BoardDifficulty);
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
    }
}
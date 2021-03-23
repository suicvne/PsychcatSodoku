using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.Sodoku;
using UnityEngine;
using static ModifyShaderOffset;

namespace IgnoreSolutions.PsychSodoku
{
    public class SudokuParametersInjest : Singleton<SudokuParametersInjest>
    {
        [SerializeField] LevelData _LevelToLoad;
        [SerializeField] int _LevelIndex;
        [SerializeField] PlayDifficulty _Difficulty;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void SetSudokuParameters(int levelIndex, LevelData level, PlayDifficulty difficulty)
        {
            _LevelIndex = levelIndex;
            _LevelToLoad = level;
            _Difficulty = difficulty;
        }

        public int GetLevelIndex() => _LevelIndex;
        public LevelData GetLevel() => _LevelToLoad;
        public PlayDifficulty GetDifficulty() => _Difficulty;
        
    }
}
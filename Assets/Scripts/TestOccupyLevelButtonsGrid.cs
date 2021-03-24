using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.Sodoku;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    public class TestOccupyLevelButtonsGrid : MonoBehaviour
    {
        [SerializeField] LevelButton _Prefab;
        [SerializeField] LevelList _LevelList;
        [SerializeField] int _LevelsPerPage = 20;

        [SerializeField] List<LevelButton> _LevelButtons = new List<LevelButton>();
        int _ListOffset = 0;

        public void Start()
        {
            
        }

        void GenerateLevelButtons()
        {
            for(int i = 0; i < _LevelsPerPage; i++)
            {
                LevelButton _newButton = Instantiate(_Prefab, transform);
                _newButton.name = $"Level {i} ({i + 1 + _ListOffset}";
            }
        }
    }
}
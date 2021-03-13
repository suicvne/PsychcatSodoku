using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IgnoreSolutions.PsychSodoku.Extensions;
using UnityEngine;

namespace IgnoreSolutions.Sodoku
{
    [CreateAssetMenu(menuName = "Ignore Solutions/Level List")]
    public class LevelList : ScriptableObject
    {
        [SerializeField]
        List<LevelData> _Levels = new List<LevelData>();

        public List<LevelData> GetLevelList() => _Levels;

        public void AddLevelsToList(params LevelData[] _levelsToAdd)
        {
            int levelsAdded = 0;
            for(int i = 0; i < _levelsToAdd.Length; i++)
            {
                if(_Levels.Find(x => x.name == _levelsToAdd[i].name) != null)
                {
                    Debug.LogWarning($"Skipping existing level {_levelsToAdd[i].name} already in the list.");
                    continue;
                }

                _Levels.Add(_levelsToAdd[i]);
                levelsAdded++;
            }

            _Levels = _Levels.OrderBy(x => Extensions.PadNumbers(x.name)).ToList();
        }
    }
}
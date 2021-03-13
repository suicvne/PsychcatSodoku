using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.Sodoku
{
    [CreateAssetMenu(menuName = "Ignore Solutions/Level List")]
    public class LevelList : ScriptableObject
    {
        [SerializeField]
        List<LevelData> _Levels = new List<LevelData>();

        public List<LevelData> GetLevelData() => _Levels;
    }
}
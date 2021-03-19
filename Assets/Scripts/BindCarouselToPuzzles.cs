using System;
using IgnoreSolutions.Sodoku;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    public class BindCarouselToPuzzles : BindCarouselItems<LevelData>
    {
        [SerializeField] LevelList _LevelList;
    }
}
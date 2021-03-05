using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    [System.Serializable]
    public struct NumberToSprite
    {
        [Range(0, 9)]
        public short _Number;
        public Sprite _Sprite;
    }

    [CreateAssetMenu(menuName = "Ignore Solutions/Number -> Sprite Lookup Table")]
    public class NumberToSpriteLookup : ScriptableObject
    {
        [SerializeField] List<NumberToSprite> NumberToSpriteTable = new List<NumberToSprite>();

        public Sprite GetSpriteByNumber(short number)
        {
            for(int i = 0; i < NumberToSpriteTable.Count; i++)
            {
                if (NumberToSpriteTable[i]._Number == number) return NumberToSpriteTable[i]._Sprite;
            }

            return null;
        }
    }
}
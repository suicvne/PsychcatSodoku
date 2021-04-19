using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace IgnoreSolutions.PsychSodoku
{
    public class SodukoGriidSpot : MonoBehaviour
    {
        public bool _AllowInputEvents = false;
        public bool _CanBeSelected = true;
        public Vector2Int GridSpot;
        internal ModifyShaderOffset parent;

        public int DebugValue = -1;
        public int _SquareSolution = -1;
        public int _SquareFilledValue = -1;

        public int _LevelIndex = -1;
        public int _SquareGroupNo = -1;

        internal List<int> PossibleNumbers = new List<int>();

        public UnityEvent<SodukoGriidSpot> _OnGridSpotTapped;

        [SerializeField] internal TMP_Text _PossibleNumbersText;
        [SerializeField] internal SpriteRenderer _SolutionNumberSprite;

        private void OnEnable()
        {
            UpdatePossibleNumbers();
        }

        public void SetAllowInputEvents(bool allowed) => _AllowInputEvents = allowed;

        public void SetCanBeSelected(bool canBeSelected)
        {
            if (_CanBeSelected != canBeSelected)
            {
                _CanBeSelected = canBeSelected;
                _SolutionNumberSprite.color = _CanBeSelected ? Color.white : Color.gray;
            }
        }

        public void SetTMP(TMP_Text text)
        {
            _PossibleNumbersText = text;
        }

        public void ClearPossibleNumbers()
        {
            PossibleNumbers.Clear();
        }

        public void AddPossibleNumber(int number)
        {
            if (PossibleNumbers.Contains(number))
            {
                PossibleNumbers.Remove(number);
            }
            else
            {
                PossibleNumbers.Add(number);
            }
        }

        private char[] _FullWidth = new char[]
        {
        '０', '１', '２', '３', '４', '５', '６', '７', '８', '９'
        };

        private char NumberToFullWidth(int number)
        {
            if (number > 9) return Char.MaxValue;

            return _FullWidth[number];
        }

        public void UpdatePossibleNumbers()
        {
            if (_PossibleNumbersText == null)
            {
                Debug.LogWarning($"Tried to update when _PossibleNumbersText is equal to null.", gameObject);
                return;
            }

            /*
            string format = "{0}　{1}　{2}\n{3}　{4}　{5}\n{6}　{7}　{8}";
            string[] formatNumbers = new string[] { "　", "　", "　",  "　", "　", "　",  "　", "　", "　" };
            */

            string format = "{0} {1} {2}\n{3} {4} {5}\n{6} {7} {8}";
            string[] formatNumbers = new string[] { " ", " ", " ", /**/ " ", " ", " ", /**/ " ", " ", " " };

            for (int i = 0; i < formatNumbers.Length; i++)
            {
                if (PossibleNumbers.Contains(i + 1))
                {
                    formatNumbers[i] = $"{i + 1}";
                    /*formatNumbers[i] = $"{NumberToFullWidth(i + 1)}";*/
                }
            }

            _PossibleNumbersText.text = string.Format(format, formatNumbers);
        }

        public void OnMouseDown()
        {
            if (_AllowInputEvents == false) return;
            if (_CanBeSelected == false) return;

            if (parent._DirectInputToNumberSelect == false)
            {
                Debug.Log($"{GridSpot} You pressed me!");

                _OnGridSpotTapped?.Invoke(this);
            }
        }

        public int GetValue()
        {
            if (parent != null)
            {
                return parent.GetValueAt(GridSpot);
            }

            return -2;
        }
    }
}
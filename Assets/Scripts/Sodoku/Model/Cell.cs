using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace SudokuLib.Model
{
    /// <summary>
    /// Game board cell 
    /// </summary>
    [Serializable]
    [DataContract]
    public class Cell
    {
        [HideInInspector] public string name;

        /// <summary>
        /// Row-Column number based position data structure.
        /// </summary>
        [Serializable]
        public struct RCPosition
        {
            [SerializeField] int _Row, _Column;

            /// <summary>
            /// Row number
            /// </summary>
            public int Row { get => _Row; internal set => _Row = value; }

            /// <summary>
            /// Columns number
            /// </summary>
            public int Column { get => _Column; internal set => _Column = value; }

            /// <summary>
            /// Initialize a new instance of the Row-Column based Position with given parameters.
            /// </summary>
            /// <param name="row"></param>
            /// <param name="column"></param>
            public RCPosition(int row, int column)
            {
                _Row = row;
                _Column = column;
            }
        }

        [SerializeField] int _Value, _Index, _GroupNo;
        [SerializeField] RCPosition _Position;
        

        /// <summary>
        /// Cell value.
        /// </summary>
        public int Value
        {
            get
            {
                return _Value;
            }
            internal set
            {
                _Value = value; name = $"({_Position.Row}, {_Position.Column}) = {value}";
            }
        }

        /// <summary>
        /// Cell index in which the cell is located in the single-dimensional list.
        /// </summary>
        public int Index { get => _Index; }

        /// <summary>
        /// Group number in which the cell is located.
        /// </summary>
        public int GroupNo { get => _GroupNo; }

        /// <summary>
        /// Row-Column number based position of the cell.
        /// </summary>
        public RCPosition Position { get => _Position; }

        /// <summary>
        /// Initialize a new instance of the Cell with given parameters.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="groupNo"></param>
        /// <param name="position"></param>
        public Cell(int value, int index, int groupNo, RCPosition position)
        {
            Value = value;
            _Index = index;
            _GroupNo = groupNo;
            _Position = position;

            name = $"({position.Row}, {position.Column}) = {value}";
        }
    }
}
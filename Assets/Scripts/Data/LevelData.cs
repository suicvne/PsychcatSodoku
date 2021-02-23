using System;
using System.Collections;
using System.Collections.Generic;
using SudokuLib;
using SudokuLib.Model;
using UnityEngine;

namespace IgnoreSolutions.Sodoku
{
    [Serializable]
    public struct SolutionStep
    {
        public int X;
        public int Y;
        public float Value;

        public SolutionStep(int x, int y, float value)
        {
            X = x;
            Y = y;
            Value = value;
        }
    }

    [CreateAssetMenu()]
    public class LevelData : ScriptableObject
    {
        /// <summary>
        /// This is both the number of sub grids AND
        /// the size of the subgrids.
        /// sss sss sss Subgrid
        /// sss sss sss
        /// sss sss sss
        ///
        ///
        /// x x x
        /// x x x
        /// x x x Full grid.
        /// </summary>
        public static int GridSize = 3;

        public static int GridWidth = GridSize * GridSize;

        /// <summary>
        /// TotalGridSize is equal to GridSize^4.
        /// </summary>
        public static int TotalGridSize = (GridSize * GridSize * GridSize * GridSize);

        [SerializeField] Cell[] cells;

        [SerializeField]
        Texture _BoardImage;

        [Header("Stats")]
        [SerializeField] bool _BoardHasValues;
        [SerializeField] bool _BoardGenerated;
        [Header("Toggles")]
        [SerializeField] bool _ForceGenerate;

        SudokuBoard b;

        public SudokuBoard GetSudokuBoard() => b;
        public Texture GetTilesetTexture() => _BoardImage;

        private void OnValidate()
        {
            if (_ForceGenerate)
            {
                _ForceGenerate = false;
                b = new SudokuBoard();

                if (b == null || b.Solver == null) throw new Exception(" Could not creeate board. Board or solver were null.");
                b.Solver.SolveThePuzzle(UseRandomGenerator: true);

                cells = new Cell[b.Cells.Count];
                b.Cells.CopyTo(cells);
            }

            _BoardGenerated = (b != null);
            _BoardHasValues = HasAllValues();

            
        }

        private bool HasAllValues()
        {
            if (b == null) return false;

            bool hasAllValues = true;
            foreach (var c in b.Cells)
            {
                if (c.Value == -1)
                {
                    hasAllValues = false;
                    break;
                }
            }

            return hasAllValues;
        }
    }
}
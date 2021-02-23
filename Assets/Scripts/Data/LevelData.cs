using System;
using System.Collections;
using System.Collections.Generic;
using SudokuLib;
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

        [SerializeField]
        Texture _BoardImage;

        [SerializeField]
        int[] _Solution;

        SudokuBoard b;

        void OnLoad()
        {
            if (_Solution == null || _Solution.Length <= 0)
            {
                _Solution = new int[TotalGridSize];
                b = new SudokuBoard();

                int ind = 0;
                foreach(var c in b.Cells)
                {
                    _Solution[ind] = c.Value;
                    ind++;
                }
            }
        }

        void ComputeSolutions()
        {
            List<SolutionStep> solutionSteps = new List<SolutionStep>();

            for (int i = 0; i < _Solution.Length; i++)
            {
                // Get coordinate on grid.
                int _x, _y;
                _x = i % GridWidth;
                _y = i / GridWidth;

                // 
                float logIndex = Mathf.Log(_Solution[i], 2) + 1;

                if (logIndex == Mathf.Floor(logIndex) && _Solution[i] == 0)
                {
                    solutionSteps.Add(new SolutionStep(_x, _y, logIndex));
                }
            }


        }
    }
}
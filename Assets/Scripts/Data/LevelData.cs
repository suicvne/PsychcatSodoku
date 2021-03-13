using System;
using System.Collections;
using System.Collections.Generic;
using SudokuLib;
using SudokuLib.Model;
using UnityEngine;
using System.Linq;

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
        const int EASY_FILLED_SQUARES = 49;
        const int MEDIUM_FILLED_SQUARES = 26;
        const int HARD_FILLED_SQUARES = 20;

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

        [SerializeField] protected internal Texture _BoardImage;

        [Header("SUDOKULIB")]
        [Header("Stats")]
        [SerializeField] bool _BoardHasValues;
        [SerializeField] bool _BoardGenerated;
        [Header("Toggles")]
        [SerializeField] bool _ForceGenerate;
        [Header("Difficulty Arrays")]
        [SerializeField] short[] _Easy = new short[TotalGridSize], _Medium = new short[TotalGridSize], _Hard = new short[TotalGridSize];

        SudokuBoard b;

        public SudokuBoard GetSudokuBoard() => b;
        public Texture GetTilesetTexture() => _BoardImage;
        public void SetTilesetTexture(Texture tilesetTexture) => _BoardImage = tilesetTexture;

        public Cell GetCell(int x, int y)
        {
            Cell foundCell = cells.FirstOrDefault(c => (c.Position.Row == x && c.Position.Column == y));
            return foundCell;
        }

        public Cell GetCellValueByDifficulty(int x, int y, int difficulty)
        {
            Cell c = GetCell(x, y);
            switch(difficulty)
            {
                case 0:
                    if (_Easy[c.Index] != 0) return c;
                    break;
                case 1:
                    if (_Medium[c.Index] != 0) return c;
                    break;
                case 2:
                    if (_Hard[c.Index] != 0) return c;
                    break;
            }

            return null;
        }

        private void ZeroOutDifficulties()
        {
            for(int i = 0; i < TotalGridSize; i++)
            {
                _Easy[i] = 0;
                _Medium[i] = 0;
                _Hard[i] = 0;
            }
        }

        public void GenerateBoard()
        {
            b = new SudokuBoard();
            if(b == null || b.Solver == null) throw new Exception(" Could not creeate board. Board or solver were null.");
            b.Solver.SolveThePuzzle(UseRandomGenerator: true);

            cells = new Cell[b.Cells.Count];
            b.Cells.CopyTo(cells);

            ZeroOutDifficulties();
            GenerateAllDifficulties();
        }

        private void OnValidate()
        {
            if (_ForceGenerate)
            {
                _ForceGenerate = false;

                GenerateBoard();
            }

            _BoardGenerated = (b != null);
            _BoardHasValues = HasAllValues();
        }

        private Cell GetCellByIndex(int index)
        {
            return cells[index];
        }

        private Cell TestGetRandomInGroup(int groupNo)
        {
            Cell[] ranInGroup = cells.Where(x => x.GroupNo == groupNo).ToArray();
            int ran_index = UnityEngine.Random.Range(0, ranInGroup.Length);

            if (ranInGroup.Length == 0)
            {
                Debug.Log($"Could not get with group No {groupNo}.");
                return null;
            }
            if (ran_index == ranInGroup.Length) ran_index = ranInGroup.Length - 1;

            Cell c = null;
            try
            {
                c = ranInGroup[ran_index];
            }
            catch(Exception ex)
            {
                Debug.Log($"{ran_index} Length: {ranInGroup.Length}; {ex.Message}");
            }

            return c;

        }

        private void GenerateForDifficulty(ref short[] difficultyArray, int squaresLeft, int diff)
        {
            int grBias = 1;
            int iterations = 0;
            while(squaresLeft > 0)
            {
                Cell c = TestGetRandomInGroup(grBias);
                if (difficultyArray[c.Index] == 0 && c.GroupNo == grBias)
                {
                    difficultyArray[c.Index] = 1; // Revealed
                    grBias++;
                    squaresLeft = squaresLeft - 1;
                    if (grBias > 9) grBias = 1;

                }
                iterations++;

                if (iterations == 400)
                {
                    Debug.LogError($"Took more than 400 iterations to generate the solutions for {diff}. Left: {squaresLeft}");
                    break;
                }
            }
        }

        private void GenerateAllDifficulties()
        {
            GenerateForDifficulty(ref _Easy, EASY_FILLED_SQUARES, 0);
            GenerateForDifficulty(ref _Medium, MEDIUM_FILLED_SQUARES, 1);
            GenerateForDifficulty(ref _Hard, HARD_FILLED_SQUARES, 2);
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
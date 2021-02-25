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
        const int MEDIUM_FILLED_SQUARES = 39;
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

        [SerializeField]
        Texture _BoardImage;

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

                GenerateAllDifficulties();
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

            return ranInGroup[ran_index];
        }

        private void GenerateAllDifficulties()
        {
            short eSqures = EASY_FILLED_SQUARES;
            short mSquares = MEDIUM_FILLED_SQUARES;
            short hSqures = HARD_FILLED_SQUARES;

            int grBias = 1;
            while(eSqures > 0)
            {
                Cell c = TestGetRandomInGroup(grBias);
                if(_Easy[c.Index] == 0 && c.GroupNo == grBias)
                {
                    _Easy[c.Index] = 1; // Revealed
                    grBias++;
                    if (grBias > 9) grBias = 0;
                    eSqures--;
                }
            }

            //for(short i = 0; i < TotalGridSize; i++)
            //{
            //    if(eSqures > 0)
            //    {
            //        if (UnityEngine.Random.Range(0, TotalGridSize) < 10 && eSqures > 0)
            //        {
            //            _Easy[i] = 1;
            //            eSqures--;
            //        }
            //        else _Easy[i] = 0;
            //    }

            //    if(mSquares > 0)
            //    {

            //    }

            //    if(hSqures > 0)
            //    {

            //    }
            //}
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
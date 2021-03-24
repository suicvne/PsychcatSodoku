using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    public enum StepType
    {
        ChangeNumber,
        RemoveNumber
    }

    public enum StepAffection
    {
        MainNumber,
        PossibleNumber
    }

    [Serializable]
    public struct SudokuStep
    {
        public StepType _StepType;
        public StepAffection _StepAffection;
        public int _StepValue;
        public int _GridPreviousValue;
        public int _PieceIndex;
    }

    public class TestUndoFeature : MonoBehaviour
    {
        [SerializeField] Queue<SudokuStep> _UndoStepsQueue = new Queue<SudokuStep>();
        [SerializeField] ModifyShaderOffset _GameBoard;

        public void PushUndo(int oldValue, int newValue, SodukoGriidSpot gridSpot, StepType _type, StepAffection _affection)
        {
            _UndoStepsQueue.Enqueue(new SudokuStep
            {
                _StepType = _type,
                _StepAffection = _affection,
                _StepValue = newValue,
                _GridPreviousValue = oldValue,
                _PieceIndex = gridSpot._LevelIndex
            });
        }

        public void Undo()
        {
            if (_GameBoard._AnimationPending) return;
            if(_UndoStepsQueue.Count > 0)
            {
                SudokuStep undoStep = _UndoStepsQueue.Dequeue();
                SodukoGriidSpot _gridSpot = _GameBoard.Tiles[undoStep._PieceIndex];

                _GameBoard.SetSelectionLocationToTappedSpot(_gridSpot);
                if (undoStep._StepType == StepType.ChangeNumber) // put back to previous value.
                {
                    _gridSpot._SquareFilledValue = undoStep._GridPreviousValue;
                    StartCoroutine(_GameBoard.AnimateNumberUpdate(_gridSpot, _gridSpot.GetComponent<MeshRenderer>(), _gridSpot._SolutionNumberSprite, undoStep._GridPreviousValue, true));
                }
                else if(undoStep._StepType == StepType.RemoveNumber) // Remove Number means we use _StepValue to put the number back.
                {
                    if (undoStep._StepAffection == StepAffection.MainNumber)
                    {
                        _gridSpot._SquareFilledValue = undoStep._GridPreviousValue;
                        StartCoroutine(_GameBoard.AnimateNumberUpdate(_gridSpot, _gridSpot.GetComponent<MeshRenderer>(), _gridSpot._SolutionNumberSprite, undoStep._StepValue, true));
                        //_GameBoard.UpdateFilledNumberAtSelectedIndex(undoStep._StepValue);
                    }
                    else
                    {
                        _gridSpot.AddPossibleNumber(undoStep._StepValue);
                        _gridSpot.UpdatePossibleNumbers();
                    }
                }
            }
        }
    }
}
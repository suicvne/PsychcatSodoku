using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.Sodoku;
using UnityEngine;

namespace IgnoreSolutions.PsychSodoku
{
    [RequireComponent(typeof(ModifyShaderOffset))]
    public class SuspendGameState : MonoBehaviour
    {
        [SerializeField] bool _EnabledAndWorking = false;
        [SerializeField] bool _HadValidStateOnStart = false;

        public bool HadValidStateOnStart => _HadValidStateOnStart;
        public bool EnabledAndWorking => _EnabledAndWorking;

        ModifyShaderOffset _SudokuBoard;

        private void OnEnable()
        {
            GetReferences();

            if (_EnabledAndWorking)
            {
                Debug.Log($"[SuspendGameState] Enabled and working. Checking for game save state.");
                var _CurrentSave = PsychSaveManager.p_Instance.GetCurrentSave();
                if(_CurrentSave._SaveStateInformation._IsValidSaveState)
                {
                    Debug.Log($"[SuspendGameState] Valid Save State! {_CurrentSave._SaveStateInformation}");
                    _HadValidStateOnStart = true;
                    SetBackGameState(_CurrentSave);
                }
                else
                {
                    Debug.Log($"[SuspendGameState] No valid game state. Proceed as normally.");
                    _HadValidStateOnStart = false;
                }
            }
            else
            {
                Debug.LogWarning($"[SuspendGameState] Save Manager Instance was null, Suspend state will not work.");
                _HadValidStateOnStart = false;
            }
        }

        void SetBackGameState(PsychSudokuSave save)
        {
            // TODO: Convert seconds to ticks.
            // Or store the time as ticks to begin with

            // At this point, everything is in Parameters Injest it just needs to be
            // setup

            _SudokuBoard._GameShouldRestoreState = true;
            _SudokuBoard.SetLevelInformation(SudokuParametersInjest.p_Instance.GetLevel(),
                SudokuParametersInjest.p_Instance.GetDifficulty());
        }

        public void Handle_BoardFinishedGenerating()
        {
            if (_EnabledAndWorking && _HadValidStateOnStart)
            {
                // Set valid save state to false and save the game
                if (PsychSaveManager.InstanceNull() == false)
                {
                    var currentSave = PsychSaveManager.p_Instance.GetCurrentSave();

                    currentSave._SaveStateInformation._IsValidSaveState = false;

                    PsychSudokuSave.WriteSaveJSON(currentSave);
                }
            }
        }

        void GetReferences()
        {
            _SudokuBoard = GetComponent<ModifyShaderOffset>();

            if (PsychSaveManager.InstanceNull() == false)
            {
                _EnabledAndWorking = true;
            }
        }

        public void TestSuspendState()
        {
            if (_EnabledAndWorking == false) return;

            Debug.Log($"[SuspendGameState] Set Save State");
            var _CurrentSave = PsychSaveManager.p_Instance.GetCurrentSave();

            PsychSudokuSave.SetSaveState(_CurrentSave,
                _SudokuBoard,
                ((PsychSaveManager)PsychSaveManager.p_Instance).GetLevelList,
                GameTimeManager.p_Instance);

            PsychSudokuSave.WriteSaveJSON(_CurrentSave);

            Debug.Log($"[SuspendGameState] Wrote save state to JSON!");
        }


        // TODO:
        private void OnApplicationFocus(bool focus)
        {

        }

        private void OnApplicationPause(bool pause)
        {
            Debug.Log($"[SuspendGameState] OnApplicationPause pause: {pause}");
            if(pause)
            {
                // Suspend game state
                TestSuspendState();
            }
            else
            {
                // TODO:
                Debug.Log($"[SuspendGameState] Todo Reload game state?");
            }
        }

        private void OnApplicationQuit()
        {
            TestSuspendState();
        }
    }
}
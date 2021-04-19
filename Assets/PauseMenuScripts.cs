using System;
using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.UI;
using UnityEngine;
using UnityEngine.UI;

namespace IgnoreSolutions.PsychSodoku
{
    /// <summary>
    /// A class to handle pausing the game and
    /// exiting back to the main menu in the even the player wishes.
    ///
    /// This class' entire purpose is *just* UI. It will receive
    /// the events from the PauseGameHandler.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PauseMenuScripts : IAnimatableCanvasGroup
    {
        [Header("Primary Component References")]
        [SerializeField] Text _PauseMenuInfo;
        [Header("Other Component References")]
        [SerializeField] ModifyShaderOffset _SudokuBoard;
        [SerializeField] MessageBoxCanvas _MessageBox;
        [SerializeField] SudokuParametersInjest _BoardParameters;

        private string _TemplateText = null;

        public override void Awake()
        {
            base.Awake();

            if (_PauseMenuInfo != null)
                _TemplateText = _PauseMenuInfo.text;

            _BoardParameters = FindObjectOfType<SudokuParametersInjest>();

            _ThisCanvasGroup.alpha = 0f;
            _ThisCanvasGroup.interactable = false;
            _ThisCanvasGroup.blocksRaycasts = false;
        }

        public void OnGamePaused(bool wasPaused, bool isCurrentlyPaused)
        {
            if (isCurrentlyPaused) UpdatePauseMenuDetails();
            else Debug.Log($"[PauseMenuScripts] Hiding pause menu.");

            SetCanvasGroupVisiblity(isCurrentlyPaused);
        }

        private void UpdatePauseMenuDetails()
        {
            if(_PauseMenuInfo != null
                && _TemplateText != null)
            {
                int _currentLevelIndex = -1;
                if (_BoardParameters != null)
                    _currentLevelIndex = _BoardParameters.GetLevelIndex();
                TimeSpan _currentPlayTime = GameTimeManager.p_Instance.PeekPlayTime();
                PlayDifficulty _currentDifficulty = _SudokuBoard.GetLevelInformation().Item1;

                _PauseMenuInfo.text =
                    string.Format(_TemplateText,
                        _currentLevelIndex == -1 ? "Unknown." : (_currentLevelIndex + 1).ToString(),
                        $"{_currentPlayTime.Minutes.ToString("00")}:{_currentPlayTime.Seconds.ToString("00")}",
                        _currentDifficulty.ToString()
                    );
            }
        }

        public void ShowConfirmationForReturnToMenu()
        {
            if(_MessageBox != null && _SudokuBoard != null)
            {
                _MessageBox.ShowMessageBox
                (
                    "Confirmation",
                    "Are you sure you want to return to the main menu?\n\nProgress will not be saved.",
                    (responseCode) =>
                    {
                        if(responseCode == 0)
                        {
                            _SudokuBoard.ReturnToMainMenu();
                        }
                    }
                );
            }
        }
    }
}
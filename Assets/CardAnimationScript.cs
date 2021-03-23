using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.PsychSodoku;
using UnityEngine;
using UnityEngine.UI;
using static IgnoreSolutions.PsychSodoku.OrientationCanvasSwap;
using static ModifyShaderOffset;

public class CardAnimationScript : MonoBehaviour
{
    protected enum LastSelectedDifficulty
    {
        NONE, Easy, Medium, Hard
    }

    [SerializeField] float _TransitionSpeed = 1.5f;
    [SerializeField] CanvasGroup _MyCanvasGroup;
    [SerializeField] CanvasGroup _PreviousMenu;
    [SerializeField] bool _AnimationInProgress = false;
    [SerializeField] List<Button> _EnableOnDifficultySelect;
    [SerializeField] LastSelectedDifficulty _LastSelectedDifficulty = LastSelectedDifficulty.NONE;

    [SerializeField] SwitchScene _SceneSwitcher;

    Animator _thisAnimator;
    

    public void PlayHardSelect() => PlayDifficultyAnimation(LastSelectedDifficulty.Hard, false);
    public void PlayMediumSelect() => PlayDifficultyAnimation(LastSelectedDifficulty.Medium, false);
    public void PlayEasySelect() => PlayDifficultyAnimation(LastSelectedDifficulty.Easy, false);

    // Start is called before the first frame update
    void Start()
    {
        _thisAnimator = GetComponent<Animator>();
    }

    public void Next_SetupSudokuScene()
    {
        var saveMgr = ((PsychSaveManager)PsychSaveManager.p_Instance);
        saveMgr.SetParametersInjestToNextLevel(saveMgr.GetCurrentSave(), GetLastSelectedDifficulty());

        Debug.Log($"TODO: Switch scene.");
        _SceneSwitcher?.ChangeScene(1);
    }

    public PlayDifficulty GetLastSelectedDifficulty()
    {
        switch(_LastSelectedDifficulty)
        {
            case LastSelectedDifficulty.NONE:
                throw new System.Exception("Attempt made to move to next screen without selecting difficulty.");
            default: return (PlayDifficulty)(((int)_LastSelectedDifficulty) - 1);
            //case LastSelectedDifficulty.Easy: return PlayDifficulty.EASY;
        }
    }

    public void ResetAnimationInProgress()
    {
        _AnimationInProgress = false;

        if(_LastSelectedDifficulty != LastSelectedDifficulty.NONE)
        {
            SetDifficultySelectedButtonStates(true);
        }
    }

    private void PlayDifficultyAnimation(LastSelectedDifficulty difficulty, bool backAnim = false)
    {
        if (_AnimationInProgress) return;
        if (_LastSelectedDifficulty == difficulty && backAnim == false)
        {
            Next_SetupSudokuScene();
            return;
        }

        if(backAnim) SetDifficultySelectedButtonStates(false);
        _AnimationInProgress = true;

        if(StaticConstants._CurrentScreenOrientation == Orientation.Landscape)
        {
            Debug.Log($"Tr{difficulty.ToString()}" + (backAnim ? "Back" : ""));

            switch(difficulty)
            {
                case LastSelectedDifficulty.Easy:
                    _thisAnimator.ResetTrigger("TrEasy" + (backAnim ? "Back" : ""));
                    _thisAnimator.SetTrigger("TrEasy" + (backAnim ? "Back" : ""));
                    break;
                case LastSelectedDifficulty.Medium:
                    _thisAnimator.ResetTrigger("TrMedium" + (backAnim ? "Back" : ""));
                    _thisAnimator.SetTrigger("TrMedium" + (backAnim ? "Back" : ""));
                    break;
                case LastSelectedDifficulty.Hard:
                    _thisAnimator.ResetTrigger("TrHard" + (backAnim ? "Back" : ""));
                    _thisAnimator.SetTrigger("TrHard" + (backAnim ? "Back" : ""));
                    break;
            }
        }
        else
        {
            switch (difficulty)
            {
                case LastSelectedDifficulty.Easy:
                    _thisAnimator.ResetTrigger("TrEasyPortrait" + (backAnim ? "Back" : ""));
                    _thisAnimator.SetTrigger("TrEasyPortrait" + (backAnim ? "Back" : ""));
                    break;
                case LastSelectedDifficulty.Medium:
                    _thisAnimator.ResetTrigger("TrMediumPortrait" + (backAnim ? "Back" : ""));
                    _thisAnimator.SetTrigger("TrMediumPortrait" + (backAnim ? "Back" : ""));
                    break;
                case LastSelectedDifficulty.Hard:
                    _thisAnimator.ResetTrigger("TrHardPortrait" + (backAnim ? "Back" : ""));
                    _thisAnimator.SetTrigger("TrHardPortrait" + (backAnim ? "Back" : ""));
                    break;
            }
        }

        _LastSelectedDifficulty = difficulty;
    }

    public void PlayBack()
    {
        if (_AnimationInProgress) return;
        switch(_LastSelectedDifficulty)
        {
            case LastSelectedDifficulty.NONE:
                // TODO: Handle back
                break;
            default:
                PlayDifficultyAnimation(_LastSelectedDifficulty, true);
                SetDifficultySelectedButtonStates(false);
                _LastSelectedDifficulty = LastSelectedDifficulty.NONE;
                break;
        }
    }

    public void OnResolutionAspectChanged(Orientation orientation, Vector2 resolution)
    {
        _thisAnimator.speed = 5f;
        Debug.Log($"[CardAnimationScript] Resolution has changed.");
        switch(_LastSelectedDifficulty)
        {
            case LastSelectedDifficulty.NONE:
                ResetAnimationInProgress();
                break;
            case LastSelectedDifficulty.Easy:
                if(orientation == Orientation.Landscape)
                {
                    _thisAnimator.ResetTrigger("TrEasyPortrait");
                    _thisAnimator.ResetTrigger("TrEasy");
                    _thisAnimator.SetTrigger("TrEasy");
                    
                }
                else
                {
                    _thisAnimator.ResetTrigger("TrEasy");
                    _thisAnimator.ResetTrigger("TrEasyPortrait");
                    _thisAnimator.SetTrigger("TrEasyPortrait");

                }
                break;
        }

        _thisAnimator.speed = 1f;
    }

    void SetDifficultySelectedButtonStates(bool enabled)
    {
        foreach(Button b in _EnableOnDifficultySelect)
        {
            b.gameObject.SetActive(enabled);
        }
    }
}

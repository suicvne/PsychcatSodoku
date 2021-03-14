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

    [SerializeField] bool _AnimationInProgress = false;
    [SerializeField] List<Button> _EnableOnDifficultySelect;

    Animator _thisAnimator;
    LastSelectedDifficulty _LastSelectedDifficulty = LastSelectedDifficulty.NONE;

    // Start is called before the first frame update
    void Start()
    {
        _thisAnimator = GetComponent<Animator>();
    }

    public void ResetAnimationInProgress()
    {
        _AnimationInProgress = false;

        if(_LastSelectedDifficulty != LastSelectedDifficulty.NONE)
        {
            SetDifficultySelectedButtonStates(true);
        }
    }

    public void PlayHardSelect() => PlayDifficultyAnimation(LastSelectedDifficulty.Hard, false);

    public void PlayMediumSelect() => PlayDifficultyAnimation(LastSelectedDifficulty.Medium, false);

    public void PlayEasySelect()
    {
        PlayDifficultyAnimation(LastSelectedDifficulty.Easy, false);
        //if (_AnimationInProgress) return;
        //if (_LastSelectedDifficulty == LastSelectedDifficulty.Easy) return;

        //_AnimationInProgress = true;

        //if (StaticConstants._CurrentScreenOrientation == OrientationCanvasSwap.Orientation.Landscape)
        //{
        //    _thisAnimator.ResetTrigger("TrEasy");
        //    _thisAnimator.SetTrigger("TrEasy");
        //    //_thisAnimator.SetBool("Easy", true);
        //}
        //else
        //{
        //    _thisAnimator.ResetTrigger("TrEasyPortrait");
        //    _thisAnimator.SetTrigger("TrEasyPortrait");
        //    //_thisAnimator.SetBool("Easy_Portrait", true);
        //}

        //_LastSelectedDifficulty = LastSelectedDifficulty.Easy;
    }

    private void PlayDifficultyAnimation(LastSelectedDifficulty difficulty, bool backAnim = false)
    {
        if (_AnimationInProgress) return;
        if (_LastSelectedDifficulty == difficulty) return;

        if(backAnim) SetDifficultySelectedButtonStates(false);
        _AnimationInProgress = true;

        if(StaticConstants._CurrentScreenOrientation == Orientation.Landscape)
        {
            Debug.Log($"Tr{_LastSelectedDifficulty.ToString()}" + (backAnim ? "Back" : ""));

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
                // TODO: Go back to main menu
                Debug.Log($"TODO: Go back to main menu.");
                break;
            default:
                PlayDifficultyAnimation(_LastSelectedDifficulty, true);
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

    //public void PlayEasyBack()
    //{
    //    _AnimationInProgress = true;
    //    SetDifficultySelectedButtonStates(false);

    //    if(StaticConstants._CurrentScreenOrientation == Orientation.Portrait)
    //    {
    //        _thisAnimator.ResetTrigger("TrEasyPortraitBack");
    //        _thisAnimator.SetTrigger("TrEasyPortraitBack");
    //    }
    //    else
    //    {
    //        _thisAnimator.ResetTrigger("TrEasyBack");
    //        _thisAnimator.SetTrigger("TrEasyBack");
    //    }

    //    //if(_thisAnimator.GetBool("Easy_Portrait")) _thisAnimator.SetBool("Easy_Portrait", false);
    //    //else _thisAnimator.SetBool("Easy", false);
    //}

    void SetDifficultySelectedButtonStates(bool enabled)
    {
        foreach(Button b in _EnableOnDifficultySelect)
        {
            b.gameObject.SetActive(enabled);
        }
    }

}

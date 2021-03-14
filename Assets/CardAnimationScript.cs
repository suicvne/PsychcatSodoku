using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.PsychSodoku;
using UnityEngine;
using UnityEngine.UI;
using static IgnoreSolutions.PsychSodoku.OrientationCanvasSwap;

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

    public void PlayEasySelect()
    {
        if (_AnimationInProgress) return;
        if (_LastSelectedDifficulty == LastSelectedDifficulty.Easy) return;

        _AnimationInProgress = true;

        if (StaticConstants._CurrentScreenOrientation == OrientationCanvasSwap.Orientation.Landscape)
        {
            _thisAnimator.ResetTrigger("TrEasy");
            _thisAnimator.SetTrigger("TrEasy");
            //_thisAnimator.SetBool("Easy", true);
        }
        else
        {
            _thisAnimator.ResetTrigger("TrEasyPortrait");
            _thisAnimator.SetTrigger("TrEasyPortrait");
            //_thisAnimator.SetBool("Easy_Portrait", true);
        }

        _LastSelectedDifficulty = LastSelectedDifficulty.Easy;
    }

    public void PlayBack()
    {
        if (_AnimationInProgress) return;
        switch(_LastSelectedDifficulty)
        {
            case LastSelectedDifficulty.NONE:
                // TODO: Go back to main menu
                break;
            case LastSelectedDifficulty.Easy:
                PlayEasyBack();
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

    public void PlayEasyBack()
    {
        _AnimationInProgress = true;
        SetDifficultySelectedButtonStates(false);

        if(StaticConstants._CurrentScreenOrientation == Orientation.Portrait)
        {
            _thisAnimator.ResetTrigger("TrEasyPortraitBack");
            _thisAnimator.SetTrigger("TrEasyPortraitBack");
        }
        else
        {
            _thisAnimator.ResetTrigger("TrEasyBack");
            _thisAnimator.SetTrigger("TrEasyBack");
        }

        //if(_thisAnimator.GetBool("Easy_Portrait")) _thisAnimator.SetBool("Easy_Portrait", false);
        //else _thisAnimator.SetBool("Easy", false);
    }

    void SetDifficultySelectedButtonStates(bool enabled)
    {
        foreach(Button b in _EnableOnDifficultySelect)
        {
            b.gameObject.SetActive(enabled);
        }
    }

}

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


    Animator _thisAnimator;
    [SerializeField] LastSelectedDifficulty _LastSelectedDifficulty = LastSelectedDifficulty.NONE;

    // Start is called before the first frame update
    void Start()
    {
        _thisAnimator = GetComponent<Animator>();
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
        if (_LastSelectedDifficulty == difficulty && backAnim == false) return;

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

    public void FadeInCardSelectionScreen()
    {
        if (_AnimationInProgress) return;
        _AnimationInProgress = true;
        StartCoroutine(TransitionToMe());
    }

    public void PlayBack()
    {
        if (_AnimationInProgress) return;
        switch(_LastSelectedDifficulty)
        {
            case LastSelectedDifficulty.NONE:
                _AnimationInProgress = true;
                StartCoroutine(TransitionToPreviousMenu());
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

    IEnumerator TransitionToMe()
    {
        if (_MyCanvasGroup == null || _PreviousMenu == null) yield break;

        // Fade "previous menu" (title screen)
        for (float f = 0.0f; f <= 1.0f; f += _TransitionSpeed * Time.deltaTime)
        {
            float i_f = 1.0f - f;
            _PreviousMenu.alpha = i_f;
            yield return null;
        }

        _PreviousMenu.alpha = 0.0f;
        _PreviousMenu.blocksRaycasts = false;
        _PreviousMenu.interactable = false;

        // Fade in my menu
        for (float f = 0.0f; f <= 1.0f; f += _TransitionSpeed * Time.deltaTime)
        {
            _MyCanvasGroup.alpha = f;
            yield return null;
        }

        _MyCanvasGroup.alpha = 1.0f;
        _MyCanvasGroup.blocksRaycasts = true;
        _MyCanvasGroup.interactable = true;
        yield return null;
        _AnimationInProgress = false;
    }

    IEnumerator TransitionToPreviousMenu()
    {
        if (_MyCanvasGroup == null || _PreviousMenu == null) yield break;

        // Fade my canvas group
        for(float f = 0.0f; f <= 1.0f; f += _TransitionSpeed * Time.deltaTime)
        {
            float i_f = 1.0f - f;
            _MyCanvasGroup.alpha = i_f;
            yield return null;
        }

        _MyCanvasGroup.alpha = 0.0f;
        _MyCanvasGroup.blocksRaycasts = false;
        _MyCanvasGroup.interactable = false;

        // Fade in previous menu
        for(float f = 0.0f; f <= 1.0f; f += _TransitionSpeed * Time.deltaTime)
        {
            _PreviousMenu.alpha = f;
            yield return null;
        }

        _PreviousMenu.alpha = 1.0f;
        _PreviousMenu.blocksRaycasts = true;
        _PreviousMenu.interactable = true;
        yield return null;
        _AnimationInProgress = false;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.PsychSodoku;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    [SerializeField] float _TransitionSpeed = 1.0f;
    [SerializeField] public bool _AnimationInProgress = false;

    public delegate void OnTransitionComplete(bool wasPrevious = false);
    public event OnTransitionComplete OnFadeCompleted;

    public void ShowCanvasGroup(CanvasGroup cg) => SetCanvasGroupState(cg, true);
    public void HideCanvasGroup(CanvasGroup cg) => SetCanvasGroupState(cg, false);

    public void SetCanvasGroupState(CanvasGroup cg, bool enabled)
    {
        cg.alpha = enabled ? 1.0f : 0.0f;
        cg.interactable = enabled;
        cg.blocksRaycasts = enabled;
    }

    public void StartFadeTransition(CanvasGroup fadeIn, CanvasGroup fadeOut)
    {
        if (_AnimationInProgress) return;
        StartCoroutine(TransitionToMe(fadeIn, fadeOut));
    }

    public void StartFadeOutTransition(CanvasGroup fadeIn, CanvasGroup fadeOut)
    {
        if (_AnimationInProgress) return;
        StartCoroutine(TransitionToPreviousMenu(fadeIn, fadeOut));
    }

    IEnumerator TransitionToMe(CanvasGroup _MyCanvasGroup, CanvasGroup _FadeOutMenu)
    {
        if (_MyCanvasGroup == null || _FadeOutMenu == null) yield break;

        // Fade "previous menu" (title screen)
        for (float f = 0.0f; f <= 1.0f; f += _TransitionSpeed * Time.deltaTime)
        {
            float i_f = 1.0f - f;
            _FadeOutMenu.alpha = i_f;
            yield return null;
        }

        _FadeOutMenu.alpha = 0.0f;
        _FadeOutMenu.blocksRaycasts = false;
        _FadeOutMenu.interactable = false;

        CanvasGroupStateChangeEvents fadeOutEvents =
            _FadeOutMenu.GetComponent<CanvasGroupStateChangeEvents>();
        if(fadeOutEvents != null)
            fadeOutEvents.ExecuteOnInvisible();

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

        CanvasGroupStateChangeEvents myGroupEvents =
            _MyCanvasGroup.GetComponent<CanvasGroupStateChangeEvents>();
        if (myGroupEvents != null)
            myGroupEvents.ExecuteOnVisible();

        _AnimationInProgress = false;

        OnFadeCompleted?.Invoke();
    }

    [Obsolete]
    IEnumerator TransitionToPreviousMenu(CanvasGroup _MyCanvasGroup, CanvasGroup _FadeOutMenu)
    {
        if (_MyCanvasGroup == null || _FadeOutMenu == null) yield break;

        // Fade my canvas group
        for (float f = 0.0f; f <= 1.0f; f += _TransitionSpeed * Time.deltaTime)
        {
            float i_f = 1.0f - f;
            _MyCanvasGroup.alpha = i_f;
            yield return null;
        }

        _MyCanvasGroup.alpha = 0.0f;
        _MyCanvasGroup.blocksRaycasts = false;
        _MyCanvasGroup.interactable = false;

        // Fade in previous menu
        for (float f = 0.0f; f <= 1.0f; f += _TransitionSpeed * Time.deltaTime)
        {
            _FadeOutMenu.alpha = f;
            yield return null;
        }

        _FadeOutMenu.alpha = 1.0f;
        _FadeOutMenu.blocksRaycasts = true;
        _FadeOutMenu.interactable = true;
        yield return null;
        _AnimationInProgress = false;

        OnFadeCompleted?.Invoke(true);
    }

    public void ChangeScene(int sceneIndex)
    {
        try
        {
            SceneManager.LoadScene(sceneIndex);
        }
        catch(Exception ex)
        {
            Debug.Log($"[SwitchScene] Failed to load scene with index {sceneIndex}. Exception: {ex.Message}");
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public void ShowCanvasGroup(CanvasGroup cg) => SetCanvasGroupState(cg, true);
    public void HideCanvasGroup(CanvasGroup cg) => SetCanvasGroupState(cg, false);

    public void SetCanvasGroupState(CanvasGroup cg, bool enabled)
    {
        cg.alpha = enabled ? 1.0f : 0.0f;
        cg.interactable = enabled;
        cg.blocksRaycasts = enabled;
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

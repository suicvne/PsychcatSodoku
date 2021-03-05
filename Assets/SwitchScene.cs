using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
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

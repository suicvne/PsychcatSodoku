﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScaleCameraToObject : MonoBehaviour
{
    public void Awake()
    {
        LastResolution = ResolutionToVector2();
    }

    private Vector2 LastResolution;
    public void LateUpdate()
    {
        Vector2 curRes = ResolutionToVector2();
        if(curRes != LastResolution)
        {
            Debug.Log($"Resolution changed ({LastResolution} -> {curRes})");
            LastResolution = curRes;
            ResolutionChanged();
        }
    }

    public Vector2 ResolutionToVector2()
    {
        return new Vector2(Screen.width, Screen.height);
    }

    public void Start()
    {
        ResolutionChanged();
    }

    public void ResolutionChanged()
    {
        var camera = Camera.main;
        if (camera != null)
        {
            if (Screen.width < Screen.height) // w < h. Portrait aspect.
            {
                Debug.Log($"13");
                camera.orthographicSize = 13;
            }
            else if (Screen.width > Screen.height) // w > h. Landscape aspect.
            {
                Debug.Log($"10");
                camera.orthographicSize = 8;
            }
            else // w == h. Dunno yet.
            {
                Debug.Log($"Idk");
                camera.orthographicSize = 11;
            }
        }
    }
}
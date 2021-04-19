﻿#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using UnityEditor;
using UnityEngine;


namespace TheraBytes.BetterUi
{
    [InitializeOnLoad]
    public static class ScriptableObjectInstantiator
    {
        static ScriptableObjectInstantiator()
        {
            Initialize();
        }

        static void Initialize()
        {
            // Ensure instances
            ResolutionMonitor.EnsureInstance();
            Materials.EnsureInstance();
        }
        

        [MenuItem("Tools/Better UI/Settings/Select Resolution Monitor", false, 0)]
        static void SelectResolutionMonitor()
        {
            Selection.objects = new UnityEngine.Object[] { ResolutionMonitor.Instance };
        }

        [MenuItem("Tools/Better UI/Settings/Select Material Definitions", false, 1)]
        static void SelectMaterials()
        {
            Selection.objects = new UnityEngine.Object[] { Materials.Instance };
        }
        
        [MenuItem("Tools/Better UI/Settings/Ensure Singleton Resources", false, 30)]
        static void ManualInitialize()
        {
            if (ResolutionMonitor.HasInstance && Materials.HasInstance)
            {
                Debug.Log("Instances already present. Please Check \"Assets/Thera Bytes/Resources\"");
                return;
            }
            
            Initialize();
            
            Debug.Log("Instances have been created. Please Check  \"Assets/Thera Bytes/Resources\"");
        }
    }
}
#endif

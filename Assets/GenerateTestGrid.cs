using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.Sodoku;
using UnityEngine;
using UnityEngine.UI;

public class GenerateTestGrid : MonoBehaviour
{
    [SerializeField] Material mat;
    [SerializeField] bool ForceRegen = false;

    public void OnValidate()
    {
        if(ForceRegen)
        {
            ForceRegen = false;
        }
    }

    void ClearChildren()
    {
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                DestroyImmediate(transform.GetChild(i));
            }
        }
    }

    void OccupyChildren()
    {
        MaterialPropertyBlock mbp = new MaterialPropertyBlock();
        for(int i = 0; i < LevelData.TotalGridSize; i++)
        {
            GameObject newGridSpot = new GameObject();
            newGridSpot.transform.parent = transform;
            newGridSpot.name = $"Index Test {i}";
            Image img = newGridSpot.AddComponent<Image>();
            img.material = mat;

            
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SodukoGriidSpot : MonoBehaviour
{
    public Vector2Int GridSpot;
    internal ModifyShaderOffset parent;

    public int DebugValue = -1;

    public int UserValue = -1;
    public int NoteValue = -1;

    public void OnMouseDown()
    {
        Debug.Log($"{GridSpot} You pressed me!");
    }

    public int GetValue()
    {
        if(parent != null)
        {
            return parent.GetValueAt(GridSpot);
        }

        return -2;
    }
}

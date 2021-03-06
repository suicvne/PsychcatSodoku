using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SodukoGriidSpot : MonoBehaviour
{
    public Vector2Int GridSpot;
    internal ModifyShaderOffset parent;

    public int DebugValue = -1;
    public int _SquareSolution = -1;
    public int _SquareFilledValue = -1;

    public int _LevelIndex = -1;
    public int _SquareGroupNo = -1;

    public UnityEvent<SodukoGriidSpot> _OnGridSpotTapped;

    public void OnMouseDown()
    {
        Debug.Log($"{GridSpot} You pressed me!");

        _OnGridSpotTapped?.Invoke(this);
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

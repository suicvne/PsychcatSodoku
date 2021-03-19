using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    List<int> PossibleNumbers = new List<int>();

    public UnityEvent<SodukoGriidSpot> _OnGridSpotTapped;

    [SerializeField] internal TMP_Text _PossibleNumbersText;
    [SerializeField] internal SpriteRenderer _SolutionNumberSprite;

    private void OnEnable()
    {
        UpdatePossibleNumbers();
    }

    public void SetTMP(TMP_Text text)
    {
        _PossibleNumbersText = text;
    }

    public void ClearPossibleNumbers()
    {
        PossibleNumbers.Clear();
    }

    public void AddPossibleNumber(int number)
    {
        if(PossibleNumbers.Contains(number))
        {
            PossibleNumbers.Remove(number);
        }
        else
        {
            PossibleNumbers.Add(number);
        }
    }

    public void UpdatePossibleNumbers()
    {
        if(_PossibleNumbersText == null)
        {
            Debug.LogWarning($"Tried to update when _PossibleNumbersText is equal to null.", gameObject);
            return;
        }

        string format = "{0} {1} {2}\n{3} {4} {5}\n{6} {7} {8}";
        string[] formatNumbers = new string[] { " ", " ", " ", /**/ " ", " ", " ", /**/ " ", " ", " " };

        for(int i = 0; i < formatNumbers.Length; i++)
        {
            if(PossibleNumbers.Contains(i + 1))
            {
                formatNumbers[i] = $"{i + 1}";
            }
        }

        _PossibleNumbersText.text = string.Format(format, formatNumbers);
    }

    public void OnMouseDown()
    {
        if (parent._DirectInputToNumberSelect == false)
        {
            Debug.Log($"{GridSpot} You pressed me!");

            _OnGridSpotTapped?.Invoke(this);
        }
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

using System;
using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.PsychSodoku;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ManageFirstSelected : MonoBehaviour
{
    [SerializeField] GameObject FirstSelected;

    [SerializeField] bool SetOnMobile = false;
    [SerializeField] bool SetOnOther = false;

    [SerializeField] bool isMobile;
    [SerializeField] bool isOther;

    private void OnEnable()
    {
        ForceActivateWithGamepad forceActivate = GetComponent<ForceActivateWithGamepad>();
        if (forceActivate != null)
        {
            GetComponent<ForceActivateWithGamepad>().OnConnectedJoystickCountChanged +=
                Handle_OnConnectedJoystickCountChanged;
        }
    }

    void Handle_OnConnectedJoystickCountChanged(int oldCount, int newCount)
    {
        if (oldCount == 0 && newCount > 0)
        {
            DoInitialSet(newCount);
        }
    }

    private void Start()
    {
        DoInitialSet();
    }

    public void DoInitialSet(int joystickCountOverride = -1)
    {
        isMobile = Application.isMobilePlatform;
        isOther = !isMobile;

        Debug.Log($"[ManageFirstSelected] isMobile: {Application.isMobilePlatform} isOther: {isOther}");
        Debug.Log($"[ManageFirstSelected] Joysticks Connected: {(joystickCountOverride == -1 ? Input.GetJoystickNames().Length : joystickCountOverride)}");

        if (SetOnMobile && isMobile || SetOnOther && isOther || (Input.GetJoystickNames().Length > 0 || joystickCountOverride > 0))
        {
            SetSelectedGameObject(FirstSelected);
            //StartCoroutine(Delayed());
        }
    }

    public void SetSelectedGameObject(GameObject go)
    {
        Debug.Log($"[ManageFirstSelected] Setting first selected to {go}", go);
        EventSystem.current.SetSelectedGameObject(null);


        EventSystem.current.SetSelectedGameObject(go, new BaseEventData(EventSystem.current));

        Button b = go.GetComponent<Button>();
        if (b != null)
        {
            b.Select();
        }
    }

    public void DelayedSetSelected(GameObject go)
    {
        StartCoroutine(Delayed(go));
    }

    IEnumerator Delayed(GameObject go)
    {
        yield return null;
        
        Debug.Log($"[ManageFirstSelected] Setting first selected to {go}", go);
        EventSystem.current.SetSelectedGameObject(null);

        yield return null;

        Debug.Log($"GO Active: {go.activeInHierarchy} {go.activeSelf}");

        EventSystem.current.SetSelectedGameObject(go, new BaseEventData(EventSystem.current));

        yield return null;

        Button b = go.GetComponent<Button>();
        if (b != null)
        {
            b.Select();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
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

    private void Start()
    {
        isMobile = Application.isMobilePlatform;
        isOther = !isMobile;

        if (SetOnMobile && isMobile || SetOnOther && isOther)
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

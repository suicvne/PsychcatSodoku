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
        Debug.Log($"[ManageFirstSelected] Setting first selected to {FirstSelected}", FirstSelected);
        EventSystem.current.SetSelectedGameObject(null);


        EventSystem.current.SetSelectedGameObject(go, new BaseEventData(EventSystem.current));

        Button b = FirstSelected.GetComponent<Button>();
        if (b != null)
        {
            b.Select();
        }
    }

    IEnumerator Delayed()
    {
        yield return null;
        
    }
}

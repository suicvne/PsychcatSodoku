using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IgnoreSolutions.PsychSodoku
{
    [System.Serializable]
    public struct EnumToActionTranslator
    {
        public ActionTranslator.MenuActions ActionType;
        public UnityEvent Action;
    }
    /// <summary>
    /// SIMPLE action translator. Buttons on the UI tell this object
    /// the action they want to execute via enum. 
    /// </summary>
    public class ActionTranslator : MonoBehaviour
    {
        public enum MenuActions
        {
            GoToPlay,
            GoToOptions,
            GoToHelp,
            GoToAbout,
            GoBack,
            DifficultySelected,
            GameWon
        }

        [SerializeField] List<EnumToActionTranslator> Actions = new List<EnumToActionTranslator>();

        public void ExecuteAction(MenuActions actionType)
        {
            bool didExecute = false;
            foreach(var action in Actions)
            {
                if (action.ActionType == actionType) action.Action?.Invoke();
            }

            if (!didExecute) Debug.Log($"Warning: Unable to find action of type {actionType}");
        }
    }
}
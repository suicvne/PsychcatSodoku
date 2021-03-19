using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IgnoreSolutions.PsychSodoku
{
    [System.Serializable]
    public struct EnumToActionTranslator
    {
        public MenuActions ActionType;
        public UnityEvent Action;
        public UnityEvent UndoAction;
    }

    public enum MenuActions
    {
        GoToPlay,
        GoToOptions,
        GoToHelp,
        GoToAbout,
        GoBack,
        GoToDifficultySelection,
        GoToLevelSelection,
        GameWon
    }
    /// <summary>
    /// SIMPLE action translator. Buttons on the UI tell this object
    /// the action they want to execute via enum. 
    /// </summary>
    public class ActionTranslator : MonoBehaviour
    {
        [SerializeField] List<EnumToActionTranslator> Actions = new List<EnumToActionTranslator>();



        public void AExecuteAction(int actionType)
        {
            ExecuteAction((MenuActions)actionType, false);
        }

        public void AExecuteUndoAction(MenuActions actionType)
        {
            ExecuteAction(actionType, true);
        }

        public void ExecuteAction(MenuActions actionType, bool undo)
        {
            bool didExecute = false;
            foreach(var action in Actions)
            {
                if (action.ActionType == actionType)
                {
                    if (undo) action.UndoAction?.Invoke();
                    else action.Action?.Invoke();
                }
            }

            if (!didExecute) Debug.Log($"Warning: Unable to find action of type {actionType}");
        }
    }
}
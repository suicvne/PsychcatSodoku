using System;
using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.UI;
using UnityEngine;
using UnityEngine.UI;

namespace IgnoreSolutions.PsychSodoku
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MessageBoxCanvas : IAnimatableCanvasGroup
    {
        [SerializeField] string _HeaderText;
        [SerializeField] string _MessageBodyText;

        [Header("UI Component References")]
        [SerializeField] Text _HeaderTextComponent;
        [SerializeField] Text _MessageBodyTextComponent;

        private Action<int> _StoredResponseAction;

        public void ConfirmAction()
        {
            _StoredResponseAction?.Invoke(0);
            SetCanvasGroupVisiblity(false);
        }

        public void CancelAction()
        {
            _StoredResponseAction?.Invoke(-1);
            SetCanvasGroupVisiblity(false);
        }

        public void ShowMessageBox(string header, string message, Action<int> responseAction)
        {
            _HeaderText = header;
            _MessageBodyText = message;

            _HeaderTextComponent.text = header;
            _MessageBodyTextComponent.text = message;

            _StoredResponseAction = responseAction;

            SetCanvasGroupVisiblity(true);
        }

        public void ShowMessageBox(string header, string message)
        {
            ShowMessageBox(header, message, (response) => { });
        }
    }
}
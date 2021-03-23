using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.DuelistsOfTheRoses;
using IgnoreSolutions.GenInput;
using IgnoreSolutions.GenInput.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IgnoreSolutions.UI
{
    [System.Serializable]
    public struct ISMenuItemStruct
    {
        [HideInInspector]
        public string Name;
        public RectTransform Item;
        public IAcceptsGenericInput input;

        public bool PassInputWhenSelected;
        public bool InjectParameters;
        public UnityEvent OnSelect;
        public UnityEvent OnHighlight;


    }

    [System.Flags]
    public enum MenuStyle
    {
        Vertical = 1 << 0,
        Horizontal = 1 << 1
    }

    public class ISMenuItems : MonoBehaviour, IAcceptsGenericInput
    {
        [SerializeField] bool _EnableInput = true;
        [SerializeField] bool _EnableInputOnPlayerInput = false;
        [SerializeField] UnityEvent _OnInputEnabled;
        [SerializeField] bool SpecificPlayerInputOnly = false;
        [SerializeField] int SpecificPlayerInput = 0;

        [SerializeField]
        MenuStyle _MenuStyle;

        [SerializeField]
        RectTransform _SelectionBorder;

        [SerializeField]
        ISMenuItemStruct[] _MenuItems;

        [SerializeField] int _currentIndex = 0;

        [SerializeField] bool _OverrideOffset = false;
        [SerializeField] Vector2 TestOffset = new Vector2(48f, 128f);

        private Vector2 _TemporarySizeDelta;

        public bool _InterceptCancel { get => false; set { } }

        public bool IsInputEnabled() => _EnableInput;
        public bool SetInputEnabled(bool inputEnabled) => _EnableInput = inputEnabled;

        void Start() => Validate();
        void OnValidate()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                Validate();
            }
        }

        void Validate()
        {
                if (_MenuItems != null && _MenuItems.Length > 0)
                {
                    for (int i = 0; i < _MenuItems.Length; i++)
                    {
                        if (_MenuItems[i].Item != null)
                        {
                            _MenuItems[i].Name = _MenuItems[i].Item.name;
                            if (_MenuItems[i].PassInputWhenSelected) _MenuItems[i].input = _MenuItems[i].Item.gameObject.GetComponent<IAcceptsGenericInput>();
                        }
                    }
                }
        }

        private void InvokeMenuItem(ref ISMenuItemStruct menuItem)
        {
            Debug.Log($"Invoking {menuItem.Name}");
            if (menuItem.InjectParameters)
            {
                // TODO: Inject parameters
                _MenuItems[_currentIndex].OnSelect?.Invoke();
            }
            else _MenuItems[_currentIndex].OnSelect?.Invoke();
        }

        // TODO: Will I start adding a CardOwnership (Player) parameter to this function?
        // Or is there a better way to do 2 player input here? Hmmmmmmmmmmmmmmmm
        public void TranslateInput(int type, int playerIndex)
        {
            Debug.Log($"[ISMenuItems] TODO Reimplement TranslateInput for type: {type}, {playerIndex}");
            //if(_EnableInputOnPlayerInput && _EnableInput == false && playerIndex == (int)SpecificPlayerInput)
            //{
            //    _EnableInput = true;
            //    _OnInputEnabled?.Invoke();
            //    Debug.Log($"[Player {playerIndex + 1}] Input has been enabled.");
            //    return;
            //}

            //if (!_EnableInput) return;
            //Debug.Log($"[PlayerIndex: {playerIndex}] Player {playerIndex + 1} input {type}", gameObject);
            //if (SpecificPlayerInputOnly && playerIndex != (int)SpecificPlayerInput) return;
            //if (_MenuItems == null || _MenuItems.Length <= 0) return;
            

            

            //int newIndex;
            //if (_MenuStyle.HasFlag(MenuStyle.Vertical))
            //{
            //    switch (type)
            //    {
            //        case ControlType.Up:
            //            newIndex = Mathf.Clamp(_currentIndex - 1, 0, (_MenuItems.Length - 1));
            //            if(newIndex != _currentIndex)
            //            {
            //                _currentIndex = newIndex;
            //                bool b = SoundManager.p_Instance.PlaySoundEffect(Vector3.zero, "GridMove");
            //            }
            //            break;
            //        case ControlType.Down:
            //            newIndex = Mathf.Clamp(_currentIndex + 1, 0, (_MenuItems.Length - 1));
            //            if (newIndex != _currentIndex)
            //            {
            //                _currentIndex = newIndex;
            //                SoundManager.p_Instance.PlaySoundEffect(Vector3.zero, "GridMove");
            //            }
            //            break;
            //        case ControlType.Select:
            //            if (_MenuItems[_currentIndex].OnSelect.GetPersistentEventCount() > 0)
            //            {
            //                SoundManager.p_Instance.PlaySoundEffect("SelectionConfirm");
            //                InvokeMenuItem(ref _MenuItems[_currentIndex]);

            //            }
            //            else SoundManager.p_Instance.PlaySoundEffect("SelectionDeny");
            //            break;
            //    }
            //}

            //if (_MenuStyle.HasFlag(MenuStyle.Horizontal))
            //{
            //    switch (type)
            //    {
            //        case ControlType.Left:
            //            newIndex = Mathf.Clamp(_currentIndex - 1, 0, (_MenuItems.Length - 1));
            //            if (newIndex != _currentIndex)
            //            {
            //                _currentIndex = newIndex;
            //                SoundManager.p_Instance.PlaySoundEffect("GridMove");
            //            }
            //            break;
            //        case ControlType.Right:
            //            newIndex = Mathf.Clamp(_currentIndex + 1, 0, (_MenuItems.Length - 1));
            //            if (newIndex != _currentIndex)
            //            {
            //                _currentIndex = newIndex;
            //                SoundManager.p_Instance.PlaySoundEffect("GridMove");
            //            }
            //            break;
            //        case ControlType.Select:
            //            if (_MenuItems[_currentIndex].OnSelect.GetPersistentEventCount() > 0)
            //            {
            //                SoundManager.p_Instance.PlaySoundEffect("SelectionConfirm");
            //                InvokeMenuItem(ref _MenuItems[_currentIndex]);

            //            }
            //            else SoundManager.p_Instance.PlaySoundEffect("SelectionDeny");
            //            break;
            //    }
            //}

            UpdateSelectionBorder();
            //_MenuItems[_currentIndex].OnHighlight?.Invoke();
            if (_MenuItems[_currentIndex].PassInputWhenSelected &&
                _MenuItems[_currentIndex].input != null)
                _MenuItems[_currentIndex].input.TranslateInput(type, playerIndex);
        }


        public void UpdateSelectionBorder()
        {
            if (_SelectionBorder != null)
            {
                Vector2 offset = TestOffset;
                Vector2 offsetTmp = TestOffset;

                _SelectionBorder.transform.position = _MenuItems[_currentIndex].Item.transform.position;

                var itemRect = _MenuItems[_currentIndex].Item.sizeDelta;
                var item = _MenuItems[_currentIndex].Item;
                TMP_Text isTMPText = null;

                if ((isTMPText = item.GetComponent<TMP_Text>()) != null)
                {
                    _SelectionBorder.sizeDelta = new Vector2(isTMPText.bounds.size.x + offsetTmp.x, isTMPText.bounds.size.y + offsetTmp.y);
                }
                else
                {
                    _TemporarySizeDelta.Set(itemRect.x + offset.x, itemRect.y + offset.y);
                    if (_SelectionBorder.sizeDelta != _TemporarySizeDelta)
                    {
                        _SelectionBorder.sizeDelta = _TemporarySizeDelta;
                    }
                }   
            }
        }

        public void OnInputHandoff()
        {
            _SelectionBorder.transform.position = _MenuItems[_currentIndex].Item.transform.position;
            var itemRect = _MenuItems[_currentIndex].Item.sizeDelta;
            Debug.Log($"Set sizeDelta.");
            _TemporarySizeDelta.Set(itemRect.x + TestOffset.x, itemRect.y + TestOffset.y);
            _SelectionBorder.sizeDelta = _TemporarySizeDelta;
            Debug.Log($"Input hand off {gameObject.name}", _SelectionBorder);

            UpdateSelectionBorder();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using IgnoreSolutions.Sodoku;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IgnoreSolutions.PsychSodoku
{
    [RequireComponent(typeof(ScrollRect))]
    public class TestOccupyLotsOfButtons : MonoBehaviour
    {
        [SerializeField] bool _Enabled = false;
        [SerializeField] Button _PlayButton;
        [SerializeField] Button _BackToLevelSelection;
        [SerializeField] LevelList _LevelList;
        ScrollRect _ThisScrollRect;
        RectTransform _ScrollRectTransform;

        Vector2 _LastScrollRectPos;

        Dictionary<string, CanvasGroup> _TrackedButtons = new Dictionary<string, CanvasGroup>();

        [SerializeField] int Cutoff = 100;

        float _DistanceToRecalcVisibility = 10f;
        float _DistanceMarginForLoad = 20f;

        [SerializeField]int _SelectionOffset = 0;

        void SetInitialState()
        {
            float checkRectMinY = _ScrollRectTransform.rect.yMin;
            float checkRectMaxY = _ScrollRectTransform.rect.yMax;
            float checkRectMinX = _ScrollRectTransform.rect.xMin;
            float checkRectMaxX = _ScrollRectTransform.rect.xMax;

            for (int i = 0; i < _ThisScrollRect.content.childCount; i++)
            {
                if (!_TrackedButtons.ContainsKey(_ThisScrollRect.content.GetChild(i).name)) _TrackedButtons.Add(_ThisScrollRect.content.GetChild(i).name, _ThisScrollRect.content.GetChild(i).GetComponent<CanvasGroup>());
                if (i > 5)
                {
                    _TrackedButtons[_ThisScrollRect.content.GetChild(i).name].alpha = 0f;
                }
            }
        }
        private void Awake()
        {
            
            _ThisScrollRect = GetComponent<ScrollRect>();
            _ScrollRectTransform = _ThisScrollRect.GetComponent<RectTransform>();

            

            _ThisScrollRect.onValueChanged.AddListener((x) =>
            {
                //Debug.Log($"Value Changed. {_LastScrollRectPos} -> {_ThisScrollRect.content.transform.localPosition}");

                float scrollDirX = _ThisScrollRect.content.transform.localPosition.x - _LastScrollRectPos.x;
                float scrollDirY = _ThisScrollRect.content.transform.localPosition.y - _LastScrollRectPos.y;
                //Debug.Log($"Scroll Dir: {scrollDirX}, {scrollDirY}");

                if (Mathf.Abs(_LastScrollRectPos.x - _ThisScrollRect.content.transform.localPosition.x) > _DistanceToRecalcVisibility
                || Mathf.Abs(_LastScrollRectPos.y - _ThisScrollRect.content.transform.localPosition.y) > _DistanceToRecalcVisibility)
                {
                    _LastScrollRectPos = _ThisScrollRect.content.transform.localPosition;

                    float checkRectMinY = _ScrollRectTransform.rect.yMin - _DistanceMarginForLoad;
                    float checkRectMaxY = _ScrollRectTransform.rect.yMax + _DistanceMarginForLoad;
                    float checkRectMinX = _ScrollRectTransform.rect.xMin - _DistanceMarginForLoad;
                    float checkRectMaxX = _ScrollRectTransform.rect.xMax + _DistanceMarginForLoad;

                    foreach(Transform child in _ThisScrollRect.content)
                    {
                        RectTransform childRectTransform = child.GetComponent<RectTransform>();

                        Vector3 posInWorld = childRectTransform.parent.TransformPoint(childRectTransform.localPosition);
                        Vector3 posInScroll = _ScrollRectTransform.InverseTransformPoint(posInWorld);

                        float childMinY = posInScroll.y + childRectTransform.rect.yMin;
                        float childMaxY = posInScroll.y + childRectTransform.rect.yMax;
                        float childMinX = posInScroll.x + childRectTransform.rect.xMin;
                        float childMaxX = posInScroll.x + childRectTransform.rect.xMax;

                        if (!_TrackedButtons.ContainsKey(child.name)) _TrackedButtons.Add(child.name, child.GetComponent<CanvasGroup>());

                        if((childMaxY >= checkRectMinY && childMinY <= checkRectMaxY)
                        && (childMaxX >= checkRectMinX && childMinX <= checkRectMaxX)) // on screen.
                        {
                            if(_TrackedButtons[child.name].alpha < .8f)
                            {
                                _TrackedButtons[child.name].alpha = 1f;
                                _TrackedButtons[child.name].interactable = true;
                                _TrackedButtons[child.name].blocksRaycasts = true;
                            }
                        }
                        else
                        {
                            if (_TrackedButtons[child.name].alpha >= 1.0f)
                            {
                                _TrackedButtons[child.name].alpha = 0f;
                                _TrackedButtons[child.name].interactable = false;
                                _TrackedButtons[child.name].blocksRaycasts = false;

                                if(_LastSelectedGameObject == child.gameObject)
                                {
                                    Debug.Log($"Selected level went off screen.");
                                }
                            }
                        }
                    }
                }
            });
        }

        IEnumerator AnimateSnapTo(RectTransform target)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 viewportLocalPosition = _ThisScrollRect.viewport.localPosition;
            Vector2 childLocalPosition = target.localPosition;
            Vector2 result = new Vector2(
                0 - (viewportLocalPosition.x + childLocalPosition.x),
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
            Vector2 Origin = _ThisScrollRect.content.localPosition;

            for(float f = 0f; f <= 1.0f; f += 1.0f * Time.fixedDeltaTime)
            {
                _ThisScrollRect.content.localPosition = Vector2.Lerp(Origin, result, f);

                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForFixedUpdate();
        }

        public void SnapTo(RectTransform target)
        {
            //Debug.Log($"Snap to {target}", target);
            Canvas.ForceUpdateCanvases();

            Vector2 viewportLocalPosition = _ThisScrollRect.viewport.localPosition;
            Vector2 childLocalPosition = target.localPosition;
            Vector2 result = new Vector2(
                0 - (viewportLocalPosition.x + childLocalPosition.x),
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
            _ThisScrollRect.content.localPosition = result;

            //_ThisScrollRect.content.anchoredPosition =
            //    (Vector2)_ThisScrollRect.transform.InverseTransformPoint(_ThisScrollRect.content.position)
            //    - (Vector2)_ThisScrollRect.transform.InverseTransformPoint(target.position);
        }

        // Start is called before the first frame update
        void Start()
        {
            OccupyMenu();

            SetInitialState();
        }

        GameObject _LastSelectedGameObject;
        LevelData _CurrentSelectedLevel;

        private void LateUpdate()
        {
            if (_Enabled == false) return;

            if (EventSystem.current.currentSelectedGameObject != _LastSelectedGameObject)
            {
                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    _LastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                    RectTransform rt = _LastSelectedGameObject.GetComponent<RectTransform>();
                    if (rt != null && rt.gameObject != _PlayButton.gameObject && rt.gameObject.name.StartsWith("Level"))
                    {
                        //SnapTo(rt);
                        StartCoroutine(AnimateSnapTo(rt));
                    }
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(_LastSelectedGameObject);

                }
                
            }
        }

        void SetSelectedLevel(int index)
        {
            _CurrentSelectedLevel = _LevelList.GetLevelList()[index];
        }


        LevelButton _lastSelectedButton;

        public void SetSelectedLevel(LevelData level, LevelButton button, bool directToPlay = false)
        {
            _CurrentSelectedLevel = level;
            if (level == null)
            {
                _PlayButton.interactable = false;
            }
            else
            {
                _PlayButton.interactable = true;
                if(directToPlay)
                {
                    _lastSelectedButton = button;
                    EventSystem.current.SetSelectedGameObject(_PlayButton.gameObject);
                    _BackToLevelSelection.gameObject.SetActive(true);
                }
            }
        }

        public void BackToLevelSelection()
        {
            _BackToLevelSelection.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(_lastSelectedButton.gameObject);
            _lastSelectedButton = null;
        }

        private void OccupyMenu()
        {
            if (_LevelList == null) return;

            LevelButton _prefab = _ThisScrollRect.content.GetChild(0).GetComponent<LevelButton>();

            for (int i = 0; i < Mathf.Min(_LevelList.GetLevelList().Count, Cutoff); i++)
            {

                LevelButton clonedPrefab = Instantiate(_prefab, _ThisScrollRect.content, false);
                clonedPrefab.name = $"Level {i + 1}";

                RawImage levelImage = clonedPrefab.GetComponentInChildren<RawImage>();
                Text levelText = clonedPrefab.GetComponentInChildren<Text>();
                LevelData _thisLevel = _LevelList.GetLevelList()[i];

                levelImage.texture = _thisLevel.GetTilesetTexture();
                levelText.text = $"Level {i + 1}";

                clonedPrefab.SetMyLevel(_thisLevel);

                if(i ==0)
                {
                    EventSystem.current.firstSelectedGameObject = clonedPrefab.gameObject;
                    _CurrentSelectedLevel = _thisLevel;
                }

                clonedPrefab._OnSelect.AddListener(() =>
                {
                    clonedPrefab.SetSelected(this);
                });
                clonedPrefab.onClick.AddListener(() =>
                {
                    clonedPrefab.SetSelected(this, true);
                    clonedPrefab.ThisButtonClicked();
                });
            }

            _prefab.gameObject.SetActive(false);
        }
    }
}
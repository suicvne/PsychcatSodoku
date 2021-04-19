using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IgnoreSolutions.PsychSodoku;
using IgnoreSolutions.PsychSodoku.Extensions;
using IgnoreSolutions.Sodoku;
using SudokuLib;
using SudokuLib.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SudokuLib.Model.Cell;

namespace IgnoreSolutions.PsychSodoku
{
    public enum PlayDifficulty
    {
        EASY,
        MEDIUM,
        HARD
    }
    public class ModifyShaderOffset : MonoBehaviour
    {
        public void OnGamePaused(bool wasPaused, bool currentlyPaused)
        {
            Debug.Log($"[SudokuBoard] Pause State Change. From {wasPaused} => {currentlyPaused}");
            _DontUpdate = currentlyPaused;
            GetGridSpots().ForEach((gridSpot) =>
            {
                gridSpot.SetAllowInputEvents(!currentlyPaused);
            });
        }

        [Header("Game Properties")]
        [SerializeField] public bool _GameShouldRestoreState = false;
        [SerializeField] public bool _DontUpdate = true;
        [SerializeField] public bool _DirectInputToNumberSelect = false;
        [SerializeField] public bool _EnterAsPossibleNumber = false;
        [SerializeField] PlayDifficulty _PlayDifficulty = PlayDifficulty.EASY; // The difficulty that we want to use. This corresponds to how much of the board is shown/hidden.
        [SerializeField] LevelData _CurrentLevelData; // The current level data that we're basing the render off of.
        [SerializeField] int _SelectedIndex = -1;
        [SerializeField] Transform _SelectedTransform;
        internal static bool _ControllerInputOccurredOnce = false;

        [Header("Selection")]
        [SerializeField] Image _SelectionBorder; // Selection border to be moved around as tiles are tapped.
        [SerializeField] Image _AnimationDummy;
        [SerializeField] float _SelectionMovementSpeed = 1.75f;

        [Header("Tileset Generation")]
        public Material TileSet; // The material to use for the individual tiles. This is not the material used for the number sprites which are separate. 
        public Vector2 TileSpacing = new Vector2(1, 1); // ?
        public bool WithBlur = false; // TODO: Remove This?
        public bool IsGenerated = false; // Is the board fully generated?
        public bool DestroyOnDisable = false; // Should we destroy the board when OnDisable ?
        public bool TestCompleteAnimation = false;

        [Header("References")]
        [SerializeField] ControlNumberScreenByKeyboard _NumbersInput;
        [SerializeField] TestUndoFeature _UndoFeature;
        [SerializeField] SodukoGriidSpot _TilePrefab;
        [SerializeField] CongratulationsScreenController _CongratulationsScreen;
        [SerializeField] CanvasGroup _GameUI;
        [SerializeField] NumberToSpriteLookup _NumberToSpriteLookup; // Number -> Sprite lookup for the board.
        [SerializeField] private bool _ForceRegen = false; // Should we force regenerate? This is mostly for the editor's sake
                                                           //[SerializeField] private LineRenderer _LineRenderer; // ???? Template LineRenderer?
        [SerializeField] float _MultiplicationOffset = 4f; // ?

        [Header("Events")]
        [SerializeField] UnityEvent _OnBoardGenerated;
        [SerializeField] UnityEvent _OnBoardFullyComplete;

        internal List<SodukoGriidSpot> Tiles = new List<SodukoGriidSpot>(); // A list of all the tiles that this script manages.
        public List<LineRenderer> LineRenderers = new List<LineRenderer>(); // A list of the LineRenderers that this script manages.

        private Material lastMaterial; // Hm?
        private bool WasBlur = false; // TODO: Remove this?
        private bool done = false; // Keep this

        /// <summary>
        /// During the OnEnable step,
        /// if the SaveManager is found and setup we inject our own
        /// event into the UnityEvent "_OnBoardFullyComplete".
        ///
        /// This Listener is removed when OnDisable is called.
        /// </summary>
        UnityAction _OnBoardCompleteAction;

        /// <summary>
        /// The property block we use to assign
        /// tiling, blur or no blur, and more information.
        /// </summary>
        private MaterialPropertyBlock testBlock;

        /// <summary>
        /// Is an animation pending that should prevent
        /// us from starting the animation coroutine again?
        /// </summary>
        internal bool _AnimationPending = false;

        /// <summary>
        /// How long should the Animation wait for after it's done
        /// before we allow input again?
        /// </summary>
        private WaitForSeconds _WaitBetweenAnimations = new WaitForSeconds(.1f);

        private Vector3 _NumberOrigin;

        protected internal List<SodukoGriidSpot> GetGridSpots() => Tiles;
        protected internal Tuple<PlayDifficulty, LevelData> GetLevelInformation()
            => new Tuple<PlayDifficulty, LevelData>(_PlayDifficulty, _CurrentLevelData);

        public void OnApplicationPause(bool pause)
        {
            if (PsychSaveManager.InstanceNull() == true) return;
            if (pause)
            {
                Debug.Log($"PAUSE IS TRUE! STORING SAVE STATE");
                PsychSudokuSave.SetSaveState(PsychSaveManager.p_Instance.GetCurrentSave(),
                    this,
                    SudokuParametersInjest.p_Instance.GetLevelList(),
                    GameTimeManager.p_Instance);

                PsychSudokuSave.WriteSave(PsychSaveManager.p_Instance.GetCurrentSave());
            }
            else
            {
                // Restore state
                PsychSudokuSave save = PsychSaveManager.p_Instance.GetCurrentSave();

                if (save._SaveStateInformation._IsValidSaveState)
                {
                    Debug.Log($"[ModifyShaderOffset] TODO: Support save state restoration.");
                }
            }
        }

        public void SetPossibleNumbersMode(bool enterAsPossibleNumbers)
        {
            _EnterAsPossibleNumber = enterAsPossibleNumbers;
        }

        public void RegenerateBoard()
        {
            Debug.Log($"[ModifyShaderOffset] Setting Force Regen to true.");
            _ForceRegen = true;
        }

        public void Begin_AnimateShowGameUI()
        {
            _AnimationPending = true;
            StartCoroutine(AnimateShowGameUI());
        }

        IEnumerator AnimateShowGameUI()
        {
            for (float f = 0f; f <= 1.0f; f += 2 * Time.fixedDeltaTime)
            {
                _GameUI.alpha = f;
                yield return _FixedUpdate;
            }

            _GameUI.alpha = 1.0f;

            yield return _WaitBetweenAnimations;
            _AnimationPending = false;
        }

        public void TestAnimation()
        {
            if (_AnimationPending == false)
            {
                TestCompleteAnimation = true;
                PreTestCompleteAnimation();
                StartCoroutine(AnimateBoardCompleted());
                TestCompleteAnimation = false;
            }
        }

        public void SetNumberOriginForAnimation(Transform origin)
        {
            _NumberOrigin = origin.position;
        }

        public void SetNumberForSelectedIndex(int newValue) => SetNumberForSelectedIndex(newValue, false);

        public void SetNumberForSelectedIndex(int newValue, bool skipUndo = false)
        {
            if (_AnimationPending) return;
            if (_SelectedIndex == -1) return;

            newValue = Mathf.Clamp(newValue, 0, 10); // TODO: Valid range?

            SodukoGriidSpot gridSpot = Tiles[_SelectedIndex];
            MeshRenderer mr = gridSpot.gameObject.GetComponent<MeshRenderer>();
            SpriteRenderer sr = gridSpot.transform.GetChild(0).GetComponent<SpriteRenderer>();

            // Handle number input as possible number
            if (_EnterAsPossibleNumber)
            {
                if (newValue == 0)
                {
                    gridSpot.ClearPossibleNumbers();
                    gridSpot.UpdatePossibleNumbers();
                    return;
                }
                gridSpot.AddPossibleNumber(newValue);
                gridSpot.UpdatePossibleNumbers();
            }
            else // Handle number input as normally.
            {
                if (newValue == gridSpot._SquareFilledValue) return;

                if (!skipUndo) _UndoFeature.PushUndo(Tiles[_SelectedIndex]._SquareFilledValue, newValue, Tiles[_SelectedIndex], StepType.ChangeNumber, StepAffection.MainNumber);

                _AnimationPending = true;
                Tiles[_SelectedIndex]._SquareFilledValue = newValue;

                if (gridSpot._PossibleNumbersText.enabled == true && newValue > 0)
                {
                    gridSpot._PossibleNumbersText.enabled = false;
                }
                else if (gridSpot._PossibleNumbersText.enabled == false && newValue == 0)
                {
                    gridSpot._PossibleNumbersText.enabled = true;
                }


                StartCoroutine(AnimateNumberUpdate(gridSpot, mr, sr, newValue, skipUndo));
            }
        }

        void PreTestCompleteAnimation()
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                Transform child = Tiles[i].transform.GetChild(0);

                MeshRenderer mr = Tiles[i].GetComponent<MeshRenderer>();
                testBlock.SetFloat("_Blur", 0);
                mr.SetPropertyBlock(testBlock);

                SpriteRenderer img = child.GetComponent<SpriteRenderer>();
                img.sprite = _NumberToSpriteLookup.GetSpriteByNumber((short)Tiles[i]._SquareSolution);
            }
        }

        WaitForSeconds _TimeBetweenSweep = new WaitForSeconds(.05f);
        WaitForFixedUpdate _FixedUpdate = new WaitForFixedUpdate();

        IEnumerator AnimateBoardCompleted()
        {
            _AnimationPending = true;

            foreach (var lineRenderer in LineRenderers)
                lineRenderer.enabled = false;

            _SelectionBorder.transform.position = Vector2.left * (-Screen.width * 2);

            for (float f = 0f; f < 1.0f; f += (_SelectionMovementSpeed * 2) * Time.fixedDeltaTime)
            {
                _GameUI.alpha = 1.0f - f;
            }

            _GameUI.alpha = 0f;
            yield return _TimeBetweenSweep;


            for (int i = 0; i < Tiles.Count; i += 9)
            {
                for (float f = 0; f < 1.0f; f += (_SelectionMovementSpeed * 2) * Time.fixedDeltaTime)
                {
                    for (int b = 0; b < 9; b++)
                    {
                        Transform child = Tiles[i + b].transform.GetChild(0);
                        Vector3 childOriginScale = child.localScale;
                        child.localScale = Vector3.Lerp(childOriginScale, Vector3.zero, f);
                    }
                    yield return _FixedUpdate;
                }


                yield return _FixedUpdate;
            }

            for (int i = 0; i < Tiles.Count; i += 9)
            {
                for (float f = 0; f < 1.0f; f += (_SelectionMovementSpeed * 2) * Time.fixedDeltaTime)
                {
                    for (int b = 0; b < 9; b++)
                    {
                        Transform child = Tiles[i + b].transform;
                        Vector3 childOriginScale = child.localScale;
                        child.localScale = Vector3.Lerp(childOriginScale, Vector3.one, f);
                    }
                    yield return _FixedUpdate;
                }
            }

            _AnimationPending = false;

            _OnBoardFullyComplete?.Invoke();
        }

        IEnumerator AnimateHideImagesForGroup(int groupNo)
        {
            _AnimationPending = true;

            var grouped = Tiles.FindAll(x => x._SquareGroupNo == groupNo);
            for (int i = 0; i < grouped.Count; i++)
            {
                Transform child = grouped[i].transform.GetChild(0);
                Vector3 childOriginScale = child.localScale;

                for (float f = 0; f < 1.0f; f += _SelectionMovementSpeed * Time.deltaTime)
                {
                    child.localScale = Vector3.Lerp(childOriginScale, Vector3.zero, f);
                    yield return null;
                }
                child.localScale = Vector3.zero;

                yield return null;
            }

            for (int i = 0; i < grouped.Count; i++)
            {
                Transform child = grouped[i].transform;
                Vector3 childOriginScale = child.localScale;

                for (float f = 0; f < 1.0f; f += _SelectionMovementSpeed * Time.deltaTime)
                {
                    child.localScale = Vector3.Lerp(childOriginScale, Vector3.one, f);
                    yield return null;
                }
                child.localScale = Vector3.one;

                yield return null;
            }

            yield return _WaitBetweenAnimations;
            _AnimationPending = false;
        }

        bool CheckFullBoardForCompletion()
        {
            bool full = true;
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i]._SquareFilledValue != Tiles[i]._SquareSolution)
                {
                    full = false;
                    break;
                }
            }

            return full;
        }

        bool CheckGroupForCompletion(int groupNo)
        {
            var grouped = Tiles.FindAll(x => x._SquareGroupNo == groupNo);
            bool full = true;

            for (int i = 0; i < grouped.Count; i++)
            {
                if (grouped[i]._SquareFilledValue != grouped[i]._SquareSolution)
                {
                    full = false;
                    break;
                }
            }

            return full;
        }

        internal IEnumerator AnimateNumberUpdate(SodukoGriidSpot gridSpot, MeshRenderer mr, SpriteRenderer sr, int newValue, bool skipUndo)
        {
            // 0. Set dummy origin
            Vector3 dummyOrigin = _AnimationDummy.transform.position;
            Vector3 dummyScaleOrigin = _AnimationDummy.transform.localScale;
            Vector3 targetScale = Vector3.one * 2.5f;

            // 1. Set Animation dummy Sprite to the proper Number -> Sprite lookup.
            _AnimationDummy.sprite = _NumberToSpriteLookup.GetSpriteByNumber((short)newValue);

            // 2. Set Animation dummy transform to the NumberOrigin.
            _AnimationDummy.transform.position = _NumberOrigin;
            _AnimationDummy.transform.localScale = targetScale;

            // 3. Move Animation dummy to the grid spot.
            Vector3 finalPos = Camera.main.WorldToScreenPoint(gridSpot.transform.position);

            for (float f = 0; f <= 1.0f; f += (_SelectionMovementSpeed * 1.5f) * Time.deltaTime)
            {
                _AnimationDummy.transform.position = Vector3.Lerp(_NumberOrigin, finalPos, f);
                _AnimationDummy.transform.localScale = Vector3.Lerp(targetScale, dummyScaleOrigin, f);
                yield return null;
            }
            _AnimationDummy.transform.position = finalPos;
            _AnimationDummy.transform.localScale = dummyScaleOrigin;

            // 4. Set the proper bits
            UpdateFilledNumberAtSelectedIndex(newValue, skipUndo);

            // 5. Hide the animation dummy
            _AnimationDummy.transform.position = dummyOrigin;


            yield return null;
            yield return _WaitBetweenAnimations;


            if (CheckFullBoardForCompletion())
            {
                yield return AnimateBoardCompleted();
            }
            //else if(CheckGroupForCompletion(gridSpot._SquareGroupNo))
            //{
            //    yield return AnimateHideImagesForGroup(gridSpot._SquareGroupNo);
            //}

            _AnimationPending = false;
        }

        /// <summary>
        /// Updates the tile at the selected position
        /// with a new filled value.
        ///
        /// Verifies that if the number matches the solution, that we
        /// remove the blur. 
        /// </summary>
        /// <param name="newValue"></param>
        public void UpdateFilledNumberAtSelectedIndex(int newValue, bool skipUndo)
        {
            SodukoGriidSpot gridSpot = Tiles[_SelectedIndex];
            MeshRenderer mr = gridSpot.gameObject.GetComponent<MeshRenderer>();
            SpriteRenderer sr = gridSpot.transform.GetChild(0).GetComponent<SpriteRenderer>();

            if (newValue == 0) //erase
            {
                sr.sprite = null;
                testBlock.SetFloat("_Blur", 1);
                testBlock.SetFloat("_BlurAmount", .02f);
                mr.SetPropertyBlock(testBlock);
            }
            else if (gridSpot._SquareFilledValue == gridSpot._SquareSolution)
            {
                // Lose the blur...
                sr.sprite = _NumberToSpriteLookup.GetSpriteByNumber((short)newValue);
                testBlock.SetFloat("_Blur", 0);
                testBlock.SetFloat("_BlurAmount", .0f);
                mr.SetPropertyBlock(testBlock);
            }
            else
            {
                // Maintain blur.
                sr.sprite = _NumberToSpriteLookup.GetSpriteByNumber((short)newValue);
                testBlock.SetFloat("_Blur", 1);
                testBlock.SetFloat("_BlurAmount", .02f);
                mr.SetPropertyBlock(testBlock);
            }
        }

        public int GetValueAt(Vector2Int spot) => GetValueAt(spot.x, spot.y);

        public int GetValueAt(int x, int y)
        {
            if (_CurrentLevelData != null)
            {
                return _CurrentLevelData.GetCell(x + 1, y + 1).Value;
            }

            return -2;
        }

        public Cell GetCellAt(int x, int y)
        {
            if (_CurrentLevelData != null) return _CurrentLevelData.GetCell(x + 1, y + 1);
            return null;
        }

        public bool IsCellRevealedForCurrentDifficulty(int x, int y)
        {
            if (_CurrentLevelData != null) return _CurrentLevelData.GetCellValueByDifficulty(x + 1, y + 1, (int)_PlayDifficulty) != null;
            return false;
        }

        private SodukoGriidSpot GetGridSpot(int x, int y)
        {
            SodukoGriidSpot foundGridSpot = null;
            Tiles.Find(tiles =>
            {
                if ((foundGridSpot = tiles.GetComponent<SodukoGriidSpot>()) != null)
                {
                    if (foundGridSpot.GridSpot.x == x && foundGridSpot.GridSpot.y == y) return foundGridSpot;
                }
                return false;
            });
            return foundGridSpot;
        }

        private SodukoGriidSpot GetGridSpot(int index)
        {
            SodukoGriidSpot foundGridSpot = null;
            Tiles.Find(tiles =>
            {
                if ((foundGridSpot = tiles.GetComponent<SodukoGriidSpot>()) != null)
                {
                    if (foundGridSpot._LevelIndex == index) return foundGridSpot;
                }
                return false;
            });
            return foundGridSpot;
        }



        private void OnEnable()
        {
            UpdateFromSudokuSceneParameters();


            PsychSaveManager saveMgr = ((PsychSaveManager)PsychSaveManager.p_Instance);

            if (saveMgr != null)
            {
                Debug.Log($"[ModifyShaderOffset] Hooking up level complete events.");
                _OnBoardCompleteAction = new UnityAction(Handle_UpdatingSaveOnLevelComplete);
                _OnBoardFullyComplete.AddListener(_OnBoardCompleteAction);
            }

            if (!_GameShouldRestoreState)
            {
                RegenerateBoard();
            }

            _DontUpdate = false;
        }

        public void ReturnToMainMenu()
        {
            if (PsychSaveManager.InstanceNull() == false)
            {
                var save = PsychSaveManager.p_Instance.GetCurrentSave();
                bool saved = PsychSudokuSave.WriteSave(save);

                if (saved) Debug.Log($"Saved successfully.");
                else Debug.LogError(($"Unable to save?"));


            }
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

        void OnDisable()
        {
            try
            {
                _OnBoardFullyComplete.RemoveListener(_OnBoardCompleteAction);
            }
            catch { }
        }

        public void DoFullUpdateForNextLevel()
        {
            UpdateSudokuSceneParametersNextLevel(); ;
            UpdateFromSudokuSceneParameters();

            _DontUpdate = false;
            RegenerateBoard();
        }

        public void UpdateSudokuSceneParametersNextLevel()
        {
            var saveMgr = ((PsychSaveManager)PsychSaveManager.p_Instance);
            saveMgr.SetParametersInjestToNextLevel(saveMgr.GetCurrentSave(), _PlayDifficulty);
        }

        public void UpdateFromSudokuSceneParameters()
        {
            SudokuParametersInjest parameters = SudokuParametersInjest.p_Instance;
            PsychSaveManager saveMgr = ((PsychSaveManager)PsychSaveManager.p_Instance);

            if (parameters != null
                && parameters.GetShouldRestoreFromSaveState() == false)
            {
                _CurrentLevelData = parameters.GetLevel();
                _PlayDifficulty = parameters.GetDifficulty();

                Debug.Log($"[ModifyShaderOffset] Using level {_CurrentLevelData} at difficulty {_PlayDifficulty}");
            }
            else if (parameters != null
                && parameters.GetShouldRestoreFromSaveState() == true)
            {
                _GameShouldRestoreState = true;
                // SuspendGameState will setup the rest and then
                // call on the board to be generated.
            }
            else Debug.Log($"[ModifyShaderOffset] Unable to find SudokuParametersInjest. Using defaults.");
        }

        private void Handle_UpdatingSaveOnLevelComplete()
        {
            // When the level is completed and the SaveManager is
            // present and functioning, the following should happen.
            // 1.The level completion time is recorded.
            // 2. The save manager is notified that the current level is completed.
            //    It is also notified of the difficulty we completed it at.
            // 3. The save file is saved.
            Debug.Log($"Handle_UpdatingSaveOnLevelComplete");

            var gameTime = GameTimeManager.p_Instance;
            var paramsInjest = SudokuParametersInjest.InstanceNull();
            var saveMan = PsychSaveManager.InstanceNull();

            TimeSpan completeTime = GameTimeManager.p_Instance.GetPlayTime();
            PsychSudokuSave s = PsychSaveManager.p_Instance.GetCurrentSave();
            s.SetLevelIndexCompleted(SudokuParametersInjest.p_Instance.GetDifficulty(),
                SudokuParametersInjest.p_Instance.GetLevelIndex(),
                completeTime.TotalSeconds);
        }

        public void SetLevelInformation(LevelData _level, PlayDifficulty difficulty)
        {
            this._CurrentLevelData = _level;
            this._PlayDifficulty = difficulty;

            Regenerate();
        }


        void ClearList(bool doNextDelayed)
        {
            if (Tiles.Count <= 0 || LineRenderers.Count <= 0 && transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (Application.isPlaying)
                        Destroy(transform.GetChild(i).gameObject);
                    else DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
            if (Tiles.Count <= 0 || LineRenderers.Count <= 0 && transform.childCount == 0) return;

            for (int i = Tiles.Count - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                    Destroy(Tiles[i].gameObject);
                else DestroyImmediate(Tiles[i].gameObject);
            }

            /*
            for(int i = LineRenderers.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(LineRenderers[i].gameObject);
            }
            LineRenderers.Clear();
            */

            Tiles.Clear();

            //if(doNextDelayed) Invoke(nameof(Regenerate), .1f);
        }

        void SetRandomBlur()
        {
            testBlock = new MaterialPropertyBlock();
            for (int i = 0; i < transform.childCount; i++)
            {

                testBlock.SetFloat("_BlurAmount", UnityEngine.Random.Range(0, 2.0f));
                transform.GetChild(i).GetComponent<MeshRenderer>().SetPropertyBlock(testBlock);
            }
        }

        public void ResetSelectionBorderPosition()
        {
            if (_SelectedIndex != -1) _SelectionBorder.transform.position = Camera.main.WorldToScreenPoint(Tiles[_SelectedIndex].transform.position);
            else
            {
                _SelectionBorder.transform.position = new Vector3(-Screen.width * 2, 0, 0);
            }
        }

        private WaitForFixedUpdate _WaitForFixedUpdate = new WaitForFixedUpdate();

        IEnumerator _AnimateSpotChange(Transform target, Vector3 oldPos, Vector3 newPos)
        {
            for (float f = 0; f <= 1.0f; f += _SelectionMovementSpeed * Time.deltaTime)
            {
                target.position = Vector3.Lerp(oldPos, newPos, f);
                yield return _WaitForFixedUpdate;
            }

            target.position = newPos;
            yield return _WaitBetweenAnimations;
            _AnimationPending = false;
        }

        private static bool HasPreviouslyFilledValue(SodukoGriidSpot gridSpot)
        {
            if (PsychSaveManager.InstanceNull() == false)
            {
                var _CurrentSave = PsychSaveManager.p_Instance.GetCurrentSave();

                GridSpotInformation? value = _CurrentSave._SaveStateInformation._PlayerModifiedGridSpots
                    .FirstOrDefault(x => x._IndexOnGrid == gridSpot._LevelIndex);
                if (value.HasValue)
                {
                    if (value.Value._FilledValue != 0)
                        gridSpot._SquareFilledValue = value.Value._FilledValue;
                    if (value.Value._PossibleNumbers != null
                        && value.Value._PossibleNumbers.Length > 0)
                        gridSpot.PossibleNumbers.AddRange(value.Value._PossibleNumbers);

                    return true;
                }
            }
            else
            {
                Debug.LogWarning($"Skipping value check of {gridSpot} because the PsychSaveManager instance is null.");
            }

            return false;
        }

        void Regenerate()
        {
            if (_CurrentLevelData == null)
            {
                Debug.LogError($"No level.");
                return;
            }

            if (LineRenderers.Count > 0)
            {
                // Enable Line Renderers
                foreach (var lineRenderer in LineRenderers)
                    lineRenderer.enabled = true;
            }

            // Generate board
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    var newTile = Instantiate(_TilePrefab, transform);

                    newTile.gameObject.name = $"tile_x{x + 1}_y{y + 1}";
                    newTile.GridSpot = new Vector2Int(x, y);
                    newTile.parent = this;

                    // Get Value
                    // "Debug Value" For now. This will have to be changed later.
                    Cell sudokuCell = GetCellAt(x, y);
                    bool _revealed = IsCellRevealedForCurrentDifficulty(x, y);
                    newTile.DebugValue = sudokuCell.Value;
                    newTile._SquareSolution = newTile.DebugValue;
                    newTile._LevelIndex = sudokuCell.Index;
                    newTile._SquareGroupNo = sudokuCell.GroupNo;
                    newTile._SquareFilledValue = _revealed ? sudokuCell.Value : -1;
                    newTile.SetCanBeSelected(!_revealed);
                    newTile._OnGridSpotTapped = new UnityEvent<SodukoGriidSpot>();
                    newTile._OnGridSpotTapped?.AddListener((_gridSpot) =>
                    {
                        SetSelectionLocationToTappedSpot(_gridSpot);
                    });

                    // SAVE STATE LOAD INFO
                    if (_GameShouldRestoreState
                        && HasPreviouslyFilledValue(newTile))
                    {
                        Debug.Log($"[ModifyShaderOffset] {newTile} has previously filled value.");
                    }
                    // END SAVE STATE LOAD INFO

                    MeshRenderer mr = newTile.GetComponent<MeshRenderer>();
                    MeshFilter mf = newTile.GetComponent<MeshFilter>();

                    // ASSIGNING THE LEVEL TEXTURE
                    testBlock = new MaterialPropertyBlock();
                    mr.material = TileSet;
                    mr.sharedMaterial.SetTexture("_Tilemap", this._CurrentLevelData.GetTilesetTexture());
                    mr.sharedMaterial.SetTexture("_MainTex", this._CurrentLevelData.GetTilesetTexture());
                    //if (!_revealed)
                    if (newTile._SquareFilledValue == -1)
                    {
                        testBlock.SetFloat("_Blur", 1);
                        testBlock.SetFloat("_BlurAmount", .02f);
                        mr.SetPropertyBlock(testBlock);
                    }
                    else
                    {
                        testBlock.SetFloat("_Blur", 0);
                        testBlock.SetFloat("_BlurAmount", 0);
                        mr.SetPropertyBlock(testBlock);
                    }

                    // Creating mesh data.
                    // TODO: Cache this somewhere.
                    Mesh mesh = new Mesh();
                    mesh.vertices = new Vector3[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(+0.5f, -0.5f, 0f), new Vector3(-0.5f, +0.5f, 0f), new Vector3(+0.5f, +0.5f, 0f) };
                    mesh.normals = new Vector3[] { new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f) };


                    float th = 1f / 3f; // Tile Height
                    float tileScale = 1 / 3f; // Tile Scale

                    // Set the mesh UV offset for image tiling.
                    Vector2 tileOffset = new Vector2((float)x, (float)y) * tileScale;
                    mesh.uv = new Vector2[] { new Vector2(0f, 0f) + tileOffset, new Vector2(th, 0f) + tileOffset, new Vector2(0f, th) + tileOffset, new Vector2(th, th) + tileOffset };
                    mesh.triangles = new int[] { 0, 1, 2, 1, 3, 2 };
                    mf.mesh = mesh;

                    // Set transform position 
                    newTile.transform.position = (tileOffset + TileSpacing) * _MultiplicationOffset;

                    // Sprite Child
                    newTile._SolutionNumberSprite.sprite = _NumberToSpriteLookup.GetSpriteByNumber((short)newTile._SquareFilledValue);
                    newTile._SolutionNumberSprite.transform.localPosition = Vector3.zero;
                    newTile._SolutionNumberSprite.transform.localScale = Vector3.one * .15f;
                    newTile._SolutionNumberSprite.flipX = true;

                    // Possible Numbers

                    newTile._PossibleNumbersText.transform.localPosition = Vector3.forward;
                    newTile._PossibleNumbersText.transform.rotation = Quaternion.Euler(0, 180, 0);

                    newTile.UpdatePossibleNumbers();

                    Tiles.Add(newTile);

                    IsGenerated = true;
                }
            }

            //// Place lines
            //if(LineRenderers.Count == 0)
            //    DrawGridDividers();

            if (_CongratulationsScreen != null)
            {
                Texture2D tileSetTexture = (Texture2D)_CurrentLevelData.GetTilesetTexture();
                Sprite spr = Sprite.Create(tileSetTexture, new Rect(0, 0, tileSetTexture.width, tileSetTexture.height), Vector2.one * .5f);
                _CongratulationsScreen.SetBoardImage(spr);
            }

            _OnBoardGenerated?.Invoke();
        }

        internal void SetSelectionLocationToTappedSpot(SodukoGriidSpot gridSpot)
        {
            if (_SelectionBorder != null && !_AnimationPending)
            {
                _AnimationPending = true;
                _SelectedIndex = gridSpot._LevelIndex;
                _SelectedTransform = gridSpot.transform;
                // TODO: Cache the camera for later so we don't repeatedly call "FindObjectOfType<Camera>" every time someone taps a square.
                StartCoroutine(_AnimateSpotChange(_SelectionBorder.transform,
                    _SelectionBorder.transform.position,
                    Camera.main.WorldToScreenPoint(gridSpot.transform.position))
                );
            }
        }

        private void Update()
        {
            if (_CurrentLevelData == null) return;
            if (_DontUpdate == true) return;

            if (_ForceRegen)
            {
                Debug.Log($"[ModifyShaderOffset] Regenerating Level with {_CurrentLevelData} as its template.");
                _ForceRegen = true;
                ClearList(false);
                Regenerate();
                _ForceRegen = false;
                return;
            }

            if (_DirectInputToNumberSelect == false)
            {
                float h = Input.GetAxis("Horizontal"), v = Input.GetAxis("Vertical");

                int possibleSpot = -1;

                if (h > .2f) possibleSpot = _SelectedIndex - 9;
                else if (h < -.2f) possibleSpot = _SelectedIndex + 9;
                if (v > .2f) possibleSpot = _SelectedIndex + 1;
                else if (v < -.2f) possibleSpot = _SelectedIndex - 1;

                if (possibleSpot != -1)
                {
                    if (_ControllerInputOccurredOnce == false) _ControllerInputOccurredOnce = true;
                    int[] xy = Extensions.Extensions.Index1DTo2D(possibleSpot, 9);

                    GetGridSpot(_SelectedIndex);

                    SodukoGriidSpot gsp = GetGridSpot(xy[0], xy[1]);
                    if (gsp != null)
                    {
                        SetSelectionLocationToTappedSpot(gsp);
                    }
                    possibleSpot = -1;
                }

                if (_SelectedIndex >= 0)
                {
                    if (GetGridSpots()[_SelectedIndex]._CanBeSelected == true &&
                        Input.GetButtonDown("Submit") &&
                        !_NumbersInput.inProgress)
                    {
                        if (_ControllerInputOccurredOnce == false) _ControllerInputOccurredOnce = true;
                        Debug.Log($"Redirect");
                        _DirectInputToNumberSelect = true;
                        _DontUpdate = true;
                        _NumbersInput?.RedirectInput();
                    }
                }

                /*
                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    Debug.Log($"De-selecting for your convenience ;) Direct: {_DirectInputToNumberSelect}");
                    EventSystem.current.SetSelectedGameObject(null);
                }
                */
            }
            else
            {
                if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel"))
                    && _DirectInputToNumberSelect
                    && EventSystem.current.currentSelectedGameObject != null)
                {
                    _NumbersInput?.CancelInput();
                }
            }

            /*
            if (TileSet != lastMaterial)
            {
                Debug.Log($"Tiles changed, regenerating...");
                ClearList();
                Regenerate();
            }

            if(WasBlur != WithBlur)
            {
                ClearList();
                Regenerate();
            }
            */

            lastMaterial = TileSet;
            WasBlur = WithBlur;
        }

        private void OnValidate()
        {
            if (Application.isPlaying && TestCompleteAnimation && !_AnimationPending)
            {
                PreTestCompleteAnimation();
                StartCoroutine(AnimateBoardCompleted());
                TestCompleteAnimation = false;
            }
        }
    }

}
//[ExecuteInEditMode]

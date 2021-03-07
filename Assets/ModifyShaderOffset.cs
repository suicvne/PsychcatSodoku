﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IgnoreSolutions.PsychSodoku;
using IgnoreSolutions.Sodoku;
using SudokuLib;
using SudokuLib.Model;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static SudokuLib.Model.Cell;

[Serializable]
public class SodukoBoard
{
    public int[,] Board = new int[9, 9];

    public void InitBoard()
    {
        for(int x = 0; x < Board.GetUpperBound(0) + 1; x++)
        {
            for(int y = 0; y < Board.GetUpperBound(1) + 1; y++)
            {
                Board[x, y] = (int)UnityEngine.Random.Range(1, 10);
            }
        }
    }

    public void SerializeBoard(string path)
    {
        using(StreamWriter sw = new StreamWriter(File.OpenWrite(path)))
        {
            int[] oneDArray = To1D(Board, 9, 9);

            sw.WriteLine($"size 9x9");

            for(int i = 0; i < oneDArray.Length; i++)
            {
                int x, y;
                x = i % 9;
                y = i / 9;
                sw.WriteLine($"{x}, {y}: {oneDArray[i]}");
            }
        }
    }

    public void ReadBoard(string path)
    {
        using(StreamReader sr = new StreamReader(File.OpenRead(path)))
        {
            string sizeLine = sr.ReadLine().Split(' ')[1];

            Debug.Log($"Read Size: {sizeLine}");

            int sizeX, sizeY;

            sizeX = int.Parse(sizeLine.Split('x')[0]);
            sizeY = int.Parse(sizeLine.Split('x')[1]);

            int[,] readArray = new int[sizeX, sizeY];

            Debug.Log($"Read 2D Size: {sizeX}, {sizeY}");

            string readLine = "";
            while((readLine = sr.ReadLine()) != "")
            {
                string pos, value;
                pos = readLine.Split(':')[0].Trim();
                value = readLine.Split(':')[1].Trim();

                int xPos = int.Parse(pos.Split(',')[0]);
                int yPos = int.Parse(pos.Split(',')[1]);

                int valueInt = int.Parse(value);

                Debug.Log($"Read Value at {xPos}, {yPos} is {valueInt}");
                readArray[xPos, yPos] = valueInt;
            }

            Board = readArray;
        }
    }

    public int[] To1D(int[,] array, int width, int height)
    {
        int[] returnValue = new int[width * height];

        for(int x = 0; x < array.GetUpperBound(0) + 1; x++)
        {
            for(int y = 0; y < array.GetUpperBound(1) + 1; y++)
            {
                returnValue[x + (y * width)] = array[x, y];
            }
        }

        return returnValue;
    }

    public int[,] To2D(int[] array, int width, int height)
    {
        int[,] returnValue = new int[width, height];

        for(int i = 0; i < array.Length; i++)
        {
            int x, y;
            x = i % width;
            y = i / width;

            returnValue[x, y] = array[i];
        }

        return returnValue;
    }
}

[ExecuteInEditMode]
public class ModifyShaderOffset : MonoBehaviour
{
    public enum PlayDifficulty
    {
        EASY,
        MEDIUM,
        HARD
    }

    [Header("Game Properties")]
    [SerializeField] public bool _EnterAsPossibleNumber = false;
    [SerializeField] PlayDifficulty _PlayDifficulty = PlayDifficulty.EASY; // The difficulty that we want to use. This corresponds to how much of the board is shown/hidden.
    [SerializeField] LevelData _CurrentLevelData; // The current level data that we're basing the render off of.
    [SerializeField] int _SelectedIndex = -1;
    [SerializeField] Transform _SelectedTransform;

    [Header("Selection")]
    [SerializeField] Image _SelectionBorder; // Selection border to be moved around as tiles are tapped.
    [SerializeField] Image _AnimationDummy;
    [SerializeField] float _SelectionMovementSpeed = .75f;

    [Header("Tileset Generation")]
    public Material TileSet; // The material to use for the individual tiles. This is not the material used for the number sprites which are separate. 
    public Vector2 TileSpacing = new Vector2(1, 1); // ?
    public bool WithBlur = false; // TODO: Remove This?
    public bool IsGenerated = false; // Is the board fully generated?
    public bool DestroyOnDisable = false; // Should we destroy the board when OnDisable ?
    public bool TestCompleteAnimation = false;

    [Header("References")]
    [SerializeField] TestUndoFeature _UndoFeature;
    [SerializeField] SodukoGriidSpot _TilePrefab;
    [SerializeField] CongratulationsScreenController _CongratulationsScreen;
    [SerializeField] CanvasGroup _GameUI;
    [SerializeField] NumberToSpriteLookup _NumberToSpriteLookup; // Number -> Sprite lookup for the board.
    [SerializeField] private bool _ForceRegen = false; // Should we force regenerate? This is mostly for the editor's sake
    [SerializeField] private LineRenderer _LineRenderer; // ???? Template LineRenderer?
    [SerializeField] float _MultiplicationOffset = 4f; // ?

    [Header("Events")]
    [SerializeField] UnityEvent _OnBoardGenerated;
    [SerializeField] UnityEvent _OnBoardFullyComplete;

    internal List<SodukoGriidSpot> Tiles = new List<SodukoGriidSpot>(); // A list of all the tiles that this script manages.
    private List<LineRenderer> LineRenderers = new List<LineRenderer>(); // A list of the LineRenderers that this script manages.

    private Material lastMaterial; // Hm?
    private bool WasBlur = false; // TODO: Remove this?
    private bool done = false; // Keep this

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
    private WaitForSeconds _WaitBetweenAnimations = new WaitForSeconds(.25f);

    private Vector3 _NumberOrigin;

    public void SetPossibleNumbersMode(bool enterAsPossibleNumbers)
    {
        _EnterAsPossibleNumber = enterAsPossibleNumbers;
    }

    public void RegenerateBoard()
    {
        _ForceRegen = true;
    }

    public void Begin_AnimateShowGameUI()
    {
        _AnimationPending = true;
        StartCoroutine(AnimateShowGameUI());
    }

    IEnumerator AnimateShowGameUI()
    {
        for(float f = 0f; f <= 1.0f; f += 2 * Time.fixedDeltaTime)
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

        if (_EnterAsPossibleNumber)
        {
            if(newValue == 0)
            {
                gridSpot.ClearPossibleNumbers();
                gridSpot.UpdatePossibleNumbers();
                return;
            }
            gridSpot.AddPossibleNumber(newValue);
            gridSpot.UpdatePossibleNumbers();
        }
        else
        {
            if (newValue == gridSpot._SquareFilledValue) return;

            if(!skipUndo) _UndoFeature.PushUndo(Tiles[_SelectedIndex]._SquareFilledValue, newValue, Tiles[_SelectedIndex], StepType.ChangeNumber, StepAffection.MainNumber);

            _AnimationPending = true;
            Tiles[_SelectedIndex]._SquareFilledValue = newValue;

            // Old value, new value, grid spot.
            

            StartCoroutine(AnimateNumberUpdate(gridSpot, mr, sr, newValue, skipUndo));
        }
    }

    void PreTestCompleteAnimation()
    {
        for(int i = 0; i < Tiles.Count; i++)
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

        _SelectionBorder.transform.position = Vector2.left * (-Screen.width * 2);

        for(float f = 0f; f < 1.0f; f += (_SelectionMovementSpeed * 2) * Time.fixedDeltaTime)
        {
            _GameUI.alpha = 1.0f - f;
        }

        _GameUI.alpha = 0f;
        yield return _TimeBetweenSweep;


        for(int i = 0; i < Tiles.Count; i += 9)
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
        for(int i = 0; i < grouped.Count; i++)
        {
            Transform child = grouped[i].transform.GetChild(0);
            Vector3 childOriginScale = child.localScale;

            for(float f = 0; f < 1.0f; f += _SelectionMovementSpeed * Time.deltaTime)
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
        for(int i = 0; i < Tiles.Count; i++)
        {
            if(Tiles[i]._SquareFilledValue != Tiles[i]._SquareSolution)
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

        for(int i = 0; i < grouped.Count; i++)
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
        
        for(float f = 0; f <= 1.0f; f += (_SelectionMovementSpeed * 1.5f) * Time.deltaTime)
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


        if(CheckFullBoardForCompletion())
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
        else if(gridSpot._SquareFilledValue == gridSpot._SquareSolution)
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
        if(_CurrentLevelData != null)
        {
            return _CurrentLevelData.GetCell(x+1, y+1).Value;
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

    private void OnEnable()
    {
        ClearList(false);
        if(Application.isPlaying)
        {
            Regenerate();
        }
        //Regenerate();
    }

    private void OnDisable()
    {
        //if(DestroyOnDisable && IsGenerated)
        //{
        //    IsGenerated = false;
        //    ClearList(false);
        //}
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

        for(int i = LineRenderers.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(LineRenderers[i].gameObject);
        }

        Tiles.Clear();
        LineRenderers.Clear();

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
        if(_SelectedIndex != -1) _SelectionBorder.transform.position = Camera.main.WorldToScreenPoint(Tiles[_SelectedIndex].transform.position);
        else
        {
            _SelectionBorder.transform.position = new Vector3(-Screen.width * 2, 0, 0);
        }
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

    IEnumerator _AnimateSpotChange(Transform target, Vector3 oldPos, Vector3 newPos)
    {
        for(float f = 0; f <= 1.0f; f += _SelectionMovementSpeed * Time.deltaTime)
        {
            target.position = Vector3.Lerp(oldPos, newPos, f);
            yield return null;
        }

        target.position = newPos;
        yield return _WaitBetweenAnimations;
        _AnimationPending = false;
    }

    void Regenerate()
    {
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
                newTile._OnGridSpotTapped = new UnityEngine.Events.UnityEvent<SodukoGriidSpot>();
                newTile._OnGridSpotTapped?.AddListener((_gridSpot) =>
                {
                    SetSelectionLocationToTappedSpot(_gridSpot);
                });

                MeshRenderer mr = newTile.GetComponent<MeshRenderer>();
                MeshFilter mf = newTile.GetComponent<MeshFilter>();

                // ASSIGNING THE LEVEL TEXTURE
                testBlock = new MaterialPropertyBlock();
                mr.material = TileSet;
                mr.sharedMaterial.SetTexture("_Tilemap", this._CurrentLevelData.GetTilesetTexture());
                mr.sharedMaterial.SetTexture("_MainTex", this._CurrentLevelData.GetTilesetTexture());
                //if (!_revealed)
                if(newTile._SquareFilledValue == -1)
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

        // Place lines
        DrawGridDividers();

        if(_CongratulationsScreen != null)
        {
            Texture2D tileSetTexture = (Texture2D)_CurrentLevelData.GetTilesetTexture();
            Sprite spr = Sprite.Create(tileSetTexture, new Rect(0, 0, tileSetTexture.width, tileSetTexture.height), Vector2.one * .5f);
            _CongratulationsScreen.SetBoardImage(spr);
        }

        _OnBoardGenerated?.Invoke();
    }

    /// <summary>
    /// Clones a valid line renderer and draws lines between sodoku squares.
    /// </summary>
    private void DrawGridDividers()
    {
        if(_LineRenderer == null)
        {
            return;
        }

        // Create 4 unique line renderers for the board dividers.
        // These are cloned after the LineRenderer template provided by the prefab.
        for(int i = 0; i < 4; i++)
        {
            LineRenderer newLineRenderer = Instantiate(_LineRenderer);
            LineRenderers.Add(newLineRenderer);

            SodukoGriidSpot a = null, b = null;
            // blergh
            switch(i)
            {
                case 0:
                    a = GetGridSpot(0, 3);
                    b = GetGridSpot(8, 3);
                    LineRenderers[i].SetPosition(0, (a.transform.position - Vector3.right + (Vector3.down / 1.8f)));
                    LineRenderers[i].SetPosition(1, (b.transform.position + Vector3.right + (Vector3.down / 1.8f)));
                    break;
                case 1:
                    a = GetGridSpot(0, 6);
                    b = GetGridSpot(8, 6);
                    LineRenderers[i].SetPosition(0, (a.transform.position - Vector3.right + (Vector3.down / 1.8f)));
                    LineRenderers[i].SetPosition(1, (b.transform.position + Vector3.right + (Vector3.down / 1.8f)));
                    break;
                case 2:
                    a = GetGridSpot(3, 0);
                    b = GetGridSpot(3, 8);

                    LineRenderers[i].SetPosition(0, (a.transform.position) + (Vector3.left / 1.7f) - Vector3.up);
                    LineRenderers[i].SetPosition(1, (b.transform.position) + (Vector3.left / 1.7f) + Vector3.up);
                    break;
                case 3:
                    a = GetGridSpot(6, 0);
                    b = GetGridSpot(6, 8);

                    LineRenderers[i].SetPosition(0, (a.transform.position) + (Vector3.left / 1.7f) - Vector3.up);
                    LineRenderers[i].SetPosition(1, (b.transform.position) + (Vector3.left / 1.7f) + Vector3.up);
                    break;
            }
        }
    }

    private void Update()
    {
        if(_ForceRegen)
        {
            _ForceRegen = true;
            ClearList(false);
            Regenerate();
            _ForceRegen = false;
            return;
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
        if(Application.isPlaying && TestCompleteAnimation && !_AnimationPending)
        {
            PreTestCompleteAnimation();
            StartCoroutine(AnimateBoardCompleted());
            TestCompleteAnimation = false;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IgnoreSolutions.Sodoku;
using SudokuLib;
using SudokuLib.Model;
using TMPro;
using UnityEngine;
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

public class ModifyShaderOffset : MonoBehaviour
{
    public enum PlayDifficulty
    {
        EASY,
        MEDIUM,
        HARD
    }

    public Material TileSet;
    public Vector2 TileSpacing = new Vector2(1, 1);
    public bool WithBlur = false;

    [SerializeField] PlayDifficulty _PlayDifficulty = PlayDifficulty.EASY;
    [SerializeField] LevelData _CurrentLevelData;
    [SerializeField] private bool _ForceRegen = false;
    [SerializeField] private LineRenderer _LineRenderer;
    [SerializeField] float _MultiplicationOffset = 4f;

    private List<GameObject> Tiles = new List<GameObject>();
    private List<LineRenderer> LineRenderers = new List<LineRenderer>();

    private Material lastMaterial;
    private bool WasBlur = false;
    private bool done = false;

    private MaterialPropertyBlock testBlock;

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

    void Start()
    {
        Regenerate();
    }

    void ClearList()
    {
        for (int i = Tiles.Count - 1; i >= 0; i--)
        {
            Destroy(Tiles[i]);
        }

        for(int i = LineRenderers.Count - 1; i >= 0; i--)
        {
            Destroy(LineRenderers[i].gameObject);
        }

        Tiles.Clear();
        LineRenderers.Clear();
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

    void Regenerate()
    {
        // Generate board
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                // Create new tile gameobject
                GameObject tile = new GameObject();
                tile.transform.parent = transform; // Set parent to our transform

                // Adding Grid Spot Properties
                SodukoGriidSpot gridSpotProperties = tile.AddComponent<SodukoGriidSpot>();
                gridSpotProperties.GridSpot = new Vector2Int(x, y);
                gridSpotProperties.parent = this;

                // "Debug Value" For now. This will have to be changed later.
                Cell sudokuCell = GetCellAt(x, y);
                bool _revealed = IsCellRevealedForCurrentDifficulty(x, y);
                gridSpotProperties.DebugValue = sudokuCell.Value;
                gridSpotProperties._SquareSolution = gridSpotProperties.DebugValue;
                gridSpotProperties._LevelIndex = sudokuCell.Index;
                gridSpotProperties._SquareGroupNo = sudokuCell.GroupNo;
                gridSpotProperties._SquareFilledValue = _revealed ? sudokuCell.Value : -1;


                // Adding MeshFilter and MeshRenderer for 3D Rendering
                MeshFilter mf = tile.AddComponent<MeshFilter>();
                MeshRenderer mr = tile.AddComponent<MeshRenderer>();
                tile.name = string.Format("tile_x{0}_y{1}", x+1, y+1);

                // ASSIGNING THE LEVEL TEXTURE
                if (testBlock == null) testBlock = new MaterialPropertyBlock();
                mr.material = TileSet;
                mr.sharedMaterial.SetTexture("_Tilemap", this._CurrentLevelData.GetTilesetTexture());
                mr.sharedMaterial.SetTexture("_MainTex", this._CurrentLevelData.GetTilesetTexture());
                if (!_revealed)
                {
                    testBlock.SetInt("_Blur", 1);
                    mr.SetPropertyBlock(testBlock);
                }
                else
                {
                    testBlock.SetInt("_Blur", 0);
                    mr.SetPropertyBlock(testBlock);
                }


                // Creating mesh data.
                // TODO: Cache this somewhere.
                Mesh mesh = new Mesh();
                mesh.vertices = new Vector3[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(+0.5f, -0.5f, 0f), new Vector3(-0.5f, +0.5f, 0f), new Vector3(+0.5f, +0.5f, 0f) };
                mesh.normals = new Vector3[] { new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f) };

                
                float th = 1f / 3f; // Tile Height
                float tileScale = 1 / 3f; // Tile Scale

                // Add mesh collider component & set the shared mesh to the mesh data we've created.
                tile.AddComponent<MeshCollider>().sharedMesh = mesh;

                // Set the mesh UV offset for image tiling.
                Vector2 tileOffset = new Vector2((float)x, (float)y) * tileScale;
                mesh.uv = new Vector2[] { new Vector2(0f, 0f) + tileOffset, new Vector2(th, 0f) + tileOffset, new Vector2(0f, th) + tileOffset, new Vector2(th, th) + tileOffset };
                mesh.triangles = new int[] { 0, 1, 2, 1, 3, 2 };
                mf.mesh = mesh;

                // Set transform position 
                tile.transform.position = (tileOffset + TileSpacing) * _MultiplicationOffset;

                // TMP Child
                GameObject tmpParent = new GameObject("TMP");
                tmpParent.transform.parent = tile.transform;
                tmpParent.AddComponent<MeshRenderer>();
                TMP_Text text = tmpParent.AddComponent<TextMeshPro>();

                text.enableAutoSizing = true;
                text.fontSizeMin = 8f;
                text.alignment = TextAlignmentOptions.Center;
                text.margin = new Vector4(10, 2, 10, 2);

                tmpParent.transform.localPosition = Vector3.forward;
                tmpParent.transform.rotation = Quaternion.Euler(0, 180, 0);

                if (gridSpotProperties._SquareFilledValue == -1) text.text = "";
                else text.text = $"{gridSpotProperties.DebugValue}";


                // Add our tile to our managed tiles list.
                Tiles.Add(tile);
            }
        }

        // Place lines
        DrawGridDividers();
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
            ClearList();
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

    //private void Update()
    //{
    //    if (thisRenderer == null) thisRenderer = GetComponent<Renderer>();

    //    thisRenderer.material.mainTextureOffset = new Vector2(1f / (X * 3), 1f / (Y * 3));

    //    Debug = $"Input: {X}, {Y}\n{1f / (X * 3)}, {1f / (Y * 3)}\nResult: {thisRenderer.material.mainTextureOffset}";
    //    //Debug.Log($"Texture Offset (Input: ({X}, {Y})): {1f / (X * 3)}, {1f / (Y * 3)}; {thisRenderer.material.mainTextureOffset}");
    //}

    //private void OnValidate()
    //{
    //    //thisRenderer = GetComponent<Renderer>();

    //    //thisRenderer.material.mainTextureOffset = new Vector2(1 / (X), 1 / (Y));
    //}
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    //[Range(1, 3)]
    //public int X = 1;

    //[Range(1, 3)]
    //public int Y = 1;
    //private Renderer thisRenderer;

    //[SerializeField]
    //private string Debug = "";
    

    public Material TileSet;

    private List<GameObject> Tiles = new List<GameObject>();
    private List<LineRenderer> LineRenderers = new List<LineRenderer>();

    public Vector2 TileSpacing = new Vector2(1, 1);

    public bool WithBlur = false;

    [SerializeField]
    private bool _ForceRegen = false;

    [SerializeField]
    private LineRenderer _LineRenderer;

    public SodukoBoard TestBoard;
    void Start()
    {
        Regenerate();

        TestBoard = new SodukoBoard();
        TestBoard.InitBoard();
        TestBoard.SerializeBoard("./test.sboard");

        
    }

    public int GetValueAt(int x, int y)
    {
        if(TestBoard != null)
        {
            return TestBoard.Board[x, y];
        }

        return -2;
    }

    public int GetValueAt(Vector2Int spot) => GetValueAt(spot.x, spot.y);



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
    MaterialPropertyBlock testBlock;

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
                GameObject tile = new GameObject();
                tile.transform.parent = transform;

                SodukoGriidSpot gridSpotProperties = tile.AddComponent<SodukoGriidSpot>();
                gridSpotProperties.GridSpot = new Vector2Int(x, y);
                gridSpotProperties.parent = this;

                gridSpotProperties.DebugValue = GetValueAt(x, y);

                MeshFilter mf = tile.AddComponent<MeshFilter>();
                MeshRenderer mr = tile.AddComponent<MeshRenderer>();
                tile.name = string.Format("tile_x{0}_y{1}", x, y);
                mr.material = TileSet;
                Mesh mesh = new Mesh();
                mesh.vertices = new Vector3[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(+0.5f, -0.5f, 0f), new Vector3(-0.5f, +0.5f, 0f), new Vector3(+0.5f, +0.5f, 0f) };
                mesh.normals = new Vector3[] { new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 1f) };
                float th = 1f / 3f;
                float tileScale = 1 / 3f;

                tile.AddComponent<MeshCollider>().sharedMesh = mesh;

                Vector2 tileOffset = new Vector2((float)x, (float)y) * tileScale;
                mesh.uv = new Vector2[] { new Vector2(0f, 0f) + tileOffset, new Vector2(th, 0f) + tileOffset, new Vector2(0f, th) + tileOffset, new Vector2(th, th) + tileOffset };
                mesh.triangles = new int[] { 0, 1, 2, 1, 3, 2 };
                mf.mesh = mesh;
                tile.transform.position = (tileOffset + TileSpacing) * _MultiplicationOffset;

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

    private SodukoGriidSpot GetGridSpot(int x, int y)
    {
        SodukoGriidSpot foundGridSpot = null;
        Tiles.Find(tiles =>
        {
            if((foundGridSpot = tiles.GetComponent<SodukoGriidSpot>()) != null)
            {
                if (foundGridSpot.GridSpot.x == x && foundGridSpot.GridSpot.y == y) return foundGridSpot;
            }
            return false;
        });
        return foundGridSpot; 
    }

    [SerializeField]
    float _MultiplicationOffset = 4f;

    private Material lastMaterial;
    private bool WasBlur = false;
    private bool done = false;

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

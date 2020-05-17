using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionOnGrid : MonoBehaviour
{
    private Mesh theMesh;

    [Range(0, 2)]
    public int Quadrant = 0;

    [Range(0, 2)]
    public int X = 0;

    [Range(0, 2)]
    public int Y = 0;

    private void Awake()
    {
        
    }

    private void OnValidate()
    {
        theMesh = GetComponent<MeshFilter>().sharedMesh;

        Vector3[] vertices = theMesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / (9 / (X + 2)), vertices[i].z);
        }
        theMesh.uv = uvs;
    }
}

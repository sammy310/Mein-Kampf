using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockHighlight : MonoBehaviour
{
    Mesh mesh = null;
    MeshRenderer meshRenderer = null;
    MeshFilter meshFilter = null;

    const int BlockVerticesSize = 8;
    const int BlockIndicesSize = 24;

    public BlockPos BlockPos { get; private set; }
    public bool IsEnabled { get; private set; }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        CreateBlockHighlightMesh();
    }

    private void CreateBlockHighlightMesh()
    {
        Vector3[] vertices = new Vector3[BlockVerticesSize];
        int[] indices = new int[BlockIndicesSize];

        float blockLength = ChunkManager.BlockLength;
        int index = 0;

        /*
         *   7------6
         *  /|     /|
         * 4------5 |
         * | |    | |
         * | 3----|-2
         * |/     |/
         * 0------1
         */
        vertices[index++] = new Vector3(0, 0, 0);
        vertices[index++] = new Vector3(blockLength, 0, 0);
        vertices[index++] = new Vector3(blockLength, 0, blockLength);
        vertices[index++] = new Vector3(0, 0, blockLength);
        vertices[index++] = new Vector3(0, blockLength, 0);
        vertices[index++] = new Vector3(blockLength, blockLength, 0);
        vertices[index++] = new Vector3(blockLength, blockLength, blockLength);
        vertices[index++] = new Vector3(0, blockLength, blockLength);
        
        index = 0;
        indices[index++] = 0;
        indices[index++] = 1;
        indices[index++] = 1;
        indices[index++] = 2;
        indices[index++] = 2;
        indices[index++] = 3;
        indices[index++] = 3;
        indices[index++] = 0;

        indices[index++] = 4;
        indices[index++] = 5;
        indices[index++] = 5;
        indices[index++] = 6;
        indices[index++] = 6;
        indices[index++] = 7;
        indices[index++] = 7;
        indices[index++] = 4;

        indices[index++] = 0;
        indices[index++] = 4;
        indices[index++] = 1;
        indices[index++] = 5;
        indices[index++] = 2;
        indices[index++] = 6;
        indices[index++] = 3;
        indices[index++] = 7;

        mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        meshFilter.mesh = mesh;
    }

    public void SetPosition(BlockPos blockPos)
    {
        this.BlockPos = blockPos;
        transform.position = blockPos.GetWorldPosition();
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
        IsEnabled = value;
    }
}

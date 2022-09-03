using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public enum BlockType
    {
        None, Air, Dirt,
    }

    public ChunkPos ChunkPosition { get; private set; }
    public int ChunkX => ChunkPosition.X;
    public int ChunkZ => ChunkPosition.Z;

    Mesh mesh = null;
    MeshRenderer meshRenderer = null;
    MeshFilter meshFilter = null;
    MeshCollider meshCollider = null;

    BlockType[,,] blocks = new BlockType[ChunkManager.ChunkWidth, ChunkManager.ChunkHeight, ChunkManager.ChunkWidth];

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void SetChunk(ChunkPos chunkPos)
    {
        ChunkPosition = chunkPos;

        transform.localPosition = (new Vector3(ChunkX, 0, ChunkZ)) * ChunkManager.ChunkWidth * ChunkManager.BlockLength;

        InitBlocks();
    }

    public void InitChunk(Material material, int blockLayer)
    {
        meshRenderer.material = material;
        gameObject.layer = blockLayer;

        if (mesh == null)
        {
            mesh = new Mesh();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }
    }

    private void InitBlocks()
    {
        int chunkX = ChunkX * ChunkManager.ChunkWidth;
        int chunkZ = ChunkZ * ChunkManager.ChunkWidth;

        for (int x = 0; x < ChunkManager.ChunkWidth; x++)
        {
            for (int z = 0; z < ChunkManager.ChunkWidth; z++)
            {
                float noise = Mathf.PerlinNoise((chunkX + x) * 0.1f, (chunkZ + z) * 0.1f);
                int airY = (int)((ChunkManager.ChunkHeight - (noise * 10)) * 0.5f);
                for (int y = 0; y < ChunkManager.ChunkHeight; y++)
                {
                    if (airY < y)
                    {
                        blocks[x, y, z] = BlockType.Air;
                    }
                    else
                    {
                        blocks[x, y, z] = BlockType.Dirt;
                    }
                }
            }
        }
    }

    public void BuildMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < ChunkManager.ChunkWidth; x++)
        {
            for (int z = 0; z < ChunkManager.ChunkWidth; z++)
            {
                for (int y = 0; y < ChunkManager.ChunkHeight; y++)
                {
                    BuildMeshFromBlock(x, y, z, vertices, triangles);
                }
            }
        }
        
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        UpdateMeshCollider();
    }

    private void BuildMeshFromBlock(int x, int y, int z, List<Vector3> vertices, List<int> triangles)
    {
        var blockType = blocks[x, y, z];
        if (blockType == BlockType.Air)
        {
            return;
        }

        float blockLength = ChunkManager.BlockLength;
        float blockX = x * blockLength;
        float blockY = y * blockLength;
        float blockZ = z * blockLength;

        // Up (Y == 1)
        {
            var block = GetBlockType(x, y + 1, z);
            if (block == BlockType.Air)
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(blockX, blockY + blockLength, blockZ));
                vertices.Add(new Vector3(blockX, blockY + blockLength, blockZ + blockLength));
                vertices.Add(new Vector3(blockX + blockLength, blockY + blockLength, blockZ));
                vertices.Add(new Vector3(blockX + blockLength, blockY + blockLength, blockZ + blockLength));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
            }
        }
        // Left (X == 0)
        {
            var block = GetBlockType(x - 1, y, z);
            if (block == BlockType.Air)
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(blockX, blockY, blockZ));
                vertices.Add(new Vector3(blockX, blockY + blockLength, blockZ));
                vertices.Add(new Vector3(blockX, blockY, blockZ + blockLength));
                vertices.Add(new Vector3(blockX, blockY + blockLength, blockZ + blockLength));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);
            }
        }
        // Right (X == 1)
        {
            var block = GetBlockType(x + 1, y, z);
            if (block == BlockType.Air)
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(blockX + blockLength, blockY, blockZ));
                vertices.Add(new Vector3(blockX + blockLength, blockY + blockLength, blockZ));
                vertices.Add(new Vector3(blockX + blockLength, blockY, blockZ + blockLength));
                vertices.Add(new Vector3(blockX + blockLength, blockY + blockLength, blockZ + blockLength));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
            }
        }
        // Front (Z == 1)
        {
            var block = GetBlockType(x, y, z + 1);
            if (block == BlockType.Air)
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(blockX, blockY, blockZ + blockLength));
                vertices.Add(new Vector3(blockX, blockY + blockLength, blockZ + blockLength));
                vertices.Add(new Vector3(blockX + blockLength, blockY, blockZ + blockLength));
                vertices.Add(new Vector3(blockX + blockLength, blockY + blockLength, blockZ + blockLength));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);
            }
        }
        // Back (Z == 0)
        {
            var block = GetBlockType(x, y, z - 1);
            if (block == BlockType.Air)
            {
                int vertexIndex = vertices.Count;
                vertices.Add(new Vector3(blockX, blockY, blockZ));
                vertices.Add(new Vector3(blockX, blockY + blockLength, blockZ));
                vertices.Add(new Vector3(blockX + blockLength, blockY, blockZ));
                vertices.Add(new Vector3(blockX + blockLength, blockY + blockLength, blockZ));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
            }
        }
    }

    public void UpdateMeshCollider()
    {
        meshCollider.enabled = false;
        meshCollider.enabled = true;
    }


    public BlockType GetBlockType(int x, int y, int z)
    {
        if (ChunkManager.ChunkHeight <= y)
        {
            return BlockType.Air;
        }

        if (x < 0)
        {
            return WorldManager.Instance.ChunkManager.GetBlockType(new ChunkPos(ChunkX - 1, ChunkZ), ChunkManager.ChunkWidth + x, y, z);
        }
        if (ChunkManager.ChunkWidth <= x)
        {
            return WorldManager.Instance.ChunkManager.GetBlockType(new ChunkPos(ChunkX + 1, ChunkZ), x - ChunkManager.ChunkWidth, y, z);
        }
        if (z < 0)
        {
            return WorldManager.Instance.ChunkManager.GetBlockType(new ChunkPos(ChunkX, ChunkZ - 1), x, y, ChunkManager.ChunkWidth + z);
        }
        if (ChunkManager.ChunkWidth <= z)
        {
            return WorldManager.Instance.ChunkManager.GetBlockType(new ChunkPos(ChunkX, ChunkZ + 1), x, y, z - ChunkManager.ChunkWidth);
        }
        if (y < 0)
        {
            return BlockType.None;
        }

        return blocks[x, y, z];
    }

    public BlockType RemoveBlock(int x, int y, int z)
    {
        BlockType blockType = blocks[x, y, z];
        blocks[x, y, z] = BlockType.Air;

        BuildMesh();

        if (GetClosedChunk(x, y, z, out var chunkPos))
        {
            WorldManager.Instance.ChunkManager.GetChunk(chunkPos).BuildMesh();
        }

        return blockType;
    }

    public bool GetClosedChunk(int x, int y, int z, out ChunkPos chunkPos)
    {
        int edgeWidth = ChunkManager.ChunkWidth - 1;
        chunkPos = new ChunkPos(ChunkPosition);

        if (x == 0)
        {
            chunkPos.X--;
            return true;
        }
        if (z == 0)
        {
            chunkPos.Z--;
            return true;
        }
        if (x == edgeWidth)
        {
            chunkPos.X++;
            return true;
        }
        if (z == edgeWidth)
        {
            chunkPos.Z++;
            return true;
        }

        return false;
    }


    public static bool IsEdgePosition(int x, int y, int z)
    {
        int edgeWidth = ChunkManager.ChunkWidth - 1;
        int edgeHeight = ChunkManager.ChunkHeight - 1;
        return x == 0 || y == 0 || z == 0 || x == edgeWidth || y == edgeHeight || z == edgeWidth;
    }
}

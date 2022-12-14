using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkPos
{
    public int X { get; set; }
    public int Z { get; set; }

    public ChunkPos(ChunkPos chunkPos)
    {
        X = chunkPos.X;
        Z = chunkPos.Z;
    }

    public ChunkPos(int x, int z)
    {
        X = x;
        Z = z;
    }
}

public struct BlockPos
{
    private ChunkPos chunkPos;
    public ChunkPos ChunkPos => chunkPos;

    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public bool IsNull { get; set; }
    
    public BlockPos(ChunkPos chunkPos, int x, int y, int z)
    {
        this.chunkPos = new ChunkPos(chunkPos);
        X = x;
        Y = y;
        Z = z;

        IsNull = false;
    }

    public void SetChunkPos(int x, int z)
    {
        chunkPos.X = x;
        chunkPos.Z = z;
    }

    public void SetBlockPos(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3 GetWorldPosition()
    {
        int x = this.chunkPos.X * ChunkManager.ChunkWidth + X;
        int z = this.chunkPos.Z * ChunkManager.ChunkWidth + Z;
        return (new Vector3(x, Y, z)) * ChunkManager.BlockLength;
    }

    public static BlockPos GetNull()
    {
        return new BlockPos { IsNull = true };
    }
}

public class ChunkManager : MonoBehaviour
{
    public const float BlockLength = 1f;

    public const int ChunkWidth = 16;
    public const int ChunkHeight = 64;

    [SerializeField] GameObject ChunkPrefab;
    [SerializeField] Material ChunkMaterial;

    public int MaxChunkSize { get; private set; } = 8;

    public Transform ChunkTransform { get; private set; } = null;
    public int BlockLayerMask { get; private set; }
    
    Dictionary<ChunkPos, Chunk> Chunks = new Dictionary<ChunkPos, Chunk>();

    private void Awake()
    {
        BlockLayerMask = LayerMask.NameToLayer("Block");
    }

    public void InitChunks()
    {
        ChunkTransform = new GameObject("ChunkTransform").transform;

        for (int x = 0; x < MaxChunkSize; x++)
        {
            for (int z = 0; z < MaxChunkSize; z++)
            {
                var chunk = Instantiate(ChunkPrefab, ChunkTransform).GetComponent<Chunk>();
                ChunkPos chunkPos = new ChunkPos(x, z);
                chunk.SetChunk(chunkPos);
                chunk.InitChunk(ChunkMaterial, BlockLayerMask);

                AddChunk(chunkPos, chunk);
            }
        }

        BuildChunkMeshes();
    }


    private void BuildChunkMeshes()
    {
        foreach (var chunk in Chunks.Values)
        {
            chunk.BuildMesh();
        }
    }


    private void AddChunk(ChunkPos chunkPos, Chunk chunk)
    {
        Chunks.Add(chunkPos, chunk);
    }

    public Chunk GetChunk(ChunkPos chunkPos)
    {
        if (Chunks.TryGetValue(chunkPos, out var chunk))
        {
            return chunk;
        }
        return null;
    }


    public ItemType GetBlockType(ChunkPos chunkPos, int x, int y, int z)
    {
        var chunk = GetChunk(chunkPos);
        if (chunk == null)
        {
            return ItemType.None;
        }

        return chunk.GetBlockType(x, y, z);
    }

    public void SetBlock(BlockPos blockPos, ItemType itemType)
    {
        GetChunk(blockPos.ChunkPos).SetBlock(blockPos.X, blockPos.Y, blockPos.Z, itemType);
    }

    public ItemType RemoveBlock(BlockPos blockPos)
    {
        var chunk = GetChunk(blockPos.ChunkPos);
        if (chunk == null)
        {
            return ItemType.None;
        }

        return chunk.RemoveBlock(blockPos.X, blockPos.Y, blockPos.Z);
    }


    public static BlockPos GetBlockPosFromWorldPosition(Vector3 worldPosition)
    {
        worldPosition /= ChunkManager.BlockLength;
        int x = Mathf.FloorToInt(worldPosition.x);
        int y = Mathf.FloorToInt(worldPosition.y);
        int z = Mathf.FloorToInt(worldPosition.z);
        
        ChunkPos chunkPos = new ChunkPos(x / ChunkManager.ChunkWidth, z / ChunkManager.ChunkWidth);
        return new BlockPos(chunkPos, x % ChunkManager.ChunkWidth, y % ChunkManager.ChunkHeight, z % ChunkManager.ChunkWidth);
    }

    public static void GetBlockPosFromWorldPosition(Vector3 worldPosition, ref BlockPos blockPos)
    {
        worldPosition /= ChunkManager.BlockLength;
        int x = Mathf.FloorToInt(worldPosition.x);
        int y = Mathf.FloorToInt(worldPosition.y);
        int z = Mathf.FloorToInt(worldPosition.z);

        blockPos.SetChunkPos(x / ChunkManager.ChunkWidth, z / ChunkManager.ChunkWidth);
        blockPos.SetBlockPos(x % ChunkManager.ChunkWidth, y % ChunkManager.ChunkHeight, z % ChunkManager.ChunkWidth);
    }
}

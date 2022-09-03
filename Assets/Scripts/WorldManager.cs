using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; } = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        ChunkManager = GetComponent<ChunkManager>();
    }

    public ChunkManager ChunkManager { get; private set; } = null;

    public int Seed { get; set; }

    [SerializeField] PlayerController playerController;
    public PlayerController PlayerController => playerController;


    private void Start()
    {
        ChunkManager.InitChunks();

        SetPlayerPosition(new Vector3(16, 0, 16));
    }

    public void SetSeed(int seed)
    {
        Seed = seed;
    }

    public void SetPlayerPosition(Vector3 position)
    {
        float y = GetTopBlockPosition(position);
        if (y < 0)
        {
            return;
        }

        float blockHalf = ChunkManager.BlockLength * 0.5f;
        position.x = Mathf.FloorToInt(position.x) + blockHalf;
        position.y = y + ChunkManager.BlockLength;
        position.z = Mathf.FloorToInt(position.z) + blockHalf;
        PlayerController.transform.position = position;
    }

    public float GetTopBlockPosition(Vector3 position)
    {
        position.y = 1000f;
        Ray ray = new Ray(position, Vector3.down);
        int layerMask = 1 << ChunkManager.BlockLayerMask;
        if (Physics.Raycast(ray, out var hit, 1000f, layerMask))
        {
            return hit.point.y;
        }
        return -1f;
    }

    public bool IsBlockExistsAtBottom(Vector3 position)
    {
        Ray ray = new Ray(position, Vector3.down);
        int layerMask = 1 << ChunkManager.BlockLayerMask;
        if (Physics.Raycast(ray, out var hit, 1000f, layerMask))
        {
            return true;
        }
        return false;
    }
}

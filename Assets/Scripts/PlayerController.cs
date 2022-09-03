using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Player Info
    public const float PlayerWidth = 0.8f;
    public const float PlayerHeight = 1.6f;
    public const float PlayerDetectFactor = 0.95f;

    // Player Move
    [SerializeField] float PlayerSpeed = 1f;

    [SerializeField] float playerJump = 250f;
    public float PlayerJump => playerJump * ChunkManager.BlockLength;
    [SerializeField] float PlayerJumpCooltime = 0.1f;
    float jumpCooltime = 0f;

    public bool IsJump { get; private set; } = false;
    
    [SerializeField] Transform GroundDetectionTransform = null;
    [SerializeField] float GroundDetectionLength = 0.1f;

    // Camera
    [SerializeField] float CameraRotationLimit = 85f;
    [SerializeField] float cameraSensitivity = 1f;
    float cameraRotationX = 0f;
    float cameraRotationY = 0f;

    // Player Interact
    [SerializeField] float playerInteractLengthFactor = 4f;
    public float PlayerInteractLengthFactor => playerInteractLengthFactor * ChunkManager.BlockLength;
    
    [SerializeField] BlockHighlight BlockHighlightPrefab;
    BlockHighlight BlockHighlight = null;

    BlockPos LookAtBlockPos = new BlockPos();

    // Click
    public bool IsMouseLeftClick { get; private set; } = false;
    public bool IsMouseRightClick { get; private set; } = false;

    // Ground Check
    const float GroundCheckTime = 0.5f;
    float groundCheckTime = 0f;

    // Components
    Camera cam = null;
    Rigidbody rigid = null;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = Camera.main;

        this.BlockHighlight = Instantiate(BlockHighlightPrefab, WorldManager.Instance.ChunkManager.ChunkTransform);
    }

    private void Update()
    {
        Keyboard();
        CameraRotation();
        MouseClick();

        QuickSlot();

        CheckGround();
    }

    private void LateUpdate()
    {
        UpdateLookAtBlockPosition();
        if (LookAtBlockPos.IsNull == false)
        {
            BlockHighlight.SetPosition(LookAtBlockPos);
            BlockHighlight.SetActive(true);
        }
        else
        {
            BlockHighlight.SetActive(false);
        }
    }

    private void Keyboard()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 move = transform.forward * vertical + transform.right * horizontal;
        move.Normalize();

        move *= Time.deltaTime * PlayerSpeed;
        transform.position += move;
        //rigid.MovePosition(rigid.position + move);
        //rigid.position += move;

        if (IsJump == false)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                IsJump = true;
                rigid.AddForce(new Vector3(0, PlayerJump, 0));

                jumpCooltime = PlayerJumpCooltime;
                groundCheckTime = 0f;
            }
        }
        else
        {
            if (jumpCooltime < 0)
            {
                if (IsPlayerOnGround())
                {
                    IsJump = false;
                }
            }
            else
            {
                jumpCooltime -= Time.deltaTime;
            }
        }
    }

    private void CameraRotation()
    {
        float yRotation = Input.GetAxisRaw("Mouse X");
        cameraRotationY += yRotation * cameraSensitivity;
        transform.localEulerAngles = new Vector3(0f, cameraRotationY, 0f);

        float xRotation = Input.GetAxisRaw("Mouse Y");
        cameraRotationX += xRotation * cameraSensitivity;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -CameraRotationLimit, CameraRotationLimit);
        cam.transform.localEulerAngles = new Vector3(-cameraRotationX, 0f, 0f);
    }

    private void MouseClick()
    {
        float fire1 = Input.GetAxis("Fire1");
        if (fire1 > 0)
        {
            // Mouse Down
            if (IsMouseLeftClick == false)
            {
                if (BlockHighlight.IsEnabled)
                {
                    ItemType itemType = WorldManager.Instance.ChunkManager.RemoveBlock(BlockHighlight.BlockPos);
                    Inventory.Instance.AddItem(itemType, 1);
                }
            }

            IsMouseLeftClick = true;
        }
        else
        {
            // Mouse Up
            if (IsMouseLeftClick == true)
            {

            }

            IsMouseLeftClick = false;
        }

        float fire2 = Input.GetAxis("Fire2");
        if (fire2 > 0)
        {
            // Mouse Down
            if (IsMouseRightClick == false)
            {
                QuickSlotManager.Instance.UseCurrentItem();
            }

            IsMouseRightClick = true;
        }
        else
        {
            // Mouse Up
            if (IsMouseRightClick == true)
            {

            }

            IsMouseRightClick = false;
        }
    }

    private void QuickSlot()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(7);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(8);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            QuickSlotManager.Instance.SetQuickSlotIndex(9);
        }
    }

    private void CheckGround()
    {
        if (IsJump == false)
        {
            return;
        }

        if (GroundCheckTime < groundCheckTime)
        {
            groundCheckTime = 0f;
            if (WorldManager.Instance.IsBlockExistsAtBottom(transform.position) == false)
            {
                WorldManager.Instance.SetPlayerPosition(transform.position);
            }
        }
        groundCheckTime += Time.deltaTime;
    }

    private bool IsPlayerOnGround()
    {
        Vector3 halfExtents = (new Vector3(PlayerWidth, PlayerHeight, PlayerWidth)) * 0.5f;
        int layerMask = 1 << WorldManager.Instance.ChunkManager.BlockLayerMask;
        if (Physics.BoxCast(transform.position, halfExtents, Vector3.down, out var hit, Quaternion.identity, GroundDetectionLength, layerMask))
        {
            return true;
        }
        return false;
    }


    private void UpdateLookAtBlockPosition()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        int layerMask = 1 << WorldManager.Instance.ChunkManager.BlockLayerMask;
        if (Physics.Raycast(ray, out var hit, PlayerInteractLengthFactor, layerMask))
        {
            Vector3 blockPosition = hit.point;
            blockPosition -= hit.normal * ChunkManager.BlockLength * 0.5f;

            ChunkManager.GetBlockPosFromWorldPosition(blockPosition, ref LookAtBlockPos);
            LookAtBlockPos.IsNull = false;
        }
        else
        {
            LookAtBlockPos.IsNull = true;
        }
    }
    public BlockPos GetLookAtBlockPosition()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        int layerMask = 1 << WorldManager.Instance.ChunkManager.BlockLayerMask;
        if (Physics.Raycast(ray, out var hit, PlayerInteractLengthFactor, layerMask))
        {
            Vector3 blockPosition = hit.point;
            blockPosition -= hit.normal * ChunkManager.BlockLength * 0.5f;

            return ChunkManager.GetBlockPosFromWorldPosition(blockPosition);
        }
        return BlockPos.GetNull();
    }

    public BlockPos GetLookAtOppsiteBlockPosition()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        int layerMask = 1 << WorldManager.Instance.ChunkManager.BlockLayerMask;
        if (Physics.Raycast(ray, out var hit, PlayerInteractLengthFactor, layerMask))
        {
            Vector3 blockPosition = hit.point;
            blockPosition += hit.normal * ChunkManager.BlockLength * 0.5f;

            return ChunkManager.GetBlockPosFromWorldPosition(blockPosition);
        }
        return BlockPos.GetNull();
    }
}

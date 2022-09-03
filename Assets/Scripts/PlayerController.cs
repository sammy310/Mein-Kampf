using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
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

    // Click
    public bool IsMouseClick { get; private set; } = false;

    // Ground Check
    const float GroundCheckTime = 1f;
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

        CheckGround();
    }

    private void LateUpdate()
    {
        var blockPos = GetLookAtBlockBlockPosition();
        if (blockPos.IsNull == false)
        {
            BlockHighlight.SetPosition(blockPos);
            BlockHighlight.gameObject.SetActive(true);
        }
        else
        {
            BlockHighlight.gameObject.SetActive(false);
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
            if (IsMouseClick == false)
            {
                WorldManager.Instance.ChunkManager.RemoveBlock(BlockHighlight.BlockPos);
            }

            IsMouseClick = true;
        }
        else
        {
            // Mouse Up
            if (IsMouseClick == true)
            {

            }

            IsMouseClick = false;
        }
    }

    private void CheckGround()
    {
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
        Ray ray = new Ray(GroundDetectionTransform.position, Vector3.down);
        int layerMask = 1 << WorldManager.Instance.ChunkManager.BlockLayerMask;
        if (Physics.Raycast(ray, out var hit, GroundDetectionLength, layerMask))
        {
            return true;
        }
        return false;
    }


    public BlockPos GetLookAtBlockBlockPosition()
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
}

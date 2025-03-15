using System.Collections.Generic;
using UnityEngine;

public struct GameplayInput
{
    public Vector2 LookRotation;
    public Vector2 MoveDirection;
    public bool Jump;
    public bool Sprint;
}

public class SimplePlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cameraTransform;
    public Animator Animator;
    public Transform CameraPivot;
    public Transform CameraHandle;
    public Transform ScalingRoot;
    public ParticleSystem DustParticles;

    [Header("Movement Setup")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 8f;
    public float gravity = 15f;
    public float rotationSpeed = 8f;
    public float InitialLookRotation = 18f;

    [Header("Sounds")]
    public AudioSource FootstepSound;
    public AudioSource AudioSource;
    public AudioClip JumpAudioClip;
    public AudioClip LandAudioClip;

    [Header("Respawn Settings")]
    [SerializeField] private float fallThreshold = -10f;  // Y-position at which player respawns
    [SerializeField] private List<Transform> respawnPlatforms; // Assign platforms in the Inspector
    public List<Transform> RespawnPlatforms { set { respawnPlatforms = value; } }

    private GameplayInput _input;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;

    // Animator Parameters
    private int _animIDSpeed;
    private int _animIDGrounded;

    void Start()
    {
        _input.LookRotation = new Vector2(InitialLookRotation, 0f);

        AssignAnimationIDs();
        wasGrounded = controller.isGrounded;
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        var lookRotationDelta = new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));
        _input.LookRotation = ClampLookRotation(_input.LookRotation + lookRotationDelta);

        var moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _input.MoveDirection = moveDirection.normalized;

        _input.Jump |= Input.GetButtonDown("Jump");
        _input.Sprint |= Input.GetButton("Sprint");

        ProcessMovement();
        HandleAnimations();

        // Check if the player has fallen below the threshold
        if (transform.position.y < fallThreshold)
        {
            RespawnPlayer();
        }
    }

    private void ProcessMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 moveDirection = new Vector3(_input.MoveDirection.x, 0, _input.MoveDirection.y);
        moveDirection = CameraPivot.transform.TransformDirection(moveDirection);
        moveDirection.y = 0;
        moveDirection.Normalize();

        float speed = _input.Sprint ? sprintSpeed : walkSpeed;
        moveDirection *= speed;

        if (_input.Jump && isGrounded)
        {
            velocity.y = jumpForce;
            PlayJumpSound();
        }

        velocity.y -= gravity * Time.deltaTime;

        if (moveDirection.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        velocity.x = moveDirection.x;
        velocity.z = moveDirection.z;
        controller.Move(velocity * Time.deltaTime);

        HandleLandingSound();
        _input.Jump = false;
    }

    private void RespawnPlayer()
    {
        if (respawnPlatforms.Count == 0)
        {
            Debug.LogError("No respawn platforms assigned!");
            return;
        }

        Transform respawnPlatform = respawnPlatforms[Random.Range(0, respawnPlatforms.Count)];
        Vector3 respawnPosition = respawnPlatform.position + Vector3.up * 2f; // Slightly above platform

        controller.enabled = false; // Disable controller before teleporting
        transform.position = respawnPosition;
        controller.enabled = true;  // Re-enable after teleporting

        velocity = Vector3.zero; // Reset velocity
    }

    private void HandleAnimations()
    {
        Animator.SetFloat(_animIDSpeed, new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude);
        Animator.SetBool(_animIDGrounded, controller.isGrounded);

        FootstepSound.enabled = controller.isGrounded && controller.velocity.magnitude > 1f;
        FootstepSound.pitch = controller.velocity.magnitude > sprintSpeed - 1 ? 1.5f : 1f;

        ScalingRoot.localScale = Vector3.Lerp(ScalingRoot.localScale, Vector3.one, Time.deltaTime * 8f);
        var emission = DustParticles.emission;
        emission.enabled = controller.isGrounded && controller.velocity.magnitude > 1f;
    }

    private void HandleLandingSound()
    {
        if (!wasGrounded && isGrounded)
        {
            PlayLandSound();
        }
        wasGrounded = isGrounded;
    }

    private void PlayJumpSound()
    {
        if (JumpAudioClip != null)
        {
            AudioSource.PlayOneShot(JumpAudioClip);
        }
    }

    private void PlayLandSound()
    {
        if (LandAudioClip != null)
        {
            AudioSource.PlayOneShot(LandAudioClip);
        }
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
    }

    private void LateUpdate()
    {
        CameraPivot.rotation = Quaternion.Euler(_input.LookRotation);
        Camera.main.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
    }

    private Vector2 ClampLookRotation(Vector2 lookRotation)
    {
        lookRotation.x = Mathf.Clamp(lookRotation.x, -30f, 70f);
        return lookRotation;
    }
}

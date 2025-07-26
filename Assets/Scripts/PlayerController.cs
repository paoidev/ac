using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private CharacterController characterController;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private AudioSource footstepSound;
    [SerializeField] public Camera mainCamera;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float sprintTransitSpeed = 5f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;

    // store values
    private float verticalVelocity;
    private float currentSpeed;
    private float currentSpeedMultiplier;

    [Header("Footstep Settings")]
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private float stepInterval = 0.5f;

    [Header("Camera Bob")]
    [SerializeField] private float bobFrequency = 1f; // how fast you want the shake to occur
    [SerializeField] private float bobAmplitude = 1f; // how strong the shake should be

    private CinemachineBasicMultiChannelPerlin noiseComponent;

    private float nextStepTimer = 0f;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;

    [Header("SFX")]
    [SerializeField] private AudioClip[] groundFootsteps;
    [SerializeField] private AudioClip[] floorFootsteps;
    [SerializeField] private AudioClip[] woodFootsteps;
    [SerializeField] private AudioClip[] carpetFootsteps;

    [Header("Look Settings")]
    private float xRotation;
    [SerializeField] private float mouseSensitivity = 10;

    [Header("Input")]
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool interactInput;

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpInput = true;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            sprintInput = true;
        } else
        {
            sprintInput = false;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactInput = true;
        }
        else { interactInput = false; }
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        Cursor.lockState = CursorLockMode.Locked;
    }


    private void Update()
    {
        Movement();
        InteractionRay();
    }

    private void LateUpdate()
    {
        CameraBob();
    }

    private void Movement()
    {
        GroundMovement();
        CameraLook();
        PlayFootstepSound();
    }

    private void GroundMovement()
    {
        // Transform movement relative to player's facing direction
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        SprintMovement();

        move *= currentSpeed;
        move.y = VerticalForceCalculation(); // the gravity method

        characterController.Move(move * Time.deltaTime);

    }

    private float SprintMovement()
    {
        if (sprintInput)
        {
            currentSpeedMultiplier = sprintSpeedMultiplier;
        } else
        {
            currentSpeedMultiplier = 1f;
        }
        currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed * currentSpeedMultiplier, sprintTransitSpeed * Time.deltaTime);

        return currentSpeed;
    }

    private void CameraLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // Rotate the player body left and right
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Rotate the camera up and down
        transform.Rotate(Vector3.up * mouseX);
    }

    private void CameraBob()
    {
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            noiseComponent.m_FrequencyGain = bobFrequency * currentSpeedMultiplier;
            noiseComponent.m_AmplitudeGain = bobAmplitude * currentSpeedMultiplier;
        }
        else
        {
            noiseComponent.m_AmplitudeGain = 0.0f;
            noiseComponent.m_FrequencyGain = 0.0f;
        }
    }

    private void PlayFootstepSound()
    {
        if (!characterController.isGrounded || characterController.velocity.magnitude <= 0.1f)
        {
            return;
        }

        if (Time.time >= nextStepTimer)
        {
            AudioClip[] footstepClips = DetermineAudioClips();

            if (footstepClips.Length > 0)
            {
                AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

                footstepSound.PlayOneShot(clip);
            }
            nextStepTimer = Time.time + (stepInterval / currentSpeedMultiplier);
        }
    }

    private AudioClip[] DetermineAudioClips()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, 5f, terrainLayerMask))
        {
            string tag = hit.collider.tag;
            switch (tag)
            {
                case "Ground":
                    return groundFootsteps;
                case "Floor":
                    return floorFootsteps;
                case "Wood":
                    return woodFootsteps;
                case "Carpet":
                    return carpetFootsteps;
                default:
                    return groundFootsteps;
            }
        }
        return groundFootsteps;

    }

    private float VerticalForceCalculation()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;

            if(jumpInput)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * 2 * gravity);
                jumpInput = false;

            }
        } else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        return verticalVelocity;
    }

    public void InteractionRay()
    {
        Ray ray = mainCamera.ViewportPointToRay(Vector2.one / 2f);
        RaycastHit hit;

        bool hitSomething = false;

        // Draw a visible ray in Scene View for debugging
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                hitSomething = true;
                interactionText.text = interactable.GetDescription();

                if (interactInput)
                {
                    interactable.Interact();
                    interactInput = false;
                }
            }
        }

        interactionUI.SetActive(hitSomething);

    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }


}

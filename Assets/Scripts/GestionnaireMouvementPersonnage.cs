using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Characters.FirstPerson;
using Random = UnityEngine.Random;
using Fusion;
using Fusion.Sockets;
using System;
/*
 * Script qui exécute les déplacements du joueur et ainsi que l'ajustement de direction
 * Dérive de NetworkBehaviour. Utilisation de la fonction réseau FixedUpdateNetwork()
 */
public class GestionnaireMouvementPersonnage : NetworkBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject cameraObject;
    [SerializeField] private GameObject gunObject;
    [SerializeField] private GameObject playerObject;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private float stickToGroundForce = 5f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private bool isWalking;
    [SerializeField] [Range(0f, 1f)] private float runstepLengthen = 1f;

    [Header("Mouse Look Settings")]
    [SerializeField] private MouseLook mouseLook;

    [Header("Head Bob Settings")]
    [SerializeField] private bool useHeadBob;
    [SerializeField] private CurveControlledBob headBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob jumpBob = new LerpControlledBob();
    [SerializeField] private float stepInterval = 5f;

    [Header("FOV Kick Settings")]
    [SerializeField] private bool useFovKick;
    [SerializeField] private FOVKick fovKick = new FOVKick();

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;

    private AudioSource audioSource;
    private CharacterController characterController;
    private CollisionFlags collisionFlags;
    private Vector3 moveDirection = Vector3.zero;
    private bool previouslyGrounded = true;
    private bool isJumping = false;
    private float stepCycle = 0f;
    private float nextStep = 0f;
    private Vector3 originalCameraPosition;

    [Networked] private Vector3 Position { get; set; }
    [Networked] private Quaternion Rotation { get; set; }

    void Awake() {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Spawned() {
        if (Object.HasInputAuthority) {
            cameraObject.SetActive(true);
            SetLayerRecursively(gunObject, LayerMask.NameToLayer("Hidden"));
            SetLayerRecursively(playerObject, LayerMask.NameToLayer("Hidden"));
            mouseLook.Init(transform, cameraObject.transform);
            originalCameraPosition = cameraObject.transform.localPosition;

            if (useFovKick) {
                Camera mainCamera = cameraObject.GetComponent<Camera>();
                fovKick.Setup(mainCamera);
            }
            headBob.Setup(cameraObject.GetComponent<Camera>(), stepInterval);
        } else {
            Position = transform.position;
            Rotation = transform.rotation;
        }
    }

    public override void FixedUpdateNetwork() {
        if (Object.HasInputAuthority) {
            HandleInput();
        } else {
            SmoothMovement();
        }
    }

    private void HandleInput() {
        mouseLook.LookRotation(transform, cameraObject.transform);

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 desiredMove = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
            characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
        }

        if (characterController.isGrounded) {
            moveDirection.y = -stickToGroundForce;

            if (Input.GetButtonDown("Jump")) {
                moveDirection.y = jumpSpeed;
                PlayJumpSound();
                isJumping = true;
            }
        } else {
            moveDirection += Physics.gravity * gravityMultiplier * Runner.DeltaTime;
        }

        moveDirection.x = desiredMove.x * speed;
        moveDirection.z = desiredMove.z * speed;

        collisionFlags = characterController.Move(moveDirection * Runner.DeltaTime);

        HandleFovKick(Input.GetKey(KeyCode.LeftShift));
        ProgressStepCycle(speed, Input.GetKey(KeyCode.LeftShift));
        UpdateCameraPosition(speed);

        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetBool("Running", Input.GetKey(KeyCode.LeftShift));
        if (Input.GetButtonDown("Jump")) animator.SetTrigger("IsJumping");

        Position = transform.position;
        Rotation = transform.rotation;

        if (!previouslyGrounded && characterController.isGrounded) {
            PlayLandingSound();
            isJumping = false;
        }
        previouslyGrounded = characterController.isGrounded;

        mouseLook.UpdateCursorLock();
    }

    private void SmoothMovement() {
        transform.position = Vector3.Lerp(transform.position, Position, Runner.DeltaTime * 10f);
        transform.rotation = Quaternion.Lerp(transform.rotation, Rotation, Runner.DeltaTime * 10f);
    }

    private void HandleFovKick(bool isRunning) {
        if (!useFovKick) return;

        if (isRunning && isWalking) {
            StopAllCoroutines();
            StartCoroutine(fovKick.FOVKickUp());
        } else if (!isRunning && !isWalking) {
            StopAllCoroutines();
            StartCoroutine(fovKick.FOVKickDown());
        }

        isWalking = !isRunning;
    }

    private void ProgressStepCycle(float speed, bool isRunning) {
        float lengthenFactor = isRunning ? runstepLengthen : 1f;

        if (characterController.velocity.sqrMagnitude > 0 && characterController.isGrounded) {
            stepCycle += (characterController.velocity.magnitude + (speed * lengthenFactor)) * Runner.DeltaTime;
        }

        if (!(stepCycle > nextStep)) return;

        nextStep = stepCycle + stepInterval;
        PlayFootstepAudio();
    }

    private void UpdateCameraPosition(float speed) {
        if (!useHeadBob) return;

        Vector3 newCameraPosition;
        if (characterController.velocity.magnitude > 0 && characterController.isGrounded) {
            cameraObject.transform.localPosition = headBob.DoHeadBob(characterController.velocity.magnitude + speed);
            newCameraPosition = cameraObject.transform.localPosition;
            newCameraPosition.y -= jumpBob.Offset();
        } else {
            newCameraPosition = originalCameraPosition;
            newCameraPosition.y -= jumpBob.Offset();
        }
        cameraObject.transform.localPosition = newCameraPosition;
    }

    private void PlayJumpSound() {
        audioSource.clip = jumpSound;
        audioSource.Play();
    }

    private void PlayLandingSound() {
        audioSource.clip = landSound;
        audioSource.Play();
    }

    private void PlayFootstepAudio() {
        if (!characterController.isGrounded || footstepSounds.Length == 0) return;

        int n = Random.Range(1, footstepSounds.Length);
        audioSource.clip = footstepSounds[n];
        audioSource.PlayOneShot(audioSource.clip);

        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = audioSource.clip;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer) {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
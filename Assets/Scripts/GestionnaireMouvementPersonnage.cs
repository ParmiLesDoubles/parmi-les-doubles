using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Fusion;
using Fusion.Sockets;

[RequireComponent(typeof(FirstPersonController))]
public class GestionnaireMouvementPersonnage : NetworkBehaviour {
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private GameObject cameraObject;
    [SerializeField]
    private GameObject gunObject;
    [SerializeField]
    private GameObject playerObject;

    [Networked]
    private Vector3 Position { get; set; }
    [Networked]
    private Quaternion Rotation { get; set; }
    private bool isJumping;

    private FirstPersonController firstPersonController;

    public override void Spawned() {
        firstPersonController = GetComponent<FirstPersonController>();

        if (Object.HasInputAuthority) {
            // Enable camera and controls only for the local player
            cameraObject.SetActive(true);
            firstPersonController.enabled = true;

            // Optionally hide objects (e.g., gun and player model)
            SetLayerRecursively(gunObject, LayerMask.NameToLayer("Hidden"));
            SetLayerRecursively(playerObject, LayerMask.NameToLayer("Hidden"));
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
        // Sync inputs with the animator
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        if (Input.GetButtonDown("Jump")) {
            animator.SetTrigger("IsJumping");
        }
        animator.SetBool("Running", Input.GetKey(KeyCode.LeftShift));

        // Update networked state
        Position = transform.position;
        Rotation = transform.rotation;
    }

    private void SmoothMovement() {
        // Smoothly interpolate position and rotation for remote players
        transform.position = Vector3.Lerp(transform.position, Position, Runner.DeltaTime * 10f);
        transform.rotation = Quaternion.Lerp(transform.rotation, Rotation, Runner.DeltaTime * 10f);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer) {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
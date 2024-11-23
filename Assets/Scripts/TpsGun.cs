using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TpsGun : NetworkBehaviour {

    [Tooltip("The scaling number for changing the local position Y of TpsGun when aiming angle changes.")]
    [SerializeField]
    private float localPositionYScale = 0.007f;
    [SerializeField]
    private ParticleSystem gunParticles;
    [SerializeField]
    private AudioSource gunAudio;
    [SerializeField]
    private FpsGun fpsGun;
    [SerializeField]
    private Animator animator;

    private float timer;
    private Vector3 localPosition;
    private Quaternion localRotation;
    private float smoothing = 2.0f;
    private float defaultLocalPositionY;

    void Start() {
        if (Object.HasInputAuthority) {
            defaultLocalPositionY = transform.localPosition.y;
        } else {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
        }
    }

    void Update() {
        if (Object.HasInputAuthority) {
            transform.rotation = fpsGun.transform.rotation;
        }
    }

    void LateUpdate() {
        if (Object.HasInputAuthority) {
            float deltaEulerAngle = 0f;
            if (transform.eulerAngles.x > 180) {
                deltaEulerAngle = 360 - transform.eulerAngles.x;
            } else {
                deltaEulerAngle = -transform.eulerAngles.x;
            }
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                defaultLocalPositionY + deltaEulerAngle * localPositionYScale,
                transform.localPosition.z
            );
        } else {
            transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, Time.deltaTime * smoothing);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, localRotation, Time.deltaTime * smoothing);
        }
    }

    public void RPCShoot() {
        if (HasInputAuthority) {
            // Call an RPC on all clients
            RPC_Shoot();
        }
    }

    [Rpc]
    void RPC_Shoot() {
        gunAudio.Play();
        if (!Object.HasInputAuthority) {
            if (gunParticles.isPlaying) {
                gunParticles.Stop();
            }
            gunParticles.Play();
        }
    }
}
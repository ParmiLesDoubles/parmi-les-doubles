using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class FpsGun : NetworkBehaviour {
    [SerializeField]
    private int damagePerShot = 20;
    [SerializeField]
    private float timeBetweenBullets = 0.2f;
    [SerializeField]
    private float weaponRange = 100.0f;
    [SerializeField]
    private TpsGun tpsGun;
    [SerializeField]
    private ParticleSystem gunParticles;
    [SerializeField]
    private LineRenderer gunLine;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Camera raycastCamera;

    private float timer;

    // Prefabs for different impact effects
    [SerializeField] private GameObject impactFleshPrefab;
    [SerializeField] private GameObject impactMetalPrefab;
    [SerializeField] private GameObject impactWoodPrefab;
    [SerializeField] private GameObject impactConcretePrefab;
    [SerializeField] private GameObject impactWaterPrefab;
    [SerializeField] private GameObject impactBrickPrefab;
    [SerializeField] private GameObject impactGlassPrefab;
    [SerializeField] private GameObject impactDirtPrefab;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        timer = 0.0f;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update() {
        timer += Time.deltaTime;
        bool shooting = Input.GetButtonDown("Fire1");
        if (shooting && timer >= timeBetweenBullets && Time.timeScale != 0) {
            Shoot();
        }
        animator.SetBool("Firing", shooting);
    }

    /// <summary>
    /// Shoot once, this also calls RPCShoot for third person view gun.
    /// <summary>
    void Shoot() {
        timer = 0.0f;
        gunLine.enabled = true;
        StartCoroutine(DisableShootingEffect());
        if (gunParticles.isPlaying) {
            gunParticles.Stop();
        }
        gunParticles.Play();
        // Ray casting for shooting hit detection.
        RaycastHit shootHit;
        Ray shootRay = raycastCamera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0f));
        if (Physics.Raycast(shootRay, out shootHit, weaponRange, LayerMask.GetMask("Shootable"))) {
            string hitTag = shootHit.transform.gameObject.tag;
            GameObject impactPrefab;

        // Use a switch statement to select the correct prefab based on the hitTag
        switch (hitTag)
        {
            case "Player":
                impactPrefab = impactFleshPrefab;
                break;
            case "Metal":
                impactPrefab = impactMetalPrefab;
                break;
            case "Wood":
                impactPrefab = impactWoodPrefab;
                break;
            case "Concrete":
                impactPrefab = impactConcretePrefab;
                break;
            case "Water":
                impactPrefab = impactWaterPrefab;
                break;
            case "Brick":
                impactPrefab = impactBrickPrefab;
                break;
            case "Glass":
                impactPrefab = impactGlassPrefab;
                break;
            case "Dirt":
                impactPrefab = impactDirtPrefab;
                break;
            default:
                impactPrefab = impactDirtPrefab;
                break;
        }

        // Spawn the selected prefab using Runner.Spawn
        Runner.Spawn(impactPrefab, shootHit.point, Quaternion.Euler(shootHit.normal.x - 90, shootHit.normal.y, shootHit.normal.z));
        }
        tpsGun.RPCShoot();  // RPC for third person view
    }


    /// <summary>
    /// Coroutine function to disable shooting effect.
    /// <summary>
    public IEnumerator DisableShootingEffect() {
        yield return new WaitForSeconds(0.05f);
        gunLine.enabled = false;
    }
}
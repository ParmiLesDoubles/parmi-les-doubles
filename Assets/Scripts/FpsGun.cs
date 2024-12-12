using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Script qui contrôle l'arme en vue à la première personne.
// C'est principalement pour le tir du joueur.
// PhotonView = NetworkObject
// PhotonNetwork = SimulationBehaviour.Runner et SimulationBehaviour.Object
// PhotonNetwork.Instantiate() = Runner.Spawn()

public class FpsGun : MonoBehaviour {
    // Quantité de dommages par tir
    [SerializeField]
    private int dommagesParTir = 20;

    // Délai entre les tirs
    [SerializeField]
    private float delaiEntreTirs = 0.2f;

    // Portée de tir pour l'arme du joueur
    [SerializeField]
    private float weaponRange = 100.0f;

    // Contient la référence au script TpsGun de l'arme
    // en vue à la troisième personne
    [SerializeField]
    private TpsGun tpsGun;

    // Système de particules pour les particules de l'arme du joueur
    [SerializeField]
    private ParticleSystem gunParticles;

    // Line Renderer pour l'arme du joueur
    [SerializeField]
    private LineRenderer gunLine;

    // Animator de l'arme du joueur
    [SerializeField]
    private Animator animator;

    // Caméra à partir de laquelle le rayon passe par un point de l'écran
    [SerializeField]
    private Camera raycastCamera;

    // Timer pour chaque tir
    private float timer;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() {
        timer = 0.0f;
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() {
        timer += Time.deltaTime;
        bool shooting = Input.GetButton("Fire1");
        if (shooting && timer >= delaiEntreTirs && Time.timeScale != 0) {
            Shoot();
        }
        animator.SetBool("Firing", shooting);
    }

    /// <summary>
    /// Tirer une fois.
    /// Cela appelle également RPCShoot pour l'arme en vue à la troisième personne.
    /// </summary>
    void Shoot() {
        // On redémarre le timer, active le Line Renderer, désactive l'effet
        // du dernier tir et fait jouer les particules.
        timer = 0.0f;
        gunLine.enabled = true;
        StartCoroutine(DesactiverEffetTir());
        if (gunParticles.isPlaying) {
            gunParticles.Stop();
        }
        gunParticles.Play();
        // Raycasting pour la détection des tirs.
        RaycastHit shootHit;
        Ray shootRay = raycastCamera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0f));
        if (Physics.Raycast(shootRay, out shootHit, weaponRange, LayerMask.GetMask("Shootable"))) {
            string hitTag = shootHit.transform.gameObject.tag;
            switch (hitTag) {
                // Si un tir touche un GameObject avec le tag Player, on fait appel
                // à la RPC qui inflige des dommages et instantiate l'effet de sang.
                case "Player":
                    shootHit.collider.GetComponent<PhotonView>().RPC("PrendreDommages", RpcTarget.All, dommagesParTir, PhotonNetwork.LocalPlayer.NickName);
                    PhotonNetwork.Instantiate("ImpactFlesh", shootHit.point, Quaternion.Euler(shootHit.normal.x - 90, shootHit.normal.y, shootHit.normal.z), 0);
                    break;
                // Si un tir touche un GameObject, on instantiate
                // l'effet souhaité en fonction du tag du GameObject touché.
                default:
                    PhotonNetwork.Instantiate("Impact" + hitTag, shootHit.point, Quaternion.Euler(shootHit.normal.x - 90, shootHit.normal.y, shootHit.normal.z), 0);
                    break;
            }
        }
        // RPC pour la vue à la troisième personne.
        tpsGun.RPCShoot();
    }

    /// <summary>
    /// Fonction Coroutine pour désactiver l'effet de tir.
    /// </summary>
    public IEnumerator DesactiverEffetTir() {
        yield return new WaitForSeconds(0.05f);
        gunLine.enabled = false;
    }
}
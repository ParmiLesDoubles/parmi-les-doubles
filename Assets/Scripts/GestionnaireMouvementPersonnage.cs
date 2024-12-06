using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(FirstPersonController))]

// Script qui exécute les déplacements du joueur.
// Le script FirstPersonController est un script qui fait partie des Standard
// Assets d'Unity.
// PhotonView = NetworkObject
// photonView.IsMine = Object.HasInputAuthority / Object.HasStateAuthority

public class GestionnaireMouvementPersonnage : MonoBehaviourPunCallbacks, IPunObservable {
    // Animator du joueur
    [SerializeField]
    private Animator animator;

    // Contient la référence à la caméra du joueur
    [SerializeField]
    private GameObject cameraObject;

    // Contient la référence à l'arme en vue à la troisième personne
    [SerializeField]
    private GameObject gunObject;

    // Contient la référence au visuel du joueur
    [SerializeField]
    private GameObject playerObject;

    // Contient la référence au script NameTag qui se trouve dans CanvasNom
    [SerializeField]
    private NameTag nameTag;

    // Position actuelle du joueur
    private Vector3 position;

    // Rotation actuelle du joueur
    private Quaternion rotation;

    // Pour lisser les changements de position et de rotation
    private float smoothing = 10.0f;

    /// <summary>
    /// Déplacer les GameObjects vers un autre layer.
    /// </summary>
    void MoveToLayer(GameObject gameObject, int layer) {
        gameObject.layer = layer;
        foreach(Transform child in gameObject.transform) {
            MoveToLayer(child.gameObject, layer);
        }
    }

    /// <summary>
    /// La fonction Awake est appelée lorsque le script est en cours de chargement.
    /// </summary>
    void Awake() {
        // Le script FirstPersonController exige que cameraObject soit actif dans sa fonction Start.
        if (photonView.IsMine) {
            cameraObject.SetActive(true);
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() {
        if (photonView.IsMine) {
            // On active le script FirstPersonController
            // et cache l'arme en vue à la troisième personne et le visuel du joueur.
            GetComponent<FirstPersonController>().enabled = true;
            MoveToLayer(gunObject, LayerMask.NameToLayer("Hidden"));
            MoveToLayer(playerObject, LayerMask.NameToLayer("Hidden"));
            // Fixer la cible du name tag de l'autre joueur
            // au transform du name tag de ce joueur.
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players) {
                player.GetComponentInChildren<NameTag>().cible = nameTag.transform;
            }
        } else {
            position = transform.position;
            rotation = transform.rotation;
            // Fixer la cible du name tag de ce joueur
            // à la cible des autres joueurs.
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players) {
                if (player != gameObject) {
                    nameTag.cible = player.GetComponentInChildren<NameTag>().cible;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() {
        if (!photonView.IsMine) {
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * smoothing);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * smoothing);
        }
    }

    /// <summary>
    /// La fonction FixedUpdate est appelée à chaque image à fréquence fixe.
    /// </summary>
    void FixedUpdate() {
        if (photonView.IsMine) {
            animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
            animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
            if (Input.GetButtonDown("Jump")) {
                animator.SetTrigger("IsJumping");
            }
            animator.SetBool("Running", Input.GetKey(KeyCode.LeftShift));
        }
    }

    /// <summary>
    /// Permet de personnaliser la synchronisation des variables
    /// dans un script surveillé par un Photon Network View.
    /// </summary>
    /// <param name="stream">Le flux binaire du réseau.</param>
    /// <param name="info">Les informations sur le message réseau.</param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        } else {
            position = (Vector3)stream.ReceiveNext();
            rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
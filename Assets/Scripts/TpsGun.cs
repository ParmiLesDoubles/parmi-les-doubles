using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Script qui contrôle l'arme en vue à la troisième personne.
// L'arme va être répliqué sur le réseau.
// C'est principalement pour le transform et les particules.
// MonoBehaviourPunCallbacks = SimulationBehaviour et NetworkBehaviour
// IPunObservable.OnPhotonSerializeView() = Networked Properties
// PhotonView = NetworkObject
// photonView.IsMine = Object.HasInputAuthority et Object.HasStateAuthority

public class TpsGun : MonoBehaviourPunCallbacks, IPunObservable {
    // Pour modifier la position locale Y de TpsGun en cas de changement d'angle de visée
    [SerializeField]
    private float localPositionYScale = 0.007f;

    // Système de particules pour les particules de l'arme du joueur
    [SerializeField]
    private ParticleSystem gunParticles;

    // Son d'un coup de feu
    [SerializeField]
    private AudioSource gunAudio;

    // Contient la référence au script FpsGun de l'arme
    // en vue à la première personne
    [SerializeField]
    private FpsGun fpsGun;

    // Animator de l'arme du joueur
    [SerializeField]
    private Animator animator;

    // Timer pour chaque tir
    private float timer;

    // Position locale de l'arme du joueur
    private Vector3 localPosition;

    // Rotation locale de l'arme du joueur
    private Quaternion localRotation;

    // Pour lisser les changements de position et de rotation
    private float smoothing = 2.0f;

    // Position locale Y par défaut de l'arme du joueur
    private float defaultLocalPositionY;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() {
        if (photonView.IsMine) {
            defaultLocalPositionY = transform.localPosition.y;
        } else {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() {
        if (photonView.IsMine) {
            transform.rotation = fpsGun.transform.rotation;
        }
    }

    /// <summary>
    /// La fonction LateUpdate est appelée après que
    /// toutes les fonctions Update ont été appelées.
    /// </summary>
    void LateUpdate() {
        if (photonView.IsMine) {
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

    /// <summary>
    /// Fonction publique pour appeler la fonction RPC Shoot.
    /// </summary>
    public void ShootRPC() {
        if (photonView.IsMine) {
            photonView.RPC("Shoot", RpcTarget.All);
        }
    }

    /// <summary>
    /// Fonction RPC pour tirer une fois.
    /// </summary>
    [PunRPC]
    void Shoot() {
        gunAudio.Play();
        if (!photonView.IsMine) {
            if (gunParticles.isPlaying) {
                gunParticles.Stop();
            }
            gunParticles.Play();
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
            stream.SendNext(transform.localPosition);
            stream.SendNext(transform.localRotation);
        } else {
            localPosition = (Vector3)stream.ReceiveNext();
            localRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
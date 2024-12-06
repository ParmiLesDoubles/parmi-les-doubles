using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(Rigidbody))]

// Script qui gère les dommages et la mort quand un joueur est touché.
// Les delegates sont essentiellement des conteneurs pour les fonctions.
// Ils permettent de stocker et d'appeler une fonction
// comme s'il s'agissait d'une variable.
// Dans Unity, un event est un type particulier de delegate
// que vous pouvez utiliser pour déclencher des actions dans votre code.
// PhotonView = NetworkObject
// photonView.IsMine = Object.HasInputAuthority et Object.HasStateAuthority
// PhotonNetwork = SimulationBehaviour.Runner et SimulationBehaviour.Object

public class GestionnairePointsDeVie : MonoBehaviourPunCallbacks, IPunObservable {
    // Delegate pour la fonction Respawn qui est dans le script NetworkManager
    public delegate void Respawn(float temps);

    // Delegate pour la fonction AjouterMessage qui est dans le script NetworkManager
    public delegate void AjouterMessage(string Message);

    // Event pour la fonction Respawn qui est dans le script NetworkManager
    public event Respawn RespawnEvent;

    // Event pour la fonction AjouterMessage qui est dans le script NetworkManager
    public event AjouterMessage AjouterMessageEvent;

    // Nombre de points de vie au commencement ou après un respawn
    [SerializeField]
    private int ptsVieDepart = 100;

    // Vitesse d'immersion du cadavre d'un joueur mort
    [SerializeField]
    private float vitesseCouler = 0.08f;

    // Temps d'immersion du cadavre d'un joueur mort
    [SerializeField]
    private float tempsCouler = 2.5f;

    // Temps de respawn
    [SerializeField]
    private float tempsRespawn = 8.0f;

    // Son qui joue lorsqu'un joueur meurt
    [SerializeField]
    private AudioClip mortAudio;

    // Son qui joue lorsqu'un joueur subit des dommages
    [SerializeField]
    private AudioClip dommageAudio;
    [SerializeField]
    private AudioSource joueurAudio;
    [SerializeField]
    private float vitesseFlash = 2f;
    [SerializeField]
    private Color couleurFlash = new Color(1f, 0f, 0f, 0.1f);
    [SerializeField]
    private NameTag nameTag;
    [SerializeField]
    private Animator animator;

    private FirstPersonController firstPersonController;
    private IKControl ikControl;
    private Slider ptsVieSlider;
    private Image damageImage;
    private int ptsVie;
    private bool estMort;
    private bool coule;
    private bool dommage;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() {
        firstPersonController = GetComponent<FirstPersonController>();
        ikControl = GetComponentInChildren<IKControl>();
        damageImage = GameObject.FindGameObjectWithTag("Screen").transform.Find("DamageImage").GetComponent<Image>();
        ptsVieSlider = GameObject.FindGameObjectWithTag("Screen").GetComponentInChildren<Slider>();
        ptsVie = ptsVieDepart;
        if (photonView.IsMine) {
            gameObject.layer = LayerMask.NameToLayer("JoueurFPS");
            ptsVieSlider.value = ptsVie;
        }
        dommage = false;
        estMort = false;
        coule = false;
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() {
        if (dommage) {
            dommage = false;
            damageImage.color = couleurFlash;
        } else {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, vitesseFlash * Time.deltaTime);
        }
        if (coule) {
            transform.Translate(Vector3.down * vitesseCouler * Time.deltaTime);
        }
    }

    /// <summary>
    /// Fonction RPC permettant au joueur de subir des dommages.
    /// </summary>
    /// <param name="quantite">Quantité de dommages subis.</param>
    /// <param name="nomEnnemi">Nom de l'ennemi qui a causé la mort de ce joueur.</param>
    [PunRPC]
    public void PrendreDommages(int quantite, string nomEnnemi) {
        if (estMort) return;
        if (photonView.IsMine) {
            dommage = true;
            ptsVie -= quantite;
            if (ptsVie <= 0) {
                photonView.RPC("Mort", RpcTarget.All, nomEnnemi);
            }
            ptsVieSlider.value = ptsVie;
            animator.SetTrigger("IsHurt");
        }
        joueurAudio.clip = dommageAudio;
        joueurAudio.Play();
    }

    /// <summary>
    /// Fonction RPC permettant de déclarer la mort d'un joueur.
    /// </summary>
    /// <param name="nomEnnemi">Nom de l'ennemi qui a causé la mort d'un joueur.</param>
    [PunRPC]
    void Mort(string nomEnnemi) {
        estMort = true;
        ikControl.enabled = false;
        nameTag.gameObject.SetActive(false);
        if (photonView.IsMine) {
            firstPersonController.enabled = false;
            animator.SetTrigger("IsDead");
            AjouterMessageEvent(PhotonNetwork.LocalPlayer.NickName + " a été tué par " + nomEnnemi + " !");
            RespawnEvent(tempsRespawn);
            StartCoroutine("DestoryJoueur", tempsRespawn);
        }
        joueurAudio.clip = mortAudio;
        joueurAudio.Play();
        StartCoroutine("CommencerCouler", tempsCouler);
    }

    /// <summary>
    /// Enumarator pour détruire le joueur prefab
    /// </summary>
    /// <param name="delaiDestroy">Délai avant destroy</param>
    IEnumerator DestoryJoueur(float delaiDestroy) {
        yield return new WaitForSeconds(delaiDestroy);
        PhotonNetwork.Destroy(gameObject);
    }

    /// <summary>
    /// Enumarator permettant de commencer à couler le joueur prefab.
    /// </summary>
    /// <param name="delaiCouler">Délai avant de commencer à couler.</param>
    IEnumerator CommencerCouler(float delaiCouler) {
        yield return new WaitForSeconds(delaiCouler);
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = false;
        coule = true;
    }

    /// <summary>
    /// Permet de personnaliser la synchronisation des variables
    /// dans un script surveillé par un Photon Network View.
    /// </summary>
    /// <param name="stream">Le flux binaire du réseau.</param>
    /// <param name="info">Les informations sur le message réseau.</param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(ptsVie);
        } else {
            ptsVie = (int)stream.ReceiveNext();
        }
    }
}
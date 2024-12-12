using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

// Script qui affiche les noms des joueurs au-dessus de leurs têtes.
// MonoBehaviourPunCallbacks = SimulationBehaviour et NetworkBehaviour
// PhotonView = NetworkObject
// photonView.IsMine = Object.HasInputAuthority et Object.HasStateAuthority
// PhotonNetwork = SimulationBehaviour.Runner et SimulationBehaviour.Object

public class NameTag : MonoBehaviourPunCallbacks {
    // Où mettre le nom du joueur
    [HideInInspector]
    public Transform cible = null;

    // Nom du joueur
    [SerializeField]
    private Text texteNom;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() {
        if (photonView.IsMine) {
            photonView.RPC("MettreNom", RpcTarget.All, PhotonNetwork.NickName);
        } else {
            MettreNom(photonView.Owner.NickName);
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() {
        if (cible != null) {
            Vector3 lookAtVector = transform.position + (transform.position - cible.position);
            transform.LookAt(lookAtVector, Vector3.up);
        }
    }

    /// <summary>
    /// Fonction RPC pour définir le nom du joueur.
    /// </summary>
    [PunRPC]
    void MettreNom(string nom) {
        texteNom.text = nom;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

// Script qui contrôle l'ensemble de la connexion réseau.
// PhotonView = NetworkObject
// PhotonNetwork = SimulationBehaviour.Runner et SimulationBehaviour.Object

public class NetworkManager : MonoBehaviourPunCallbacks {
    // Texte de connexion
    [SerializeField]
    private Text texteConnexion;

    // Points de spawn où le joueur peut spawn de manière aléatoire
    [SerializeField]
    private Transform[] spawnPoints;

    // Caméra de la scène
    [SerializeField]
    private Camera sceneCamera;

    // Prefab du joueur
    [SerializeField]
    private GameObject joueurPrefab;

    // Fenêtre de connexion
    [SerializeField]
    private GameObject serverWindow;

    // Panneau de messages
    [SerializeField]
    private GameObject messageWindow;

    // Réticule de visée
    [SerializeField]
    private GameObject cibleTir;

    // InputField pour le nom du joueur
    [SerializeField]
    private InputField nomJoueur;

    // InputField pour le nom de la salle à créer ou à rejoindre
    [SerializeField]
    private InputField nomSalle;

    // InputField pour la liste des salles
    [SerializeField]
    private InputField listeSalle;

    // InputField pour le log des messages
    [SerializeField]
    private InputField messagesLog;

    // Variable pour le joueur
    private GameObject joueur;

    // Variable pour les messages
    private Queue<string> messages;

    // Nombre de messages
    private const int nombreMessages = 10;

    // Pour mémoriser le nom d'utilisateur du joueur pour une utilisation ultérieure
    private string nickNamePrefKey = "NomJoueur";

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() {
        messages = new Queue<string> (nombreMessages);
        if (PlayerPrefs.HasKey(nickNamePrefKey)) {
            nomJoueur.text = PlayerPrefs.GetString(nickNamePrefKey);
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        texteConnexion.text = "Connexion au foyer...";
    }

    /// <summary>
    /// Fonction appelée lorsque vous avez réussi à vous connecter
    /// à un master server (serveur maître).
    /// </summary>
    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// Fonction appelée lorsque la connexion a été perdue
    /// ou que vous vous êtes déconnecté du serveur.
    /// </summary>
    /// <param name="cause">Données de DisconnectCause associées à cette déconnexion.</param>
    public override void OnDisconnected(DisconnectCause cause) {
        texteConnexion.text = cause.ToString();
    }

    /// <summary>
    /// Fonction appelée lorsque vous entrez dans un lobby
    /// sur le master server (serveur maître).
    /// </summary>
    public override void OnJoinedLobby() {
        serverWindow.SetActive(true);
        texteConnexion.text = "";
    }

    /// <summary>
    /// Fonction appelée pour la mise à jour de la liste des salles.
    /// </summary>
    /// <param name="rooms">List de RoomInfo.</param>
    public override void OnRoomListUpdate(List<RoomInfo> rooms) {
        listeSalle.text = "";
        foreach (RoomInfo room in rooms) {
            listeSalle.text += room.Name + "\n";
        }
    }

    /// <summary>
    /// Fonction appelée pour rejoindre le room (la salle).
    /// </summary>
    public void JoinRoom() {
        serverWindow.SetActive(false);
        texteConnexion.text = "Rejoindre la salle...";
        PhotonNetwork.LocalPlayer.NickName = nomJoueur.text;
        PlayerPrefs.SetString(nickNamePrefKey, nomJoueur.text);
        RoomOptions roomOptions = new RoomOptions() {
            IsVisible = true,
            MaxPlayers = 8
        };
        if (PhotonNetwork.IsConnectedAndReady) {
            PhotonNetwork.JoinOrCreateRoom(nomSalle.text, roomOptions, TypedLobby.Default);
        } else {
            texteConnexion.text = "La connexion à PhotonNetwork n'est pas prête. Essayez de la redémarrer.";
        }
    }

    /// <summary>
    /// Fonction appelée lorsque vous entrez dans un room (une salle).
    /// </summary>
    public override void OnJoinedRoom() {
        texteConnexion.text = "";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Respawn(0.0f);
    }

    /// <summary>
    /// Fonction appelée pour démarrer le spawn ou le respawn d'un joueur.
    /// </summary>
    /// <param name="tempsSpawn">Temps d'attente avant le spawn d'un joueur.</param>
    void Respawn(float tempsSpawn) {
        cibleTir.SetActive(false);
        sceneCamera.enabled = true;
        StartCoroutine(RespawnCoroutine(tempsSpawn));
    }

    /// <summary>
    /// Fonction coroutine pour faire apparaître le joueur.
    /// </summary>
    /// <param name="tempsSpawn">Temps d'attente avant le spawn d'un joueur.</param>
    IEnumerator RespawnCoroutine(float tempsSpawn) {
        yield return new WaitForSeconds(tempsSpawn);
        messageWindow.SetActive(true);
        cibleTir.SetActive(true);
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        joueur = PhotonNetwork.Instantiate(joueurPrefab.name, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation, 0);
        GestionnairePointsDeVie gestionnairePointsDeVie = joueur.GetComponent<GestionnairePointsDeVie>();
        gestionnairePointsDeVie.RespawnEvent += Respawn;
        gestionnairePointsDeVie.AjouterMessageEvent += AjouterMessage;
        sceneCamera.enabled = false;
        if (tempsSpawn == 0) {
            AjouterMessage(PhotonNetwork.LocalPlayer.NickName + " a rejoint le jeu.");
        } else {
            AjouterMessage(PhotonNetwork.LocalPlayer.NickName + " a réapparu.");
        }
    }

    /// <summary>
    /// Fonction appelée pour ajouter un message au panel de messages.
    /// </summary>
    /// <param name="message">Le message que l'on veut ajouter.</param>
    void AjouterMessage(string message) {
        photonView.RPC("AjouterMessage_RPC", RpcTarget.All, message);
    }

    /// <summary>
    /// Fonction RPC pour appeler la fonction AjouterMessage pour chaque client.
    /// </summary>
    /// <param name="message">Le message que l'on veut ajouter.</param>
    [PunRPC]
    void AjouterMessage_RPC(string message) {
        messages.Enqueue(message);
        if (messages.Count > nombreMessages) {
            messages.Dequeue();
        }
        messagesLog.text = "";
        foreach (string m in messages) {
            messagesLog.text += m + "\n";
        }
    }

    /// <summary>
    /// Fonction appelée lorsqu'un autre joueur se déconnecte.
    /// </summary>
    public override void OnPlayerLeftRoom(Player autre) {
        if (PhotonNetwork.IsMasterClient) {
            AjouterMessage(autre.NickName + " a quitté le jeu.");
        }
    }
}
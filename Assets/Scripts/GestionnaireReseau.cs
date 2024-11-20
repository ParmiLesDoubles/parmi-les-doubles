using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System;

public class GestionnaireReseau : MonoBehaviour, INetworkRunnerCallbacks
{
    // Contient une référence au component NetworkRunner
    NetworkRunner _runner;
    // Index de la scène du jeu
    public int IndexSceneJeu;

    void Start()
    {
        // Création d'une partie dès le départ
        CreationPartie(GameMode.AutoHostOrClient);
    }

    // Fonction asynchrone pour démarrer Fusion et créer une partie
    async void CreationPartie(GameMode mode)
    {
        /*  1. Mémorisation du component NetworkRunner. On garde en mémoire
         *     la référence à ce component dans la variable _runner.
         *  2. Indique au NetworkRunner qu'il doit fournir les entrées (inputs) au
         *     simulateur (Fusion)
         */
        _runner = gameObject.GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        /* Méthode du NetworkRunner qui permet d'initialiser une partie
         * GameMode : reçu en argument. Valeur possible : Client, Host, Server,
         * AutoHostOrClient, etc.)
         * SessionName : Nom de la chambre (room) pour cette partie
         * Scene : la scène qui doit être utilisée pour la simulation
         * SceneManager : référence au component script
         * NetworkSceneManagerDefault qui est ajouté au même moment
         */
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "Chambre de test",
            Scene = SceneRef.FromIndex(IndexSceneJeu),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }
}

using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class NameTag : NetworkBehaviour {
    [HideInInspector]
    public Transform target = null;

    [SerializeField]
    private Text nameText;

    // A synced variable to store the player's name.
    [Networked]
    public string PlayerName { get; set; }

    private void Start() {
        if (Object.HasInputAuthority) {
            // Set the player's name for all clients.
            // RPC_SetName(SessionManager.LocalPlayerName); // Replace later with the method to get the player's name.
        }

        // Update the name immediately on local start.
        UpdateNameTag();
    }

    private void Update() {
        if (target != null) {
            Vector3 lookAtVec = transform.position + (transform.position - target.position);
            transform.LookAt(lookAtVec, Vector3.up);
        }

        // Periodically check for updates (fallback for older Fusion versions).
        UpdateNameTag();
    }

    // Update the name tag UI.
    private void UpdateNameTag() {
        if (nameText != null && nameText.text != PlayerName) {
            nameText.text = PlayerName;
        }
    }

    // Fusion RPC to set the name across all clients.
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetName(string name) {
        PlayerName = name;
    }
}
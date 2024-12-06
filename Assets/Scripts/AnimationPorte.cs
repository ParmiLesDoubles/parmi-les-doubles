using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

// Script qui ouvre une porte lorsque le joueur s'en approche
// et la ferme lorsque le joueur s'en éloigne.

public class AnimationPorte : MonoBehaviour {
    // Valeur minimale de la position Y d'une porte
    [SerializeField]
    private float minPosY = 2.74f;

    // Valeur maximale de la position Y d'une porte
    [SerializeField]
    private float maxPosY = 5.9f;

    // Animator d'une porte
    private Animator animator;

    // Variable pour la position X d'une porte
    private float posx;

    // Variable pour la position Z d'une porte
    private float posz;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() {
        animator = GetComponent<Animator>();
        posx = transform.position.x;
        posz = transform.position.z;
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() {
        transform.position = new Vector3(posx, Mathf.Clamp(transform.position.y, minPosY, maxPosY), posz);
    }

    /// <summary>
    /// OnTriggerStay est appelé pour chaque autre collider qui touche le trigger.
    /// </summary>
    /// <param name="autre">L'autre collider impliqué dans cette collision.</param>
    void OnTriggerStay(Collider autre) {
        if (autre.gameObject.tag == "Player") {
            animator.SetBool("Trigger", true);
        }
    }

    /// <summary>
    /// OnTriggerExit est appelé lorsque l'autre collider a cessé de toucher le trigger.
    /// </summary>
    /// <param name="autre">L'autre collider impliqué dans cette collision.</param>
    void OnTriggerExit(Collider autre) {
        if (autre.gameObject.tag == "Player") {
            animator.SetBool("Trigger", false);
        }
    }
}
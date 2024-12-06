using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]

// Script qui détruit la balle qui a été tirée après quelques secondes
// pour économiser le temps CPU et la mémoire.

public class ImpactLifeCycle : MonoBehaviour {
    // Durée de vie de la balle qui a été tirée
    [SerializeField]
    private float lifespan = 1.5f;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() {
        GetComponent<ParticleSystem>().Play();
        Destroy(gameObject, lifespan);
    }
}
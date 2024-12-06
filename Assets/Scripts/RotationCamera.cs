using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script qui fait pivoter la caméra de la scène.

public class RotationCamera : MonoBehaviour {
    // Où la caméra de la scène regarde
    [SerializeField]
    private Transform lookPoint;

    // Vitesse de la rotation de la caméra de la scène
    [SerializeField]
    private float vitesseRotation = 12f;

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update() {
        transform.RotateAround(lookPoint.position, Vector3.up, vitesseRotation * Time.deltaTime);
        transform.LookAt(lookPoint, Vector3.up);
    }
}
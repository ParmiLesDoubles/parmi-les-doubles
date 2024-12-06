using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

// Script qui assure que le personnage tient une arme,
// quels que soient les mouvements ou les rotations.

public class IKControl : MonoBehaviour {
    // Pour déterminer si l'IK est actif ou inactif
    [SerializeField]
    private bool ikActive = false;

    // Où se trouve la main droite du personnage
    [SerializeField]
    private Transform rightHandObj = null;

    // Où se trouve la main gauche du personnage
    [SerializeField]
    private Transform leftHandObj = null;

    // Où se trouve le coude droit du personnage
    [SerializeField]
    private Transform rightElbowObj = null;

    // Où se trouve le coude gauche du personnage
    [SerializeField]
    private Transform leftElbowObj = null;

    // Où le personnage regarde
    [SerializeField]
    private Transform lookObj = null;

    // Animator du personnage
    private Animator animator;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    void Start() {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Fonction callback pour la mise en place de l'animation IK (Inverse Kinematics).
    /// </summary>
    /// <param name="layerIndex">Index du layer sur laquelle le solveur IK est appelé.</param>
    void OnAnimatorIK(int layerIndex) {
        // Si l'IK est actif, définir la position et la rotation directement vers le but.
        // Si l'IK n'est pas actif, remettre la position et la rotation des parties
        // du corps du personnage dans la position d'origine.
        if (ikActive) {
            // Définir la position de la cible du regard si elle a été attribuée.
            if (lookObj != null) {
                animator.SetLookAtWeight(1);
                animator.SetLookAtPosition(lookObj.position);
            }
            // Définir la position et la rotation de la main droite si elle a été assignée.
            if (rightHandObj != null) {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
            }
            // Définir la position et la rotation de la main gauche si elle a été assignée.
            if (leftHandObj != null) {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
            }
            // Définir la position et la rotation du coude droit s'il a été assigné.
            if (rightElbowObj != null) {
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowObj.position);
            }
            // Définir la position et la rotation du coude gauche s'il a été assigné.
            if (leftElbowObj != null) {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowObj.position);
            }
        } else {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
            animator.SetLookAtWeight(0);
        }
    }
}
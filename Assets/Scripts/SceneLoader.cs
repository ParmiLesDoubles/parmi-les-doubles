using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Fonction pour charger une scène par son nom
    public void LoadScene(string nomScene)
    {
        SceneManager.LoadScene(nomScene);
    }
}
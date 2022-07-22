using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour {
    public GameManager gameManager;

    void Awake() {
        if (GameManager.instance == null) {
            Application.targetFrameRate = 60;
            Instantiate(gameManager);
        }
    }
}

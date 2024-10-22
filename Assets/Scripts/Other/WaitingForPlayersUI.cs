using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForPlayersUI : MonoBehaviour
{
    private void Start() {
        HellGameManager.Instance.OnLocalPlayerReadyChanged += HellGameManager_OnlocalPLayerReadyChanged;
        HellGameManager.Instance.OnStateChanged += HellGameManager_OnStateChanged;
        Hide();
    }

    private void HellGameManager_OnStateChanged(object sender, System.EventArgs e) {
        if (HellGameManager.Instance.IsCountdownToStartActive()) {
            Debug.Log("Countdown to start active. Showing countdown UI.");
            Hide();
        }
        else
    {
        Debug.Log("Countdown to start not active. Hiding countdown UI.");
        Hide();
    }
    }
    private void HellGameManager_OnlocalPLayerReadyChanged(object sender, EventArgs e)
    {
        if (HellGameManager.Instance.IsLocalPlayerReady()) {
             Debug.Log("Local player is ready. Showing waiting for players UI.");
            Show();
        }
        else
    {
        Debug.Log("Local player is not ready. Hiding waiting for players UI.");
        Hide();
    }
    }

    private void Show() {
    gameObject.SetActive(true);
 }

 private void Hide() {
    gameObject.SetActive(false);
 }
}

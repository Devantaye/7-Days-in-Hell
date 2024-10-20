using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button quitBtn;

    private void Awake() {
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            LoadGameScene();
        });

        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });

        quitBtn.onClick.AddListener(() => {
            Application.Quit();
        });
    }

    private void LoadGameScene() {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost) {
            NetworkManager.Singleton.SceneManager.LoadScene("Game Scene", LoadSceneMode.Single);
        }
    }
}


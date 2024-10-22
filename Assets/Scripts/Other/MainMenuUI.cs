using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button quitBtn;

    private void Awake() {
        hostBtn.onClick.AddListener(() => {
            HellGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        });

        clientBtn.onClick.AddListener(() => {
            HellGameMultiplayer.Instance.StartClient();
        });

        quitBtn.onClick.AddListener(() => {
            Application.Quit();
        });
    }
}


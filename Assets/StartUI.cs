using System;
using System.Collections;
using UnityEngine;

public class StartUI : MonoBehaviour
{
    public static StartUI Instance { get; private set; }

    public event EventHandler SpacebarPressed;

  private void Awake()
    {
        // Implementing the singleton pattern.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HellGameManager.Instance.OnLocalPlayerReadyChanged += HellGameManager_OnLocalPlayerReadyChanged;
        Show(); // Initially, show the "press space to start" UI.
    }
private void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
    {
        SpacebarPressed?.Invoke(this, EventArgs.Empty);
    }
}

    private void HellGameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e) {
        if (HellGameManager.Instance.IsLocalPlayerReady()) {
            Hide();
        }
    }

private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks.
        if (HellGameManager.Instance != null)
        {
            HellGameManager.Instance.OnLocalPlayerReadyChanged -= HellGameManager_OnLocalPlayerReadyChanged;
        }
    }
    private void Show() {
    gameObject.SetActive(true);
 }

 private void Hide() {
    gameObject.SetActive(false);
 }
}




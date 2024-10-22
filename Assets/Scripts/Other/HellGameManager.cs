using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HellGameManager : NetworkBehaviour
{
    
    public static HellGameManager Instance { get; private set; }
    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalPlayerReadyChanged;

 private enum State {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

 private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
private bool isLocalPlayerReady;
private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
private Dictionary<ulong, bool> playerReadyDictionary;



private void Awake() {
    Instance = this;

    playerReadyDictionary = new Dictionary<ulong, bool>();
}

private void Start() {
StartUI.Instance.SpacebarPressed += GameInput_OnInteraction;
    
}

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        Debug.Log($"State changed from {previousValue} to {newValue}");
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteraction(object sender, EventArgs e) {

        Debug.Log("Spacebar pressed, attempting to mark player as ready.");
    if (state.Value == State.WaitingToStart) {
        isLocalPlayerReady = true;
        Debug.Log("Player marked as ready.");
        OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

        SetPlayerReadyServerRpc();
    }
    else{
        Debug.Log("Spacebar pressed but not in WaitingToStart state. Current state: " + state.Value);
    }
}

[ServerRpc(RequireOwnership = false)]
private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
{
    ulong clientId = serverRpcParams.Receive.SenderClientId;

    // Ensure that the dictionary is updated with the client ID.
    if (!playerReadyDictionary.ContainsKey(clientId))
    {
        playerReadyDictionary[clientId] = false; // Initialize the player's readiness as false.
    }

    // Mark the client as ready.
    playerReadyDictionary[clientId] = true;
    Debug.Log($"Player {clientId} is ready.");

    // Check if all players are ready.
    bool allClientsReady = true;
    foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
    {
        if (!playerReadyDictionary.ContainsKey(id) || !playerReadyDictionary[id])
        {
            Debug.Log($"Player {id} is not ready.");
            allClientsReady = false;
            break;
        }
    }

    if (allClientsReady)
    {
        Debug.Log("All players are ready. Starting countdown.");
        state.Value = State.CountdownToStart;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    else
    {
        Debug.Log("Not all players are ready.");
    }
}



 private void Update() {
    if (!IsServer) {
        return;
    }

    switch (state.Value) {
        case State.WaitingToStart:
            break;
        case State.CountdownToStart:
        Debug.Log("Countdown active. Timer: " + countdownToStartTimer.Value); 
            countdownToStartTimer.Value -= Time.deltaTime;
            if (countdownToStartTimer.Value < 0f) {
                Debug.Log("Countdown complete. Transitioning to GamePlaying.");
                state.Value = State.GamePlaying;

            }
            break;
        case State.GamePlaying: 
         Debug.Log("Game is in progress.");
            gamePlayingTimer.Value -= Time.deltaTime;
            if (gamePlayingTimer.Value < 0f) {
                state.Value = State.GameOver;
            }
            break;
        case State.GameOver:
        Debug.Log("Game over state reached.");
            break;
    }

    }

    public bool IsGamePlaying() {
        return state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive() {
        return state.Value == State.CountdownToStart;
    }

    public bool IsLocalPlayerReady() {
        return isLocalPlayerReady;
    }

    public float GetCountdownToStartTimer() {
        return countdownToStartTimer.Value;
    }

}

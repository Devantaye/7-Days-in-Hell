using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HellGameMultiplayer : MonoBehaviour
{
public const int MAX_PLAYER_AMOUNT = 2;

public static HellGameMultiplayer Instance { get; private set;}

public static bool playMultiplayer = true;

    

private void Awake() {
        Instance = this;

        DontDestroyOnLoad(gameObject);
}
public void StartHost() {
    NetworkManager.Singleton.StartHost();
}

public void StartClient() {
    NetworkManager.Singleton.StartClient();
}
}

using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance { get; private set; }

    private void Awake()
    {
        // If there's already an instance and it's not this one, destroy the new one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, set this instance and mark it as persistent
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

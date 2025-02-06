using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance{get; private set; }

    public event EventHandler<OnClickOnGridPosEventArgs> OnClickOnGridPos;
    public class OnClickOnGridPosEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }
    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePLayerTypeChange;

    public enum PlayerType {
        None,
        Cross,
        Circle
    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();

    private void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        } 
        else
        {
            Instance = this;
        }   
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0) //server
        {
            localPlayerType = PlayerType.Cross;
        } 
        else
        {
            localPlayerType = PlayerType.Circle; // Client
        }

        if(IsServer) {
            // currentPlayablePlayerType = PlayerType.Cross;

            // this code run everytime client connected
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayablePLayerTypeChange?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if(NetworkManager.Singleton.ConnectedClientsList.Count == 2) {
            // if there are 2 client connected, start the game
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void TriggerGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickOnGridPosRpc(int x, int y, PlayerType playerType)
    {
        if(playerType != currentPlayablePlayerType.Value) {
            // check is player turn, if not then do nothing
            return;
        }
        OnClickOnGridPos?.Invoke(this, new OnClickOnGridPosEventArgs{
            x = x ,
            y = y ,
            playerType = playerType
        });

        // change player turn
        switch (playerType) 
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

    }



    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }
}

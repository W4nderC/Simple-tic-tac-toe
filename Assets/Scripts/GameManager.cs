using System;
using System.Collections.Generic;
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
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }
    public event EventHandler OnCurrentPlayablePLayerTypeChange;
    public event EventHandler OnRematch;
    public event EventHandler OnGameTie;
    public event EventHandler OnScoreChange;
    public event EventHandler OnSoundObj;

    public enum PlayerType {
        None,
        Cross,
        Circle
    }

    public enum Orientation {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }

    public struct Line{
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPos;
        public Orientation orientation ;
    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;
    private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
    private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();

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

        playerTypeArray = new PlayerType[3,3];

        lineList = new List<Line>{
            //Horizontal
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0)},
                centerGridPos = new Vector2Int(1,0),
                orientation = Orientation.Horizontal,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1)},
                centerGridPos = new Vector2Int(1,1),
                orientation = Orientation.Horizontal,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2)},
                centerGridPos = new Vector2Int(1,2),
                orientation = Orientation.Horizontal,
            },

            //Vertical
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2)},
                centerGridPos = new Vector2Int(0,1),
                orientation = Orientation.Vertical,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2)},
                centerGridPos = new Vector2Int(1,1),
                orientation = Orientation.Vertical,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2)},
                centerGridPos = new Vector2Int(2,1),
                orientation = Orientation.Vertical,
            },

            //Diagonals
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2)},
                centerGridPos = new Vector2Int(1,1),
                orientation = Orientation.DiagonalA,
            },
            new Line {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0)},
                centerGridPos = new Vector2Int(1,1),
                orientation = Orientation.DiagonalB,
            },
        };
        
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

        playerCrossScore.OnValueChanged += (int prevScore, int newScore) => {
            OnScoreChange?.Invoke(this, EventArgs.Empty);
        };
        playerCircleScore.OnValueChanged += (int prevScore, int newScore) => {
            OnScoreChange?.Invoke(this, EventArgs.Empty);
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

        if(playerTypeArray[x, y] != PlayerType.None) {
            return;
        }

        playerTypeArray[x, y] = playerType;
        TriggerOnPlaceObjRpc();

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

        TestWinner();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlaceObjRpc(){
        OnSoundObj?.Invoke(this, EventArgs.Empty);
    }

    private bool TestWinnerLine (Line line) {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }

    private bool TestWinnerLine (PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType) {
        return 
        aPlayerType != PlayerType.None &&
        aPlayerType == bPlayerType &&
        bPlayerType == cPlayerType; 
    }

    private void TestWinner () {
        for(int i = 0; i<lineList.Count; i++){
            Line line = lineList[i];
            if(TestWinnerLine(line)) {
                //Win!
                print("Winner!");
                currentPlayablePlayerType.Value = PlayerType.None;
                PlayerType winPlayerType = playerTypeArray[line.centerGridPos.x, line.centerGridPos.y];
                switch(winPlayerType)
                {
                    case PlayerType.Cross:
                        playerCrossScore.Value++;
                        break;
                    case PlayerType.Circle:
                        playerCircleScore.Value++;
                        break;
                }
                TriggerGameWinRpc(i, winPlayerType);
                return;
            }
        }

        bool hasTie = true;
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {   
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                if(playerTypeArray[x,y] == PlayerType.None) {
                    hasTie = false;
                }
            }
            
        }

        if(hasTie) {
            TriggerGameTieRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerGameTieRpc()
    {
        OnGameTie?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerGameWinRpc (int lineIndex, PlayerType winPlayerType) {
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs{
            line = line,
            winPlayerType = winPlayerType,
        });
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc () {
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }
        currentPlayablePlayerType.Value = PlayerType.Cross;

        TriggerOnRemathRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRemathRpc () {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

    public void GetScores (out int playerCrossScore, out int playerCircleScore) {
        playerCrossScore = this.playerCrossScore.Value;
        playerCircleScore = this.playerCircleScore.Value;
    }
}

using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform finishedLinePrefab;

    private List<GameObject> visualGameObjectList;

    private void Awake() {
        visualGameObjectList = new List<GameObject>(); 
    }

    private void Start() 
    {
        GameManager.Instance.OnClickOnGridPos += GameManager_OnClickOnGridPos;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        foreach (GameObject visualGameObject in visualGameObjectList)
        {
            Destroy(visualGameObject);
        }
        visualGameObjectList.Clear();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer) {
            return;
        }
        float eulerZ = 0f;
        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal:
                eulerZ = 0f;
                break;
            case GameManager.Orientation.Vertical:
                eulerZ = 90f;
                break;
            case GameManager.Orientation.DiagonalA:
                eulerZ = 45f;
                break;
            case GameManager.Orientation.DiagonalB:
                eulerZ = -45f;
                break;
        }

        Transform lineCompleteTransform =
        Instantiate
        (
            finishedLinePrefab, 
            GetGridWorldPos(e.line.centerGridPos.x, e.line.centerGridPos.y), 
            Quaternion.Euler(0, 0, eulerZ));
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);

        visualGameObjectList.Add(lineCompleteTransform.gameObject);
    }

    private void GameManager_OnClickOnGridPos(object sender, GameManager.OnClickOnGridPosEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    } 

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Transform prefab;
        switch(playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
            break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
            break;
        }
        Transform spawnedCrossTransform = Instantiate(prefab, GetGridWorldPos(x, y), Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
        
        visualGameObjectList.Add(spawnedCrossTransform.gameObject);
    }

    private Vector2 GetGridWorldPos(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }

    void Update()
    {
        
    }
}

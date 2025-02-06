using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowImage;
    [SerializeField] private GameObject circleArrowImage;
    [SerializeField] private GameObject crossPlayerTextImage;
    [SerializeField] private GameObject circlePlayerTextImage;

    private void Awake() {
        crossArrowImage.SetActive(false);
        circleArrowImage.SetActive(false);
        crossPlayerTextImage.SetActive(false);
        circlePlayerTextImage.SetActive(false);
    }

    private void Start() {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePLayerTypeChange += GameManager_OnCurrentPlayablePLayerType;
    }

    private void GameManager_OnCurrentPlayablePLayerType(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossPlayerTextImage.SetActive(true);
        }
        else 
        {
            circlePlayerTextImage.SetActive(true);
        }
    
    
    }

    private void UpdateCurrentArrow()
    {
        if(GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrowImage.SetActive(true);
            circleArrowImage.SetActive(false);
        }
        else
        {
            crossArrowImage.SetActive(false);
            circleArrowImage.SetActive(true);
        }
    }
}

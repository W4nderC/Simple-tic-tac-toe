using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowImage;
    [SerializeField] private GameObject circleArrowImage;
    [SerializeField] private GameObject crossPlayerTextImage;
    [SerializeField] private GameObject circlePlayerTextImage;
    [SerializeField] private TextMeshProUGUI playerCrossScoreTxt;
    [SerializeField] private TextMeshProUGUI playerCircleScoreTxt;

    private void Awake() {
        crossArrowImage.SetActive(false);
        circleArrowImage.SetActive(false);
        crossPlayerTextImage.SetActive(false);
        circlePlayerTextImage.SetActive(false);

        playerCrossScoreTxt.text = "";
        playerCircleScoreTxt.text = "";
    }

    private void Start() {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChange += GameManager_OnCurrentPlayablePLayerType;
        GameManager.Instance.OnScoreChange += GameManager_OnScoreChange;
    }

    private void GameManager_OnScoreChange(object sender, EventArgs e)
    {
        GameManager.Instance.GetScores(out int playerCrossScore, out int playerCircleScore);

        playerCrossScoreTxt.text = playerCrossScore.ToString();
        playerCircleScoreTxt.text = playerCircleScore.ToString();
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
    
        playerCrossScoreTxt.text = "0";
        playerCircleScoreTxt.text = "0";

        UpdateCurrentArrow();
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

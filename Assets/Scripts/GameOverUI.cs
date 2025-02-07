using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Button rematchBtn;

    private void Awake() {
        rematchBtn.onClick.AddListener(() => {
            GameManager.Instance.RematchRpc();
        });
    }

    void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
        Hide();
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(e.winPlayerType == GameManager.Instance.GetLocalPlayerType()) 
        {
            resultText.text = "YOU WIN!";
            resultText.color = winColor;         
        } 
        else 
        {
            resultText.text = "YOU LOSE!";
            resultText.color = loseColor;
        }
        Show();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Show () {
        gameObject.SetActive(true);
    }

    private void Hide () {
        gameObject.SetActive(false);
        
    }
}

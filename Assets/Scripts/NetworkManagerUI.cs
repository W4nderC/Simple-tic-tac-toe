using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()  
    {
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            Hide();
        });

        Show();
    }

    private void Hide () {
        gameObject.SetActive(false);
    }

    private void Show () {
        gameObject.SetActive(true);
    }
}

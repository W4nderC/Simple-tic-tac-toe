using UnityEngine;

public class SquarePos : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;

    void OnMouseDown()
    {
        print("Square pos: "+ x + "_"+y);
        GameManager.Instance.ClickOnGridPosRpc(x, y, GameManager.Instance.GetLocalPlayerType());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

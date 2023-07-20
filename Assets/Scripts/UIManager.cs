using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private TMP_Text status;

    private void Awake()
    {
        Instance = this;
    }

    public void StartHost()
    {
        // NetworkManager.Singleton.tra
        NetworkManager.Singleton.StartHost();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void SetStatus(string content)
    {
        status.SetText(content);
    }

    public void AppendStatus(string line)
    {
        if (status.text.Length == 0)
        {
            SetStatus(line);
        }
        else
        {
            SetStatus($"{status.text}\n{line}");
        }
    }
}

using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonFunctions : MonoBehaviour
{
	//private NetworkManager nm;
	private Button startBtn;
	private Button startClient;

	private void Awake()
	{
		//nm = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
	}

	private void OnEnable()
	{
		UIDocument doc = GetComponent<UIDocument>();

		if (doc != null) 
		{
			startBtn = doc.rootVisualElement.Q("StartGame") as Button;
			startBtn.RegisterCallback<ClickEvent>(StartGame);

			startClient = doc.rootVisualElement.Q("StartClient") as Button;
			startClient.RegisterCallback<ClickEvent>(StartClient);
		}

	}

	private void OnDisable()
	{
		if (startBtn != null)
		{
			startBtn.UnregisterCallback<ClickEvent>(StartGame);
			startClient.UnregisterCallback<ClickEvent>(StartClient);
		}

	}

	private void StartGame(ClickEvent e)
	{
		if (NetworkManager.Singleton != null)
		{
			if (NetworkManager.Singleton.StartHost())
			{
				Debug.Log("Host Started!");
				Debug.Assert(NetworkManager.Singleton.IsServer);
				Debug.Assert(NetworkManager.Singleton.IsHost);
				NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
			}
			else
			{
				Debug.LogError("Unable to start Host!");
			}
		}
	}

	private void StartClient(ClickEvent e)
	{
		if (NetworkManager.Singleton != null)
		{
			if (NetworkManager.Singleton.StartClient())
			{
				Debug.Log("Client Started!");
				Debug.Assert(NetworkManager.Singleton.IsClient);
			}
			else
			{
				Debug.LogError("Unable to start Client!");
			}
		}
	}
}

using Unity.Netcode;
using UnityEngine;

public class SpawnerTest : NetworkBehaviour
{
	private static bool init = false;
	public GameObject prefab;

    void FixedUpdate()
    {
        if (!init)
		{
			if (NetworkManager.Singleton.IsServer)
			{
				GameObject tower = Instantiate(prefab, this.gameObject.transform);
				NetworkObject no = tower.GetComponent<NetworkObject>();
				if (no != null)
				{
					no.Spawn(true);
				}
				init = true;
			}
		}
    }
}

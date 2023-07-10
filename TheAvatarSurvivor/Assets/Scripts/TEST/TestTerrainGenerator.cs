using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestTerrainGenerator : NetworkBehaviour
{
    [SerializeField] Transform spherePrefab;
    Transform sphereInst;

    public static TestTerrainGenerator Instance;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SpawnSphereOnServer(Vector3 playerPos)
    {
        SpawnSphereServerRpc(playerPos);
    }

    [ServerRpc]
    void SpawnSphereServerRpc(Vector3 playerPos)
    {
        Vector3 pos = new(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        sphereInst = Instantiate(spherePrefab, playerPos + pos, Quaternion.identity);
        sphereInst.GetComponent<NetworkObject>().Spawn(true);
    }

    [ClientRpc]
    void SpawnSphereClientRpc()
    {

    }
}

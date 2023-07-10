using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestPlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            TestTerrainGenerator.Instance.SpawnSphereOnServer(transform.position);
        }
    }
}

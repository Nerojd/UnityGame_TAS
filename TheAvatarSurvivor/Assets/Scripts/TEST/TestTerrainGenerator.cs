using DoDo.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.AssetImporters;
using UnityEngine;

public class TestTerrainGenerator : NetworkBehaviour
{
    [SerializeField] Transform spherePrefab;
    [SerializeField] Mesh cubeMesh;
    [SerializeField] Mesh sphereMesh;
    bool isSphereMesh = false;

    [SerializeField] MeshSettings meshSettings;

    public event EventHandler OnTerrainCreationStarted;
    public event EventHandler OnTerrainCreationFinished;

    Material mat;
    Transform sphereInst = null;

    MeshGenerator meshGenerator;
    TextureGenerator textureGenerator;

    Dictionary<Vector3, Chunk> chunkDictionary;

    public static TestTerrainGenerator Instance;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;

        meshGenerator = GetComponent<MeshGenerator>();
        textureGenerator = GetComponent<TextureGenerator>();
    }

    private void Start()
    {
        chunkDictionary = new Dictionary<Vector3, Chunk>();

        mat = textureGenerator.GetMaterial();
        meshGenerator.Setup(meshSettings.numChunks, meshSettings.boundsSize, meshSettings.isoLevel, meshSettings.numPointsPerAxis);
    }


    public void ChangeMeshOnServer(Vector3 pos)
    {
        ChangeMeshServerRpc(pos);
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeMeshServerRpc(Vector3 pos)
    {
        if (sphereInst == null)
        {
            // Instantiation on SERVER
            sphereInst = Instantiate(spherePrefab, pos + new Vector3(5, 0, 5), Quaternion.identity);

            // Instantiation on CLIENTS
            NetworkObject networkChunkObj = sphereInst.GetComponent<NetworkObject>();
            networkChunkObj.Spawn(true);
            networkChunkObj.TrySetParent(gameObject.GetComponent<NetworkObject>());
        }
        else
        {
            NetworkObject networkChunkObj = sphereInst.GetComponent<NetworkObject>();
            MeshFilter meshFilter = networkChunkObj.gameObject.GetComponent<MeshFilter>();
            if (isSphereMesh)
            {
                meshFilter.mesh = cubeMesh;
                meshFilter.sharedMesh = cubeMesh;
                isSphereMesh = false;
            }
            else
            {
                meshFilter.mesh = sphereMesh;
                meshFilter.sharedMesh = sphereMesh;
                isSphereMesh = true;
            }
        }
        //ChangeMeshClientRpc(networkChunkObj);
    }

    [ClientRpc]
    void ChangeMeshClientRpc(NetworkObjectReference target)
    {
        target.TryGet(out NetworkObject targetObject);

        Transform targetInst = targetObject.GetComponent<Transform>();
        if (targetInst != null)
        {


        }
        else
        {
        }
    }

    public static Vector3 CentreFromCoord(Vector3 coord, Vector3Int numChunks, float boundsSize)
    {
        // Centre entire map at origin
        Vector3 totalBounds = (Vector3)numChunks * boundsSize;
        return -totalBounds / 2 + coord * boundsSize + Vector3.one * boundsSize / 2;
    }
    public MeshSettings GetMeshSettings() => meshSettings;
}

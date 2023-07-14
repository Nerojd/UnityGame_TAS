using DoDo.Terrain;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.AssetImporters;
using UnityEngine;

public class TestTerrainGenerator : NetworkBehaviour
{
    [SerializeField] Transform chunkPrefab;
    [SerializeField] MeshSettings meshSettings;

    Material mat;

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

    // Update is called once per frame
    void Update()
    {
        
    }


    public void InitChunkOnServer()
    {
        InitChunkServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    void InitChunkServerRpc()
    {
        for (int x = 0; x < meshSettings.numChunks.x; x++)
        {
            for (int y = 0; y < meshSettings.numChunks.y; y++)
            {
                for (int z = 0; z < meshSettings.numChunks.z; z++)
                {
                    // Instantiation on SERVER
                    Transform chunkInst = Instantiate(chunkPrefab);

                    // Instantiation on CLIENTS
                    NetworkObject networkChunkObj = chunkInst.GetComponent<NetworkObject>();
                    networkChunkObj.Spawn(true);
                    networkChunkObj.TrySetParent(gameObject.GetComponent<NetworkObject>());

                    Vector3Int coord = new(x, y, z);
                    InitChunkClientRpc(networkChunkObj, coord);
                    chunkDictionary[coord] = chunkInst.gameObject.GetComponent<Chunk>();
                }
            }
        }          
    }

    [ClientRpc]
    void InitChunkClientRpc(NetworkObjectReference target, Vector3Int coord)
    {
        target.TryGet(out NetworkObject targetObject);

        Transform targetInst = targetObject.GetComponent<Transform>();
        if (targetInst != null)
        {
            // Setup Chunk
            Vector3 center = CentreFromCoord(coord, meshSettings.numChunks, meshSettings.boundsSize);
            targetInst.name = $"Chunk ({coord.x}, {coord.y}, {coord.z})";
            Chunk chunk = targetInst.gameObject.GetComponent<Chunk>();
            chunk.Setup(coord, center, meshSettings.boundsSize, meshSettings.numChunks, meshSettings.visibleDstThreshold, targetInst.gameObject, meshGenerator);
            chunk.SetMaterial(mat);
            
            // Create Mesh
            meshGenerator.GenerateDensity(chunk);
            meshGenerator.UpdateChunkMesh(chunk);

            chunkDictionary[chunk.coord] = chunk;

        }
        else
        {
            Debug.Log("ClientRpc - Can't access object of coord : " + coord);
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

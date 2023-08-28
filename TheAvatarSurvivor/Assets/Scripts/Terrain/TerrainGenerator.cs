using DoDo.Core;
using DoDo.Player;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;

/// <summary>
/// This component will allow to create LOD chunks so that nearby chunks are more detailed than a further chunks
/// </summary>
namespace DoDo.Terrain
{
    //[ExecuteInEditMode]
    public class TerrainGenerator : NetworkBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField] MeshSettings meshSettings;
        [SerializeField] NoiseSettings noiseSettings;
        [SerializeField] ComputeShaderSettings computeShaderSettings;

        [SerializeField] Transform chunkPrefab;
        [SerializeField] MeshGenerator meshGenerator;
        [SerializeField] TextureGenerator textureGenerator;

        public event EventHandler OnTerrainCreationStarted;
        public event EventHandler OnTerrainCreationFinished;

        Material mat;

        Dictionary<Vector3, Chunk> chunkDictionary;

        public static TerrainGenerator Instance;

        /*******************************************/
        /*              Unity Methods              */
        /*******************************************/
        private void Awake()
        {
            Instance = this;
            //if (Instance == null)
            //{
            //    Instance = this;
            //}
            //else
            //{
            //    Debug.LogError("More than one TerrainGenerator instance in our scene");
            //    return;
            //}
        }

        void Start()
        {
            chunkDictionary = new Dictionary<Vector3, Chunk>();

            mat = textureGenerator.GetMaterial();
            meshGenerator.Setup(meshSettings.numChunks, meshSettings.boundsSize, meshSettings.isoLevel, meshSettings.numPointsPerAxis);
        }

        public void EditorUpdate()
        {
            mat = textureGenerator.GetMaterial();
            meshGenerator.Setup(meshSettings.numChunks, meshSettings.boundsSize, meshSettings.isoLevel, meshSettings.numPointsPerAxis);

            //EditorInitChunks();

            // Release buffers immediately in editor
            if (!Application.isPlaying)
            {
                meshGenerator.ReleaseBuffers();
            }
        }


        /*******************************************/
        /*             Private Methods             */
        /*******************************************/
        public void ReinitChunkHolder()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).GetComponent<Chunk>() == null)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
                else
                {
                    transform.GetChild(i).GetComponent<Chunk>().DestroyOrDisable();
                }
            }
        }

        /// <summary>
        /// Create and Initialize all chunks
        /// </summary>
        void InitChunksSinglePlayer()
        {
            ReinitChunkHolder();

            for (int x = 0; x < meshSettings.numChunks.x; x++)
            {
                for (int y = 0; y < meshSettings.numChunks.y; y++)
                {
                    for (int z = 0; z < meshSettings.numChunks.z; z++)
                    {
                        Vector3Int coord = new(x, y, z);
                        //float posX = (-(meshSettings.numChunks.x - 1f) / 2 + x) * (meshSettings.boundsSize) / meshSettings.numChunks.x;
                        //float posY = (-(meshSettings.numChunks.y - 1f) / 2 + y) * (meshSettings.boundsSize) / meshSettings.numChunks.y;
                        //float posZ = (-(meshSettings.numChunks.z - 1f) / 2 + z) * (meshSettings.boundsSize) / meshSettings.numChunks.z;
                        Vector3 center = CentreFromCoord(coord, meshSettings.numChunks, meshSettings.boundsSize);

                        GameObject chunkObj = new($"Chunk ({coord.x}, {coord.y}, {coord.z})");

                        // LAYER
                        chunkObj.layer = LayerMask.NameToLayer(meshSettings.terrainLayer);

                        // TYPE
                        ObjectType objType = chunkObj.AddComponent<ObjectType>();
                        objType.SetObjectType(EObjectType.Terrain);

                        // CHUNK
                        Chunk chunk = chunkObj.AddComponent<Chunk>();
                        chunk.Setup(coord, center, chunkObj, meshSettings, noiseSettings, computeShaderSettings);
                        chunk.SetMaterial(mat);

                        // PARENT
                        chunkObj.transform.parent = transform;

                        meshGenerator.GenerateDensity(chunk);
                        meshGenerator.UpdateChunkMesh(chunk);

                        chunkDictionary[coord] = chunk;
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void InitChunkServerRpc()
        {
            OnTerrainCreationStarted?.Invoke(this, EventArgs.Empty);

            for (int x = 0; x < meshSettings.numChunks.x; x++)
            {
                for (int y = 0; y < meshSettings.numChunks.y; y++)
                {
                    for (int z = 0; z < meshSettings.numChunks.z; z++)
                    {
                        // Instantiation on SERVER
                        Transform chunkInst = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);

                        // Instantiation on CLIENTS
                        NetworkObject networkChunkObj = chunkInst.GetComponent<NetworkObject>();
                        networkChunkObj.Spawn(true);
                        networkChunkObj.TrySetParent(gameObject.GetComponent<NetworkObject>());

                        Vector3Int coord = new(x, y, z);
                        InitChunkClientRpc(networkChunkObj, coord);

                        chunkDictionary[coord] = chunkInst.gameObject.GetComponent<Chunk>();

                        Vector3 center = CentreFromCoord(coord, meshSettings.numChunks, meshSettings.boundsSize);
                        chunkInst.name = $"Chunk ({coord.x}, {coord.y}, {coord.z})";
                        Chunk chunk = chunkInst.gameObject.GetComponent<Chunk>();
                        chunk.Setup(coord, center, chunkInst.gameObject, meshSettings, noiseSettings, computeShaderSettings);
                        chunk.SetMaterial(mat);
                        chunk.GenerateDensity();
                    }
                }
            }

            OnTerrainCreationFinished?.Invoke(this, EventArgs.Empty);
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
                chunk.Setup(coord, center, targetInst.gameObject, meshSettings, noiseSettings, computeShaderSettings);
                chunk.SetMaterial(mat);
                chunk.GenerateDensity();
                //meshGenerator.UpdateChunkMesh(chunk);

                chunkDictionary[chunk.m_coord] = chunk;
            }
            else
            {
                Debug.Log("ClientRpc - Cannot access object of coord : " + coord);
            }
        }


        /// <summary>
        /// Terraforming
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        void TerraformChunkServerRpc(Vector3 chunkCoord, Vector3 brushCenter, int weight, float brushRadius, float brushPower, bool isAddingMatter)
        {
            TerraformChunkClientRpc(chunkCoord, brushCenter, weight, brushRadius, brushPower, isAddingMatter);
        }

        [ClientRpc]
        void TerraformChunkClientRpc(Vector3 chunkCoord, Vector3 brushCenter, int weight, float brushRadius, float brushPower, bool isAddingMatter)
        {
            Chunk chunk = chunkDictionary[chunkCoord];
            meshGenerator.TerraformChunkMesh(chunk, brushCenter, weight, brushRadius, brushPower, isAddingMatter);
            //meshGenerator.UpdateChunkMesh(chunk);
        }


        // HOW TO INIT CHUNK VISIBLE using Server network visibility
        //
        //foreach (Chunk chunk in chunkDictionary.Values)
        //{
        //    NetworkObject netObject = chunk.GetComponent<NetworkObject>();

        //    foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        //    {
        //        if (!netObject.IsNetworkVisibleTo(clientId))
        //        {
        //            netObject.NetworkShow(clientId);
        //            netObject.CheckObjectVisibility = (clientId) => { return true; };
        //        }
        //    }

        //}

        /******************************************/
        /*             Public Methods             */
        /******************************************/
        public void InitChunkOnServer()
        {
            InitChunkServerRpc();
        }

        public void Terraform(Vector3 chunkCoord, Vector3 brushCenter, int weight, float brushRadius, float brushPower, bool isAddingMatter)
        {
            Chunk chunk = chunkDictionary[chunkCoord];
            meshGenerator.TerraformChunkMesh(chunk, brushCenter, weight, brushRadius, brushPower, isAddingMatter);
        }

        public void TerraformOnServer(Vector3 chunkCoord, Vector3 brushCenter, int weight, float brushRadius, float brushPower, bool isAddingMatter)
        {
            TerraformChunkServerRpc(chunkCoord, brushCenter, weight, brushRadius, brushPower, isAddingMatter);

            //Chunk chunk = chunkDictionary[chunkCoord];
            //var targetObject = chunk.GetComponent<NetworkObject>();
            //meshGenerator.TerraformChunkMeshServerRpc(targetObject, brushCenter, weight, brushRadius, brushPower);
        }

        // NEW METHOD to terraform
        // 
        // Terraform() called TerraformChunkMeshServerRpc(...); to create to displacement of points and then Clients calculate the mesh with the new points
        // 
        // Clients get all chunk from server and server notify clients that a chunk has been modified. 
        // If chunk vivible then the client update with marching cube algo
        // Else the chunk is just marked has "terraformed"


        // NEW METHOD to create physics object
        // 
        // UpdateChunkSeparatedPoint()
        //      Loop on all chunks
        //      Check for Point that are not closer to another (or not a lot of them). If so create a new Mesh with this point and create a GameObject for it (with rigidbody)

        public static Vector3 CentreFromCoord(Vector3 coord, Vector3Int numChunks, float boundsSize)
        {
            // Centre entire map at origin
            Vector3 totalBounds = (Vector3)numChunks * boundsSize;
            return -totalBounds / 2 + coord * boundsSize + Vector3.one * boundsSize / 2;
        }

        public bool IsChunkCoordInDictionary(Vector3 coord) => chunkDictionary.ContainsKey(coord);
        public Chunk GetChunk(Vector3 coord) => chunkDictionary[coord];
        public MeshSettings GetMeshSettings() => meshSettings;


        public override void OnDestroy()
        {
            meshGenerator.ReleaseBuffers();
        }
    }

}
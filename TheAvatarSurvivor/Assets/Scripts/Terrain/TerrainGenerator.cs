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
        [SerializeField] GameObject chunkPrefab;
        [SerializeField] TextMeshProUGUI textId;

        public event EventHandler OnTerrainCreationStarted;
        public event EventHandler OnTerrainCreationFinished;

        Material mat;

        MeshGenerator meshGenerator;
        TextureGenerator textureGenerator;

        Dictionary<Vector3, Chunk> chunkDictionary = new Dictionary<Vector3, Chunk>();

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

            meshGenerator = GetComponent<MeshGenerator>();
            textureGenerator = GetComponent<TextureGenerator>();
        }

        void Start()
        {
            mat = textureGenerator.GetMaterial();

            meshGenerator.Setup(meshSettings.numChunks, meshSettings.boundsSize, meshSettings.isoLevel, meshSettings.numPointsPerAxis);

            //ReinitChunkHolder();
            if (IsHost)
            {
                //InitChunksServerRpc();

                //CreateChunkMesh();
            }
        }

        public override void OnNetworkSpawn()
        {
            //mat = textureGenerator.GetMaterial();

            //meshGenerator.Setup(meshSettings.numChunks, meshSettings.boundsSize, meshSettings.isoLevel, meshSettings.numPointsPerAxis);

        }

        public void EditorUpdate()
        {
            textureGenerator = GetComponent<TextureGenerator>();
            mat = textureGenerator.GetMaterial();


            meshGenerator = GetComponent<MeshGenerator>();
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
        void InitChunks_WorkingFunction()
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
                        chunk.Setup(coord, center, meshSettings.boundsSize, meshSettings.numChunks, meshSettings.visibleDstThreshold, chunkObj, meshGenerator);
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

        /// <summary>
        /// Create and Initialize all chunks on server
        /// </summary>
        //[ServerRpc(RequireOwnership = false)]
        //void InitChunksServerRpc()
        //{
        //    for (int x = 0; x < meshSettings.numChunks.x; x++)
        //    {
        //        for (int y = 0; y < meshSettings.numChunks.y; y++)
        //        {
        //            for (int z = 0; z < meshSettings.numChunks.z; z++)
        //            {
        //                // GAMEOBJECT
        //                Vector3Int coord = new(x, y, z);
        //                GameObject chunkObj = new($"Chunk ({coord.x}, {coord.y}, {coord.z})");
        //                Vector3 center = CentreFromCoord(coord, meshSettings.numChunks, meshSettings.boundsSize);

        //                // LAYER
        //                chunkObj.layer = LayerMask.NameToLayer(meshSettings.terrainLayer);

        //                // TYPE
        //                ObjectType objType = chunkObj.AddComponent<ObjectType>();
        //                objType.SetObjectType(EObjectType.Terrain);

        //                // CHUNK
        //                Chunk chunk = chunkObj.AddComponent<Chunk>();
        //                chunk.Setup(coord, center, meshSettings.boundsSize, meshSettings.numChunks, meshSettings.visibleDstThreshold, chunkObj, meshGenerator);
        //                chunk.SetMaterial(mat);
        //                meshGenerator.GenerateDensity(chunk);

        //                // NETWORK
        //                NetworkObject networkObj = chunkObj.AddComponent<NetworkObject>();
        //                networkObj.Spawn(true);
        //                networkObj.TrySetParent(transform);

        //                InitChunksClientRpc(networkObj);

        //                chunkDictionary[coord] = chunk;

        //                CreateChunkMeshClientRpc(networkObj);
        //            }
        //        }
        //    }
        //    OnTerrainCreationFinished?.Invoke(this, EventArgs.Empty);
        //}

        [ClientRpc]
        void InitChunksClientRpc(NetworkObjectReference target)
        {
            NetworkObject targetObject = target;

            Chunk chunk = targetObject.GetComponent<Chunk>();
            if (chunk != null)
            {
                chunkDictionary[chunk.coord] = chunk;
                //CreateChunkMesh(chunk);
            }
        }

        /// <summary>
        /// Create mesh for each chunks, depending on the generated noise density
        /// </summary>
        //void CreateChunkMesh()
        //{
        //    foreach (Chunk chunk in chunkDictionary.Values)
        //    {
        //        // Generate density on the HOST
        //        PointData[] points = meshGenerator.GenerateDensity(chunk);

        //        // Create mesh on all clients
        //        //var targetObject = chunk.GetComponent<NetworkObject>();
        //        GenerateMeshServerRpc(chunk.gameObject, points);
        //    }
        //}

        //[ServerRpc(RequireOwnership = false)]
        //void GenerateMeshServerRpc(NetworkObjectReference target, PointData[] points)
        //{
        //    NetworkObject targetObject = target;
        //    GenerateMeshClientRpc(targetObject, points);
        //}

        //[ClientRpc]
        //void GenerateMeshClientRpc(NetworkObjectReference target, PointData[] points)
        //{
        //    NetworkObject targetObject = target;

        //    Chunk chunk = targetObject.GetComponent<Chunk>();
        //    if (chunk == null) return;

        //    chunk.SetPointsData(points, points.Length);
        //    meshGenerator.UpdateChunkMesh(chunk);
        //}

        void CreateChunkMesh(Chunk chunk)
        {
            meshGenerator.GenerateDensity(chunk);
            meshGenerator.UpdateChunkMesh(chunk);
        }

        void InitChunks()
        {
            for (int x = 0; x < meshSettings.numChunks.x; x++)
            {
                for (int y = 0; y < meshSettings.numChunks.y; y++)
                {
                    for (int z = 0; z < meshSettings.numChunks.z; z++)
                    {
                        Vector3Int coord = new(x, y, z);
                        Vector3 center = CentreFromCoord(coord, meshSettings.numChunks, meshSettings.boundsSize);

                        // NETWORK SERVER INSTANTIATION
                        GameObject chunkObj = Instantiate(chunkPrefab);

                        // SETUP
                        chunkObj.name = $"Chunk ({coord.x}, {coord.y}, {coord.z})";
                        Chunk chunk = chunkObj.GetComponent<Chunk>();
                        chunk.Setup(coord, center, meshSettings.boundsSize, meshSettings.numChunks, meshSettings.visibleDstThreshold, chunkObj, meshGenerator);
                        chunk.SetMaterial(mat);

                        // NETWORK CLIENT INSTANTIATION
                        NetworkObject networkObj = chunkObj.GetComponent<NetworkObject>();
                        NetworkObject.SpawnWithObservers = true;
                        networkObj.Spawn();
                        networkObj.TrySetParent(gameObject.GetComponent<NetworkObject>());
                        networkObj.RemoveOwnership();

                        InitChunksClientRpc(networkObj);
                        chunkDictionary[coord] = chunk;
                    }
                }
            }

            foreach (Chunk chunk in chunkDictionary.Values)
            {
                NetworkObject netObject = chunk.GetComponent<NetworkObject>();

                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (!netObject.IsNetworkVisibleTo(clientId))
                    {
                        netObject.NetworkShow(clientId);
                        netObject.CheckObjectVisibility = (clientId) => { return true; };
                    }
                }

            }

            if (IsClient)
            {
                textId.text = "ISCLIENT - ";
            }
            else
            {
                textId.text = "ISSERVER - ";
            }
            /* TEST */
            foreach (Chunk chunk in chunkDictionary.Values)
            {
                NetworkObject netObject = chunk.GetComponent<NetworkObject>();
                TestClientRpc(netObject);
            }

            OnTerrainCreationFinished?.Invoke(this, EventArgs.Empty);
        }

        /* TEST */
        [ClientRpc]
        void TestClientRpc(NetworkObjectReference target)
        {
            NetworkObject targetObject = target;

            Chunk chunk = targetObject.GetComponent<Chunk>();
            if (chunk != null)
            {
                //chunk.transform.parent = transform;
                bool isVisible = targetObject.IsNetworkVisibleTo(MultiplayerManager.Instance.GetPlayerData().clientId);
                textId.text += isVisible.ToString();
                // Instancie une sphère
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                // Positionne la sphère
                sphere.transform.position = chunk.center;
                // Applique une échelle à la sphère si nécessaire
                sphere.transform.localScale = new Vector3(1f, 1f, 1f);
                sphere.SetActive(true);
            }
            else
            {
                textId.text += "ChunkNotFound";
            }
        }

        /******************************************/
        /*             Public Methods             */
        /******************************************/
        public void CreateTerrain()
        {
            OnTerrainCreationStarted?.Invoke(this, EventArgs.Empty);

            InitChunks();
        }


        public void Terraform(Vector3 chunkCoord, Vector3 brushCenter, int weight, float brushRadius, float brushPower)
        {
            Chunk chunk = chunkDictionary[chunkCoord];
            meshGenerator.TerraformChunkMesh(chunk, brushCenter, weight, brushRadius, brushPower);
        }

        public void TerraformFromServer(Vector3 chunkCoord, Vector3 brushCenter, int weight, float brushRadius, float brushPower)
        {
            Chunk chunk = chunkDictionary[chunkCoord];
            var targetObject = chunk.GetComponent<NetworkObject>();
            meshGenerator.TerraformChunkMeshServerRpc(targetObject, brushCenter, weight, brushRadius, brushPower);
        }

        // NEW METHOD to terraform
        // 
        // Terraform() called TerraformChunkMeshServerRpc(...); to create to displacement of points and then Clients calculate the mesh with the new points



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
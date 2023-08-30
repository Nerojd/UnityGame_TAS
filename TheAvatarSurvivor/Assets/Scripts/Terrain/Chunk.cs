using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace DoDo.Terrain
{
    public class Chunk : NetworkBehaviour
    {
        const int _threadGroupSize = 8;

        /****************************/
        /* Private Data Information */
        /****************************/
        public Vector3Int m_coord;
        public Vector3 m_center;
        Bounds m_bounds;
        public Mesh m_mesh;

        GameObject m_chunkObj;
        bool m_hasSetCollider = false;
        bool m_isTerraformed = false;
        bool m_isDensityListChanged = false;
        bool m_isSetup = false;

        /*****************/
        /* Mesh Settings */
        /*****************/
        int m_numPoints;
        int m_numVoxelsPerAxis;
        int m_numVoxels;
        int m_maxTriangleCount;
        int m_maxVertexCount;

        float m_pointSpacing;

        /* Noise Settings */
        ComputeShader m_densityComputeShader;
        NoiseSettings m_noiseSettings;

        /* Voxel Settings */
        ComputeShader m_meshComputeShader;
        MeshSettings m_meshSettings;

        /* Terraform Shader */
        ComputeShader m_terraformComputeShader;

        /* Buffers */
        ComputeBuffer m_triangleDataBuffer;
        ComputeBuffer m_positionPointsBuffer;
        ComputeBuffer m_densityPointsBuffer;
        ComputeBuffer m_triCountBuffer;

        bool isJobScheduled = false;
        TerraformChunkJob m_terraformChunkJob;
        JobHandle m_terraformDensityJobHandle;
        NativeArray<float3> m_positionNativeArray;
        NativeArray<float> m_densityNativeArray, m_newDensityNativeArray;
        NativeArray<TerraformingData> m_terraformingDataNativeArray;
        List<TerraformingData> m_terraformingDataList = new List<TerraformingData>();

        /**************/
        /* Components */
        /**************/
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;

        /*******************************************/
        /*              Unity Methods              */
        /*******************************************/
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {

            }
            else
            {
                
            }
        }

        private void Update()
        {
            if (!m_isSetup) return;
            if (!IsVisible()) return;

            // Faire dans un Update moins rapide ou sauter des frames d'update ?
            if (m_isTerraformed && m_terraformingDataList.Count > 0)
            {
                m_terraformingDataNativeArray = new NativeArray<TerraformingData>(m_terraformingDataList.Count, Allocator.Persistent);
                m_terraformingDataNativeArray.CopyFrom(m_terraformingDataList.ToArray());
                m_terraformingDataList.Clear();

                m_terraformChunkJob = new TerraformChunkJob()
                {
                    _originalPoints = m_densityNativeArray,
                    _numPointsPerAxis = m_meshSettings.numPointsPerAxis,
                    _boundsSize = m_meshSettings.boundsSize,
                    _isoLevel = m_meshSettings.isoLevel,
                    _pointSpacing = m_pointSpacing,
                    _chunkCenter = m_center,
                    _deltaTime = Time.deltaTime,
                    _terraformData = m_terraformingDataNativeArray,
                    _terraformedPoints = m_newDensityNativeArray
                };
                m_terraformDensityJobHandle = m_terraformChunkJob.Schedule(m_densityNativeArray.Length, 64);
                JobHandle.ScheduleBatchedJobs();

                isJobScheduled = true;
            }
        }

        private void LateUpdate()
        {
            if (!m_isSetup) return;
            if (!IsVisible()) return;

            if (m_isTerraformed && isJobScheduled)
            {
                // Essayer de le mettre dans l'update directement ?
                m_terraformDensityJobHandle.Complete();
                UpdateChunkMesh(m_terraformChunkJob._terraformedPoints);
                m_isTerraformed = false;
            }
        }

        /*******************************************/
        /*             Private Methods             */
        /*******************************************/
        private bool IsVisible()
        {
            return m_chunkObj.activeSelf;
        }
        private void SetVisible(bool visible)
        {
            m_chunkObj.SetActive(visible);
        }

        /******************************************/
        /*             Public Methods             */
        /******************************************/
        public void Setup(Vector3Int coord, Vector3 center, GameObject chunkObj, MeshSettings meshSettings, NoiseSettings noiseSettings, ComputeShaderSettings computeShaderSettings)
        {
            m_coord                  = coord;
            m_center                 = center;
            m_chunkObj               = chunkObj;
            m_meshSettings           = meshSettings;
            m_noiseSettings          = noiseSettings;
            m_densityComputeShader   = computeShaderSettings.densityComputeShader;
            m_meshComputeShader      = computeShaderSettings.meshComputeShader;
            m_terraformComputeShader = computeShaderSettings.terraformComputeShader;

            m_bounds = new Bounds(center, Vector3.one * meshSettings.boundsSize);

            m_mesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };

            // Mesh rendering and collision components
            if (!chunkObj.TryGetComponent<MeshFilter>(out meshFilter))
            {
                meshFilter = chunkObj.AddComponent<MeshFilter>();
            }
            meshFilter.mesh = m_mesh;

            if (!chunkObj.TryGetComponent<MeshRenderer>(out meshRenderer))
            {
                meshRenderer = chunkObj.AddComponent<MeshRenderer>();
            }

            if (!chunkObj.TryGetComponent<MeshCollider>(out meshCollider))
            {
                meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
            }
            if (meshCollider.sharedMesh == null)
            {
                SetCollider();
            }

            CreateBuffers();

            m_pointSpacing = m_meshSettings.boundsSize / m_numVoxelsPerAxis;

            m_positionNativeArray = new NativeArray<float3>(m_numPoints, Allocator.Persistent);
            m_densityNativeArray = new NativeArray<float>(m_numPoints, Allocator.Persistent);
            m_newDensityNativeArray = new NativeArray<float>(m_numPoints, Allocator.Persistent);
            m_terraformingDataNativeArray = new NativeArray<TerraformingData>();

            m_isSetup = true;
        }

        public void UpdateVisibleChunks(Vector3 viewerPosition)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(m_bounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= m_meshSettings.visibleDstThreshold;

            if (wasVisible != visible)
            {
                SetVisible(visible);
            }

            if (visible)
            {
                if (!m_hasSetCollider)
                {
                    SetCollider();
                    m_hasSetCollider = true;
                }
            }
            else
            {
                m_hasSetCollider = false;
            }
        }

        /*****************************/
        /*        MANAGE MESH        */
        /*****************************/
        /* Size all buffers depending on the size of the number of points on each axes */
        void CreateBuffers()
        {
            m_numPoints = m_meshSettings.numPointsPerAxis * m_meshSettings.numPointsPerAxis * m_meshSettings.numPointsPerAxis;
            m_numVoxelsPerAxis = m_meshSettings.numPointsPerAxis - 1;
            m_numVoxels = m_numVoxelsPerAxis * m_numVoxelsPerAxis * m_numVoxelsPerAxis;
            m_maxTriangleCount = m_numVoxels * 5;
            m_maxVertexCount = m_maxTriangleCount * 3;

            m_triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            m_triangleDataBuffer = new ComputeBuffer(m_maxTriangleCount, ComputeHelper.GetStride<TriangleData>(), ComputeBufferType.Append);
            m_positionPointsBuffer = new ComputeBuffer(m_numPoints, ComputeHelper.GetStride<float3>(), ComputeBufferType.Structured);
            m_densityPointsBuffer = new ComputeBuffer(m_numPoints, ComputeHelper.GetStride<float>(), ComputeBufferType.Structured);
        }

        public void ReleaseBuffers()
        {
            ComputeHelper.Release(m_triangleDataBuffer, m_triCountBuffer, m_positionPointsBuffer, m_densityPointsBuffer);
        }

        /* Generate the density of each point using perlin noise */
        public void GenerateDensity()
        {
            int numThreadsPerAxis = Mathf.CeilToInt(m_meshSettings.numPointsPerAxis / (float)_threadGroupSize);

            Vector3 center = m_center;
            Vector3 worldBounds = new Vector3(m_meshSettings.numChunks.x, m_meshSettings.numChunks.y, m_meshSettings.numChunks.z) * m_meshSettings.boundsSize;

            // Points buffer is populated inside shader with pos (xyz) + density (w).
            // Set paramaters
            m_densityComputeShader.SetBuffer(0, "positionPoints", m_positionPointsBuffer);
            m_densityComputeShader.SetBuffer(0, "densityPoints", m_densityPointsBuffer);
            m_densityComputeShader.SetInt("numPointsPerAxis", m_meshSettings.numPointsPerAxis);
            m_densityComputeShader.SetFloat("boundsSize", m_meshSettings.boundsSize);
            m_densityComputeShader.SetVector("centre", new Vector4(center.x, center.y, center.z));
            m_densityComputeShader.SetFloat("spacing", m_pointSpacing);
            m_densityComputeShader.SetVector("worldSize", worldBounds);
            m_densityComputeShader.SetFloat("isoLevel", m_meshSettings.isoLevel);

            // Noise parameters
            var prng = new System.Random(m_noiseSettings.seed);
            var offsets = new Vector3[m_noiseSettings.numOctaves];
            float offsetRange = 1000;
            for (int i = 0; i < m_noiseSettings.numOctaves; i++)
            {
                offsets[i] = new Vector3((float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1) * offsetRange;
            }
            var offsetsBuffer = new ComputeBuffer(offsets.Length, sizeof(float) * 3);
            offsetsBuffer.SetData(offsets);

            m_densityComputeShader.SetVector("offset", new Vector4(m_noiseSettings.offset.x, m_noiseSettings.offset.y, m_noiseSettings.offset.z));
            m_densityComputeShader.SetInt("octaves", Mathf.Max(1, m_noiseSettings.numOctaves));
            m_densityComputeShader.SetFloat("lacunarity", m_noiseSettings.lacunarity);
            m_densityComputeShader.SetFloat("persistence", m_noiseSettings.persistence);
            m_densityComputeShader.SetFloat("noiseScale", m_noiseSettings.noiseScale);
            m_densityComputeShader.SetFloat("noiseWeight", m_noiseSettings.noiseWeight);
            m_densityComputeShader.SetBuffer(0, "offsets", offsetsBuffer);
            m_densityComputeShader.SetFloat("floorOffset", m_noiseSettings.floorOffset);
            m_densityComputeShader.SetFloat("weightMultiplier", m_noiseSettings.weightMultiplier);

            m_densityComputeShader.SetVector("params", m_noiseSettings.shaderParams);

            // Dispatch shader
            m_densityComputeShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

            offsetsBuffer.Release();

            // Get positions and stock them for Marching Cubes Algorithm and Terraforming
            float3[] posPoints = new float3[m_numPoints];
            m_positionPointsBuffer.GetData(posPoints, 0, 0, m_numPoints);

            // Get positions and stock them for Marching Cubes Algorithm and Terraforming
            float[] denPoints = new float[m_numPoints];
            m_densityPointsBuffer.GetData(denPoints, 0, 0, m_numPoints);

            for (int i = 0; i < m_numPoints; i++)
            {
                m_positionNativeArray[i] = posPoints[i];
                m_densityNativeArray[i] = denPoints[i];
                m_newDensityNativeArray[i] = denPoints[i];
            }

            UpdateChunkMesh(m_densityNativeArray);
        }

        // Utiliser le Job system pour éviter de faire des passages entre GPU et CPU (Il faut générer le collision mesh
        public void UpdateChunkMesh(NativeArray<float> densityPoints)
        {
            // COMMENT UPDATE LE MESH SANS REPASSER PAR LE CPU ?
            // (hypothese : Graphics.RenderPrimitives();)
            // OU
            // COMMENT NE PAS PASSER PAR LE GPU ET GARDER LES PERFS ?
            // (hypothese : JobSystem)

            int marchKernel = 0;
            m_triangleDataBuffer.SetCounterValue(0);
            m_positionPointsBuffer.SetData(m_positionNativeArray.ToArray());
            m_densityPointsBuffer.SetData(densityPoints.ToArray());

            m_meshComputeShader.SetBuffer(marchKernel, "triangles", m_triangleDataBuffer);
            m_meshComputeShader.SetBuffer(marchKernel, "positionPoints", m_positionPointsBuffer);
            m_meshComputeShader.SetBuffer(marchKernel, "densityPoints", m_densityPointsBuffer);
            m_meshComputeShader.SetInt("numPointsPerAxis", m_meshSettings.numPointsPerAxis);
            m_meshComputeShader.SetFloat("isoLevel", m_meshSettings.isoLevel);

            ComputeHelper.Dispatch(m_meshComputeShader, m_numVoxelsPerAxis, m_numVoxelsPerAxis, m_numVoxelsPerAxis, marchKernel);

            // Get number of triangles in the triangle buffer
            ComputeBuffer.CopyCount(m_triangleDataBuffer, m_triCountBuffer, 0);
            int[] triCountArray = new int[1];
            m_triCountBuffer.GetData(triCountArray);
            int numTris = triCountArray[0];

            // Get triangle data from ComputeShader
            TriangleData[] trianglesDataArray = new TriangleData[numTris];
            m_triangleDataBuffer.GetData(trianglesDataArray, 0, 0, numTris);

            Vector3[] vertices = new Vector3[numTris * 3];
            int[] triangles = new int[numTris * 3];

            for (int i = 0; i < numTris; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    triangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = trianglesDataArray[i][j];
                }
            }

            if (vertices.Length > 0)
            {
                AssignNewMesh(vertices, triangles);
            }
        }

        // UNUSED
        public void TerraformChunkMesh(Vector3 brushCenter, int weight, float brushRadius, float brushPower)
        {
            // Terraform
            Vector3 center = m_center;
            m_densityPointsBuffer.SetData(m_newDensityNativeArray);

            m_terraformComputeShader.SetBuffer(0, "densityPoints", m_densityPointsBuffer);
            m_terraformComputeShader.SetInt("numPointsPerAxis", m_meshSettings.numPointsPerAxis);
            m_terraformComputeShader.SetFloat("boundsSize", m_meshSettings.boundsSize);
            m_terraformComputeShader.SetVector("centre", new Vector4(center.x, center.y, center.z));
            m_terraformComputeShader.SetFloat("spacing", m_pointSpacing);
            m_terraformComputeShader.SetInt("weight", weight);
            m_terraformComputeShader.SetFloat("deltaTime", Time.deltaTime); // NetworkManager.Singleton.LocalTime.TimeAsFloat
            m_terraformComputeShader.SetFloats("brushCentre", brushCenter.x, brushCenter.y, brushCenter.z);
            m_terraformComputeShader.SetFloat("brushRadius", brushRadius);
            m_terraformComputeShader.SetFloat("brushPower", brushPower);
            m_terraformComputeShader.SetFloat("isoLevel", m_meshSettings.isoLevel);

            ComputeHelper.Dispatch(m_terraformComputeShader, m_numVoxelsPerAxis, m_numVoxelsPerAxis, m_numVoxelsPerAxis);

            int numPoints = m_meshSettings.numPointsPerAxis * m_meshSettings.numPointsPerAxis * m_meshSettings.numPointsPerAxis;
            float[] pointsData = new float[numPoints];
            m_densityPointsBuffer.GetData(pointsData, 0, 0, numPoints);

            // Set data in the array
        }

        public void UpdateDensityPoint(Vector3 brushCenter, int weight, float brushRadius, float brushPower)
        {
            if (IsOwner)
            {
                //Terraform chunk mesh directly onto the Owner;
            }
            else
            {
                //Send Update to Server -> Client to terraform the chunk mesh;
            }
            UpdateDensityPointServerRpc(brushCenter, weight, brushRadius, brushPower);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateDensityPointServerRpc(Vector3 brushCenter, int weight, float brushRadius, float brushPower, ServerRpcParams serverRpcParams = default)
        {
            // Send works to clients
            UpdateDensityPointClientRpc(brushCenter, weight, brushRadius, brushPower, serverRpcParams.Receive.SenderClientId);
        }

        [ClientRpc]
        void UpdateDensityPointClientRpc(Vector3 brushCenter, int weight, float brushRadius, float brushPower, ulong senderClientId)
        {
            //if (senderClientId == OwnerClientId) return;

            TerraformingData data = new TerraformingData
            {
                brushCenter = brushCenter,
                brushRadius = brushRadius,
                brushPower =  brushPower,
                weight =      weight
            };
            m_terraformingDataList.Add(data);

            m_isTerraformed = true;
        }

        public void AssignNewMesh(Vector3[] vertices, int[] triangles)
        {
            m_mesh.Clear();
            m_mesh.vertices = vertices;
            m_mesh.triangles = triangles;
            m_mesh.RecalculateNormals();

            meshCollider.sharedMesh = m_mesh;
            SetCollider();
        }

        public void SetCollider()
        {
            // force update
            meshCollider.enabled = false;
            meshCollider.enabled = true;
        }

        public void SetMaterial(Material material)
        {
            meshRenderer.material = material;
        }

        /***********/
        /* DESRTOY */
        /***********/
        public void DestroyOrDisable()
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject, false);
            }
        }

        public override void OnDestroy()
        {
            ReleaseBuffers();

            if (m_positionNativeArray.IsCreated) m_positionNativeArray.Dispose();
            if (m_densityNativeArray.IsCreated) m_densityNativeArray.Dispose();
            if (m_newDensityNativeArray.IsCreated) m_newDensityNativeArray.Dispose();
            if (m_terraformingDataNativeArray.IsCreated) m_terraformingDataNativeArray.Dispose();
        }
    }
}
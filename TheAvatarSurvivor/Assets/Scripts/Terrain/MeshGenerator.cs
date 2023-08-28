using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace DoDo.Terrain
{
    public class MeshGenerator : MonoBehaviour
    {
        const int threadGroupSize = 2;

        [Header("Noise Settings")]
        [SerializeField] ComputeShader densityComputeShader;
        [SerializeField] NoiseSettings noiseSettings;

        [Header("Voxel Settings")]
        [SerializeField] ComputeShader meshComputeShader;

        [Header("Terraform Shader")]
        [SerializeField] ComputeShader terraformComputeShader;

        Vector3Int numChunks;
        float boundsSize;
        float isoLevel;
        int numPointsPerAxis;

        NativeArray<float3> positionNativeArray;
        NativeArray<float> densityNativeArray, newDensityNativeArray;

        // Buffers
        ComputeBuffer triangleBuffer;
        ComputeBuffer positionPointsBuffer;
        ComputeBuffer densityPointsBuffer;
        ComputeBuffer triCountBuffer;

        int numPoints;
        int numVoxelsPerAxis;
        int numVoxels;
        int maxTriangleCount;
        int maxVertexCount;

        public void Setup(Vector3Int numChunks, float boundsSize, float isoLevel, int numPointsPerAxis)
        {
            this.numChunks = numChunks;
            this.boundsSize = boundsSize;
            this.isoLevel = isoLevel;
            this.numPointsPerAxis = numPointsPerAxis;

            CreateBuffers();

            positionNativeArray = new NativeArray<float3>(numPoints, Allocator.Persistent);
            densityNativeArray = new NativeArray<float>(numPoints, Allocator.Persistent);
            newDensityNativeArray = new NativeArray<float>(numPoints, Allocator.Persistent);
        }

        /* Size all buffers depending on the size of the number of points on each axes */
        void CreateBuffers()
        {
            numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
            numVoxelsPerAxis = numPointsPerAxis - 1;
            numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
            maxTriangleCount = numVoxels * 5;
            maxVertexCount = maxTriangleCount * 3;

            triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            triangleBuffer = new ComputeBuffer(maxTriangleCount, ComputeHelper.GetStride<TriangleData>(), ComputeBufferType.Append);
            positionPointsBuffer = new ComputeBuffer(numPoints, ComputeHelper.GetStride<float3>(), ComputeBufferType.Structured);
            densityPointsBuffer = new ComputeBuffer(numPoints, ComputeHelper.GetStride<float>(), ComputeBufferType.Structured);
        }

        public void ReleaseBuffers()
        {
            ComputeHelper.Release(triangleBuffer, triCountBuffer, positionPointsBuffer, densityPointsBuffer);
        }

        /* Generate the density of each point using perlin noise */
        public PointData[] GenerateDensity(Chunk chunk)
        {
            int numThreadsPerAxis = Mathf.CeilToInt(numPointsPerAxis / (float)threadGroupSize);

            float spacing = boundsSize / (numPointsPerAxis - 1);
            Vector3 center = chunk.m_center;

            Vector3 worldBounds = new Vector3(numChunks.x, numChunks.y, numChunks.z) * boundsSize;

            // Points buffer is populated inside shader with pos (xyz) + density (w).
            // Set paramaters
            densityComputeShader.SetBuffer(0, "positionPoints", positionPointsBuffer);
            densityComputeShader.SetBuffer(0, "densityPoints", densityPointsBuffer);
            densityComputeShader.SetInt("numPointsPerAxis", numPointsPerAxis);
            densityComputeShader.SetFloat("boundsSize", boundsSize);
            densityComputeShader.SetVector("centre", new Vector4(center.x, center.y, center.z));
            densityComputeShader.SetFloat("spacing", spacing);
            densityComputeShader.SetVector("worldSize", worldBounds);
            densityComputeShader.SetFloat("isoLevel", isoLevel);

            // Noise parameters
            var prng = new System.Random(noiseSettings.seed);
            var offsets = new Vector3[noiseSettings.numOctaves];
            float offsetRange = 1000;
            for (int i = 0; i < noiseSettings.numOctaves; i++)
            {
                offsets[i] = new Vector3((float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1) * offsetRange;
            }
            var offsetsBuffer = new ComputeBuffer(offsets.Length, sizeof(float) * 3);
            offsetsBuffer.SetData(offsets);

            densityComputeShader.SetVector("offset", new Vector4(noiseSettings.offset.x, noiseSettings.offset.y, noiseSettings.offset.z));
            densityComputeShader.SetInt("octaves", Mathf.Max(1, noiseSettings.numOctaves));
            densityComputeShader.SetFloat("lacunarity", noiseSettings.lacunarity);
            densityComputeShader.SetFloat("persistence", noiseSettings.persistence);
            densityComputeShader.SetFloat("noiseScale", noiseSettings.noiseScale);
            densityComputeShader.SetFloat("noiseWeight", noiseSettings.noiseWeight);
            densityComputeShader.SetBuffer(0, "offsets", offsetsBuffer);
            densityComputeShader.SetFloat("floorOffset", noiseSettings.floorOffset);
            densityComputeShader.SetFloat("weightMultiplier", noiseSettings.weightMultiplier);

            densityComputeShader.SetVector("params", noiseSettings.shaderParams);

            // Dispatch shader
            densityComputeShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

            offsetsBuffer.Release();

            // Get positions and stock them for Marching Cubes Algorithm and Terraforming
            float3[] posPoints = new float3[numPoints];
            positionPointsBuffer.GetData(posPoints, 0, 0, numPoints);

            // Get positions and stock them for Marching Cubes Algorithm and Terraforming
            float[] denPoints = new float[numPoints];
            densityPointsBuffer.GetData(denPoints, 0, 0, numPoints);
            //chunk.SetPointsData(points);

            PointData[] pointDatas = new PointData[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                pointDatas[i].position = posPoints[i];
                pointDatas[i].density = denPoints[i];
            }

            return pointDatas;
        }

        public void UpdateChunkMesh(Chunk chunk)
        {
            //int numVoxelsPerAxis = numPointsPerAxis - 1;

            //int marchKernel = 0;
            //triangleBuffer.SetCounterValue(0);
            //positionPointsBuffer.SetData(positionNativeArray);
            //densityPointsBuffer.SetData(newDensityNativeArray);

            //meshComputeShader.SetBuffer(marchKernel, "triangles", triangleBuffer);
            //meshComputeShader.SetBuffer(marchKernel, "positionPoints", positionPointsBuffer);
            //meshComputeShader.SetBuffer(marchKernel, "densityPoints", densityPointsBuffer);
            //meshComputeShader.SetInt("numPointsPerAxis", numPointsPerAxis);
            //meshComputeShader.SetFloat("isoLevel", isoLevel);

            //ComputeHelper.Dispatch(meshComputeShader, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis, marchKernel);

            //// Get number of triangles in the triangle buffer
            //ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
            //int[] triCountArray = new int[1];
            //triCountBuffer.GetData(triCountArray);
            //int numTris = triCountArray[0];
            //// Get triangle data from shader
            //TriangleData[] trianglesDataArray = new TriangleData[numTris];
            //triangleBuffer.GetData(trianglesDataArray, 0, 0, numTris);

            //Vector3[] vertices = new Vector3[numTris * 3];
            //int[] triangles = new int[numTris * 3];

            //for (int i = 0; i < numTris; i++)
            //{
            //    for (int j = 0; j < 3; j++)
            //    {
            //        triangles[i * 3 + j] = i * 3 + j;
            //        vertices[i * 3 + j] = trianglesDataArray[i][j];
            //    }
            //}

            //chunk.AssignNewMesh(vertices, triangles);
        }


        public void TerraformChunkMesh(Chunk chunk, Vector3 brushCenter, int weight, float brushRadius, float brushPower, bool isAddingMatter)
        {
            int numVoxelsPerAxis = numPointsPerAxis - 1;
            float pointSpacing = boundsSize / numVoxelsPerAxis;

            // Terraform
            Vector3 center = chunk.m_center;
            densityPointsBuffer.SetData(newDensityNativeArray);

            terraformComputeShader.SetBuffer(0, "densityPoints", densityPointsBuffer);
            terraformComputeShader.SetInt("numPointsPerAxis", numPointsPerAxis);
            terraformComputeShader.SetFloat("boundsSize", boundsSize);
            terraformComputeShader.SetVector("centre", new Vector4(center.x, center.y, center.z));
            terraformComputeShader.SetFloat("spacing", pointSpacing);
            terraformComputeShader.SetInt("weight", weight);
            terraformComputeShader.SetFloat("deltaTime", Time.deltaTime);
            terraformComputeShader.SetFloats("brushCentre", brushCenter.x, brushCenter.y, brushCenter.z);
            terraformComputeShader.SetFloat("brushRadius", brushRadius);
            terraformComputeShader.SetFloat("brushPower", brushPower);
            terraformComputeShader.SetFloat("isoLevel", isoLevel);

            ComputeHelper.Dispatch(terraformComputeShader, numVoxelsPerAxis, numVoxelsPerAxis, numVoxelsPerAxis);

            int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
            float[] pointsData = new float[numPoints];
            densityPointsBuffer.GetData(pointsData, 0, 0, numPoints);
            //chunk.SetPointsData(pointsData);

            //UpdateChunkMesh(chunk);

        }

        private void OnDestroy()
        {
            ReleaseBuffers();
        }
    }
}
using System;
using System.Security.Policy;
using Unity.Netcode;
using UnityEngine;

namespace DoDo.Terrain
{
    public class Chunk : NetworkBehaviour
    {
        public Vector3Int coord;
        public Vector3 center;
        Bounds bounds;
        public float size;
        public Mesh mesh;
        public PointData[] pointsData;

        GameObject chunkObj;
        MeshGenerator generator;

        public bool hasBeenTerraformed = false;
        bool hasSetCollider = false;
        float maxViewDst;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;

        public void Setup(Vector3Int coord, Vector3 center, float size, Vector3Int numChunks, float visibleDstThreshold, GameObject chunkObj, MeshGenerator meshGenerator)
        {
            this.coord = coord;
            this.center = center;
            this.size = size;
            this.chunkObj = chunkObj;
            this.generator = meshGenerator;
            bounds = new Bounds(center, Vector3.one * size);
            maxViewDst = visibleDstThreshold;

            mesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };

            // Mesh rendering and collision components
            if (!chunkObj.TryGetComponent<MeshFilter>(out meshFilter))
            {
                meshFilter = chunkObj.AddComponent<MeshFilter>();
            }
            meshFilter.mesh = mesh;

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
        }

        public void UpdateVisibleChunks(Vector3 viewerPosition)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (wasVisible != visible)
            {
                SetVisible(visible);
            }

            if (visible)
            {
                if (hasBeenTerraformed)
                {
                    generator.UpdateChunkMesh(this);
                    hasBeenTerraformed = false;
                }

                if (!hasSetCollider)
                {
                    SetCollider();
                    hasSetCollider = true;
                }
            }
            else
            {
                hasSetCollider = false;
            }
        }

        public void AssignMesh(Vector3[] vertices, int[] triangles)
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            meshCollider.sharedMesh = null;
            SetCollider();
        }
        public void SetCollider()
        {
            meshCollider.sharedMesh = mesh;
            // force update
            meshCollider.enabled = false;
            meshCollider.enabled = true;
        }

        public void SetPointsData(PointData[] points, int numPoints)
        {
            pointsData = new PointData[numPoints];
            pointsData = points;
        }
        public PointData[] GetPointsData()
        {
            return pointsData;
        }

        public void SetMaterial(Material material)
        {
            meshRenderer.material = material;
        }

        public void SetVisible(bool visible)
        {
            chunkObj.SetActive(visible);
        }

        public bool IsVisible()
        {
            return chunkObj.activeSelf;
        }

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



        // SERIALIZATION EXTENSION
        //public static void ReadValueSafe(this FastBufferReader reader, out Chunk chunk)
        //{
        //    reader.ReadValueSafe(out string val);
        //    chunk = new Chunk(val);
        //}

        //public static void WriteValueSafe(this FastBufferWriter writer, in Chunk chunk)
        //{
        //    writer.WriteValueSafe(chunk.Value);
        //}
    }
}
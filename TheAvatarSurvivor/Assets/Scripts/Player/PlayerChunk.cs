using DoDo.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;
using Unity.VisualScripting;

namespace DoDo.Player
{
    public class PlayerChunk : NetworkBehaviour
    {
        const float viewerMoveThresholdForChunkUpdate = 3f;
        const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

        [SerializeField] Material materialValid;
        [SerializeField] Material materialInvalid;

        Vector3 viewerOldPosition;
        int chunksVisibleInViewDst;
        MeshSettings meshSettings;

        /*******************************************/
        /*              Unity Methods              */
        /*******************************************/
        private void Start()
        {
            meshSettings = TerrainGenerator.Instance.GetMeshSettings();
            //meshSettings = TestTerrainGenerator.Instance.GetMeshSettings();
            chunksVisibleInViewDst = Mathf.RoundToInt(meshSettings.visibleDstThreshold);
        }

        void Update()
        {
            if (!IsOwner) return;

            Vector3 viewerPosition = transform.position;

            if ((viewerOldPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
            {
                viewerOldPosition = viewerPosition;
                //StartCoroutine(UpdateNearbyChunks(viewerPosition));
            }
        }

        /*******************************************/
        /*             Private Methods             */
        /*******************************************/
        private IEnumerable<Vector3> GetUpdatableChunks(Vector3 pos, Vector3 offsetDistance)
        {
            //Debug.Log("pos : " + pos);
            //Debug.Log("offsetDistance : " + offsetDistance);
            //// Divide the viewer position by number of chunk to get the relative center position of the player in relation to the center chunks
            //// Round up to the nearest integer to obtain a chunk coordinate
            //int currentChunkCenterX = Mathf.RoundToInt(pos.x / meshSettings.numChunks.x);
            //int currentChunkCenterY = Mathf.RoundToInt(pos.y / meshSettings.numChunks.y);
            //int currentChunkCenterZ = Mathf.RoundToInt(pos.z / meshSettings.numChunks.z);
            //Vector3 viewerChunkCenter = new(currentChunkCenterX, currentChunkCenterY, currentChunkCenterZ);
            //Debug.Log("viewerChunkCenter : " + viewerChunkCenter);

            //Vector3 viewerChunkCoord = ChunkCoordFromCenter(viewerChunkCenter);
            //Debug.Log("viewerChunkCoord : " + viewerChunkCoord);
            //int currentChunkCoordX = Mathf.RoundToInt(viewerChunkCoord.x);
            //int currentChunkCoordY = Mathf.RoundToInt(viewerChunkCoord.y);
            //int currentChunkCoordZ = Mathf.RoundToInt(viewerChunkCoord.z);
            //Vector3 viewerChunkCoordRounded = new(currentChunkCoordX, currentChunkCoordY, currentChunkCoordZ);
            //Debug.Log("viewerChunkCoordRounded : " + viewerChunkCoordRounded);

            //for (int xOffset = -(int)offsetDistance.x; xOffset <= offsetDistance.x; xOffset++)
            //{
            //    for (int yOffset = -(int)offsetDistance.y; yOffset <= offsetDistance.y; yOffset++)
            //    {
            //        for (int zOffset = -(int)offsetDistance.z; zOffset <= offsetDistance.z; zOffset++)
            //        {
            //            yield return viewerChunkCoordRounded + new Vector3(xOffset, yOffset, zOffset);
            //        }
            //    }
            //}

            // Cross product between the number of chunks X, Y, Z and the given position relative to the maximum boundsSize X, Y, Z
            float currentChunkCoordX = CenterToChunkCoord(pos.x);
            float currentChunkCoordY = CenterToChunkCoord(pos.y);
            float currentChunkCoordZ = CenterToChunkCoord(pos.z);

            int currentChunkCoordXRounded = Mathf.RoundToInt(currentChunkCoordX);
            int currentChunkCoordYRounded = Mathf.RoundToInt(currentChunkCoordY);
            int currentChunkCoordZRounded = Mathf.RoundToInt(currentChunkCoordZ);
            Vector3 viewerChunkCoordRounded = new(currentChunkCoordXRounded, currentChunkCoordYRounded, currentChunkCoordZRounded);

            for (int xOffset = -(int)offsetDistance.x; xOffset <= offsetDistance.x; xOffset++)
            {
                for (int yOffset = -(int)offsetDistance.y; yOffset <= offsetDistance.y; yOffset++)
                {
                    for (int zOffset = -(int)offsetDistance.z; zOffset <= offsetDistance.z; zOffset++)
                    {
                        yield return viewerChunkCoordRounded + new Vector3(xOffset, yOffset, zOffset);
                    }
                }
            }
        }

        float CenterToChunkCoord(float pos)
        {
            return (meshSettings.numChunks.x - 1) * (pos + (meshSettings.numChunks.x * meshSettings.boundsSize / 2)) / (meshSettings.numChunks.x * meshSettings.boundsSize);
        }

        Vector3 ChunkCoordFromCenter(Vector3 center)
        {
            return new Vector3(CoordFromCenter(center.x, meshSettings.numChunks.x),
                               CoordFromCenter(center.y, meshSettings.numChunks.y),
                               CoordFromCenter(center.z, meshSettings.numChunks.z));
            //(center + (Vector3.one * (numChunks - 1))) * (numChunks / boundsSize);
        }
        float CoordFromCenter(float center, float chunkSide)
        {
            return center + (chunkSide - 1) * (chunkSide / meshSettings.boundsSize);
        }

        /******************************************/
        /*             Public Methods             */
        /******************************************/
        public IEnumerator UpdateNearbyChunks(Vector3 viewerPosition)
        {
            Vector3 offsetVector = new( Mathf.CeilToInt(chunksVisibleInViewDst),
                                        Mathf.CeilToInt(chunksVisibleInViewDst),
                                        Mathf.CeilToInt(chunksVisibleInViewDst) );

            foreach (Vector3 chunkCoord in GetUpdatableChunks(viewerPosition, offsetVector))
            {
                if (TerrainGenerator.Instance.IsChunkCoordInDictionary(chunkCoord))
                {
                    //Chunk chunk = TerrainGenerator.Instance.GetChunk(chunkCoord);
                    //chunk.UpdateVisibleChunks(viewerPosition);
                    //chunk.CheckTerraforming();
                }
            }
            yield return null;
        }

        public void Terraform(Vector3 brushCenter, int weight, float brushRadius, float brushPower)
        {
            Vector3 offsetVector = (Vector3)meshSettings.numChunks / meshSettings.boundsSize;
            Vector3 impactRatio = new( Mathf.CeilToInt(brushRadius / offsetVector.x), 
                                       Mathf.CeilToInt(brushRadius / offsetVector.y),
                                       Mathf.CeilToInt(brushRadius / offsetVector.z) );

            foreach (Vector3 chunkCoord in GetUpdatableChunks(brushCenter, impactRatio))
            {
                if (TerrainGenerator.Instance.IsChunkCoordInDictionary(chunkCoord))
                {
                    Chunk chunk = TerrainGenerator.Instance.GetChunk(chunkCoord);
                    chunk.SetMaterial(materialInvalid);

                    if (!MathUtility.SphereIntersectsBox(brushCenter, brushRadius, TerrainGenerator.CentreFromCoord(chunkCoord, meshSettings.numChunks, meshSettings.boundsSize), Vector3.one * meshSettings.boundsSize)) 
                        continue;

                    chunk = TerrainGenerator.Instance.GetChunk(chunkCoord);
                    //chunk.TerraformChunkMesh(brushCenter, weight, brushRadius, brushPower);
                    chunk.UpdateDensityPoint(brushCenter, weight, brushRadius, brushPower);
                    //chunk.SetMaterial(materialValid);
                    //TerrainGenerator.Instance.TerraformOnServer(chunkCoord, brushCenter, weight, brushRadius, brushPower, isAddingMatter);
                }
            }
        }
    }
}
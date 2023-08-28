﻿using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace DoDo.Terrain
{
    [BurstCompile]
    struct TerraformChunkJob : IJobParallelFor
    {
        public NativeArray<float> _originalPoints;
        [ReadOnly] public int _numPointsPerAxis;
        [ReadOnly] public float _boundsSize;
        [ReadOnly] public float _isoLevel;
        [ReadOnly] public float _pointSpacing;
        [ReadOnly] public float3 _chunkCenter;
        [ReadOnly] public float _deltaTime;
        [ReadOnly] public NativeArray<TerraformingData> _terraformData;

        [WriteOnly] public NativeArray<float> _terraformedPoints;

        [BurstCompile]
        private int3 CoordFromIndex(int i)
        {
            int3 coord;
            coord.z = i / (_numPointsPerAxis * _numPointsPerAxis);
            i -= (int)(coord.z * _numPointsPerAxis * _numPointsPerAxis);
            coord.y = i / _numPointsPerAxis;
            coord.x = i % _numPointsPerAxis;
            return coord;
        }

        [BurstCompile]
        public void Execute(int index)
        {
            for (int i = 0; i < _terraformData.Length; i++)
            {
                float3 pos = _chunkCenter + (float3)CoordFromIndex(index) * _pointSpacing - _boundsSize / 2;
                float3 offset = pos - _terraformData[i].brushCenter;
                float sqrDst = math.dot(offset, offset);

                if (sqrDst <= _terraformData[i].brushRadius * _terraformData[i].brushRadius)
                {
                    float dst = math.sqrt(sqrDst);
                    dst = math.clamp((dst - (_terraformData[i].brushRadius * 0.5f)) / (_terraformData[i].brushRadius - (_terraformData[i].brushRadius * 0.5f)), 0, 1);
                    float brushWeight = 1 - (dst * dst * (3 - 2 * dst));

                    float result = _originalPoints[index];
                    result += _terraformData[i].weight * _deltaTime * brushWeight * _terraformData[i].brushPower;

                    float maxIsoLevel = _isoLevel * 2f;
                    float minIsoLevel = 0f;
                    if (result > maxIsoLevel)
                    {
                        result = maxIsoLevel;
                    }
                    else if (result < minIsoLevel)
                    {
                        result = minIsoLevel;
                    }

                    _originalPoints[index] = result;
                    _terraformedPoints[index] = result;
                }
            }
        }
    }

    public struct TerraformingData
    {
        public float3 brushCenter;
        public float  brushRadius;
        public float  brushPower;
        public int    weight;
    }
}
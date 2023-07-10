using UnityEngine;
using Unity.Netcode;
using System;

public struct PointData : IEquatable<PointData>, INetworkSerializable
{
    public Vector3 position;
    public float density;

    public bool Equals(PointData other)
    {
        return
            position == other.position &&
            density == other.density;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        //serializer.SerializeValue(ref position);
        //serializer.SerializeValue(ref density);

        if (serializer.IsWriter)
        {
            serializer.GetFastBufferWriter().WriteValueSafe(position);
            serializer.GetFastBufferWriter().WriteValueSafe(density);
        }
        else
        {
            serializer.GetFastBufferReader().ReadValueSafe(out position);
            serializer.GetFastBufferReader().ReadValueSafe(out density);
        }
    }
}
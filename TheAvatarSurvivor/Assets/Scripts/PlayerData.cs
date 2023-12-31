using System;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int colorId;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;
    public bool isHost;

    public bool Equals(PlayerData other)
    {
        return
            clientId == other.clientId &&
            colorId == other.colorId &&
            playerName == other.playerName &&
            playerId == other.playerId &&
            isHost == other.isHost;
    }

    public bool NotEqual(PlayerData other)
    {
        return
            clientId != other.clientId ||
            colorId != other.colorId ||
            playerName != other.playerName ||
            playerId != other.playerId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref isHost);
    }

}
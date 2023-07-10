using UnityEngine;

namespace DoDo.Core
{
    public enum EObjectType
    {
        Terrain,
        Tree,
        Grass,
        Player,
        Enemy,
        NPC,
        Slime,
        Goblin,
        Orc,
        Wolf,

    }

    public class ObjectType : MonoBehaviour
    {
        [SerializeField] EObjectType objectType;

        public EObjectType GetObjectType() => objectType;
        public EObjectType SetObjectType(EObjectType pObjType) => objectType = pObjType;
    }
}
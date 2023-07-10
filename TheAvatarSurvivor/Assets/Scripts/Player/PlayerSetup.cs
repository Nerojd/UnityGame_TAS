using DoDo.Player.Control;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace DoDo.Player
{
    [RequireComponent(typeof(Player))]
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerSetup : NetworkBehaviour
    {
        [SerializeField] Behaviour[] componantsToDisable;

        // Start is called before the first frame update
        void Start()
        {
            // loop on all components that are not our player's components and disable them
            if (!IsOwner)
            {
                DisableComponents();
                AssignRemoteLayer();
            }
            else
            {
                // disable graphic parts of the local player (for weapon)

                // instantiate player UI for the local player

                // configuration player UI

                // stock the disabled components for the death
                GetComponent<Player>().Setup();
            }
        }

        [ServerRpc]
        void SetUsernameServerRpc(string playerID, string username)
        {
            //Player player = GameManager.GetPlayer(playerID);
            //if (player == null) return;

            //Debug.Log("username : " + username);
            //player.username = username;
        }


        public void StartClient()
        {
            // Todo override when connecting 

            RegisterPlayerAndSetUsername();

            AddPlayerGameManagerServerRpc();
        }

        // Use when the build is for server (and host) only
        public void OnStartServer()
        {
            RegisterPlayerAndSetUsername();
        }

        private void RegisterPlayerAndSetUsername()
        {
            string netObjId = GetComponent<NetworkObject>().NetworkObjectId.ToString();
            Player player = GetComponent<Player>();

            //GameManager.RegisterPlayer(netObjId, player);

            string username = UserAccountManager.Instance.GetPlayerName();
            SetUsernameServerRpc(transform.name, username);
        }

        [ServerRpc]
        public void AddPlayerGameManagerServerRpc()
        {
            PlayerGameManagerClientRpc(true);
        }

        [ServerRpc]
        public void RemovePlayerGameManagerServerRpc()
        {
            PlayerGameManagerClientRpc(false);
        }

        [ClientRpc]
        public void PlayerGameManagerClientRpc(bool isAlive)
        {
            GameManager.Instance.PlayerAlive(isAlive);
        }

        void AssignRemoteLayer()
        {
            // Change the layer of each remote player
            //gameObject.layer = LayerMask.NameToLayer(Utils.remoteLayerName);
        }

        void DisableComponents()
        {
            for (int i = 0; i < componantsToDisable.Length; i++)
            {
                componantsToDisable[i].enabled = false;
            }
        }


        void OnDisable()
        {
            //Destroy(playerUIInstance);
        }
    }

}
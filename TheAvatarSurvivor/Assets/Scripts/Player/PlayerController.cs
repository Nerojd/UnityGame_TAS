using DoDo.Core;
using Unity.Netcode;
using UnityEngine;

namespace DoDo.Player.Core
{
    public class PlayerController : MonoBehaviour
    {

        /*******************************************/
        /*              Unity Methods              */
        /*******************************************/
        void Update()
        {
            if (PauseMenu.isOn)
            {
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                }

                return;
            }
        }
       
        /*******************************************/
        /*             Private Methods             */
        /*******************************************/
       

        /******************************************/
        /*             Public Methods             */
        /******************************************/

    }
}
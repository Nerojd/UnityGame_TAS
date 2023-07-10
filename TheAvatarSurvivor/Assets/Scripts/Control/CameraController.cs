using System.Globalization;
using Unity.Netcode;
using UnityEngine;

namespace DoDo.Control
{
    public class CameraController : MonoBehaviour
    {
        [Header("TRANSFORMS")]
        [SerializeField]
        private Transform cameraRotation;
        [SerializeField]
        private Transform playerOrientation;

        [Header("PARAMETERS")]
        [SerializeField]
        private float mouseSensitivityX = 5.5f;
        [SerializeField]
        private float mouseSensitivityY = 5f;
        [SerializeField]
        private float maxPitch = 80f;
        [SerializeField]
        private float minPitch = 80f;
        [SerializeField]
        private float smoothing = 2.0f;     // Lissage des mouvements

        private Vector2 mouseLook;         // Rotation de la caméra
        private Vector2 smoothV;           // Vecteur lissé

        /*******************************************/
        /*              Unity Methods              */
        /*******************************************/
        void Update()
        {
            // Obtenir le mouvement de la souris
            Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Appliquer la sensibilité à la rotation
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(mouseSensitivityX, mouseSensitivityY));

            // Lissage des mouvements
            smoothV.x = Mathf.Lerp(smoothV.x, mouseDelta.x, 1f / smoothing);
            smoothV.y = Mathf.Lerp(smoothV.y, mouseDelta.y, 1f / smoothing);
            mouseLook += smoothV;

            // Rotation de la caméra
            mouseLook.y = Mathf.Clamp(mouseLook.y, -minPitch, maxPitch);
            cameraRotation.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
            // Rotation du player
            playerOrientation.localRotation = Quaternion.AngleAxis(mouseLook.x, Vector3.up);
        }


        /*******************************************/
        /*             Private Methods             */
        /*******************************************/

        /******************************************/
        /*             Public Methods             */
        /******************************************/
        public void ChangeMouseSensitivityX(float pSensitivityX)
        {
            mouseSensitivityX = pSensitivityX;
        }
        public void ChangeMouseSensitivityY(float pSensitivityY)
        {
            mouseSensitivityY = pSensitivityY;
        }
    }
}
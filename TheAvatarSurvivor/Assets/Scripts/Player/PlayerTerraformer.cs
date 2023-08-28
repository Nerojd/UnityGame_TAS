using UnityEngine;
using DoDo.Terrain;
using System;

namespace DoDo.Player
{
    public class PlayerTerraformer : MonoBehaviour
    {
        public static event EventHandler OnAnyTerraformation;

        const float distanceNear = 2;

        [Header("TERRAFORM")]
        [SerializeField] float terraformRadius = 1f;
        [SerializeField] float terraformPower = 20f;
        [SerializeField] float terraformPowerNear = 5f;
        [SerializeField] float terraformPowerFar = 20f;
        [SerializeField] float terraformDistanceFar = 25f;

        [Header("SUPERSEDE")]
        [SerializeField] float supersedeRadius = 2f;
        [SerializeField] float supersedePower = 30f;
        [SerializeField] float supersedePowerNear = 5f;
        [SerializeField] float supersedePowerFar = 20f;
        [SerializeField] float supersedeDistanceFar = 10f;

        [SerializeField] Camera cam;

        bool hasHit = false;
        Vector3 onHitPoint;
        Plane terraformPlane;

        PlayerChunk playerChunk;

        // Start is called before the first frame update
        void Awake()
        {
            playerChunk = GetComponent<PlayerChunk>();
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        void Update()
        {
            // Add terrain
            if (Input.GetMouseButton(0))
            {
                TerraformTerrain();
                // Emettre un son de creusement
            }
            else
            {
                hasHit = false;
            }

            // Subtract terrain
            if (Input.GetMouseButton(1))
            {
                SupersedeTerrain();
                // Emettre un son de creusement en inverse
            }
        }

        private void TerraformTerrain()
        {
            // DO THE SPHERE CAST ALL TO GET ALL CHUNKS (on peut terraformer que s'il y a de la matière donc on peut juste update les chunks à proximiter)
            // Faire ensuite une boucle dans TERRAIN GENERATOR pour générer une nouveau mesh sur chaque LOD et mettre à les jours les chunks

            // Raycast to hit the terrain
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, terraformDistanceFar)
             && hit.collider.GetComponent<Chunk>() != null)
            {
                if (!hasHit)
                {
                    onHitPoint = hit.point;
                    hasHit = true;

                    // Define a Plane with the normal of the camera and the on hit point of the terrain
                    Vector3 normalizedCameraNormal = cam.transform.forward.normalized;
                    terraformPlane = new Plane(normalizedCameraNormal, onHitPoint);
                }
                else
                {
                    // Obtenir le rayon partant de la position de la caméra et passant par le point central de la vue du joueur
                    Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f);
                    Ray ray = cam.ViewportPointToRay(viewportCenter);

                    // Calculer l'intersection entre le rayon et le plan
                    if (terraformPlane.Raycast(ray, out float distance))
                    {
                        // Calculer le point d'intersection
                        Vector3 terraformPoint = ray.origin + ray.direction * distance;

                        float dstFromCam = (terraformPoint - cam.transform.position).magnitude;
                        float weight01 = Mathf.InverseLerp(distanceNear, terraformDistanceFar, dstFromCam);
                        float power = Mathf.Lerp(terraformPowerNear, terraformPowerFar, weight01);
                        int wieght = 1;
                        playerChunk.Terraform(terraformPoint, wieght, terraformRadius, power * terraformPower);
                    }
                }
            }            
        }

        private void SupersedeTerrain()
        {
            // Raycast to hit the terrain
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, supersedeDistanceFar) &&
                hit.collider.GetComponent<Chunk>() != null)
            {
                Vector3 supersedePoint = hit.point;

                float dstFromCam = (supersedePoint - cam.transform.position).magnitude;
                float weight01 = Mathf.InverseLerp(distanceNear, supersedeDistanceFar, dstFromCam);
                float power = Mathf.Lerp(supersedePowerNear, supersedePowerFar, weight01);

                int wieght = -1;
                playerChunk.Terraform(supersedePoint, wieght, supersedeRadius, power * supersedePower);
            }
        }
    }

}
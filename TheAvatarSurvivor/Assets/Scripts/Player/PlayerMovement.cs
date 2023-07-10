using DoDo.Core;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

namespace DoDo.Player.Control
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField]
        Transform playerGazeOrientation;

        [Header("Movement")]
        // Walking speed
        [SerializeField] float moveSpeedWalk = 40f;
        // Running speed
        [SerializeField] float moveSpeedRun = 65f;
        float currentSpeed;
        float horizontalInput;
        float verticalInput;
        Vector3 moveDirection;
        bool isWalking = true;
        bool isRunning = true;

        [Header("Jump")]
        // Jumping force
        [SerializeField] float jumpForce = 8f;
        // Cooldown for press & hold jumps
        [SerializeField] float jumpCooldown = 0.25f;
        // Speed in the air when using forces
        [SerializeField] float airMultiplier = 0.1f;
        bool canJump = true;

        [Header("Ground Check")]
        [SerializeField] float playerHeight;
        [SerializeField] LayerMask whatIsGround;
        [SerializeField] float groundDrag = 5f;
        bool isGrounded;

        [Header("Keybinds")]
        [SerializeField] KeyCode jumpKey = KeyCode.Space;
        [SerializeField] KeyCode walkKey = KeyCode.LeftShift;

        [Header("Components")]
        Rigidbody rigidbodyComponent;

        /*******************************************/
        /*              Unity Methods              */
        /*******************************************/
        void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            rigidbodyComponent = GetComponent<Rigidbody>();
            //rigidbodyComponent.freezeRotation = true;
        }
        void Update()
        {
            if (!IsOwner) return;

            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
            isGrounded = IsGrounded();

            // Handle Jump
            if (Input.GetKey(jumpKey) && canJump && isGrounded)
            {
                canJump = false;
                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }

            // Handle Speed
            SpeedControl();

            // Handle Drag
            DragControl();

            MoveServerRpc();
            
        }
        void FixedUpdate()
        {
            if (!IsOwner) return;

            FixedMoveServerAuth();
        }

        /*******************************************/
        /*             Private Methods             */
        /*******************************************/
        private void Jump()
        {
            rigidbodyComponent.velocity = new(rigidbodyComponent.velocity.x, 0f, rigidbodyComponent.velocity.z);

            rigidbodyComponent.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        private void ResetJump()
        {
            canJump = true;
        }
        private bool IsGrounded()
        {
            // Go down by the size of the player + 0.2f to ensure hitting the ground
            Ray ray = new(transform.position, Vector3.down);
            return Physics.SphereCast(ray, 0.3f, playerHeight * 0.5f, whatIsGround);
        }

        private void SpeedControl()
        {
            Vector3 flatVelocity = new(rigidbodyComponent.velocity.x, 0f, rigidbodyComponent.velocity.z);

            if (Input.GetKey(walkKey) && isGrounded)
            {
                currentSpeed = moveSpeedWalk;
                isWalking = true;
                isRunning = false;
            }
            else
            {
                currentSpeed = moveSpeedRun;
                isWalking = false;
                isRunning = true;
            }

            // Limit velocity
            if (flatVelocity.magnitude > currentSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * currentSpeed;
                rigidbodyComponent.velocity = new(limitedVelocity.x, rigidbodyComponent.velocity.y, limitedVelocity.z);
            }
        }

        private void DragControl()
        {
            rigidbodyComponent.drag = isGrounded ? groundDrag : 0f;
        }

        /*******************************************/
        /*             Network Methods             */
        /*******************************************/
        [ServerRpc(RequireOwnership = false)]
        private void MoveServerRpc()
        {

        }

        private void FixedMoveServerAuth()
        {
            // Calculate movement direction with the player gaze orientation
            moveDirection = playerGazeOrientation.forward * verticalInput
                                    + playerGazeOrientation.right * horizontalInput;

            if (isGrounded)
            {
                rigidbodyComponent.AddForce(moveDirection.normalized * currentSpeed, ForceMode.Force);
            }
            else if (!isGrounded)
            {
                rigidbodyComponent.AddForce(airMultiplier * currentSpeed * moveDirection.normalized, ForceMode.Force);
            }
            HandleMovementServerRpc(moveDirection, isGrounded, currentSpeed);

        }
        [ServerRpc(RequireOwnership = false)]
        private void HandleMovementServerRpc(Vector3 inputDirection, bool isGrounded, float currentSpeed)
        {

        }


        /******************************************/
        /*             Public Methods             */
        /******************************************/
        public bool IsPlayerWalking() => isWalking;
        public bool IsPlayerRunning() => isRunning;
        public bool IsPlayerGrounded() => isGrounded;
    }
}
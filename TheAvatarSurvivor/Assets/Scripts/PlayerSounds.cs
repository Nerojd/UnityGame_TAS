using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DoDo.Player.Control;

public class PlayerSounds : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private float footstepTimer;
    private float footstepTimerMax = .1f;


    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        footstepTimer -= Time.deltaTime;
        if (footstepTimer < 0f)
        {
            footstepTimer = footstepTimerMax;

            if (playerMovement.IsPlayerWalking())
            {
                float volume = 1f;
                SoundManager.Instance.PlayFootstepsSound(playerMovement.transform.position, volume);
            }
        }
    }
}
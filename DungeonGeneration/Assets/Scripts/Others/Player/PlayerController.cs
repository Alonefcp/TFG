using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Camera mainCamera = null;
    [SerializeField] float speed = 5.0f;
    [SerializeField] Rigidbody2D playerRigidbody2D = null;

    private Vector2 movement = Vector2.zero;

    private void LateUpdate()
    {
        //Camera follow
        mainCamera.transform.position = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
    }

    private void Update()
    {
        //Player input
        float dirX = Input.GetAxisRaw("Horizontal");
        float dirY = Input.GetAxisRaw("Vertical");

        movement = new Vector2(dirX, dirY).normalized;
    }

    private void FixedUpdate()
    {
        //Player movement
        playerRigidbody2D.velocity = movement * speed;
    }

    /// <summary>
    /// Sets the player position and scale
    /// </summary>
    /// <param name="newPos">New position</param>
    /// <param name="newScale">New scale</param>
    /// <param name="playerSpeed">New player speed</param>
    /// <param name="cameraSize">New camera size</param>
    public void SetPlayer(Vector2 newPos, Vector3 newScale, float playerSpeed=4.0f, float cameraSize = 3.0f)
    {
        transform.position = newPos + new Vector2(0.5f,0.5f);
        transform.localScale = newScale;
        speed = playerSpeed;
        mainCamera.orthographicSize = cameraSize;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Camera camera = null;
    [SerializeField] float speed = 5.0f;
    [SerializeField] Rigidbody2D rigidbody2D = null;

    private Vector2 movement = Vector2.zero;

    private void LateUpdate()
    {
        //Camera follow
        camera.transform.position = new Vector3(transform.position.x, transform.position.y, camera.transform.position.z);
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
        rigidbody2D.velocity = movement * speed;
    }

    public void SetPlayerPosition(Vector2 newPos)
    {
        transform.position = newPos+new Vector2(0.5f,0.5f);
    }
}

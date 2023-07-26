using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    //Reference to Game Manager
    private GameManager gameManager;

    //Get Player and Wall layers
    private int layerMask;

    //Basic Player Movement
    public float baseSpeed;
    public float minSpeed;
    public float maxSpeed;
    private Vector2 targetPosition;
    private Rigidbody2D rb;

    private void Awake()
    {
        //Get reference to Game Manager
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //Get Player and Wall layers
        layerMask = LayerMask.GetMask("Player", "Wall");

        //Get rigidbody for movement
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //Freeze player position at start
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    // Update is called once per frame
    void Update()
    {
        //Check if player is in shadow
        if (IsPlayerInShadow())
        {
            Debug.Log("Player is in shadow");
        }
        else
        {
            Debug.Log("Player is in light");
        }

        //Check for moving the player
        MovePlayer();
    }

    private void FixedUpdate()
    {
        //Check for moving the player
        MovePlayerCalculation();
    }

    //Iterates through all light sources and determines if the player is in shadow
    bool IsPlayerInShadow()
    {
        // Iterate through all light sources
        foreach (GameObject lightSource in gameManager.lightSources)
        {
            // Cast a ray from the light source to the player
            Vector2 rayDirection = transform.position - lightSource.transform.position;
            float lightRange = lightSource.GetComponent<Light2D>().pointLightOuterRadius; // get the Light2D component

            RaycastHit2D hit = Physics2D.Raycast(lightSource.transform.position, rayDirection, lightRange, layerMask);

            // Check if the raycast hit the player before it hit a wall
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // The raycast hit the player before it hit a wall, so the player is in light
                return false;
            }
        }

        // The raycast hit a wall before it hit the player for all light sources, so the player is in shadow
        return true;
    }

    //Basic Player Movement
    void MovePlayer()
    {
        // Check for touch input
        if (Input.touchCount > 0)
        {
            // Get the first touch
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                // unfreeze position when trying to move
                rb.constraints = RigidbodyConstraints2D.None;

                // Convert touch position to world coordinates
                targetPosition = Camera.main.ScreenToWorldPoint(touch.position);
            }
        }
        // Add mouse input for testing on PC
        else if (Input.GetMouseButton(0))
        {
            // unfreeze position when trying to move
            rb.constraints = RigidbodyConstraints2D.None;

            // Convert mouse position to world coordinates
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else
        {
            // freeze position when not moving
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    void MovePlayerCalculation()
    {
        // Calculate the direction and distance the player should move in this frame
        Vector2 direction = (targetPosition - rb.position).normalized;

        // Calculate the speed based on the distance to the target
        float distanceToTarget = Vector2.Distance(rb.position, targetPosition);
        float speed = Mathf.Clamp(distanceToTarget, minSpeed, maxSpeed) * baseSpeed;

        Vector2 movePosition = rb.position + direction * speed * Time.fixedDeltaTime;

        // Update the player's position
        rb.MovePosition(movePosition);
    }
}

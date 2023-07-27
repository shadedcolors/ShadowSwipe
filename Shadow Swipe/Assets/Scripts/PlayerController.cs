using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    //Reference to Game Manager
    private GameManager gameManager;

    //Player Collider
    private CircleCollider2D playerCollider;

    //Get layers
    private int layerMaskPlayer;
    private int layerMaskShadowDetector;
    private int layerMaskWall;

    //Shadow Detector Prefab
    public GameObject shadowDetector;

    //Basic Player Movement
    public float baseSpeed;
    public float minSpeed;
    public float maxSpeed;
    private Vector2 targetPosition;
    private Rigidbody2D rb;

    // Shadow Blink
    public float tapSpeed;
    private float lastTapTime;
    public float ShadowBlinkCastTime;

    //Double Tap Detection for Shadow Blink

    private void Awake()
    {
        //Get reference to Game Manager
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Get Layer Masks
        layerMaskPlayer = LayerMask.GetMask("Player", "Wall");
        layerMaskShadowDetector = LayerMask.GetMask("Wall", "ShadowDetector");
        layerMaskWall = LayerMask.GetMask("Wall");

        //Get rigidbody for movement
        rb = GetComponent<Rigidbody2D>();

        //Get Player Collider
        playerCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        //Freeze player position at start
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForDoubleTap();

        //Check if player is in shadow
        if (IsInShadow(gameObject))
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
    bool IsInShadow(GameObject target)
    {
        //go through all light sources
        foreach (GameObject lightSource in gameManager.lightSources)
        {
            // Cast a ray from the light source to the target
            Vector2 rayDirection = target.transform.position - lightSource.transform.position;
            float lightRange = lightSource.GetComponent<Light2D>().pointLightOuterRadius;

            if (target == gameObject) // Player
            {
                // Use raycast with layerMaskPlayer
                RaycastHit2D hit = Physics2D.Raycast(lightSource.transform.position, rayDirection, lightRange, layerMaskPlayer);

                // If the raycast hit something, check if it hit the target
                if (hit.collider != null)
                {
                    // If the raycast hit the player, the player is not in shadow
                    if (hit.collider.tag == "Player")
                    {
                        return false;
                    }
                }
            }
            else if (target == shadowDetector) // Shadow detector
            {
                // Use raycast with layerMaskShadowDetector
                RaycastHit2D hit = Physics2D.Raycast(lightSource.transform.position, rayDirection, lightRange, layerMaskShadowDetector);

                // If the raycast hit something, check if it hit the target
                if (hit.collider != null)
                {
                    // If the raycast hit the shadow detector, the the shadow blink position is not in shadow
                    if (hit.collider.tag == "ShadowDetector")
                    {
                        return false;
                    }
                }
            }
        }

        // The raycast hit a wall before it hit the target for all light sources, or it did not hit anything (i.e., it's out of range), so the target is in shadow
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

    public void CheckForDoubleTap()
    {
        //Mouse Input
        if (Input.GetMouseButtonDown(0))
        {

            // Check if the time between taps is less than the tap speed
            if (Time.time - lastTapTime < tapSpeed)
            {
                // Double Tap
                StartCoroutine(DoubleTapped(Input.mousePosition));
            }

            //Reset last tap time
            lastTapTime = Time.time;
        }

        // Touch Input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Check if the time between taps is less than the tap speed
            if (Time.time - lastTapTime < tapSpeed)
            {
                // Double Tap
                StartCoroutine(DoubleTapped(Input.GetTouch(0).position));
            }

            //Reset last tap time
            lastTapTime = Time.time;
        }
    }

    public IEnumerator DoubleTapped(Vector2 screenPosition)
    {
        // Convert screen position to world position
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        // Check if the player's sprite overlaps with a wall
        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, playerCollider.radius / 2, layerMaskWall);

        if (hitCollider != null && hitCollider.gameObject.tag == "Wall")
        {
            // If the hit collider's gameobject is tagged as "Wall", the point is inside a wall
            Debug.Log("Point is inside a wall");
            yield break;
        }
        else
        {
            // The point is not inside a wall
            Debug.Log("Point is not inside a wall");
        }

        // Cast a ray from the player to the desired blink position
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, worldPosition - (Vector2)transform.position, Vector2.Distance(transform.position, worldPosition));

        // Go through all the objects hit by the raycast
        foreach (RaycastHit2D hit in hits)
        {
            // If we hit a wall, we cannot blink
            if (hit.collider.gameObject.tag == "Wall")
            {
                Debug.Log("Can't blink here: wall in the way");
                yield break;
            }
        }

        shadowDetector.transform.position = worldPosition;

        // Wait for a duration before checking IsInShadow
        yield return new WaitForSeconds(ShadowBlinkCastTime);

        if (IsInShadow(shadowDetector))
        {
            // Use Shadow Blink
            Debug.Log("Blink successful");
            transform.position = worldPosition;

            // Reset the last tap time after successful blink
            lastTapTime = 0;
        }
    }
}

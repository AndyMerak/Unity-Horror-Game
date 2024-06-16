using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Girl : MonoBehaviour
{
    //<Moving
    public float normalSpeed = 2.5f;
    public float sprintSpeed = 5f;
    public float crouchSpeed = 1.5f;
    private float speed;
    Vector3 forward;
    Vector3 right;
    //Moving>

    //<Jumping
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private Rigidbody rb;
    private bool isGrounded;
    //Jumping>

    public Animator anim;

    private bool isRandomIdleActive = false;
    private Coroutine idleCoroutine;
    private bool isCrouching = false;

    // Tripping
    public float trippingMoveDistance = 3f; // Adjust this value as needed
    private bool isTripping = false;
    private bool isRunning = false;
    private bool isMovementDisabled = false;
    private Vector3 tripDirection;

    private bool leftBootDropped = false;
    private bool rightBootDropped = false;
    public GameObject bootLeftPrefab;
    public GameObject bootRightPrefab;
    public GameObject bootLeft;
    public GameObject bootRight;

    void Start()
    {
        //<Moving
        forward = Camera.main.transform.forward;
        forward.y = 0; // Ensure the forward vector is parallel to the ground
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
        speed = normalSpeed;
        //Moving>

        //<Jumping
        rb = GetComponent<Rigidbody>();
        //Jumping>
    }

    void Update()
    {
        if (isTripping || isMovementDisabled) return;

        //<Moving
        if (Input.anyKey)
        {
            Move();
            StopIdleCoroutine();
        }
        else
        {
            anim.SetBool("isWalking", false); // Stop the walking animation when no key is pressed
            anim.SetBool("isRunning", false); // Ensure running animation stops when no key is pressed
            if (!isRandomIdleActive)
            {
                idleCoroutine = StartCoroutine(RandomIdleCoroutine());
            }
        }

        HandleSprinting();
        HandleCrouching();
        //Moving>

        //<Jumping
        CheckGroundStatus();
        if (isGrounded && Input.GetButtonDown("Jump") && !isCrouching) // Prevent jumping while crouched
        {
            Jump();
            StopIdleCoroutine();
        }
        //Jumping>

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!leftBootDropped && !rightBootDropped)
            {
                // Randomly choose which boot to drop
                if (Random.Range(0, 2) == 0)
                {
                    DropLeftBoot();
                }
                else
                {
                    DropRightBoot();
                }
            }
            else if (!leftBootDropped)
            {
                DropLeftBoot();
            }
            else if (!rightBootDropped)
            {
                DropRightBoot();
            }
        }
    }

    void DropLeftBoot()
    {
        // Deactivate Skinned Mesh Renderer of the original boot
        bootLeft.GetComponent<SkinnedMeshRenderer>().enabled = false;

        // Instantiate the left boot prefab slightly behind and above the girl's position
        Vector3 dropPosition = transform.position - transform.forward * 0.4f + Vector3.up * 0.2f;
        GameObject droppedBoot = Instantiate(bootLeftPrefab, dropPosition, Quaternion.identity);

        // Apply a slight upward and diagonal force opposite to the girl's movement direction
        Rigidbody bootRB = droppedBoot.GetComponent<Rigidbody>();
        bootRB.AddForce(-transform.forward * 2f + Vector3.up * 1.5f, ForceMode.Impulse);

        // Mark left boot as dropped
        leftBootDropped = true;
    }

    void DropRightBoot()
    {
        // Deactivate Skinned Mesh Renderer of the original boot
        bootRight.GetComponent<SkinnedMeshRenderer>().enabled = false;

        // Instantiate the right boot prefab slightly behind and above the girl's position
        Vector3 dropPosition = transform.position - transform.forward * 0.4f + Vector3.up * 0.2f;
        GameObject droppedBoot = Instantiate(bootRightPrefab, dropPosition, Quaternion.identity);

        // Apply a slight upward and diagonal force opposite to the girl's movement direction
        Rigidbody bootRB = droppedBoot.GetComponent<Rigidbody>();
        bootRB.AddForce(-transform.forward * 2f + Vector3.up * 1.5f, ForceMode.Impulse);

        // Mark right boot as dropped
        rightBootDropped = true;
    }

    //<Moving
    void Move()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (direction.magnitude > 0)
        {
            direction = Vector3.Normalize(direction); // Normalize direction to ensure consistent speed
            Vector3 movement = (right * direction.x + forward * direction.z);
            movement = Vector3.Normalize(movement) * speed * Time.deltaTime;

            if (isCrouching)
            {
                anim.SetBool("isCrouchWalking", true);
            }
            else if (isRunning)
            {
                anim.SetBool("isRunning", true); // Start the running animation
            }
            else
            {
                anim.SetBool("isWalking", true); // Start the walking animation
            }

            transform.forward = right * direction.x + forward * direction.z; // Ensure character faces the movement direction
            transform.position += movement;
        }
        else
        {
            anim.SetBool("isWalking", false); // Ensure walking animation stops if there's no movement
            anim.SetBool("isCrouchWalking", false); // Ensure crouch walking animation stops if there's no movement
            anim.SetBool("isRunning", false); // Ensure running animation stops if there's no movement
        }
    }
    //Moving>

    //<Jumping
    void Jump()
    {
        anim.SetBool("isJumping", true); // Start the jumping animation
        rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
    }

    void CheckGroundStatus()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        anim.SetBool("isGrounded", isGrounded); // Update the animator with the grounded status

        if (isGrounded)
        {
            anim.SetBool("isJumping", false); // Reset jumping animation when grounded
        }
    }
    //Jumping>

    void HandleSprinting()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching && new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).magnitude > 0)
        {
            if (speed != sprintSpeed)
            {
                speed = sprintSpeed;
                anim.SetBool("isRunning", true);
                StopIdleCoroutine();
            }
            isRunning = true;
        }
        else
        {
            if (speed != normalSpeed && !isCrouching)
            {
                speed = normalSpeed;
                anim.SetBool("isRunning", false);
            }
            isRunning = false;
        }
    }

    void HandleCrouching()
    {
        if (Input.GetKeyDown(KeyCode.C) && isGrounded && !anim.GetBool("isJumping") && !anim.GetBool("isRunning"))
        {
            isCrouching = !isCrouching;
            anim.SetBool("isCrouching", isCrouching);

            if (isCrouching)
            {
                speed = crouchSpeed;
            }
            else
            {
                speed = normalSpeed;
                anim.SetBool("isCrouchWalking", false);
            }

            StopIdleCoroutine();
        }
    }

    IEnumerator RandomIdleCoroutine()
    {
        isRandomIdleActive = true;

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(30f, 120f));

            anim.SetBool("idle2", true);
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
            anim.SetBool("idle2", false);
        }
    }

    void StopIdleCoroutine()
    {
        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            anim.SetBool("idle2", false);
            isRandomIdleActive = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isRunning && other.CompareTag("Trippable"))
        {
            tripDirection = transform.forward;
            StartCoroutine(HandleTripping());
        }

        if (other.CompareTag("LeftBootPrefab") && leftBootDropped)
        {
            Destroy(other.gameObject); // Destroy the pickup object

            // Activate the Skinned Mesh Renderer of the original left boot
            bootLeft.GetComponent<SkinnedMeshRenderer>().enabled = true;
            leftBootDropped = false;
        }
        else if (other.CompareTag("RightBootPrefab") && rightBootDropped)
        {
            Destroy(other.gameObject); // Destroy the pickup object

            // Activate the Skinned Mesh Renderer of the original right boot
            bootRight.GetComponent<SkinnedMeshRenderer>().enabled = true;
            rightBootDropped = false;
        }
    }

    IEnumerator HandleTripping()
    {
        isTripping = true;
        isMovementDisabled = true;
        anim.SetBool("isTripping", true);

        float trippingDuration = anim.GetCurrentAnimatorStateInfo(0).length;
        float elapsedTime = 0f;

        while (elapsedTime < trippingDuration)
        {
            transform.position += tripDirection * (trippingMoveDistance / trippingDuration) * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        anim.SetBool("isTripping", false);
        anim.SetBool("isStandingUp", true);

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        anim.SetBool("isStandingUp", false);
        isMovementDisabled = false;
        isTripping = false;
    }
}

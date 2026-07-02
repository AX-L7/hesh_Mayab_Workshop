using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SimpleMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float acceleration = 50f;

    [Header("Visual Feedback")]
    [SerializeField] Color movingColor = Color.green;
    [SerializeField] Color idleColor = Color.white;

    Rigidbody rb;
    Renderer cubeRenderer;
    Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cubeRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        ReadInput();
        UpdateVisuals();
    }

    void ReadInput()
    {
        moveInput = Vector2.zero;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.wKey.isPressed) moveInput.y += 1f;
        if (kb.sKey.isPressed) moveInput.y -= 1f;
        if (kb.aKey.isPressed) moveInput.x -= 1f;
        if (kb.dKey.isPressed) moveInput.x += 1f;

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        Vector3 currentVelocity = rb.linearVelocity;

        rb.linearVelocity = new Vector3(
            Mathf.MoveTowards(currentVelocity.x, targetVelocity.x, acceleration * Time.fixedDeltaTime),
            currentVelocity.y,
            Mathf.MoveTowards(currentVelocity.z, targetVelocity.z, acceleration * Time.fixedDeltaTime)
        );
    }

    void UpdateVisuals()
    {
        if (cubeRenderer == null) return;
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        cubeRenderer.material.color = isMoving ? movingColor : idleColor;
    }
}

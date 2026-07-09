using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rb;
    private Camera _mainCam;
    private float _currentSpeed;
    private Vector3 _aimpoint;

    [SerializeField] private InputReader _input;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runSpeed = 12f;
    [SerializeField] private float _groundPlaneHeight;
    [SerializeField] private Transform _aimPivot;
    [SerializeField] private float _aimSmoothing = 10f;
    [SerializeField] private PlayerDash _dash;

    [SerializeField] Animator _animator;

    public Vector3 AimPoint => _aimpoint;
    public Vector3 AimDirection => (_aimpoint - transform.position).normalized;
    public Vector3 MoveDirection { get; private set; }
    public float CurrentSpeed => _currentSpeed;



    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezeRotationY;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _mainCam = Camera.main;

    }


    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCam = Camera.main;
    }

    void Update()
    {
        UpdateAiming();
    }


    private void UpdateAiming()
    {
        Vector2 mousePos = _input.MousePosition;
        Ray ray = _mainCam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0f));

        Plane ground = new Plane(Vector3.up, new Vector3(0f, _groundPlaneHeight, 0f));
        if (ground.Raycast(ray, out float distance))
        {
            _aimpoint = ray.GetPoint(distance);
        }

        Vector3 lookDir = _aimpoint - _aimPivot.position;
        lookDir.y = 0f;

        if (lookDir.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            _aimPivot.rotation = targetRotation;
        }
    }


    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        if (_dash != null && _dash.IsDashing) return;

        Vector2 rawInput = _input.Move;
        Vector3 inputDir = new Vector3(rawInput.x, 0f, rawInput.y);



        if (_mainCam != null)
        {
            Vector3 camForward = _mainCam.transform.forward;
            Vector3 camRight = _mainCam.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            inputDir = camForward * rawInput.y + camRight * rawInput.x;
        }

        MoveDirection = inputDir;

        float currentSpeed = _moveSpeed;
        if (_input.Sprint) currentSpeed = _runSpeed;
        _animator.SetBool("ifRun", _input.Sprint);

        inputDir.Normalize();

        _rb.linearVelocity = inputDir * currentSpeed;

        _animator.SetFloat("Blend", _rb.linearVelocity.magnitude / _moveSpeed);
    }
}



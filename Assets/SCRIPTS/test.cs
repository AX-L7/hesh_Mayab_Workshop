using UnityEngine;

public class test : MonoBehaviour
{
    // PRIMITIVE DATA TYPES :D
    private int _varInt = 5;
    private float _varFloat = 1.0f;
    private string _varString = "Helloo";
    private bool _varBool = false;

    // COMPLEX DATA TYPES D:
    private Collider _playerCollider;
    public Rigidbody rb;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("AAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHH");
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector3(_varFloat, 0f, 0f);
    }
}

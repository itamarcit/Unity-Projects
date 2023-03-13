using UnityEngine;

public class Arrow : MonoBehaviour
{
    private const int LEFT_LOWER_BOUND = 45;
    private const int LEFT_UPPER_BOUND = 70;
    private const int RIGHT_UPPER_BOUND = 315;
    private const int RIGHT_LOWER_BOUND = 300;
    private float _t;
    private readonly Vector2 _start = new (1, 1);
    private bool _isLeft;

    [SerializeField]
    private float arrowSpeed = 50f;
    void Start()
    {
        transform.eulerAngles = new Vector3(0, 0, CalculateAngleFromVec(_start));
    }

    private void Update()
    {
        if (transform.eulerAngles.z > LEFT_LOWER_BOUND && transform.eulerAngles.z < LEFT_UPPER_BOUND)
        {
            _isLeft = false;
        }
        if (transform.eulerAngles.z < RIGHT_UPPER_BOUND && transform.eulerAngles.z > RIGHT_LOWER_BOUND)
        {
            _isLeft = true;
        }

        transform.Rotate(_isLeft
            ? new Vector3(0, 0, Time.deltaTime * arrowSpeed)
            : new Vector3(0, 0, -Time.deltaTime * arrowSpeed));
    }

    private float CalculateAngleFromVec(Vector2 vec)
    {   
        Vector2 temp = vec.normalized;
        return Mathf.Atan2(temp.x, temp.y) * (180 / Mathf.PI);
    }
}
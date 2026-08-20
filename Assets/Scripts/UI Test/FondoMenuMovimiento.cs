using UnityEngine;

public class MenuBackgroundMovement : MonoBehaviour
{
    public float horizontalMovement = 10f;
    public float verticalMovement = 5f;
    public float movementSpeed = 0.5f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * movementSpeed) * horizontalMovement;
        float y = Mathf.Sin(Time.time * movementSpeed * 0.7f) * verticalMovement;

        transform.localPosition = startPosition + new Vector3(x, y, 0f);
    }
}
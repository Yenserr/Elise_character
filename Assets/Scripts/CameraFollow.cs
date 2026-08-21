using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Arrastra aquí el objeto de Elisa desde la Jerarquía")]
    public Transform target;

    [Header("Ajustes de Vista 2.5D")]
    [Tooltip("Distancia de la cámara respecto a Elisa (X: centrado, Y: altura, Z: distancia hacia atrás)")]
    public Vector3 offset = new Vector3(0f, 10f, -6f);
    
    [Tooltip("Velocidad con la que la cámara sigue al personaje (evita tirones)")]
    public float smoothSpeed = 10f;

    [Header("Ángulo de la Cámara")]
    [Tooltip("Inclinación de la cámara para lograr el efecto cenital/2.5D")]
    public Vector3 cameraRotation = new Vector3(55f, 0f, 0f);

    void Start()
    {
        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
    }
}
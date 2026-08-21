using UnityEngine;

public class ColliderChecker : MonoBehaviour
{
    void Start()
    {
        foreach (var col in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (col.GetComponent<Collider>() == null)
                Debug.LogWarning($"{col.gameObject.name} no tiene Collider!");
        }
    }
}
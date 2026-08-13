using UnityEngine;

public class Movement : MonoBehaviour
{
    public Input inputPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputPlayer = new Input();
        inputPlayer.Enable();
        inputPlayer.Player.Attack.performed += ctx => Attack();
    }

    private void Attack()
    {
        Debug.Log("Attack");
    }

    void OnDisable()
    {
        inputPlayer.Disable();
    }
}
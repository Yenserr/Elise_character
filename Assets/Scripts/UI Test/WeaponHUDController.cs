using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponHUDController : MonoBehaviour
{
    [Header("Espacios del HUD")]
    public Image weaponCurrent;
    public Image weaponRight;

    [Header("Armas de prueba")]
    public Sprite weapon1;
    public Sprite weapon2;

    private Sprite currentWeapon;
    private Sprite rightWeapon;

    void Start()
    {
        // Al comenzar:
        // Arma 1 equipada
        // Arma 2 disponible a la derecha
        currentWeapon = weapon1;
        rightWeapon = weapon2;

        UpdateWeaponHUD();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            SelectRightWeapon();
        }
    }

    void SelectRightWeapon()
    {
        // Si no hay arma a la derecha, no hacemos nada
        if (rightWeapon == null)
            return;

        // La derecha pasa a ser la actual
        currentWeapon = rightWeapon;

        // El espacio de la derecha queda vacío
        rightWeapon = null;

        UpdateWeaponHUD();
    }

    void UpdateWeaponHUD()
    {
        // ARMA ACTUAL
        if (currentWeapon != null)
        {
            weaponCurrent.gameObject.SetActive(true);
            weaponCurrent.sprite = currentWeapon;
        }
        else
        {
            weaponCurrent.gameObject.SetActive(false);
        }

        // ARMA DERECHA
        if (rightWeapon != null)
        {
            weaponRight.gameObject.SetActive(true);
            weaponRight.sprite = rightWeapon;
        }
        else
        {
            weaponRight.gameObject.SetActive(false);
        }
    }
}
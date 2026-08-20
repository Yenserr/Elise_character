using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponHUDController : MonoBehaviour
{
    [Header("Espacios del HUD")]
    public Image weaponLeft;
    public Image weaponCurrent;
    public Image weaponRight;

    [Header("Armas de prueba")]
    public Sprite weapon1;
    public Sprite weapon2;
    public Sprite weapon3;

    private Sprite leftWeapon;
    private Sprite currentWeapon;
    private Sprite rightWeapon;

    void Start()
    {
        // Empezamos con las 3 armas visibles
        leftWeapon = weapon1;
        currentWeapon = weapon2;
        rightWeapon = weapon3;

        UpdateWeaponHUD();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            SelectLeftWeapon();
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            SelectRightWeapon();
        }
    }

    void SelectLeftWeapon()
{
    if (leftWeapon == null)
        return;

    currentWeapon = leftWeapon;
    leftWeapon = null;

    UpdateWeaponHUD();
}

void SelectRightWeapon()
{
    if (rightWeapon == null)
        return;

    currentWeapon = rightWeapon;
    rightWeapon = null;

    UpdateWeaponHUD();
}

    void UpdateWeaponHUD()
    {
        // IZQUIERDA
        if (leftWeapon != null)
        {
            weaponLeft.gameObject.SetActive(true);
            weaponLeft.sprite = leftWeapon;
        }
        else
        {
            weaponLeft.gameObject.SetActive(false);
        }

        // CENTRO
        if (currentWeapon != null)
        {
            weaponCurrent.gameObject.SetActive(true);
            weaponCurrent.sprite = currentWeapon;
        }
        else
        {
            weaponCurrent.gameObject.SetActive(false);
        }

        // DERECHA
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
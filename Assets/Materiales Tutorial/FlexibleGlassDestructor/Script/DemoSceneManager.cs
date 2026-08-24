using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FlexibleGlassDestructor
{
    /// <summary>
    /// Manages the demo scene environment, including first-person camera controls,
    /// projectile launching mechanics, and automatic glass fracture.
    /// </summary>
    public class DemoSceneManager : MonoBehaviour
    {
        [Header("Ball Settings")]
        public GameObject ballPrefab;
        public float launchForce = 20f;
        public float ballScale = 0.5f;

        [Header("Spawn Position")]
        public Transform spawnPoint;

        [Header("Explode Position")]
        public Transform explodePoint;
        public float explodeRadius = 10f;
        public float explodeDamege = 100f;

        [Header("Camera Control Settings")]
        public float moveSpeed = 5f;
        public float lookSensitivity = 2f;

        private float rotationX = 0f;
        private float rotationY = 0f;

        [Header("Automatic Glass Break")]
        [SerializeField] private float automaticBreakDelay = 3f;

        /// <summary>
        /// Standard Unity callback for initialization.
        /// </summary>
        private void Start()
        {
            // Lock and hide the mouse cursor during execution.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Automatically fracture the glass after the configured delay.
            Invoke(nameof(AutoBreakGlass), automaticBreakDelay);
        }

        /// <summary>
        /// Automatically fractures the glass.
        /// </summary>
        private void AutoBreakGlass()
        {
            if (explodePoint != null)
            {
                Fracture(explodePoint.position, explodeRadius);
            }
            else
            {
                Debug.LogWarning(
                    "FlexibleGlassDestructor: Explode Point is not assigned."
                );
            }
        }

        /// <summary>
        /// Standard Unity callback for frame-by-frame updates.
        /// </summary>
        private void Update()
        {
            // 1. Camera rotation
            this.HandleRotation();

            // 2. Camera movement
            this.HandleMovement();

            if (Keyboard.current == null || Mouse.current == null)
                return;

            // Space - Launch ball
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                this.LaunchBall();
            }

            // F1 - Reload scene
            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager
                        .GetActiveScene()
                        .name
                );
            }

            // ESC - Release cursor
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Left Click - Fire weapon
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                this.FireWeapon();
            }

            // Right Click - Explosion
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (explodePoint != null)
                {
                    this.Explode(
                        explodePoint.position,
                        explodeRadius,
                        explodeDamege
                    );
                }
            }

            // F2 - Manual fracture
            if (Keyboard.current.f2Key.wasPressedThisFrame)
            {
                if (explodePoint != null)
                {
                    this.Fracture(
                        explodePoint.position,
                        explodeRadius
                    );
                }
            }
        }

        /// <summary>
        /// Processes camera rotation based on mouse input.
        /// </summary>
        private void HandleRotation()
        {
            if (Mouse.current == null)
                return;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            this.rotationX +=
                mouseDelta.x *
                this.lookSensitivity *
                Time.deltaTime;

            this.rotationY -=
                mouseDelta.y *
                this.lookSensitivity *
                Time.deltaTime;

            // Limit vertical rotation.
            this.rotationY = Mathf.Clamp(
                this.rotationY,
                -90f,
                90f
            );

            this.transform.localRotation =
                Quaternion.Euler(
                    this.rotationY,
                    this.rotationX,
                    0
                );
        }

        /// <summary>
        /// Processes camera movement using WASD.
        /// </summary>
        private void HandleMovement()
        {
            if (Keyboard.current == null)
                return;

            float moveX = 0f;
            float moveZ = 0f;

            if (Keyboard.current.aKey.isPressed)
                moveX = -1f;

            if (Keyboard.current.dKey.isPressed)
                moveX = 1f;

            if (Keyboard.current.wKey.isPressed)
                moveZ = 1f;

            if (Keyboard.current.sKey.isPressed)
                moveZ = -1f;

            Vector3 move =
                this.transform.right * moveX +
                this.transform.forward * moveZ;

            this.transform.position +=
                move *
                this.moveSpeed *
                Time.deltaTime;
        }

        /// <summary>
        /// Spawns and launches a ball object.
        /// </summary>
        private void LaunchBall()
        {
            GameObject ball;

            if (this.ballPrefab != null)
            {
                ball = Instantiate(this.ballPrefab);
            }
            else
            {
                ball = GameObject.CreatePrimitive(
                    PrimitiveType.Sphere
                );

                ball.transform.localScale =
                    new Vector3(
                        0.1f,
                        0.1f,
                        0.1f
                    );

                ball.AddComponent<Rigidbody>();
            }

            Transform origin =
                this.spawnPoint != null
                    ? this.spawnPoint
                    : this.transform;

            ball.transform.position = origin.position;

            ball.transform.localScale =
                Vector3.one * this.ballScale;

            Rigidbody rb =
                ball.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForce(
                    this.transform.forward *
                    this.launchForce,
                    ForceMode.Impulse
                );
            }

            // Destroy the ball after 3 seconds.
            Destroy(ball, 3f);
        }

        /// <summary>
        /// Fires a raycast from the center of the camera
        /// and damages breakable glass.
        /// </summary>
        private void FireWeapon()
        {
            if (Camera.main == null)
                return;

            Ray ray =
                Camera.main.ViewportPointToRay(
                    new Vector3(
                        0.5f,
                        0.5f,
                        0
                    )
                );

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                10f
            ))
            {
                hit.collider
                    .GetComponentInParent<IGlassBreakable>()
                    ?.TakeDamage(
                        hit.point,
                        ray.direction,
                        10f
                    );

                // Display a small red sphere at the impact point.
                GameObject viewSphere =
                    GameObject.CreatePrimitive(
                        PrimitiveType.Sphere
                    );

                viewSphere.transform.position =
                    hit.point;

                viewSphere.transform.localScale =
                    Vector3.one * 0.05f;

                Renderer sphereRenderer =
                    viewSphere.GetComponent<Renderer>();

                if (sphereRenderer != null)
                {
                    sphereRenderer.material.color =
                        Color.red;
                }

                Destroy(viewSphere, 0.2f);
            }
        }

        /// <summary>
        /// Triggers an explosion that damages nearby glass.
        /// </summary>
        private void Explode(
            Vector3 explosionPos,
            float radius,
            float maxDamage
        )
        {
            Collider[] colliders =
                Physics.OverlapSphere(
                    explosionPos,
                    radius
                );

            HashSet<IGlassBreakable>
                processedSystems =
                new HashSet<IGlassBreakable>();

            foreach (Collider hit in colliders)
            {
                var glass =
                    hit.GetComponentInParent<
                        IGlassBreakable
                    >();

                if (glass != null)
                {
                    if (processedSystems.Contains(glass))
                        continue;

                    processedSystems.Add(glass);

                    Vector3 diff =
                        hit.transform.position -
                        explosionPos;

                    Vector3 direction =
                        diff.normalized;

                    float proximity =
                        (radius - diff.magnitude) /
                        radius;

                    float damage =
                        (proximity * proximity) *
                        maxDamage;

                    if (damage <= 0)
                        continue;

                    glass.TakeDamage(
                        hit.ClosestPoint(
                            explosionPos
                        ),
                        direction,
                        damage
                    );
                }
            }
        }

        /// <summary>
        /// Finds all breakable glass within the radius
        /// and triggers their fracture.
        /// </summary>
        private void Fracture(
            Vector3 explosionPos,
            float radius
        )
        {
            Collider[] colliders =
                Physics.OverlapSphere(
                    explosionPos,
                    radius
                );

            foreach (Collider hit in colliders)
            {
                var glass =
                    hit.GetComponentInParent<
                        IGlassBreakable
                    >();

                if (glass != null)
                {
                    glass.Fracture();
                }
            }
        }
    }
}
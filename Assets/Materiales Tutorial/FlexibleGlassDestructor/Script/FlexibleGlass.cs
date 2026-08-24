using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlexibleGlassDestructor
{
    [ExecuteInEditMode]
    public class FlexibleGlass : MonoBehaviour, IGlassBreakable
    {
        // =========================================================
        // SETTINGS
        // =========================================================

        [Header("Settings")]

        public Vector2 glassSize = new Vector2(1f, 1f);

        [Range(0.001f, 0.5f)]
        public float thickness = 0.05f;

        public Material glassMaterial;
        public Material glassCrossSectionMaterial;


        // =========================================================
        // AUTOMATIC BREAK
        // =========================================================

        [Header("Automatic Break")]

        public bool automaticBreak = false;

        [Min(0f)]
        public float automaticBreakDelay = 3f;


        // =========================================================
        // FRAGMENT ANIMATION
        // =========================================================

        [Header("Fragment Animation")]

        [Tooltip("Activates the animation where the glass pieces move outward before falling.")]
        public bool animateFragments = true;

        [Tooltip("How long the outward animation lasts.")]
        [Range(0.05f, 3f)]
        public float fragmentAnimationDuration = 0.5f;

        [Tooltip("How far the fragments move outward.")]
        [Range(0f, 5f)]
        public float fragmentExplosionForce = 1.5f;

        [Tooltip("Additional random movement applied to the fragments.")]
        [Range(0f, 5f)]
        public float fragmentUpwardForce = 0.5f;

        [Tooltip("Random rotation applied during the animation.")]
        [Range(0f, 10f)]
        public float fragmentTorque = 2f;

        [Tooltip("Delay before physics is enabled after the animation.")]
        [Range(0f, 2f)]
        public float fragmentPhysicsDelay = 0f;


        // =========================================================
        // DURABILITY
        // =========================================================

        [Header("Durability Settings")]

        [Tooltip("Determines if the glass can be broken by physical collisions.")]
        public bool isBreakableByPhysics = true;

        [SerializeField]
        private float healthPhysics = 10f;

        private float currentHealthPhysics;


        [Tooltip("Determines if the glass can be broken by raycast-based damage.")]
        public bool isBreakableByRaycast = true;

        [SerializeField]
        private float healthRaycast = 10f;

        private float currentHealthRaycast;


        // =========================================================
        // FRACTURE PROPAGATION
        // =========================================================

        [Header("Fracture Propagation")]

        public float baseRippleRadius = 0.2f;

        public float forceMultiplier = 0.05f;

        public float maxRippleRadius = 2f;


        // =========================================================
        // FRACTURE SETTINGS
        // =========================================================

        [Header("Fracture Settings")]

        [Range(1, 100)]
        public int XAxisDivisionCount = 10;

        [Range(1, 100)]
        public int YAxisDivisionCount = 10;

        [Range(0, 10)]
        public float tiltIntensity = 5f;

        [Range(1f, 60f)]
        public float fragmentLifetime = 3f;


        // =========================================================
        // SOUND
        // =========================================================

        [Header("Sound Settings")]

        public AudioSource audioSource;

        public AudioClip shatterSound;

        public AudioClip fractureSound;

        [Range(0f, 2f)]
        public float audioVolume = 1f;

        [Range(0f, 2f)]
        public float pitchRandomness = 0.2f;


        // =========================================================
        // CINEMATIC
        // =========================================================

        [Header("Cinematic Settings")]

        public bool cinematicMode = false;

        public float floatDuration = 3f;

        public float airDragDuringFloat = 2f;


        // =========================================================
        // REFERENCES
        // =========================================================

        [Header("References")]

        [SerializeField, HideInInspector]
        private GameObject visualMesh;

        [SerializeField, HideInInspector]
        private GameObject fracturedInstance;


        // =========================================================
        // INTERNAL STATE
        // =========================================================

        [SerializeField, HideInInspector]
        private bool previewShattered = false;

        private bool isCracked = false;

        private bool animationRunning = false;

        private Coroutine fragmentAnimationCoroutine;


        // =========================================================
        // START
        // =========================================================

        private void Start()
        {
            InitializeGlass();

            SetupAudioSource();

            if (Application.isPlaying && automaticBreak)
            {
                StartCoroutine(AutomaticBreakRoutine());
            }
        }


        // =========================================================
        // INITIALIZATION
        // =========================================================

        private void InitializeGlass()
        {
            currentHealthPhysics = healthPhysics;
            currentHealthRaycast = healthRaycast;

            isCracked = false;
            animationRunning = false;

            if (visualMesh != null)
            {
                visualMesh.SetActive(true);
            }

            if (fracturedInstance != null)
            {
                fracturedInstance.SetActive(false);

                Rigidbody[] rbs =
                    fracturedInstance.GetComponentsInChildren<Rigidbody>(true);

                foreach (Rigidbody rb in rbs)
                {
                    if (rb == null)
                        continue;

                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            Rigidbody myRb = GetComponent<Rigidbody>();

            if (myRb != null)
            {
                myRb.isKinematic = true;
            }
        }


        // =========================================================
        // AUTOMATIC BREAK
        // =========================================================

        private IEnumerator AutomaticBreakRoutine()
        {
            yield return new WaitForSeconds(automaticBreakDelay);

            if (!isCracked)
            {
                Fracture();
            }
        }


        // =========================================================
        // AUDIO
        // =========================================================

        private void SetupAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();

                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }


        // =========================================================
        // ON VALIDATE
        // =========================================================

        private void OnValidate()
        {
            /*
             * IMPORTANTE:
             *
             * NO crear GameObjects aquí.
             * NO usar SetParent aquí.
             * NO llamar CreatePrimitive aquí.
             *
             * Esto evita:
             *
             * SendMessage cannot be called during Awake,
             * CheckConsistency or OnValidate.
             */
        }


        // =========================================================
        // UPDATE GLASS
        // =========================================================

        public void UpdateGlass()
        {
            if (visualMesh == null)
            {
                Transform existing =
                    transform.Find("Glass_Solid");

                if (existing != null)
                {
                    visualMesh = existing.gameObject;
                }
            }

            if (visualMesh == null)
            {
                visualMesh =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);

                visualMesh.name = "Glass_Solid";

                visualMesh.transform.SetParent(
                    transform,
                    false
                );

                visualMesh.transform.localPosition =
                    Vector3.zero;

                visualMesh.transform.localRotation =
                    Quaternion.identity;
            }

            visualMesh.transform.localScale =
                new Vector3(
                    glassSize.x,
                    glassSize.y,
                    thickness
                );

            if (glassMaterial != null)
            {
                MeshRenderer renderer =
                    visualMesh.GetComponent<MeshRenderer>();

                if (renderer != null)
                {
                    renderer.sharedMaterial =
                        glassMaterial;
                }
            }
        }


        // =========================================================
        // CREATE FRACTURED BODY
        // =========================================================

        public void CreateFracturedBodyInScene()
        {
            CleanupOldFractures();

            SetupFractureContainer();

            GeneratePiecesFromMap();

            fracturedInstance.SetActive(false);
        }


        // =========================================================
        // CLEANUP
        // =========================================================

        private void CleanupOldFractures()
        {
            if (fracturedInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(fracturedInstance);
                }
                else
                {
                    DestroyImmediate(fracturedInstance);
                }

                fracturedInstance = null;
            }
        }


        // =========================================================
        // FRACTURE CONTAINER
        // =========================================================

        private void SetupFractureContainer()
        {
            fracturedInstance =
                new GameObject("Glass_Fractured");

            fracturedInstance.transform.SetParent(
                transform,
                false
            );

            fracturedInstance.transform.localPosition =
                Vector3.zero;

            fracturedInstance.transform.localRotation =
                Quaternion.identity;

            fracturedInstance.transform.localScale =
                Vector3.one;
        }


        // =========================================================
        // CREATE TRIANGLE
        // =========================================================

        private void CreateTrianglePiece(
            Vector3 v0,
            Vector3 v1,
            Vector3 v2,
            string pieceName,
            float pieceThickness)
        {
            GameObject piece =
                new GameObject(pieceName);

            piece.transform.SetParent(
                fracturedInstance.transform,
                false
            );

            Vector3 center =
                (v0 + v1 + v2) / 3f;

            piece.transform.localPosition =
                center;

            piece.transform.localRotation =
                Quaternion.Euler(
                    Random.Range(
                        -tiltIntensity,
                        tiltIntensity
                    ),
                    Random.Range(
                        -tiltIntensity,
                        tiltIntensity
                    ),
                    Random.Range(
                        -tiltIntensity * 0.5f,
                        tiltIntensity * 0.5f
                    )
                );

            piece.transform.localScale =
                Vector3.one;


            // -----------------------------------------------------
            // MESH
            // -----------------------------------------------------

            MeshFilter mf =
                piece.AddComponent<MeshFilter>();

            MeshRenderer mr =
                piece.AddComponent<MeshRenderer>();

            Mesh mesh =
                new Mesh();


            Vector3[] vertices =
                new Vector3[6];

            float halfThickness =
                pieceThickness * 0.5f;


            vertices[0] =
                (v0 - center) +
                Vector3.back * halfThickness;

            vertices[1] =
                (v1 - center) +
                Vector3.back * halfThickness;

            vertices[2] =
                (v2 - center) +
                Vector3.back * halfThickness;

            vertices[3] =
                (v0 - center) +
                Vector3.forward * halfThickness;

            vertices[4] =
                (v1 - center) +
                Vector3.forward * halfThickness;

            vertices[5] =
                (v2 - center) +
                Vector3.forward * halfThickness;


            int[] mainTris =
            {
                0, 1, 2,
                5, 4, 3
            };


            int[] sideTris =
            {
                0, 3, 4,
                0, 4, 1,

                1, 4, 5,
                1, 5, 2,

                2, 5, 3,
                2, 3, 0
            };


            int totalVertices =
                mainTris.Length +
                sideTris.Length;


            Vector3[] flatVertices =
                new Vector3[totalVertices];

            int[] flatMainTris =
                new int[mainTris.Length];

            int[] flatSideTris =
                new int[sideTris.Length];

            Vector2[] uvs =
                new Vector2[totalVertices];


            for (int i = 0; i < mainTris.Length; i++)
            {
                flatVertices[i] =
                    vertices[mainTris[i]];

                flatMainTris[i] =
                    i;

                uvs[i] =
                    new Vector2(
                        flatVertices[i].x,
                        flatVertices[i].y
                    );
            }


            float side1Len =
                Vector3.Distance(v0, v1);

            float side2Len =
                Vector3.Distance(v1, v2);

            float side3Len =
                Vector3.Distance(v2, v0);


            for (int i = 0; i < sideTris.Length; i++)
            {
                int index =
                    mainTris.Length + i;

                flatVertices[index] =
                    vertices[sideTris[i]];

                flatSideTris[i] =
                    index;

                float currentSideLen;

                if (i < 6)
                    currentSideLen = side1Len;
                else if (i < 12)
                    currentSideLen = side2Len;
                else
                    currentSideLen = side3Len;

                float uBase =
                    (float)(i % 6) / 5f;

                float u =
                    uBase *
                    currentSideLen *
                    100f;

                float v =
                    flatVertices[index].z > 0
                        ? 1f
                        : 0f;

                uvs[index] =
                    new Vector2(u, v);
            }


            mesh.vertices =
                flatVertices;

            mesh.uv =
                uvs;

            mesh.subMeshCount = 2;

            mesh.SetIndices(
                flatMainTris,
                MeshTopology.Triangles,
                0
            );

            mesh.SetIndices(
                flatSideTris,
                MeshTopology.Triangles,
                1
            );

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();


            mf.sharedMesh =
                mesh;


            if (glassCrossSectionMaterial != null)
            {
                mr.sharedMaterials =
                    new Material[]
                    {
                        glassMaterial,
                        glassCrossSectionMaterial
                    };
            }
            else
            {
                mr.sharedMaterial =
                    glassMaterial;
            }


            // -----------------------------------------------------
            // COLLIDER
            // -----------------------------------------------------

            MeshCollider mc =
                piece.AddComponent<MeshCollider>();

            mc.convex = true;

            mc.sharedMesh =
                mesh;


            // -----------------------------------------------------
            // RIGIDBODY
            // -----------------------------------------------------

            Rigidbody rb =
                piece.AddComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;

            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.05f;


            // -----------------------------------------------------
            // INDIVIDUAL FRAGMENT
            // -----------------------------------------------------

            IndividualFragment fragment =
                piece.AddComponent<IndividualFragment>();

            fragment.parentGlass =
                this;
        }


        // =========================================================
        // GENERATE VERTEX MAP
        // =========================================================

        private Vector3[,] GenerateVertexMap()
        {
            Vector3[,] grid =
                new Vector3[
                    XAxisDivisionCount + 1,
                    YAxisDivisionCount + 1
                ];


            float pieceWidth =
                glassSize.x /
                XAxisDivisionCount;

            float pieceHeight =
                glassSize.y /
                YAxisDivisionCount;


            float jitterAmount =
                0.4f;


            for (
                int x = 0;
                x <= XAxisDivisionCount;
                x++
            )
            {
                for (
                    int y = 0;
                    y <= YAxisDivisionCount;
                    y++
                )
                {
                    float posX =
                        x * pieceWidth -
                        glassSize.x * 0.5f;

                    float posY =
                        y * pieceHeight -
                        glassSize.y * 0.5f;


                    Vector3 pos =
                        new Vector3(
                            posX,
                            posY,
                            0f
                        );


                    if (
                        x > 0 &&
                        x < XAxisDivisionCount &&
                        y > 0 &&
                        y < YAxisDivisionCount
                    )
                    {
                        pos.x +=
                            Random.Range(
                                -pieceWidth * jitterAmount,
                                pieceWidth * jitterAmount
                            );

                        pos.y +=
                            Random.Range(
                                -pieceHeight * jitterAmount,
                                pieceHeight * jitterAmount
                            );
                    }


                    grid[x, y] =
                        pos;
                }
            }


            return grid;
        }


        // =========================================================
        // GENERATE PIECES
        // =========================================================

        private void GeneratePiecesFromMap()
        {
            Vector3[,] grid =
                GenerateVertexMap();


            for (
                int x = 0;
                x < XAxisDivisionCount;
                x++
            )
            {
                for (
                    int y = 0;
                    y < YAxisDivisionCount;
                    y++
                )
                {
                    Vector3 v0 =
                        grid[x, y];

                    Vector3 v1 =
                        grid[x, y + 1];

                    Vector3 v2 =
                        grid[x + 1, y + 1];

                    Vector3 v3 =
                        grid[x + 1, y];


                    if (Random.Range(0, 2) == 0)
                    {
                        CreateTrianglePiece(
                            v0,
                            v1,
                            v2,
                            $"Piece_{x}_{y}_A",
                            thickness
                        );

                        CreateTrianglePiece(
                            v0,
                            v2,
                            v3,
                            $"Piece_{x}_{y}_B",
                            thickness
                        );
                    }
                    else
                    {
                        CreateTrianglePiece(
                            v0,
                            v1,
                            v3,
                            $"Piece_{x}_{y}_A",
                            thickness
                        );

                        CreateTrianglePiece(
                            v1,
                            v2,
                            v3,
                            $"Piece_{x}_{y}_B",
                            thickness
                        );
                    }
                }
            }
        }


        // =========================================================
        // PREVIEW
        // =========================================================

        public void TogglePreview()
        {
            previewShattered =
                !previewShattered;


            if (visualMesh != null)
            {
                visualMesh.SetActive(
                    !previewShattered
                );
            }


            if (fracturedInstance != null)
            {
                fracturedInstance.SetActive(
                    previewShattered
                );
            }
        }


        // =========================================================
        // SHOW CRACKS
        // =========================================================

        public void ShowCracks()
        {
            if (isCracked)
                return;


            isCracked = true;


            // -----------------------------------------------------
            // HIDE SOLID GLASS
            // -----------------------------------------------------

            if (visualMesh != null)
            {
                visualMesh.SetActive(false);
            }


            // -----------------------------------------------------
            // SHOW FRACTURED GLASS
            // -----------------------------------------------------

            if (fracturedInstance == null)
            {
                Debug.LogWarning(
                    "FlexibleGlass: fracturedInstance is NULL. " +
                    "Generate the fractured body first."
                );

                return;
            }


            fracturedInstance.SetActive(true);


            // -----------------------------------------------------
            // START ANIMATION
            // -----------------------------------------------------

            if (fragmentAnimationCoroutine != null)
            {
                StopCoroutine(
                    fragmentAnimationCoroutine
                );
            }


            if (animateFragments)
            {
                fragmentAnimationCoroutine =
                    StartCoroutine(
                        AnimateFragmentsRoutine()
                    );
            }
            else
            {
                EnableFragmentPhysics();
            }
        }


        // =========================================================
        // FRAGMENT ANIMATION
        // =========================================================

        private IEnumerator AnimateFragmentsRoutine()
        {
            animationRunning = true;


            Transform[] pieces =
                GetFragmentTransforms();


            if (pieces.Length == 0)
            {
                animationRunning = false;

                EnableFragmentPhysics();

                yield break;
            }


            // -----------------------------------------------------
            // SAVE INITIAL TRANSFORMS
            // -----------------------------------------------------

            Vector3[] startPositions =
                new Vector3[pieces.Length];

            Quaternion[] startRotations =
                new Quaternion[pieces.Length];

            Vector3[] directions =
                new Vector3[pieces.Length];


            for (int i = 0; i < pieces.Length; i++)
            {
                Transform piece =
                    pieces[i];


                startPositions[i] =
                    piece.localPosition;

                startRotations[i] =
                    piece.localRotation;


                Vector3 direction =
                    piece.localPosition.normalized;


                if (direction.sqrMagnitude < 0.001f)
                {
                    direction =
                        Random.insideUnitSphere.normalized;
                }


                // Small random variation
                direction +=
                    Random.insideUnitSphere * 0.25f;


                direction.Normalize();


                directions[i] =
                    direction;


                Rigidbody rb =
                    piece.GetComponent<Rigidbody>();


                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;

                    rb.linearVelocity =
                        Vector3.zero;

                    rb.angularVelocity =
                        Vector3.zero;
                }
            }


            // -----------------------------------------------------
            // ANIMATION
            // -----------------------------------------------------

            float elapsed = 0f;


            while (
                elapsed <
                fragmentAnimationDuration
            )
            {
                elapsed +=
                    Time.deltaTime;


                float t =
                    Mathf.Clamp01(
                        elapsed /
                        fragmentAnimationDuration
                    );


                // Smooth animation
                float smoothT =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        t
                    );


                for (int i = 0; i < pieces.Length; i++)
                {
                    Transform piece =
                        pieces[i];


                    if (piece == null)
                        continue;


                    // -------------------------------------------------
                    // MOVE OUTWARD
                    // -------------------------------------------------

                    Vector3 targetPosition =
                        startPositions[i] +
                        directions[i] *
                        fragmentExplosionForce;


                    targetPosition +=
                        Vector3.up *
                        fragmentUpwardForce *
                        smoothT;


                    piece.localPosition =
                        Vector3.Lerp(
                            startPositions[i],
                            targetPosition,
                            smoothT
                        );


                    // -------------------------------------------------
                    // ROTATE
                    // -------------------------------------------------

                    Vector3 rotationAxis =
                        Random.onUnitSphere;


                    Quaternion additionalRotation =
                        Quaternion.AngleAxis(
                            fragmentTorque *
                            100f *
                            smoothT,
                            rotationAxis
                        );


                    piece.localRotation =
                        startRotations[i] *
                        additionalRotation;
                }


                yield return null;
            }


            // -----------------------------------------------------
            // OPTIONAL DELAY
            // -----------------------------------------------------

            if (fragmentPhysicsDelay > 0f)
            {
                yield return new WaitForSeconds(
                    fragmentPhysicsDelay
                );
            }


            // -----------------------------------------------------
            // ENABLE PHYSICS
            // -----------------------------------------------------

            EnableFragmentPhysics();


            animationRunning = false;
            fragmentAnimationCoroutine = null;
        }


        // =========================================================
        // GET PIECES
        // =========================================================

        private Transform[] GetFragmentTransforms()
        {
            if (fracturedInstance == null)
            {
                return new Transform[0];
            }


            List<Transform> pieces =
                new List<Transform>();


            foreach (
                Transform child
                in fracturedInstance.transform
            )
            {
                if (child == null)
                    continue;


                if (
                    child.GetComponent<Rigidbody>() != null
                )
                {
                    pieces.Add(child);
                }
            }


            return pieces.ToArray();
        }


        // =========================================================
        // ENABLE PHYSICS
        // =========================================================

        private void EnableFragmentPhysics()
        {
            if (fracturedInstance == null)
                return;


            Rigidbody[] rbs =
                fracturedInstance
                .GetComponentsInChildren<Rigidbody>(
                    true
                );


            foreach (Rigidbody rb in rbs)
            {
                if (rb == null)
                    continue;


                rb.isKinematic = false;

                rb.useGravity = true;

                rb.linearDamping = 0.05f;

                rb.angularDamping = 0.05f;


                // Small physical impulse
                Vector3 randomDirection =
                    Random.insideUnitSphere;

                randomDirection.y =
                    Mathf.Abs(
                        randomDirection.y
                    );


                randomDirection.Normalize();


                rb.AddForce(
                    randomDirection *
                    fragmentExplosionForce,
                    ForceMode.Impulse
                );


                rb.AddTorque(
                    Random.insideUnitSphere *
                    fragmentTorque,
                    ForceMode.Impulse
                );


                // -------------------------------------------------
                // IndividualFragment
                // -------------------------------------------------

                IndividualFragment fragment =
                    rb.GetComponent<IndividualFragment>();


                if (fragment != null)
                {
                    fragment.ActivatePhysics(
                        randomDirection *
                        fragmentExplosionForce,
                        fragmentLifetime
                    );
                }
            }


            // -----------------------------------------------------
            // CINEMATIC MODE
            // -----------------------------------------------------

            if (cinematicMode)
            {
                List<Rigidbody> activeBodies =
                    new List<Rigidbody>();


                foreach (Rigidbody rb in rbs)
                {
                    if (rb != null)
                    {
                        activeBodies.Add(rb);
                    }
                }


                if (activeBodies.Count > 0)
                {
                    ApplyCinematicFloat(
                        activeBodies
                    );
                }
            }
        }


        // =========================================================
        // WAKE NEIGHBORS
        // =========================================================

        public void WakeNeighbors(
            Vector3 center,
            float radius,
            Vector3 force)
        {
            Collider[] nearby =
                Physics.OverlapSphere(
                    center,
                    radius
                );


            List<Rigidbody> activatedRigidbodies =
                new List<Rigidbody>();


            foreach (Collider col in nearby)
            {
                if (
                    col.TryGetComponent<
                        IndividualFragment
                    >(out var fragment)
                )
                {
                    fragment.ActivatePhysics(
                        force,
                        fragmentLifetime
                    );


                    if (
                        fragment.TryGetComponent<
                            Rigidbody
                        >(out var rb)
                    )
                    {
                        activatedRigidbodies.Add(rb);
                    }
                }
            }


            if (
                cinematicMode &&
                activatedRigidbodies.Count > 0
            )
            {
                ApplyCinematicFloat(
                    activatedRigidbodies
                );
            }
        }


        // =========================================================
        // CINEMATIC FLOAT
        // =========================================================

        public void ApplyCinematicFloat(
            List<Rigidbody> fragments)
        {
            StartCoroutine(
                FloatRoutine(fragments)
            );
        }


        private IEnumerator FloatRoutine(
            List<Rigidbody> fragments)
        {
            foreach (Rigidbody rb in fragments)
            {
                if (rb == null)
                    continue;


                rb.useGravity = false;

                rb.linearDamping =
                    airDragDuringFloat;

                rb.angularDamping = 1f;


                rb.AddTorque(
                    Random.insideUnitSphere *
                    0.5f,
                    ForceMode.Impulse
                );
            }


            yield return new WaitForSeconds(
                floatDuration
            );


            foreach (Rigidbody rb in fragments)
            {
                if (rb == null)
                    continue;


                rb.useGravity = true;

                rb.linearDamping = 0.05f;

                rb.angularDamping = 0.05f;
            }
        }


        // =========================================================
        // COLLISION
        // =========================================================

        public void OnCollisionEnter(
            Collision collision)
        {
            if (!isBreakableByPhysics)
                return;


            if (
                collision.gameObject
                .GetComponent<IndividualFragment>()
                != null
            )
            {
                return;
            }


            float force =
                collision.relativeVelocity.magnitude;


            currentHealthPhysics -=
                force;


            if (currentHealthPhysics < 1f)
            {
                ShowCracks();
            }


            if (isCracked)
            {
                Vector3 impactVelocity =
                    collision.relativeVelocity;


                float radius =
                    calcDynamicRadius(force);


                if (
                    collision.contacts != null &&
                    collision.contacts.Length > 0
                )
                {
                    WakeNeighbors(
                        collision.contacts[0].point,
                        radius,
                        impactVelocity
                    );
                }


                PlaySound(
                    shatterSound,
                    audioVolume
                );
            }
        }


        // =========================================================
        // RAYCAST DAMAGE
        // =========================================================

        public void TakeDamage(
            Vector3 hitPoint,
            Vector3 direction,
            float force)
        {
            if (!isBreakableByRaycast)
                return;


            currentHealthRaycast -=
                force;


            if (currentHealthRaycast < 1f)
            {
                ShowCracks();
            }


            if (isCracked)
            {
                Vector3 impactVelocity =
                    direction.normalized *
                    force;


                float radius =
                    calcDynamicRadius(force);


                WakeNeighbors(
                    hitPoint,
                    radius,
                    impactVelocity
                );


                PlaySound(
                    shatterSound,
                    audioVolume
                );
            }
        }


        // =========================================================
        // DYNAMIC RADIUS
        // =========================================================

        private float calcDynamicRadius(
            float force)
        {
            float dynamicRadius =
                baseRippleRadius +
                force *
                forceMultiplier;


            dynamicRadius =
                Mathf.Min(
                    dynamicRadius,
                    maxRippleRadius
                );


            return dynamicRadius;
        }


        // =========================================================
        // SOUND
        // =========================================================

        private void PlaySound(
            AudioClip clip,
            float volume)
        {
            if (
                clip == null ||
                audioSource == null
            )
            {
                return;
            }


            audioSource.pitch =
                1f +
                Random.Range(
                    -pitchRandomness,
                    pitchRandomness
                );


            audioSource.PlayOneShot(
                clip,
                volume
            );
        }


        // =========================================================
        // FRACTURE
        // =========================================================

        public void Fracture()
        {
            if (isCracked)
                return;


            ShowCracks();


            PlaySound(
                fractureSound,
                audioVolume
            );
        }
    }
}
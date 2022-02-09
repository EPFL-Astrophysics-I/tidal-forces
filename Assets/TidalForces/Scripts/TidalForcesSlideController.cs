using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TidalForcesSlideController : SimulationSlideController
{
    [Header("Visibility")]
    public bool useEarth2D;
    public bool showGravityVectorCM;
    public bool showGravityVectors;
    public bool showInteriorVectors;
    public bool showTidalVectors;
    public bool showLights;

    [Header("Vectors")]
    public float scaleFactor = 1f;
    public int numGravityVectors = 8;
    public bool forcePointAtMoon = false;
    //public float tidalScaleFactor = 1f;
    public int numTidalVectors = 8;

    [Header("Rotation")]
    public bool earthIsRotating;
    public bool moonIsRotating;

    [Header("Interactivity")]
    public bool moonIsDraggable;
    public float moonMinX = 0;
    public float moonMaxX = 100;

    [Header("Eloigner la Lune")]
    public bool sendMoonAway;
    public Vector3 sendMoonTo = 100 * Vector3.right;
    public float sendTime = 5f;

    // Simulation and prefab manager
    private TidalForcesSimulation sim;
    private TidalForcesPrefabs prefabs;

    // Mouse clicks
    private SphereCollider moonCollider;
    private Camera mainCamera;
    private bool clickedOnUIElement;
    private Vector3 moonStartPosition;
    private bool draggingMoon;
    private float viewportClickedX;
    private float visibleWorldX;

    // To avoid calling OnEnable when first entering play mode
    private bool canEnable;

    private void Awake()
    {
        // Get reference to the active simulation and prefab manager
        sim = (TidalForcesSimulation)simulation;
        if (!sim.TryGetComponent(out prefabs))
        {
            Debug.LogWarning(transform.name + " did not find a prefab manager");
        }
        else
        {
            if (showInteriorVectors)
            {
                numGravityVectors *= 2;
            }
            prefabs.numGravityVectors = numGravityVectors;
            prefabs.numTidalVectors = numTidalVectors;
        }

        // Get reference to the main camera once at the start
        mainCamera = Camera.main;
    }

    private void Start()
    {
        // The moon should have been instantiated in TidalForcesSimulation.Awake
        if (sim.moon)
        {
            sim.moon.TryGetComponent(out moonCollider);
        }
    }

    private void Update()
    {
        // Gravity vectors are updated when the moon is moved
        HandleMouseInput();
        RotateCelestialBodies();
    }

    // Called automatically by SlideManager BEFORE OnEnable and OnDisable
    public override void ShowAndHideUIElements()
    {
        if (!prefabs)
        {
            return;
        }

        prefabs.SetEarthVisibility(useEarth2D);
        prefabs.SetGravityVectorCMVisibility(showGravityVectorCM);
        prefabs.SetGravityVectorsVisibility(showGravityVectors);
        prefabs.SetTidalVectorsVisibility(showTidalVectors);
        prefabs.SetLightsVisibility(showLights);
    }

    private void OnEnable()
    {
        if (!canEnable)
        {
            canEnable = true;
            return;
        }

        if (sim.moon)
        {
            if (sendMoonAway)
            {
                StartCoroutine(SendMoonAway(sendMoonTo, sendTime, 0.3f));
            }
            else if (sim.moon.Position.x < moonMinX || sim.moon.Position.x > moonMaxX)
            {
                StartCoroutine(SendMoonAway(sim.moonPosition, 1, 0));
            }
        }

        UpdateGravityVectors(true);
        UpdateTidalVectors(true);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void HandleMouseInput()
    {
        if (!moonIsDraggable || moonCollider == null)
        {
            return;
        }

        // Initial mouse click on the moon
        if (Input.GetMouseButtonDown(0))
        {
            clickedOnUIElement = EventSystem.current.IsPointerOverGameObject();

            if (clickedOnUIElement)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (moonCollider.Raycast(ray, out _, 1000f))
            {
                moonStartPosition = sim.moon.Position;
                viewportClickedX = mainCamera.ScreenToViewportPoint(Input.mousePosition).x;
                visibleWorldX = Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad) * mainCamera.aspect * Mathf.Abs(mainCamera.transform.position.z);
                draggingMoon = true;
            }
        }

        // Hitting while dragging
        if (Input.GetMouseButton(0) && draggingMoon && !clickedOnUIElement)
        {
            // Convert Viewport distance to distance along the world space x-axis
            float viewportX = mainCamera.ScreenToViewportPoint(Input.mousePosition).x;
            float worldDeltaX = (viewportX - viewportClickedX) * visibleWorldX;
            float moonNewX = Mathf.Clamp(moonStartPosition.x + worldDeltaX, moonMinX, moonMaxX);
            Vector3 moonNewPosition = new Vector3(moonNewX, moonStartPosition.y, moonStartPosition.z);
            sim.moon.Position = moonNewPosition;
            UpdateGravityVectors();
            UpdateTidalVectors();
        }

        if (Input.GetMouseButtonUp(0))
        {
            clickedOnUIElement = false;
            draggingMoon = false;
        }
    }

    private void RotateCelestialBodies()
    {
        if (earthIsRotating)
        {
            sim.earth.IncrementRotation(Time.deltaTime * sim.earth.RotationPeriod * Vector3.down);
        }

        if (moonIsRotating)
        {
            sim.moon.IncrementRotation(Time.deltaTime * sim.moon.RotationPeriod * Vector3.down);
        }
    }

    private void UpdateGravityVectors(bool firstTime = false)
    {
        // Gravity vector at the center of mass
        if (showGravityVectorCM && prefabs.gravityVectorCM != null)
        {
            prefabs.gravityVectorCM.SetPositions(sim.earth.Position, sim.earth.Position + GravityVector(sim.earth.Position));
            prefabs.gravityVectorCM.Redraw();
        }

        // Other gravity vectors
        if (showGravityVectors)
        {
            // Set tail positions evenly around the earth
            if (firstTime)
            {
                if (numGravityVectors != prefabs.numGravityVectors)
                {
                    Debug.Log("Creating " + numGravityVectors + " new gravity vectors");
                    prefabs.InstantiateGravityVectors(numGravityVectors);
                }

                if (showInteriorVectors)
                {
                    for (int i = 0; i < prefabs.gravityVectors.Count; i+=2)
                    {
                        float angle = i * 360f / (numGravityVectors / 2);
                        Vector gravityVector1 = prefabs.gravityVectors[i];
                        Vector gravityVector2 = prefabs.gravityVectors[i + 1];
                        Vector3 R = Quaternion.Euler(0, 0, angle) * (sim.earth.radius * Vector3.right);
                        gravityVector1.TailPosition = sim.earth.Position + R;
                        gravityVector2.TailPosition = sim.earth.Position + R / 2;
                        gravityVector1.Redraw();
                        gravityVector2.Redraw();
                    }
                }
                else
                {
                    for (int i = 0; i < prefabs.gravityVectors.Count; i++)
                    {
                        float angle = i * 360f / numGravityVectors;
                        Vector gravityVector = prefabs.gravityVectors[i];
                        Vector3 R = Quaternion.Euler(0, 0, angle) * (sim.earth.radius * Vector3.right);
                        gravityVector.TailPosition = sim.earth.Position + R;
                        gravityVector.Redraw();
                    }
                }
            }

            // Update only head positions
            foreach (Vector gravityVector in prefabs.gravityVectors)
            {
                gravityVector.HeadPosition = gravityVector.TailPosition + GravityVector(gravityVector.TailPosition);
                gravityVector.Redraw();
            }
        }
    }

    private void UpdateTidalVectors(bool firstTime = false)
    {
        if (showTidalVectors)
        {
            // Set tail positions evenly around the earth
            if (firstTime)
            {
                if (numTidalVectors != prefabs.numTidalVectors)
                {
                    Debug.Log("Creating new tidal vectors");
                    prefabs.InstantiateTidalVectors(numTidalVectors);
                }

                for (int i = 0; i < prefabs.tidalVectors.Count; i++)
                {
                    float angle = i * 360f / numTidalVectors;
                    Vector tidalVector = prefabs.tidalVectors[i];
                    Vector3 R = Quaternion.Euler(0, 0, angle) * (sim.earth.radius * Vector3.right);
                    tidalVector.TailPosition = sim.earth.Position + R;
                    tidalVector.Redraw();
                }
            }

            // Update only head positions
            foreach (Vector tidalVector in prefabs.tidalVectors)
            {
                tidalVector.HeadPosition = tidalVector.TailPosition + TidalVector(tidalVector.TailPosition);
                tidalVector.Redraw();
            }
        }
    }

    private Vector3 GravityVector(Vector3 position)
    {
        Vector3 r = sim.moon.Position - sim.earth.Position;
        Vector3 gAtCM = scaleFactor * sim.NewtonG * sim.moon.Mass / Mathf.Log10(r.magnitude) * r.normalized;
        Vector3 gAtP = gAtCM + TidalVector(position);
        if (forcePointAtMoon)
        {
            // Actually point the vector at the moon
            gAtP = gAtP.magnitude * (sim.moon.Position - position).normalized;
        }
        return gAtP;
    }

    private Vector3 TidalVector(Vector3 position)
    {
        Vector3 r = sim.moon.Position - position;
        Vector3 R = position - sim.earth.Position;
        float theta = Mathf.Atan2(R.y, R.x);
        float magnitudeAtCM = scaleFactor * sim.NewtonG * sim.moon.Mass / Mathf.Log10((r + R).magnitude);
        float tidalMagnitude = 0.25f * (R.magnitude / sim.earth.radius) * magnitudeAtCM;
        return tidalMagnitude * new Vector3(2 * Mathf.Cos(theta), -Mathf.Sin(theta), 0);
    }

    private IEnumerator SendMoonAway(Vector3 targetPosition, float moveTime, float startDelay = 0)
    {
        yield return new WaitForSeconds(startDelay);

        // Don't allow dragging while the coroutine is running
        bool canDragMoon = moonIsDraggable;
        moonIsDraggable = false;

        float time = 0;
        moonStartPosition = sim.moon.Position;

        while (time < moveTime)
        {
            time += Time.deltaTime;
            float t = time / moveTime;
            t = t * t * (3f - (2f * t));
            sim.moon.Position = Vector3.Lerp(moonStartPosition, targetPosition, t);
            UpdateGravityVectors();
            UpdateTidalVectors();
            yield return null;
        }

        // Allow dragging again (if the slide controller agrees)
        moonIsDraggable = canDragMoon;
    }
}

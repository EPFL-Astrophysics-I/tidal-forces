using UnityEngine;
using UnityEngine.EventSystems;

public class DragRadiusSlideController : SimulationSlideController
{
    [Header("Tidal Vector")]
    public GameObject tidalVectorPrefab;
    public bool showTidalVector;
    public float tidalMagnitude = 1f;

    [Header("Coordinate Origin")]
    public GameObject coordinateOriginPrefab;
    public bool showCoordinateOrigin;

    [Header("Radius")]
    public GameObject radiusVectorPrefab;
    public bool showRadiusVector;

    [Header("Angle")]
    public GameObject circleArcPrefab;
    public GameObject angleLabelPrefab;
    public bool showAngle;

    // Simulation and prefab manager
    private DragRadiusSimulation sim;

    // Mouse clicks
    private SphereCollider pointCollider;
    private Camera mainCamera;
    private bool clickedOnUIElement;
    private float pointStartAngle;  // In radians
    private bool draggingPoint;
    private float viewportClickAngle;
    private Vector2 viewportOrigin;

    // References to instantiated objects
    private Vector tidalVector;
    private Transform coordinateOrigin;
    private Vector radiusVector;
    private CircleArc circleArc;
    private SpriteRenderer angleLabel;

    private void Awake()
    {
        // Get reference to the active simulation and prefab manager
        sim = (DragRadiusSimulation)simulation;
        
        // Get reference to the main camera once at the start
        mainCamera = Camera.main;
    }

    private void OnDisable()
    {
        if (tidalVector != null)
        {
            Destroy(tidalVector.gameObject);
            tidalVector = null;
        }

        if (coordinateOrigin != null)
        {
            Destroy(coordinateOrigin.gameObject);
            coordinateOrigin = null;
        }

        if (radiusVector != null)
        {
            Destroy(radiusVector.gameObject);
            radiusVector = null;
        }

        if (circleArc != null)
        {
            Destroy(circleArc.gameObject);
            circleArc = null;
        }

        if (angleLabel != null)
        {
            Destroy(angleLabel.gameObject);
            angleLabel = null;
        }
    }

    // Called automatically by SlideManager BEFORE OnEnable and OnDisable
    public override void InitializeSlide()
    {
        // The point should have been instantiated in DragRadiusSimulation.Awake()
        if (sim.point && !pointCollider)
        {
            sim.point.TryGetComponent(out pointCollider);
        }

        UpdateTidalVector(true);
        CreateCoordinateOrigin();
        UpdateRadiusVector(true);
        UpdateCircleArc(true);
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (!pointCollider)
        {
            return;
        }

        // Initial mouse click on the point
        if (Input.GetMouseButtonDown(0))
        {
            clickedOnUIElement = EventSystem.current.IsPointerOverGameObject();

            if (clickedOnUIElement)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (pointCollider.Raycast(ray, out _, 1000f))
            {
                pointStartAngle = Mathf.Atan2(sim.point.position.y, sim.point.position.x);
                Vector2 viewportClickPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);
                viewportOrigin = mainCamera.WorldToViewportPoint(sim.earthPosition);
                Vector2 viewportDisplacement = viewportClickPosition - viewportOrigin;
                viewportClickAngle = Mathf.Atan2(viewportDisplacement.y, viewportDisplacement.x * mainCamera.aspect);
                draggingPoint = true;
            }
        }

        // Hitting while dragging
        if (Input.GetMouseButton(0) && draggingPoint && !clickedOnUIElement)
        {
            Vector2 viewportPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);
            Vector2 viewportDisplacement = viewportPosition - viewportOrigin;
            float deltaAngle = Mathf.Atan2(viewportDisplacement.y, viewportDisplacement.x * mainCamera.aspect) - viewportClickAngle;
            float pointNewX = sim.earthRadius * Mathf.Cos(pointStartAngle + deltaAngle);
            float pointNewY = sim.earthRadius * Mathf.Sin(pointStartAngle + deltaAngle);
            sim.point.position = new Vector3(pointNewX, pointNewY, sim.earthPosition.z);
            UpdateTidalVector();
            UpdateRadiusVector();
            UpdateCircleArc();
        }

        if (Input.GetMouseButtonUp(0))
        {
            clickedOnUIElement = false;
            draggingPoint = false;
        }
    }

    private void UpdateTidalVector(bool firstTime = false)
    {
        if (showTidalVector)
        {
            if (firstTime)
            {
                //Debug.Log("Creating new tidal vector");
                tidalVector = Instantiate(tidalVectorPrefab, sim.transform).GetComponent<Vector>();
                tidalVector.name = "Tidal Vector";
            }

            if (tidalVector != null)
            {
                tidalVector.TailPosition = sim.point.position;
                Vector2 pointPosition = sim.point.position - sim.earthPosition;
                float angle = Mathf.Atan2(pointPosition.y, pointPosition.x);
                Vector3 tide = tidalMagnitude * (2 * Mathf.Cos(angle) * Vector3.right - Mathf.Sin(angle) * Vector3.up);
                tidalVector.HeadPosition = tidalVector.TailPosition + tide;
                tidalVector.Redraw();
            }
        }
    }

    private void CreateCoordinateOrigin()
    {
        if (showCoordinateOrigin && coordinateOriginPrefab != null)
        {
            //Debug.Log("Creating new origin");
            coordinateOrigin = Instantiate(coordinateOriginPrefab, sim.transform).transform;
            coordinateOrigin.position = sim.earthPosition;
            coordinateOrigin.name = "Origin";
        }
    }

    private void UpdateRadiusVector(bool firstTime = false)
    {
        if (showRadiusVector)
        {
            if (firstTime && radiusVectorPrefab != null)
            {
                //Debug.Log("Creating new radius vector");
                radiusVector = Instantiate(radiusVectorPrefab, sim.transform).GetComponent<Vector>();
                radiusVector.name = "Radius Vector";
                radiusVector.TailPosition = sim.earthPosition;
            }

            if (radiusVector != null)
            {
                radiusVector.HeadPosition = sim.point.position;
                radiusVector.Redraw();
            }
        }
    }

    private void UpdateCircleArc(bool firstTime = false)
    {
        if (showAngle)
        {
            if (firstTime && circleArcPrefab != null)
            {
                //Debug.Log("Creating new circle arc");
                circleArc = Instantiate(circleArcPrefab, sim.transform).GetComponent<CircleArc>();
                circleArc.name = "Circle Arc";
                circleArc.radius = 0.2f * sim.earthRadius;
                circleArc.angle = 0;

                if (angleLabelPrefab != null)
                {
                    //Debug.Log("Creating angle label sprite");
                    angleLabel = Instantiate(angleLabelPrefab, sim.transform).GetComponent<SpriteRenderer>();
                    angleLabel.color = circleArc.color;
                }
            }

            if (circleArc != null)
            {
                Vector2 pointPosition = sim.point.position - sim.earthPosition;
                float theta = Mathf.Atan2(pointPosition.y, pointPosition.x);
                if (theta < 0)
                {
                    theta += 2 * Mathf.PI;
                }
                circleArc.angle = Mathf.Rad2Deg * theta;
                circleArc.Redraw();

                if (angleLabel != null)
                {
                    float radius = 1.8f * circleArc.radius;
                    Vector3 position = radius * new Vector3(Mathf.Cos(theta / 2), Mathf.Sin(theta / 2), 0);
                    angleLabel.transform.position = position;

                    angleLabel.gameObject.SetActive(theta / 2 > 14 * Mathf.Deg2Rad);
                }
            }
        }
    }
}
using static Units;
using UnityEngine;
using System.Collections.Generic;

// TODO Could use a PrefabManager, since the file is getting a bit bloated
public class EarthMoonSimulation : Simulation
{
    [Header("Prefabs")]
    public GameObject earthPrefab = default;
    public GameObject moonPrefab = default;
    public GameObject oceanPrefab = default;
    public GameObject[] lightPrefabs = default;
    public GameObject connectorPrefab = default;
    public GameObject orbitPrefab = default;
    public GameObject orbitNotchPrefab = default;

    [Header("Simulation")]
    public int numSubsteps = 100;
    public bool resetAfterOnePeriod = true;
    public UnitTime unitTime = UnitTime.Month;
    public UnitLength unitLength = UnitLength.EarthRadius;
    public UnitMass unitMass = UnitMass.EarthMass;
    public float timeScale = 1;

    [Header("Earth")]
    public Vector3 earthPosition = Vector3.zero;
    public float earthRadius = 1;
    public bool earthIsRotating = false;
    public bool earthIsDeforming = false;

    [Header("Moon")]
    public bool moonIsRotating = true;

    // Celestial bodies
    [HideInInspector] public CelestialBody earth;
    [HideInInspector] public CelestialBody moon;
    [HideInInspector] public CelestialBody ocean;

    // Lighting
    [HideInInspector] public List<Transform> lights;

    // Vector connector
    [HideInInspector] public Vector connector;

    // Orbit
    [HideInInspector] public CircularOrbit orbit;
    [HideInInspector] public Vector orbitNotch;

    // Timer for resetting the simulation after one orbital period
    [HideInInspector] public float resetTimer;
    private Vector3 initMoonPosition;
    private float moonDistance;

    private Material earthMaterial;

    // Gravitational constant
    private float _newtonG;
    public float NewtonG => (_newtonG != 0) ? _newtonG : Units.NewtonG(unitTime, unitLength, unitMass);

    // Orbital period
    public float Period => 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(moonDistance, 3) / NewtonG / earth.Mass);

    private void Awake()
    {
        // Create CelestialBodies and assign their properties
        Reset();

        // Compute Newton's constant only once
        _newtonG = NewtonG;
    }

    private void Start()
    {
        if (earth != null)
        {
            Vector3 scale = 2 * earthRadius * Vector3.one;
            earth.SetScale(scale);
            earthMaterial = earth.GetComponent<MeshRenderer>().material;
            earthMaterial.mainTextureOffset = 0.3f * Vector2.right;
        }
    }

    private void FixedUpdate()
    {
        if (paused)
        {
            return;
        }

        if (resetAfterOnePeriod)
        {
            // Re-establish the system to exact initial positions after one period to avoid numerical errors
            if (resetTimer >= Period)
            {
                resetTimer = 0;
                moon.Position = initMoonPosition;
                //Debug.Log("Resetting sim...");
            }

            resetTimer += timeScale * Time.fixedDeltaTime;
        }

        // Solve the equation of motion
        float substep = timeScale * Time.fixedDeltaTime / numSubsteps;
        for (int i = 1; i <= numSubsteps; i++)
        {
            StepForward(substep);
        }

        if (earthIsRotating)
        {
            float deltaAngle = timeScale * Time.fixedDeltaTime * 360 / earth.RotationPeriod;
            earth.IncrementRotation(deltaAngle * Vector3.down);
        }

        if (earthIsDeforming)
        {
            float offsetAngle = earth.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            Vector3 direction = (moon.Position - earth.Position).normalized;
            float angle = 2 * (Mathf.Atan2(direction.z, direction.x) + offsetAngle);
            if (earth.TryGetComponent(out SphereToEllipsoid s2e))
            {
                s2e.ShearXZ(0.15f, angle);
            }

            if (ocean != null)
            {
                if (ocean.TryGetComponent(out SphereToEllipsoid s2eOcean))
                {
                    // Bulge the ocean slightly more than the earth
                    s2eOcean.ShearXZ(0.3f, angle);
                }
            }
        }

        if (moonIsRotating)
        {
            float deltaAngle = timeScale * Time.fixedDeltaTime * 360 / moon.RotationPeriod;
            moon.IncrementRotation(deltaAngle * Vector3.down);
        }

        if (connector != null)
        {
            connector.SetPositions(earth.Position, moon.Position);
            connector.Redraw();
        }
    }

    private void StepForward(float deltaTime)
    {
        // Solve the equation of motion in polar coordinates
        Vector3 vectorR = moon.Position - earth.Position;
        float theta = Mathf.Atan2(vectorR.z, vectorR.x);

        // Update moon position
        float angularMomentum = Mathf.Sqrt(NewtonG * earth.Mass * moonDistance);
        theta += angularMomentum * deltaTime / vectorR.sqrMagnitude;
        float r = vectorR.magnitude;
        Vector3 position = new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
        moon.Position = earth.Position + position;
    }

    public override void Reset()
    {
        resetTimer = 0;

        // Earth
        if (earthPrefab != null)
        {
            if (earth == null)
            {
                earth = Instantiate(earthPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<CelestialBody>();
                earth.gameObject.name = "Earth";
            }
            earth.Position = earthPosition;
            earth.Mass = EarthMass(unitMass);
            earth.SetRadius(earthRadius * EarthRadius(unitLength));
            earth.RotationPeriod = EarthRotationPeriod(unitTime);
        }

        // Moon
        if (moonPrefab != null)
        {
            if (moon == null)
            {
                moon = Instantiate(moonPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<CelestialBody>();
                moon.gameObject.name = "Moon";
            }
            moon.Position = earth.Position + LunarDistance(unitLength) * Vector3.right;
            moon.Mass = LunarMass(unitMass);
            moon.SetRadius(earthRadius * LunarRadius(unitLength));
            initMoonPosition = moon.Position;
            moonDistance = (moon.Position - earth.Position).magnitude;
            moon.RotationPeriod = Period;
        }

        // Ocean
        if (oceanPrefab != null)
        {
            if (ocean == null)
            {
                ocean = Instantiate(oceanPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<CelestialBody>();
                ocean.gameObject.name = "Ocean";
            }
            if (earth != null)
            {
                ocean.transform.parent = earth.transform;
                ocean.Position = Vector3.zero;
                ocean.SetRadius(0.51f);
            }
        }

        // Create lights only the first time Reset() is called
        if (lights.Count == 0 && lightPrefabs.Length > 0)
        {
            lights = new List<Transform>();
            foreach (GameObject lightPrefab in lightPrefabs)
            {
                lights.Add(Instantiate(lightPrefab, transform).transform);
            }
        }

        if (connectorPrefab != null)
        {
            if (connector == null)
            {
                connector = Instantiate(connectorPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Vector>();
                connector.gameObject.name = "Connector";
            }
            connector.SetPositions(earth.Position, moon.Position);
            connector.Redraw();
        }

        if (orbitPrefab != null)
        {
            if (orbit == null)
            {
                orbit = Instantiate(orbitPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<CircularOrbit>();
                orbit.gameObject.name = "Orbit";
            }
            orbit.DrawOrbit(earth.Position, LunarDistance(unitLength), 100);
        }

        if (orbitNotchPrefab != null)
        {
            if (orbitNotch == null)
            {
                orbitNotch = Instantiate(orbitNotchPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Vector>();
                orbitNotch.gameObject.name = "Orbit Notch";
            }
            float radius = LunarDistance(unitLength);
            Vector3 tailPosition = earth.Position + radius * Vector3.right;
            Vector3 headPosition = tailPosition + 0.1f * radius * Vector3.left;
            orbitNotch.SetPositions(tailPosition, headPosition);
            orbitNotch.Redraw();
        }

        //Debug.Log(" G = " + NewtonG);
        //Debug.Log(" period is " + Period);
        //Debug.Log(" Earth mass is " + earth.Mass);
        //Debug.Log(" Earth radius is " + earth.radius);
        //Debug.Log(" Earth rotation is " + earth.RotationPeriod);
        //Debug.Log(" Moon mass is " + moon.Mass);
        //Debug.Log(" Moon radius is " + moon.radius);
        //Debug.Log(" Moon rotation is " + moon.RotationPeriod);
    }

    //private void OnDrawGizmos()
    //{
    //    Vector3 direction = (moon.Position - earth.Position).normalized;
    //    float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
    //    Gizmos.color = Color.magenta;
    //    Gizmos.DrawLine(earth.Position, earth.Position + 100 * direction);
    //}
}

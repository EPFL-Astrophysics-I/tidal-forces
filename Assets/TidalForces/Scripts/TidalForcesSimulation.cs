using static Units;
using UnityEngine;

[RequireComponent(typeof(TidalForcesPrefabs))]
public class TidalForcesSimulation : Simulation
{
    [Header("Units")]
    public float timeScale = 1;
    public UnitTime unitTime = UnitTime.Day;
    public UnitLength unitLength = UnitLength.EarthRadius;
    public UnitMass unitMass = UnitMass.EarthMass;

    [Header("Earth")]
    public float earthRadius = 1f;
    public Vector3 earthPosition = Vector3.zero;

    [Header("Moon")]
    public float moonRadius = 1f;
    public Vector3 moonPosition = Vector3.right;

    private TidalForcesPrefabs prefabs;
    [HideInInspector] public CelestialBody earth;
    [HideInInspector] public Transform earth2D;
    [HideInInspector] public CelestialBody moon;

    // Gravitational constant
    private float _newtonG;
    public float NewtonG => (_newtonG != 0) ? _newtonG : Units.NewtonG(unitTime, unitLength, unitMass);

    private void Awake()
    {
        if (!TryGetComponent(out prefabs))
        {
            Debug.LogWarning("No TidalForcesPrefabs component found.");
            return;
        }

        prefabs.InstantiatePrefabs();

        earth = prefabs.earth;
        earth2D = prefabs.earth2D;
        moon = prefabs.moon;

        // Create CelestialBodies and assign their properties
        Reset();

        // Compute Newton's constant only once
        _newtonG = NewtonG;
    }

    public override void Reset()
    {
        if (earth)
        {
            earth.Position = earthPosition;
            earth.SetRotation(new Vector3(0, 104f, 0));
            earth.Mass = 1f;
            earth.SetRadius(earthRadius);
            earth.RotationPeriod = 2;
        }

        if (earth2D)
        {
            earth2D.transform.position = earthPosition;
            earth2D.transform.localScale = 2 * earthRadius * Vector3.one;
        }

        if (moon)
        {
            moon.Position = moonPosition;
            moon.SetRotation(new Vector3(0, 193f, 0));
            moon.Mass = 0.0123f;
            moon.SetRadius(moonRadius);
        }
    }
}

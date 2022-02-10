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

        // Create CelestialBodies and assign their properties
        prefabs.InstantiatePrefabs();
        Reset();

        // Compute Newton's constant only once
        _newtonG = NewtonG;
    }

    public override void Reset()
    {
        CelestialBody earth = prefabs.earth;
        if (earth)
        {
            earth.Position = earthPosition;
            earth.SetRotation(new Vector3(0, 104f, 0));
            earth.Mass = 1f;
            earth.SetRadius(earthRadius);
            earth.RotationPeriod = 2;
        }

        Transform earth2D = prefabs.earth2D;
        if (earth2D)
        {
            earth2D.transform.position = earthPosition;
            earth2D.transform.localScale = 2 * earthRadius * Vector3.one;
        }

        CelestialBody moon = prefabs.moon;
        if (moon)
        {
            moon.Position = moonPosition;
            moon.SetRotation(new Vector3(0, 193f, 0));
            moon.Mass = 0.0123f;
            moon.SetRadius(moonRadius);
        }
    }
}

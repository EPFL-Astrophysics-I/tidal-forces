using System.Collections.Generic;
using UnityEngine;

public class DragRadiusSimulation : Simulation
{
    [Header("Prefabs")]
    public GameObject earthPrefab = default;
    public GameObject pointPrefab = default;

    [Header("Earth")]
    public float earthRadius = 1f;
    public Vector3 earthPosition = Vector3.zero;

    [Header("Point")]
    [Range(0, 360)] public float pointStartAngle = 0;
    [Min(1)] public float pointScale = 1;

    [Header("Lights")]
    public List<GameObject> lightPrefabs;

    [HideInInspector] public Transform earth;
    [HideInInspector] public Transform point;
    [HideInInspector] public List<Light> lights;

    private void Awake()
    {
        // Create CelestialBodies and assign their properties
        if (earthPrefab)
        {
            earth = Instantiate(earthPrefab, transform).transform;
            earth.gameObject.name = "Earth";
        }

        if (pointPrefab)
        {
            point = Instantiate(pointPrefab, transform).transform;
            point.gameObject.name = "Point";
        }

        foreach (GameObject lightPrefab in lightPrefabs)
        {
            Light light = Instantiate(lightPrefab, transform).GetComponent<Light>();
            lights.Add(light);
        }

        Reset();
    }

    public override void Reset()
    {
        if (earth)
        {
            earth.position = earthPosition;
            earth.localScale = 2 * earthRadius * Vector3.one;
            earth.localRotation = Quaternion.Euler(0, 104f, 0);
        }

        if (point)
        {
            float angle = pointStartAngle * Mathf.Deg2Rad;
            point.position = earthPosition + earthRadius * (Mathf.Cos(angle) * Vector3.right + Mathf.Sin(angle) * Vector3.up);
            point.localScale = pointScale * Vector3.one;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class SquashEarthAnimation : Simulation
{
    [Header("Prefabs")]
    public GameObject earthPrefab = default;
    public GameObject[] lightPrefabs = default;

    [Header("Earth")]
    public float earthRadius = 1f;
    public Vector3 earthPosition = Vector3.zero;

    [HideInInspector] public Transform earth;

    // Lights
    private List<Transform> lights;

    private void Awake()
    {
        if (earthPrefab)
        {
            earth = Instantiate(earthPrefab, transform).transform;
            earth.gameObject.name = "Earth";
        }

        lights = new List<Transform>();
        foreach (GameObject lightPrefab in lightPrefabs)
        {
            Transform light = Instantiate(lightPrefab, transform).transform;
            lights.Add(light);
        }

        Reset();
    }

    public override void Reset()
    {
        if (earth)
        {
            earth.transform.position = earthPosition;
            //earth.transform.rotation = Quaternion.Euler(0, 104, 0);
            earth.transform.localScale = 2 * earthRadius * Vector3.one;
            earth.GetComponent<MeshRenderer>().material.mainTextureOffset = 0.3f * Vector2.right;
        }
    }
}

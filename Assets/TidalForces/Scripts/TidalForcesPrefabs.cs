using System.Collections.Generic;
using UnityEngine;

public class TidalForcesPrefabs : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject earthPrefab;
    public GameObject earth2DPrefab;
    public GameObject moonPrefab;
    public GameObject gravityVectorCMPrefab;  // Gravitational acceleration at Earth's center of mass
    public GameObject gravityVectorPrefab;    // ... at any other point P
    public GameObject tidalVectorPrefab;      // Tidal force vector
    public List<GameObject> lightPrefabs;

    public CelestialBody earth;
    public Transform earth2D;
    public CelestialBody moon;

    // Parameters set by TidalForcesSlideController.Awake
    [HideInInspector] public int numGravityVectors;
    [HideInInspector] public int numTidalVectors;

    // Actual instance references
    [HideInInspector] public Vector gravityVectorCM;
    [HideInInspector] public List<Vector> gravityVectors;
    [HideInInspector] public List<Vector> tidalVectors;
    [HideInInspector] public List<Light> lights;

    private void Start()
    {
        // The idea here is to instantiate all assigned prefabs at the start
        // and then let the SlideController control each gameObject's visibility;
        // Each simulation owns its set of instantiated gameObjects, and multiple
        // slides can have access to them;
        // Prefabs that are not assigned in the inspector cannot be instantiated
        //InstantiatePrefabs();

        // Need to force reset here to make sure no residual values are lingering
        // due to the variables being hidden but public
        numGravityVectors = 0;
        numTidalVectors = 0;
    }

    public void InstantiatePrefabs()
    {
        if (earthPrefab)
        {
            GameObject go = Instantiate(earthPrefab, transform);
            if (!go.transform.TryGetComponent(out earth))
            {
                Debug.LogWarning(go.name + " does not have a CelestialBody component");
            }
            go.name = "Earth";
        }

        if (earth2DPrefab)
        {
            earth2D = Instantiate(earth2DPrefab, transform).transform;
            earth2D.gameObject.name = "Earth 2D";
        }

        if (moonPrefab)
        {
            GameObject go = Instantiate(moonPrefab, transform);
            if (!go.transform.TryGetComponent(out moon))
            {
                Debug.LogWarning(go.name + " does not have a CelestialBody component");
            }
            go.name = "Moon";
        }

        if (gravityVectorCMPrefab != null)
        {
            gravityVectorCM = Instantiate(gravityVectorCMPrefab, transform).GetComponent<Vector>();
            gravityVectorCM.name = "Gravity Vector CM";
        }

        foreach (GameObject lightPrefab in lightPrefabs)
        {
            Light light = Instantiate(lightPrefab, transform).GetComponent<Light>();
            lights.Add(light);
        }
    }

    public void InstantiateGravityVectors(int numVectors)
    {
        if (gravityVectorPrefab != null)
        {
            // Clear out any previous gravity vectors
            foreach (Vector gravityVector in gravityVectors)
            {
                Destroy(gravityVector.gameObject);
            }

            // Start with a clean slate
            gravityVectors = new List<Vector>();

            // Create the new gravity vectors
            for (int i = 0; i < numVectors; i++)
            {
                Vector gravityVector = Instantiate(gravityVectorPrefab, transform).GetComponent<Vector>();
                gravityVectors.Add(gravityVector);
                gravityVector.name = "Gravity Vector " + i;
            }

            numGravityVectors = numVectors;
        }
    }

    public void InstantiateTidalVectors(int numVectors)
    {
        if (tidalVectorPrefab != null)
        {
            // Clear out any previous tidal vectors
            foreach (Vector tidalVector in tidalVectors)
            {
                Destroy(tidalVector.gameObject);
            }

            // Start with a clean slate
            tidalVectors = new List<Vector>();

            // Create the new tidal vectors
            for (int i = 0; i < numVectors; i++)
            {
                Vector tidalVector = Instantiate(tidalVectorPrefab, transform).GetComponent<Vector>();
                tidalVectors.Add(tidalVector);
                tidalVector.name = "Tidal Vector " + i;
            }

            numTidalVectors = numVectors;
        }
    }

    public void SetGravityVectorCMVisibility(bool visible)
    {
        if (gravityVectorCM != null)
        {
            gravityVectorCM.gameObject.SetActive(visible);
        }
    }

    public void SetGravityVectorsVisibility(bool visible)
    {
        foreach (Vector gravityVector in gravityVectors)
        {
            gravityVector.gameObject.SetActive(visible);
        }
    }

    public void SetTidalVectorsVisibility(bool visible)
    {
        foreach (Vector tidalVector in tidalVectors)
        {
            tidalVector.gameObject.SetActive(visible);
        }
    }

    public void SetLightsVisibility(bool visible)
    {
        foreach (Light light in lights)
        {
            light.gameObject.SetActive(visible);
        }
    }

    public void SetEarthVisibility(bool use2D)
    {
        if (earth)
        {
            earth.gameObject.SetActive(!use2D);
        }

        if (earth2D != null)
        {
            earth2D.gameObject.SetActive(use2D);
        }
    }
}

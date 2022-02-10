using UnityEngine;

public class TidalVectorAnimation : Simulation
{
    [Header("Prefabs")]
    public GameObject gravityVectorCMPrefab = default;
    public GameObject gravityVectorPrefab = default;
    public GameObject tidalVectorPrefab = default;

    [Header("Vectors")]
    public Vector3 origin;
    public Vector3 gravityVectorCMComponents;
    public Vector3 gravityVectorComponents;
    public Vector3 tidalVectorComponents;

    [HideInInspector] public Vector gravityVectorCM;
    [HideInInspector] public Vector gravityVector;
    [HideInInspector] public Vector tidalVector;

    private void Awake()
    {
        Reset();
    }

    public override void Reset()
    {
        if (gravityVectorCMPrefab)
        {
            if (gravityVectorCM == null)
            {
                gravityVectorCM = Instantiate(gravityVectorCMPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Vector>();
                gravityVectorCM.gameObject.name = "Gravity Vector CM";
            }
            gravityVectorCM.SetPositions(origin, origin + gravityVectorCMComponents);
        }

        if (gravityVectorPrefab)
        {
            if (gravityVector == null)
            {
                gravityVector = Instantiate(gravityVectorPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Vector>();
                gravityVector.gameObject.name = "Gravity Vector";
            }
            gravityVector.SetPositions(origin, origin + gravityVectorComponents);
        }

        if (tidalVectorPrefab)
        {
            if (tidalVector == null)
            {
                tidalVector = Instantiate(tidalVectorPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Vector>();
                tidalVector.gameObject.name = "Tidal Vector";
            }
            tidalVector.SetPositions(origin, origin + tidalVectorComponents);
        }
    }
}

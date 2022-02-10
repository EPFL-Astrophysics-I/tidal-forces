using System.Collections;
using UnityEngine;

public class SquashEarthSlideController : SimulationSlideController
{
    public float squashTime = 2f;
    public float restartDelay = 2f;

    // Simulation reference
    private SquashEarthAnimation sim;

    private Vector3 startScale;

    private void Awake()
    {
        // Get reference to the active simulation and prefab manager
        sim = (SquashEarthAnimation)simulation;
    }

    // Called automatically by SlideManager BEFORE OnEnable and OnDisable
    public override void InitializeSlide()
    {
        startScale = sim.earth.transform.localScale;
        StartCoroutine(SquashEarth(sim.earth, squashTime, 1.5f));
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (sim != null)
        {
            sim.Reset();
        }
    }

    private IEnumerator SquashEarth(Transform earth, float squashTime, float startDelay = 0)
    {
        yield return new WaitForSeconds(startDelay);

        float time = 0;

        float targetScaleX = 1.2f * startScale.x;
        float targetScaleY = 0.85f * startScale.y;
        float targetScaleZ = 0.85f * startScale.z;
        Vector3 targetScale = new Vector3(targetScaleX, targetScaleY, targetScaleZ);
        while (time < squashTime)
        {
            time += Time.deltaTime;
            float t = time / squashTime;
            t = t * t * (3f - (2f * t));
            earth.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        yield return new WaitForSeconds(restartDelay);
        sim.earth.transform.localScale = startScale;
        StartCoroutine(SquashEarth(sim.earth, squashTime));
    }
}

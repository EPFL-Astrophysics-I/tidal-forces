using System.Collections;
using UnityEngine;

public class TidalVectorAnimationSlideController : SimulationSlideController
{
    public float moveTime = 2f;
    public float fadeTime = 1f;
    public float restartDelay = 2f;

    // Simulation reference
    private TidalVectorAnimation sim;

    private Vector3 startTail;
    private Vector3 startHead;
    private Color colorOpaque;
    private Color colorTransparent;

    private void Awake()
    {
        // Get reference to the active simulation and prefab manager
        sim = (TidalVectorAnimation)simulation;
    }

    // Called automatically by SlideManager BEFORE OnEnable and OnDisable
    public override void InitializeSlide()
    {
        //Debug.Log("Starting animation");
        startTail = sim.gravityVectorCM.TailPosition;
        startHead = sim.gravityVectorCM.HeadPosition;
        colorOpaque = sim.tidalVector.Color + new Color(0, 0, 0, 1 - sim.tidalVector.Color.a);
        colorTransparent = colorOpaque - new Color(0, 0, 0, colorOpaque.a);
        sim.tidalVector.Color = colorTransparent;
        sim.gravityVectorCM.Redraw();
        sim.gravityVector.Redraw();
        sim.tidalVector.Redraw();
        StartCoroutine(MoveVector(sim.gravityVectorCM, sim.gravityVector.Displacement, moveTime));
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (sim != null)
        {
            sim.Reset();
        }
    }

    private IEnumerator MoveVector(Vector vector, Vector3 displacement, float moveTime)
    {
        float time = 0;

        Vector3 tailTarget = vector.TailPosition + displacement;
        Vector3 headTarget = vector.HeadPosition + displacement;
        while (time < moveTime)
        {
            time += Time.deltaTime;
            float t = time / moveTime;
            t = t * t * (3f - (2f * t));
            Vector3 newTail = Vector3.Lerp(startTail, tailTarget, t);
            Vector3 newHead = Vector3.Lerp(startHead, headTarget, t);
            vector.SetPositions(newTail, newHead);
            vector.Redraw();
            yield return null;
        }

        StartCoroutine(FadeVectorIn(sim.tidalVector, fadeTime));
        yield return new WaitForSeconds(restartDelay);
        sim.gravityVectorCM.SetPositions(startTail, startHead);
        sim.gravityVectorCM.Redraw();
        sim.tidalVector.Color = colorTransparent;
        sim.tidalVector.Redraw();
        StartCoroutine(MoveVector(sim.gravityVectorCM, displacement, moveTime));
    }

    private IEnumerator FadeVectorIn(Vector vector, float fadeTime)
    {
        float time = 0;

        while (time < fadeTime)
        {
            time += Time.deltaTime;
            float t = time / fadeTime;
            t = t * t * (3f - (2f * t));
            vector.Color = Color.Lerp(colorTransparent, colorOpaque, t);
            vector.Redraw();
            yield return null;
        }
    }
}

using TMPro;
using UnityEngine;

public class EarthMoonSlideController : SimulationSlideController
{
    [Header("Visibility")]
    public bool showEarth = true;
    public bool showMoon = true;
    public bool useLighting = true;

    [Header("Deforming/Rotating")]
    public bool earthIsDeforming;
    public bool earthIsRotating;
    public bool moonIsRotating = true;

    [Header("Orbit")]
    public bool showOrbit = true;
    public Color orbitColor = Color.black;
    public float orbitWidth = 1f;
    public bool showOrbitNotch = true;

    [Header("Timer")]
    public TextMeshProUGUI timerTMP;
    public bool showTimer = true;

    private EarthMoonSimulation sim;

    private void Awake()
    {
        // Get reference to the active simulation
        sim = (EarthMoonSimulation)simulation;
    }

    // Called automatically by SlideManager BEFORE OnEnable and OnDisable
    public override void InitializeSlide()
    {
        if (sim.earth != null)
        {
            sim.earth.gameObject.SetActive(showEarth);
        }
        if (sim.moon != null)
        {
            sim.moon.gameObject.SetActive(showMoon);
        }
        if (sim.orbit != null)
        {
            sim.orbit.gameObject.SetActive(showOrbit);
            sim.orbit.SetColor(orbitColor);
            sim.orbit.SetLineWidth(orbitWidth);
        }
        if (sim.orbitNotch != null)
        {
            sim.orbitNotch.gameObject.SetActive(showOrbitNotch);
            sim.orbitNotch.Color = orbitColor;
            sim.orbitNotch.LineWidth = orbitWidth;
        }
        if (timerTMP != null)
        {
            timerTMP.gameObject.SetActive(showTimer);
        }

        foreach (Transform light in sim.lights)
        {
            light.gameObject.SetActive(useLighting);
        }

        sim.earthIsDeforming = earthIsDeforming;
        sim.earthIsRotating = earthIsRotating;
        sim.moonIsRotating = moonIsRotating;
    }

    private void Update()
    {
        if (timerTMP != null && showTimer)
        {
            timerTMP.text = sim.resetTimer.ToString("0.0");
        }
    }
}

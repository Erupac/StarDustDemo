using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftControllerActions : AbstractControllerActions
{


    public GameObject EngineObject;

    private SteamVR_TrackedController controller;
    private Engine engine;

    private void OnEnable()
    {
        controller = GetComponent<SteamVR_TrackedController>();
        engine = EngineObject.GetComponent<Engine>();


        controller.MenuButtonClicked += HandleMenuPressed;
        controller.PadClicked += HandlePadClicked;
        controller.Gripped += HandleGripped;
    }

    private void OnDisable()
    {
        controller.MenuButtonClicked -= HandleMenuPressed;
        controller.PadClicked -= HandlePadClicked;
        controller.Gripped -= HandleGripped;
    }

    // Pause Simulation
    private void HandleMenuPressed(object sender, ClickedEventArgs e)
    {
        engine.TogglePause();
    }

    // Change Damping
    private void HandlePadClicked(object sender, ClickedEventArgs e)
    {

        float inputY = (e.padY) * 5 - 3;

        engine.SetDamping(Mathf.Pow(2.0f, inputY));
        float acceleration = engine.Acceleration;


        float multi = 5;
        float thresh = Mathf.Clamp(acceleration * multi - (Mathf.Pow(2.0f, inputY) * multi), 0.1f, 100f);
        engine.SetSpeedColorThreshold(thresh);
    }

    // Change Color
    private void HandleGripped(object sender, ClickedEventArgs e)
    {
        engine.setColorIndex(--colorIndex);
    }
}

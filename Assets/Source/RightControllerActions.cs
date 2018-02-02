using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightControllerActions : AbstractControllerActions
{


    public GameObject EngineObject;

    private SteamVR_TrackedController controller;
    private Engine engine;

    private void OnEnable()
    {
        controller = GetComponent<SteamVR_TrackedController>();
        engine = EngineObject.GetComponent<Engine>();

        controller.TriggerClicked += HandleTriggerClicked;
        controller.PadClicked += HandlePadClicked;
        controller.Gripped += HandleGripped;
        controller.MenuButtonClicked += HandleMenuPressed;

    }

    private void OnDisable()
    {
        controller.TriggerClicked -= HandleTriggerClicked;
        controller.PadClicked -= HandlePadClicked;
        controller.Gripped -= HandleGripped;
        controller.MenuButtonClicked -= HandleMenuPressed;
    }

    // Reset Simulation
    private void HandleTriggerClicked(object sender, ClickedEventArgs e)
    {
        engine.Reset();
    }

    // Change Acceleration
    private void HandlePadClicked(object sender, ClickedEventArgs e)
    {
        float inputY = (e.padY)*3 - 1;


        engine.SetAcceleration(Mathf.Pow(2.0f, inputY));
        float damping = engine.Damping;


        float multi = 5;
        float thresh = Mathf.Clamp(Mathf.Pow(2.0f, inputY) * multi - (damping * multi), 0.1f, 100f);
        engine.SetSpeedColorThreshold(thresh);
    }

    // Change Color
    private void HandleGripped(object sender, ClickedEventArgs e)
    {
        engine.setColorIndex(++colorIndex);
    }

    // Pause Game
    private void HandleMenuPressed(object sender, ClickedEventArgs e)
    {
        engine.TogglePause();
    }
}

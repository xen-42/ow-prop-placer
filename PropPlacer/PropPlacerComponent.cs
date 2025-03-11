using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Props;
using NewHorizons.Utility.Files;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PropPlacer;

public class PropPlacerComponent : MonoBehaviour
{
    private ScreenPrompt _placePrompt, _undoPrompt, _redoPrompt;

    public Stack<(GameObject planet, DetailInfo info, GameObject obj)> placedObjects = new();
    public Stack<(GameObject planet, DetailInfo)> undoneObjects = new();

    public Action<string> DetailPlaced;

    public Key placeKey = Key.G;
    public Key undoKey = Key.Minus;
    public Key redoKey = Key.Equals;

    public static string objectPath = "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_BlackHoleForge/BlackHoleForgePivot/Props_BlackHoleForge/Prefab_NOM_VaseThin";

    public KeyCode KeyToKeyCode(Key code) => (KeyCode)Enum.Parse(typeof(KeyCode), code.ToString());

    public void Awake()
    {
        _placePrompt = new ScreenPrompt("Place" + " <CMD>", ImageUtilities.GetButtonSprite(KeyToKeyCode(placeKey)));
        _undoPrompt = new ScreenPrompt("Undo" + " <CMD>", ImageUtilities.GetButtonSprite(KeyToKeyCode(undoKey)));
        _redoPrompt = new ScreenPrompt("Redo" + " <CMD>", ImageUtilities.GetButtonSprite(KeyToKeyCode(redoKey)));

        Locator.GetPromptManager().AddScreenPrompt(_placePrompt, PromptPosition.UpperRight, false);
        Locator.GetPromptManager().AddScreenPrompt(_undoPrompt, PromptPosition.UpperRight, false);
        Locator.GetPromptManager().AddScreenPrompt(_redoPrompt, PromptPosition.UpperRight, false);
    }

    public void OnDestroy()
    {
        var promptManager = Locator.GetPromptManager();
        if (promptManager == null) return;
        promptManager.RemoveScreenPrompt(_placePrompt, PromptPosition.UpperRight);
        promptManager.RemoveScreenPrompt(_undoPrompt, PromptPosition.UpperRight);
        promptManager.RemoveScreenPrompt(_redoPrompt, PromptPosition.UpperRight);
    }

    public void Update()
    {
        if (Keyboard.current[placeKey].wasReleasedThisFrame)
        {
            PlaceObject();
        }
        if (Keyboard.current[undoKey].wasReleasedThisFrame)
        {
            Undo();
        }
        if (Keyboard.current[redoKey].wasReleasedThisFrame)
        {
            Redo();
        }

        UpdatePromptVisibility();
    }

    public void UpdatePromptVisibility()
    {
        var visible = !OWTime.IsPaused();
        _placePrompt.SetVisibility(visible);
        _undoPrompt.SetVisibility(visible && placedObjects.Count > 0);
        _redoPrompt.SetVisibility(visible && undoneObjects.Count > 0);
    }

    public void Undo()
    {
        var (planet, info, obj) = placedObjects.Pop();
        GameObject.Destroy(obj);
        undoneObjects.Push((planet, info));
    }

    public void Redo()
    {
        var (planet, info) = undoneObjects.Pop();
        var obj = DetailBuilder.Make(planet, planet.GetComponent<AstroObject>()?.GetRootSector() ?? null, PropPlacer.Instance, info);
        placedObjects.Push((planet, info, obj));
    }

    public void PlaceObject()
    {
        if (RaycastUtil.Raycast(out var position, out var rotation, out var normal, out var hitObject))
        {
            var planet = hitObject.GetAttachedOWRigidbody().gameObject;
            var info = new DetailInfo()
            {
                position = position,
                rotation = rotation.eulerAngles,
                keepLoaded = true,
                path = objectPath.Trim()
            };
            var obj = DetailBuilder.Make(planet, planet.GetComponent<AstroObject>()?.GetRootSector() ?? null, PropPlacer.Instance, info);

            placedObjects.Push((planet, info, obj));

            if (obj != null)
            {
                DetailPlaced?.Invoke(info.path);
            }

            // Clears the redo list else it'll get confusing
            undoneObjects.Clear();
        }
    }
}

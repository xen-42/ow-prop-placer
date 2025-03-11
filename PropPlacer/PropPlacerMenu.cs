using NewHorizons.External.Modules.Props;
using NewHorizons.Utility.Files;
using Newtonsoft.Json;
using OWML.Common;
using System.Collections.Generic;
using UnityEngine;

namespace PropPlacer;

public class PropPlacerMenu : MonoBehaviour
{
    public Vector2 position = new Vector2(40, 40);
    public Vector2 size = new Vector2(600, 600);
    public GUIStyle style;

    public PropPlacerComponent component;

    private HashSet<string> _paths = new();

    public static bool visible;

    public void Start()
    {
        style = new GUIStyle
        {
            normal =
            {
                background = ImageUtilities.MakeSolidColorTexture(1, 1, Color.black)
            }
        };

        component = this.gameObject.GetComponent<PropPlacerComponent>();
        component.DetailPlaced += (path) =>
        {
            _paths.Add(path);
        };
    }

    public void OnGUI()
    {
        // Always close when unpausing
        if (!OWTime.IsPaused())
        {
            visible = false;
        }

        if (!visible)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(position.x, position.y, size.x, size.y), style);

        if (GUILayout.Button("Print placed prop configs to logs"))
        {
            PrintConfigsToLogs();
        }

        GUILayout.Label("Object to place:");
        PropPlacerComponent.objectPath = GUILayout.TextArea(PropPlacerComponent.objectPath);

        GUILayout.Space(5);

        if (_paths.Count > 0)
        {
            GUILayout.Label("Quick select previous path:");
            foreach (var path in _paths)
            {
                if (GUILayout.Button(path, GUILayout.ExpandWidth(false)))
                {
                    PropPlacerComponent.objectPath = path;
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    public static void InitializePauseMenu(IPauseMenuManager pauseMenu)
    {
        var reloadButton = pauseMenu.MakeSimpleButton("Prop Placer Menu".ToUpperInvariant(), 3, true);
        reloadButton.OnSubmitAction += () => visible = !visible;
    }

    public void PrintConfigsToLogs()
    {
        PropPlacer.Instance.ModHelper.Console.WriteLine($"PRINTING PLACED PROPS =================", MessageType.Info);

        // Sort details by planet
        var dictionary = new Dictionary<string, List<DetailInfo>>();
        foreach (var (planet, detail, _) in component.placedObjects)
        {
            if (!dictionary.ContainsKey(planet.name))
            {
                dictionary[planet.name] = new();
            }
            dictionary[planet.name].Add(detail);
        }

        // Print all
        foreach (var key in dictionary.Keys)
        {
            PropPlacer.Instance.ModHelper.Console.WriteLine($"On planet {key}", MessageType.Info);
            foreach (var detail in dictionary[key])
            {
                PropPlacer.Instance.ModHelper.Console.WriteLine(JsonConvert.SerializeObject(detail, Formatting.Indented), MessageType.Info);
            }
        }

        PropPlacer.Instance.ModHelper.Console.WriteLine($"=======================================", MessageType.Info);
    }
}

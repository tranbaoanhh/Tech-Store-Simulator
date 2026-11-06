#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TechStoreSimulator;

public static class TechStoreSceneBuilder
{
    [MenuItem("Tools/Tech Store/Create Demo Store")]
    public static void CreateStore()
    {
        if (!EditorUtility.DisplayDialog("Tech Store", "Tao Scene demo moi? Scene chua luu co the mat.", "Tao", "Huy")) return;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCube("Floor", new Vector3(0, -0.25f, 0), new Vector3(12, .5f, 12), new Color(.43f,.46f,.50f));
        MakeCube("Back Wall", new Vector3(0, 2, 6), new Vector3(12, 4, .3f), new Color(.86f,.88f,.89f));
        MakeCube("Left Wall", new Vector3(-6, 2, 0), new Vector3(.3f, 4, 12), new Color(.86f,.88f,.89f));
        MakeCube("Right Wall", new Vector3(6, 2, 0), new Vector3(.3f, 4, 12), new Color(.86f,.88f,.89f));
        MakeCube("Counter", new Vector3(0, .55f, 3.8f), new Vector3(3, 1.1f, 1.1f), new Color(.35f,.24f,.18f));
        MakeCube("Shelf Left", new Vector3(-4.2f, .7f, 1.1f), new Vector3(1.3f, 1.4f, 3), new Color(.40f,.28f,.20f));
        MakeCube("Shelf Right", new Vector3(4.2f, .7f, 1.1f), new Vector3(1.3f, 1.4f, 3), new Color(.40f,.28f,.20f));
        MakeCube("Assembly Table", new Vector3(0, .5f, -2.5f), new Vector3(2.8f, 1, 1.4f), new Color(.25f,.30f,.37f));

        CreateProduct("Keyboard", new Vector3(-3.8f, 1.55f, 1.5f), new Vector3(.9f,.18f,.35f), 350000, new Color(.12f,.12f,.15f));
        CreateProduct("Mouse", new Vector3(4.1f, 1.55f, 1f), new Vector3(.28f,.15f,.4f), 200000, new Color(.13f,.22f,.40f));
        CreateProduct("SSD", new Vector3(4.1f, 1.55f, .2f), new Vector3(.4f,.12f,.3f), 800000, new Color(.20f,.52f,.27f));

        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, 1.1f, -4f);
        var controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = .35f;
        controller.center = new Vector3(0, 0, 0);
        player.AddComponent<PlayerController>();
        GameObject cameraGo = new GameObject("PlayerCamera");
        cameraGo.tag = "MainCamera";
        cameraGo.transform.SetParent(player.transform);
        cameraGo.transform.localPosition = new Vector3(0, .6f, 0);
        cameraGo.AddComponent<Camera>();
        cameraGo.AddComponent<AudioListener>();
        new GameObject("GameManager").AddComponent<GameManager>();

        GameObject lightGo = new GameObject("Sun Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
        RenderSettings.ambientLight = new Color(.72f,.72f,.72f);

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/TechStore.unity");
        AssetDatabase.Refresh();
        Debug.Log("Tech Store demo created. Press Play. Input setting: Active Input Handling = Both (or Input Manager Old).");
    }

    static void CreateProduct(string name, Vector3 position, Vector3 scale, int price, Color color)
    {
        GameObject go = MakeCube(name, position, scale, color);
        go.AddComponent<ProductInteraction>().Configure(name, price);
    }

    static GameObject MakeCube(string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        material.color = color;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }
}
#endif

using UnityEngine;

[ExecuteAlways]
public class GreenhouseBuilder : MonoBehaviour
{
    [Header("Greenhouse Prefabs")]
    public GameObject Env_GreenhouseBack;
    public GameObject Env_GreenhouseMiddle;

    [Header("Grid")]
    [Min(0)]
    public int Length = 5;

    public float moduleLength = 1f;

    private Transform generatedModulesRoot;
    private GameObject greenhouseBackInstance;

    void Update()
    {
        if (Env_GreenhouseBack == null || Env_GreenhouseMiddle == null)
        {
            return;
        }

        BuildGreenhouse();
    }

    void BuildGreenhouse()
    {
        CreateOrFindGeneratedRoot();
        CreateOrFindBackPiece();

        ClearMiddlePieces();

        for (int i = 0; i <= Length; i++)
        {
            GameObject middlePiece = Instantiate(
                Env_GreenhouseMiddle,
                generatedModulesRoot
            );

            middlePiece.name = $"Middle_{i}";

            // Assumes the module is 1 unit long on local Z
            // and the prefab pivot is at its front edge.
            middlePiece.transform.localPosition =
                new Vector3(0f, 0f, (i + 1) * moduleLength);

            middlePiece.transform.localRotation = Quaternion.identity;
        }

        // Places the back directly after the last middle module.
        greenhouseBackInstance.transform.localPosition =
            new Vector3(0f, 0f, (Length + 1) * moduleLength);

        greenhouseBackInstance.transform.localRotation = Quaternion.identity;
    }

    void CreateOrFindGeneratedRoot()
    {
        if (generatedModulesRoot != null)
        {
            return;
        }

        Transform existingRoot = transform.Find("GeneratedMiddleModules");

        if (existingRoot != null)
        {
            generatedModulesRoot = existingRoot;
        }
        else
        {
            GameObject root = new GameObject("GeneratedMiddleModules");
            root.transform.SetParent(transform, false);
            generatedModulesRoot = root.transform;
        }
    }

    void CreateOrFindBackPiece()
    {
        if (greenhouseBackInstance != null)
        {
            return;
        }

        Transform existingBack = transform.Find("BackPiece");

        if (existingBack != null)
        {
            greenhouseBackInstance = existingBack.gameObject;
        }
        else
        {
            greenhouseBackInstance = Instantiate(Env_GreenhouseBack, transform);
            greenhouseBackInstance.name = "BackPiece";
        }
    }

    void ClearMiddlePieces()
    {
        for (int i = generatedModulesRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(generatedModulesRoot.GetChild(i).gameObject);
        }
    }
}
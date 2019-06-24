using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightningRender : MonoBehaviour
{
    public bool enableLightning = false;
    public GameObject sourceBox;
    public GameObject targetBox;
    public AnimationCurve boltProcessCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve branchInProcessCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve branchOutProcessCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve boltBrightnessScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0.0f, 10.0f)]
    public float lightningSunIntensity = 3.0f;
    [Range(0.0f, 10.0f)]
    public float lightningSkyExposure = 3.0f;
    public Mesh[] lightningMeshs = new Mesh[4];
    private string[] lightningModelName = new string[4];
    public Material lightningMaterial;

    private Matrix4x4 lightningModelMat = new Matrix4x4();
    private Bounds sourceBounds = new Bounds();
    private Bounds targetBounds = new Bounds();
    private float boltProcess = 0.0f;
    private float branchInProcess = 0.0f;
    private float branchOutProcess = 0.0f;
    private float boltBrightnessScale = 0.0f;
    private float currentCurveTime = 0.0f;
    private float curveUpdatePeriod = 0.01f;
    private Light mainLight = null;
    private float defaultSunIntensity = 0.3f;
    private float defaultSkyExposure = 0.0f;
    private Material skyboxMat;

    // Start is called before the first frame update
    void Start()
    {
        SetLightningMeshModelMatrix();
        var lights = Object.FindObjectsOfType(typeof(Light)) as Light[];
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional)
            {
                mainLight = light;
                break;
            }
        }

        // Should Fix Projects
        skyboxMat = RenderSettings.skybox;
        //lightningModelName[0] = "WeatherSystem/Lightning/Meshes/SM_Lightning_00";
        //lightningModelName[1] = "WeatherSystem/Lightning/Meshes/SM_Lightning_01";
        //lightningModelName[2] = "WeatherSystem/Lightning/Meshes/SM_Lightning_02";
        //lightningModelName[3] = "WeatherSystem/Lightning/Meshes/SM_Lightning_03";
        //for (int i = 0; i < 4; ++i)
        //{
        //    GameObject lightningModel = Resources.Load<GameObject>(lightningModelName[i]); // blender file called tank
        //    lightningMeshs[i] = lightningModel.GetComponent<MeshFilter>().sharedMesh;
        //}
    }

    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private void SetLightningMeshModelMatrix()
    {
        if (sourceBox)
        {
            sourceBounds = sourceBox.GetComponent<Collider>().bounds;
        }
        if (targetBox)
        {
            targetBounds = targetBox.GetComponent<Collider>().bounds;
        }
        Vector3 randomPosInSourceBox = RandomPointInBounds(sourceBounds);
        Vector3 randomPosInTargetBox = RandomPointInBounds(targetBounds);
        Vector3 lightningVector = randomPosInSourceBox - randomPosInTargetBox;
        Vector3 lightningPos = randomPosInTargetBox;
        float lightningScale = Vector3.Magnitude(lightningVector);
        Vector3 lightningScaleVec3 = new Vector3(lightningScale, lightningScale, lightningScale);
        Quaternion lightningRotation = new Quaternion();
        lightningRotation.SetFromToRotation(new Vector3(0.0f, 1.0f, 0.0f), lightningVector);
        lightningRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f) * lightningRotation;
        lightningModelMat.SetTRS(lightningPos, lightningRotation, lightningScaleVec3);
    }

    // Update is called once per frame
    void Update()
    {
        if(!enableLightning)
        {
            return;
        }

        int randomMeshID = 0;

        // set curve value
        boltProcess = boltProcessCurve.Evaluate(currentCurveTime * 5.0f);
        branchInProcess = branchInProcessCurve.Evaluate(currentCurveTime * 5.0f);
        branchOutProcess = branchOutProcessCurve.Evaluate(currentCurveTime * 5.0f);
        boltBrightnessScale = boltBrightnessScaleCurve.Evaluate(currentCurveTime * 5.0f);
        lightningMaterial.SetFloat("_BoltBrightnessScale", boltBrightnessScale);
        lightningMaterial.SetFloat("_BoltProgress", boltProcess);
        lightningMaterial.SetFloat("_BranchInProcess", branchInProcess);
        lightningMaterial.SetFloat("_BranchOutProcess", branchOutProcess);

        if(boltProcess > 0.96f && boltBrightnessScale > 0.5f)
        {
            if(mainLight)
            {
                mainLight.intensity = lightningSunIntensity;
            }

            if(skyboxMat)
            {
                skyboxMat.SetFloat("_Exposure", lightningSkyExposure);
            }
        }
        else
        {
            if(mainLight)
            {
                mainLight.intensity = defaultSunIntensity;
            }

            if (skyboxMat)
            {
                skyboxMat.SetFloat("_Exposure", defaultSkyExposure);
            }
        }

        if (currentCurveTime >= 1.0f)
        {
            currentCurveTime = 0.0f;
            randomMeshID = Random.Range(0, lightningMeshs.Length);
            SetLightningMeshModelMatrix();
        }
        else
        {
            currentCurveTime += curveUpdatePeriod;
        }

        if (lightningMeshs[randomMeshID] && lightningMaterial)
        {
            Graphics.DrawMesh(lightningMeshs[randomMeshID], lightningModelMat, lightningMaterial, 0);
        }
    }
}

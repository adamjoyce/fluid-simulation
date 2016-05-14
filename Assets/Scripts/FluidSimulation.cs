using UnityEngine;
using System.Collections;

public class FluidSimulation : MonoBehaviour {

    public Material advectMaterial;
    public Material jacobiMaterial;
    public Material divergenceMaterial;
    public Material gradientMaterial;
    public Material boundaryMaterial;
    public Material displayMaterial;
    public Material solidsMaterial;

    private RenderTexture displayTexture;
    private RenderTexture solidsTexture;

    private GUITexture display;
    private int displayWidth, displayHeight;
    private Vector2 displayArea;

	// Use this for initialization
	void Start () {
        // Setup the main GUI texture.
        display = GetComponent<GUITexture>();
        displayWidth = (int)display.pixelInset.width;
        displayHeight = (int)display.pixelInset.height;

        displayArea = new Vector2(1.0f / displayWidth, 1.0f / displayHeight);

        // Setup the main render texture.
        displayTexture = new RenderTexture(displayWidth, displayHeight, 0, RenderTextureFormat.ARGB32);
        displayTexture.wrapMode = TextureWrapMode.Clamp;
        displayTexture.filterMode = FilterMode.Bilinear;
        displayTexture.Create();

        GetComponent<GUITexture>().texture = displayTexture;

        // Setup the solid shapes texture.
        solidsTexture = new RenderTexture(displayWidth, displayHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        solidsTexture.wrapMode = TextureWrapMode.Clamp;
        solidsTexture.filterMode = FilterMode.Point;
        solidsTexture.Create();

        displayMaterial.SetTexture("_Solids", solidsTexture);
        placeSolids();
	}
	
	// Update is called once per frame
	void Update () {
        placeSolids();
        Graphics.Blit(solidsTexture, displayTexture, displayMaterial);
    }

    private void placeSolids() {
        solidsMaterial.SetVector("_Size", displayArea);
        solidsMaterial.SetVector("_Location", new Vector2(0.5f, 0.5f));
        solidsMaterial.SetFloat("_Radius", 0.1f);
        Graphics.Blit(null, solidsTexture, solidsMaterial);
    }
}

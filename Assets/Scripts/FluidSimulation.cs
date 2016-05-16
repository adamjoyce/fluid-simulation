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
    private RenderTexture[] velocityTexture;

    private GUITexture display;
    private int displayWidth, displayHeight;
    private Vector2 displayArea;

    private Vector2 solidPosition = new Vector2(0.5f, 0.5f);

    private float timeIncrement = 0.1f;
    private float velocityDissipation = 0.5f;

	// For initialization.
	private void Start () {
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

        // Setup the solid shapes texture.
        solidsTexture = new RenderTexture(displayWidth, displayHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        solidsTexture.wrapMode = TextureWrapMode.Clamp;
        solidsTexture.filterMode = FilterMode.Point;
        solidsTexture.Create();

        // Setup the 0 index read and 1 index write render textures.
        velocityTexture = new RenderTexture[2];

        GetComponent<GUITexture>().texture = displayTexture;
        displayMaterial.SetTexture("_Solids", solidsTexture);
        PlaceSolids();
	}
	
	// Update is called once per frame
	private void Update () {
        Advect(velocityTexture[0], velocityTexture[0], velocityTexture[1], velocityDissipation);
        SwapTextures(velocityTexture);
        Graphics.Blit(solidsTexture, displayTexture, displayMaterial);
    }

    // Draws the solids texture and copies it into the solid render texture.
    private void PlaceSolids() {
        solidsMaterial.SetVector("_Size", displayArea);
        solidsMaterial.SetVector("_Location", solidPosition);
        solidsMaterial.SetFloat("_Radius", 0.1f);
        Graphics.Blit(null, solidsTexture, solidsMaterial);
    }

    // Advect's a source texture against the veloicty texture.
    private void Advect(RenderTexture velocity, RenderTexture source, RenderTexture destination, float dissipation) {
        advectMaterial.SetTexture("_VelocityTexture", velocity);
        advectMaterial.SetTexture("_SourceTexture", source);
        advectMaterial.SetTexture("_Solids", solidsTexture);
        advectMaterial.SetVector("_Size", displayArea);
        advectMaterial.SetFloat("_TimeIncrement", timeIncrement);
        advectMaterial.SetFloat("_Dissipation", dissipation);
        Graphics.Blit(null, destination, advectMaterial);
    }

    // Swap the read an write render textures.
    private void SwapTextures(RenderTexture[] texture) {
        RenderTexture tempTexture = texture[0];
        texture[0] = texture[1];
        texture[1] = tempTexture;
    }
}

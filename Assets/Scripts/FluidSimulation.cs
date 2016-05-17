using UnityEngine;
using System.Collections;

public class FluidSimulation : MonoBehaviour {

    public Material advectMaterial;
    public Material jacobiMaterial;
    public Material divergenceMaterial;
    public Material gradientMaterial;
    public Material buoyancyMaterial;
    public Material displayMaterial;
    public Material solidsMaterial;

    private RenderTexture displayTexture;
    private RenderTexture solidsTexture;
    private RenderTexture[] velocityTexture;
    private RenderTexture[] densityTexture;
    private RenderTexture[] temperatureTexture;

    private GUITexture display;
    private int displayWidth, displayHeight;
    private Vector2 displayArea;

    private Vector2 solidPosition = new Vector2(0.5f, 0.5f);

    private float timeIncrement = 0.1f;
    private float velocityDissipation = 0.5f;
    private float densityDissipation = 0.5f;
    private float temperatureDissipation = 0.5f;

    private float ambientTemperature = 0.0f;
    private float fluidBuoyancy = 1.0f;
    private float fluidWeight = 0.1f;

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
        CreateTextures(velocityTexture, RenderTextureFormat.RGFloat);

        // Setup textures for the density and temperature of the fluid.
        densityTexture = new RenderTexture[2];
        CreateTextures(densityTexture, RenderTextureFormat.RFloat);
        temperatureTexture = new RenderTexture[2];
        CreateTextures(temperatureTexture, RenderTextureFormat.RFloat);

        GetComponent<GUITexture>().texture = displayTexture;
        displayMaterial.SetTexture("_Solids", solidsTexture);
        PlaceSolids();
	}
	
	// Update is called once per frame
	private void Update () {
        // Advect the velocity, temperature, and density against the velocity.
        Advect(velocityTexture[0], velocityTexture[0], velocityTexture[1], velocityDissipation);
        Advect(velocityTexture[0], densityTexture[0], densityTexture[1], densityDissipation);
        Advect(velocityTexture[0], temperatureTexture[0], temperatureTexture[1], temperatureDissipation);

        // Switch out the textures.
        SwapTextures(velocityTexture);
        SwapTextures(densityTexture);
        SwapTextures(temperatureTexture);

        Graphics.Blit(solidsTexture, displayTexture, displayMaterial);
    }

    // Setup and create the textures in the texture arrays.
    private void CreateTextures(RenderTexture[] texture, RenderTextureFormat format, FilterMode filterMode = FilterMode.Bilinear) {
        // Setup read texture.
        texture[0] = new RenderTexture(displayWidth, displayHeight, 0, format, RenderTextureReadWrite.Linear);
        texture[0].wrapMode = TextureWrapMode.Clamp;
        texture[0].filterMode = filterMode;
        texture[0].Create();

        // Setup write texture.
        texture[1] = new RenderTexture(displayWidth, displayHeight, 0, format, RenderTextureReadWrite.Linear);
        texture[1].wrapMode = TextureWrapMode.Clamp;
        texture[1].filterMode = filterMode;
        texture[1].Create();
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

    // Simulates convection currents by using fluid buoyancy.
    private void AddBuoyancy(RenderTexture velocity, RenderTexture density, RenderTexture temperature, RenderTexture destination) {
        buoyancyMaterial.SetTexture("_VelocityTexture", velocity);
        buoyancyMaterial.SetTexture("_DensityTexture", density);
        buoyancyMaterial.SetTexture("_TemperatureTexture", temperature);
        buoyancyMaterial.SetFloat("_TimeIncrement", timeIncrement);
        buoyancyMaterial.SetFloat("_AmbientTemperature", ambientTemperature);
        buoyancyMaterial.SetFloat("_FluidBuoyancy", fluidBuoyancy);
        buoyancyMaterial.SetFloat("_FluidWeight", fluidWeight);
        Graphics.Blit(null, destination, buoyancyMaterial);
    }
}

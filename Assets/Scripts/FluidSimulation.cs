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
    public Material impulseMaterial;

    public Vector2 solidPosition = new Vector2(0.5f, 0.5f);

    public float timeIncrement = 0.125f;
    public float velocityDissipation = 0.99f;
    public float densityDissipation = 0.9999f;
    public float temperatureDissipation = 0.99f;

    public float ambientTemperature = 0.0f;
    public float fluidBuoyancy = 1.0f;
    public float fluidWeight = 0.05f;

    public Vector2 mainImpulsePosition = new Vector2(0.5f, 0.0f);
    public float impulseRadius = 0.5f;
    public float impulseTemperature = 10.0f;
    public float impulseDensity = 1.0f;

    public float cellSize = 1.0f;
    public float gradientScale = 1.0f;

    public int numberOfJacobiIterations = 50;
    public float jacobiBeta = 0.25f;

    private RenderTexture displayTexture;
    private RenderTexture solidsTexture;
    private RenderTexture divergenceTexture;
    private RenderTexture[] velocityTexture;
    private RenderTexture[] densityTexture;
    private RenderTexture[] temperatureTexture;
    private RenderTexture[] pressureTexture;

    private GUITexture display;
    private int displayWidth, displayHeight;
    private Vector2 displayArea;

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

        //Setup the fluid divergence texture.
        divergenceTexture = new RenderTexture(displayWidth, displayHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        divergenceTexture.wrapMode = TextureWrapMode.Clamp;
        divergenceTexture.filterMode = FilterMode.Point;
        divergenceTexture.Create();

        // Setup the 0 index read and 1 index write render textures.
        velocityTexture = new RenderTexture[2];
        CreateTextures(velocityTexture, RenderTextureFormat.RGFloat);

        // Setup textures for the density, temperature, and pressure.
        densityTexture = new RenderTexture[2];
        CreateTextures(densityTexture, RenderTextureFormat.RFloat);
        temperatureTexture = new RenderTexture[2];
        CreateTextures(temperatureTexture, RenderTextureFormat.RFloat);
        pressureTexture = new RenderTexture[2];
        CreateTextures(pressureTexture, RenderTextureFormat.RFloat, FilterMode.Point);

        GetComponent<GUITexture>().texture = displayTexture;
        displayMaterial.SetTexture("_Solids", solidsTexture);
        PlaceSolids();
	}

    // Update is called once per frame
    private void Update() {
        // Advect the density against the velocity.
        Advect(velocityTexture[0], densityTexture[0], densityTexture[1], densityDissipation);
        SwapTextures(densityTexture);

        // Advect the temperature against the velocity.
        Advect(velocityTexture[0], temperatureTexture[0], temperatureTexture[1], temperatureDissipation);
        SwapTextures(temperatureTexture);

        // Advect the velocity against itself.
        Advect(velocityTexture[0], velocityTexture[0], velocityTexture[1], velocityDissipation);
        SwapTextures(velocityTexture);

        // Get the changes in velocity due to convection currents.
        AddBuoyancy(velocityTexture[0], densityTexture[0], temperatureTexture[0], velocityTexture[1]);
        SwapTextures(velocityTexture);

        // Add the density impulse each frame.
        AddImpulse(densityTexture[0], densityTexture[1], mainImpulsePosition, impulseRadius, impulseDensity);
        SwapTextures(densityTexture);

        // Add the temperature impulse each frame.
        AddImpulse(temperatureTexture[0], temperatureTexture[1], mainImpulsePosition, impulseRadius, impulseTemperature);
        SwapTextures(temperatureTexture);

        // Add secondary impulse at mouse here.

        // Begin the projection steps.
        // Calucate the fluid's velocity divergence and use zero as our initial guess for the pressure field.
        CalculateDivergence(velocityTexture[0], divergenceTexture);
        ResetField(pressureTexture[0]);

        // Jacobi iterations.
        for (int i = 0; i < numberOfJacobiIterations; i++) {
            iterateJacobi(divergenceTexture, pressureTexture[0], pressureTexture[1]);
            SwapTextures(pressureTexture);
        }

        // Subtracts the graident of the solved pressure equation from the intermediate velocity field.
        SubtractGradient(velocityTexture[0], pressureTexture[0], velocityTexture[1]);
        SwapTextures(velocityTexture);

        Graphics.Blit(densityTexture[0], displayTexture, displayMaterial);
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

    // Simulates a impulse to the fluid.
    private void AddImpulse(RenderTexture source, RenderTexture destination, Vector2 location, float radius, float fill) {
        impulseMaterial.SetTexture("_SourceTexture", source);
        impulseMaterial.SetVector("_Location", location);
        impulseMaterial.SetFloat("_Radius", radius);
        impulseMaterial.SetFloat("_Fill", fill);
        Graphics.Blit(null, destination, impulseMaterial);
    }

    // Calculates the divergence of the fluid's velocity into the surronding cells.
    private void CalculateDivergence(RenderTexture velocity, RenderTexture destination) {
        divergenceMaterial.SetTexture("_VelocityTexture", velocity);
        divergenceMaterial.SetTexture("_SolidsTexture", solidsTexture);
        divergenceMaterial.SetFloat("_HalfCellSize", 0.5f / cellSize);
        divergenceMaterial.SetVector("_Size", displayArea);
        Graphics.Blit(null, destination, divergenceMaterial);
    }

    // Clears a render texture field to zeros.
    private void ResetField(RenderTexture field) {
        Color backgroundColour = new Color(0, 0, 0, 0);
        Graphics.SetRenderTarget(field);
        GL.Clear(false, true, backgroundColour);
        Graphics.SetRenderTarget(null);
    }

    // Run a Jacobi iteration for solving the diffusion equation.
    private void iterateJacobi(RenderTexture divergence, RenderTexture pressure, RenderTexture destination) {
        jacobiMaterial.SetTexture("_DivergenceTexture", divergence);
        jacobiMaterial.SetTexture("_PressureTexture", pressure);
        jacobiMaterial.SetTexture("_SolidsTexture", solidsTexture);
        jacobiMaterial.SetVector("_Size", displayArea);
        jacobiMaterial.SetFloat("_Alpha", cellSize * -cellSize);
        jacobiMaterial.SetFloat("_Beta", jacobiBeta);
        Graphics.Blit(null, destination, jacobiMaterial);
    }

    // Subtracts a gradient from a velocity field.
    private void SubtractGradient(RenderTexture velocity, RenderTexture pressure, RenderTexture destination) {
        gradientMaterial.SetTexture("_VelocityTexture", velocity);
        gradientMaterial.SetTexture("_PressureTexture", pressure);
        gradientMaterial.SetTexture("_SolidsTexture", solidsTexture);
        gradientMaterial.SetVector("_Size", displayArea);
        gradientMaterial.SetFloat("_GradientScale", gradientScale);
        Graphics.Blit(null, destination, gradientMaterial);
    }
}

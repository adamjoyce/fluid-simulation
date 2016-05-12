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
    int displayWidth, displayHeight;

	// Use this for initialization
	void Start () {
        // Setup the main GUI texture.
        display = GetComponent<GUITexture>();
        displayWidth = (int)display.pixelInset.width;
        displayHeight = (int)display.pixelInset.height;

        // Setup the main render texture.
        displayTexture = new RenderTexture(displayWidth, displayHeight, 0, RenderTextureFormat.ARGB32);
        displayTexture.wrapMode = TextureWrapMode.Clamp;
        displayTexture.filterMode = FilterMode.Bilinear;
        displayTexture.Create();

        GetComponent<GUITexture>().texture = displayTexture;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

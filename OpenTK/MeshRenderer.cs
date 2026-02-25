using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq; // Önemli: Select ve ToArray için
using OpenTK.Graphics.OpenGL4;
using Base;
using Base.Rendering;

public class MeshRenderer : Behaviour
{
    int shaderProgram;
    Renderer.RenderEntity entity;
    uint[] indices = Array.Empty<uint>();
    float[] vertices = Array.Empty<float>();

    string objPath = "default.obj";
    string fragShaderPath = "testFrag";
    string vertShaderPath = "testVert";

    public MeshRenderer(string fragPath, string vertPath, string meshPath) : base()
    {
        this.objPath = meshPath;
        this.fragShaderPath = fragPath;
        this.vertShaderPath = vertPath;

        InitializeUI();

        Renderer.ReloadEverything(ref entity, vertShaderPath, fragShaderPath, objPath);
    }

    public MeshRenderer() : base()
    {
        // Varsayılan değerler
        this.objPath = "default.obj";
        this.fragShaderPath = "testFrag";
        this.vertShaderPath = "testVert";

        InitializeUI();

        Renderer.ReloadEverything(ref entity, vertShaderPath, fragShaderPath, objPath);
    }

    private void InitializeUI()
    {
        uiElements.Clear();

        // 1. Mesh Path (InputText)
        uiElements.Add(new ImgUiElement(UiElementType.InputText, "Mesh Path", () => { }) { inputValue = objPath });

        // Shaders klasörünü tara
        string shaderFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders");
        string[] shaderFiles = Directory.Exists(shaderFolder)
            ? Directory.GetFiles(shaderFolder).Select(Path.GetFileName).ToArray()
            : new string[] { "Klasor Bulunamadi" };

        // 2. Fragment Shader (DropDown)
        var fragDrop = new ImgUiElement(UiElementType.DropDown, "Frag Shader", () => { });
        fragDrop.options = shaderFiles;
        fragDrop.selectedIndex = Math.Max(0, Array.IndexOf(shaderFiles, fragShaderPath));
        fragDrop.inputValue = fragShaderPath;
        uiElements.Add(fragDrop);

        // 3. Vertex Shader (DropDown)
        var vertDrop = new ImgUiElement(UiElementType.DropDown, "Vert Shader", () => { });
        vertDrop.options = shaderFiles;
        vertDrop.selectedIndex = Math.Max(0, Array.IndexOf(shaderFiles, vertShaderPath));
        vertDrop.inputValue = vertShaderPath;
        uiElements.Add(vertDrop);
    }

    public override void Update()
    {
        bool needsReload = false;

        // Mesh yolu değişti mi?
        if (uiElements[0].inputValue != objPath)
        {
            objPath = uiElements[0].inputValue;
            needsReload = true;
        }

        // Shaderlar değişti mi?
        if (uiElements[1].inputValue != fragShaderPath || uiElements[2].inputValue != vertShaderPath)
        {
            fragShaderPath = uiElements[1].inputValue;
            vertShaderPath = uiElements[2].inputValue;
            needsReload = true;
        }

        if (needsReload)
        {
            Renderer.DeleteEntity(entity);
            Renderer.ReloadEverything(ref entity, vertShaderPath, fragShaderPath, objPath);
        }
    }



}

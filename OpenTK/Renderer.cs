using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq; // Önemli: Select ve ToArray için
using OpenTK.Graphics.OpenGL4;

namespace Base.Rendering
{
    public class Renderer : Behaviour
    {
        int vao, vbo, ebo, shaderProgram;
        uint[] indices = Array.Empty<uint>();
        float[] vertices = Array.Empty<float>();

        string objPath = "default.obj";
        string fragShaderPath = "testFrag";
        string vertShaderPath = "testVert";

        public Renderer(string fragPath, string vertPath, string meshPath) : base()
        {
            this.objPath = meshPath;
            this.fragShaderPath = fragPath;
            this.vertShaderPath = vertPath;

            InitializeUI();

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            ReloadEverything();
        }

        public Renderer() : base()
        {
            // Varsayılan değerler
            this.objPath = "default.obj";
            this.fragShaderPath = "testFrag";
            this.vertShaderPath = "testVert";

            InitializeUI();

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            ReloadEverything();
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
            // UI'dan gelen değişiklikleri kontrol et ve uygula
            if (uiElements[0].inputValue != objPath)
            {
                objPath = uiElements[0].inputValue;
                LoadObj(objPath, out vertices, out indices);
                GenerateMesh();
            }

            if (uiElements[1].inputValue != fragShaderPath || uiElements[2].inputValue != vertShaderPath)
            {
                fragShaderPath = uiElements[1].inputValue;
                vertShaderPath = uiElements[2].inputValue;
                ReloadShader();
            }

            // Çizim işlemi
            if (indices.Length > 0 && shaderProgram > 0)
            {
                GL.UseProgram(shaderProgram);
                GL.BindVertexArray(vao);
                GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
        }

        private void ReloadEverything()
        {
            try
            {
                LoadObj(objPath, out vertices, out indices);
                GenerateMesh();
                ReloadShader();
            }
            catch (Exception e) { Console.WriteLine("Hata: " + e.Message); }
        }

        private void ReloadShader()
        {
            try
            {
                string vSrc = File.ReadAllText(Path.Combine("Shaders", vertShaderPath));
                string fSrc = File.ReadAllText(Path.Combine("Shaders", fragShaderPath));
                shaderProgram = CompileCustomShader(vSrc, fSrc);
            }
            catch { /* Dosya okuma hatası */ }
        }

        public static void LoadObj(string path, out float[] outVertices, out uint[] outIndices)
        {
            List<float> vList = new List<float>();
            List<uint> iList = new List<uint>();
            string fullPath = Path.Combine("Meshes", path);

            if (!File.Exists(fullPath)) { outVertices = Array.Empty<float>(); outIndices = Array.Empty<uint>(); return; }

            foreach (var line in File.ReadAllLines(fullPath))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                if (parts[0] == "v")
                {
                    vList.Add(float.Parse(parts[1], CultureInfo.InvariantCulture));
                    vList.Add(float.Parse(parts[2], CultureInfo.InvariantCulture));
                    vList.Add(float.Parse(parts[3], CultureInfo.InvariantCulture));
                }
                else if (parts[0] == "f")
                {
                    for (int i = 1; i <= 3; i++) iList.Add(uint.Parse(parts[i].Split('/')[0]) - 1);
                }
            }
            outVertices = vList.ToArray();
            outIndices = iList.ToArray();
        }

        void GenerateMesh()
        {
            if (vertices.Length == 0) return;
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        int CompileCustomShader(string vSource, string fSource)
        {
            int v = GL.CreateShader(ShaderType.VertexShader); GL.ShaderSource(v, vSource); GL.CompileShader(v);
            int f = GL.CreateShader(ShaderType.FragmentShader); GL.ShaderSource(f, fSource); GL.CompileShader(f);
            int prog = GL.CreateProgram();
            GL.AttachShader(prog, v); GL.AttachShader(prog, f);
            GL.LinkProgram(prog);
            GL.DeleteShader(v); GL.DeleteShader(f);
            return prog;
        }
    }
}
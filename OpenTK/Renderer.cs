using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using OpenTK.Graphics.OpenGL4;

namespace Base.Rendering
{
    public class Renderer : Behaviour
    {
        int vao, vbo, ebo, shaderProgram;
        uint[] indices = Array.Empty<uint>();
        float[] vertices = Array.Empty<float>();

        // Hataları önlemek için default stringler
        string objPath = "default.obj";
        string fragShaderPath = "testFrag";
        string vertShaderPath = "testVert";

        // --- 1. Boş Constructor (UI üzerinden yükleme için) ---
        public Renderer() : base()
        {
            InitializeUI();

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            CompileDefaultShader();
        }

        // --- 2. Direkt Veriyle Çalışan Constructor ---
        public Renderer(float[] vertices, uint[] indices, string fragShaderSource, string vertShaderSource) : base()
        {
            InitializeUI();
            this.vertices = vertices;
            this.indices = indices;

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            // Kaynaktan gelen shader'ı derle
            shaderProgram = CompileCustomShader(vertShaderSource, fragShaderSource);
            GenerateMesh();
        }

        // --- 3. Sadece Dosya Yollarıyla Çalışan Constructor ---
        public Renderer(string fragPath, string vertPath, string meshPath) : base()
        {
            InitializeUI();
            this.objPath = meshPath;
            this.fragShaderPath = fragPath;
            this.vertShaderPath = vertPath;

            // UI kutucuklarını güncelle
            uiElements[0].inputValue = meshPath;
            uiElements[1].inputValue = fragPath;
            uiElements[2].inputValue = vertPath;

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            try
            {
                LoadObj(objPath, out vertices, out indices);
                // Shader dosyalarını oku ve derle (ReadShaderSources metodun varsa buraya ekle)
                CompileDefaultShader();
                GenerateMesh();
            }
            catch { }
        }

        private void InitializeUI()
        {
            // inputValue'ları boş bırakmıyoruz ki ImGui patlamasın
            uiElements.Add(new ImgUiElement(UiElementType.InputText, "Mesh Path", () => { }) { inputValue = "default.obj" });
            uiElements.Add(new ImgUiElement(UiElementType.InputText, "Fragment Shader Path", () => { }) { inputValue = "testFrag" });
            uiElements.Add(new ImgUiElement(UiElementType.InputText, "Vertex Shader Path", () => { }) { inputValue = "testVert" });
        }

        public override void Update()
        {
            // UI'dan gelen değişiklik kontrolü
            if (uiElements[0].inputValue != objPath)
            {
                objPath = uiElements[0].inputValue;
                try
                {
                    LoadObj(objPath, out vertices, out indices);
                    GenerateMesh();
                }
                catch { }
            }

            // Çizim
            if (indices.Length > 0 && shaderProgram > 0)
            {
                GL.UseProgram(shaderProgram);
                GL.BindVertexArray(vao);
                GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
        }

        // --- Yardımcı Metotlar ---
        public static void LoadObj(string path, out float[] outVertices, out uint[] outIndices)
        {
            List<float> vList = new List<float>();
            List<uint> iList = new List<uint>();
            string fullPath = Path.Combine("Meshes", path);

            if (!File.Exists(fullPath))
            {
                outVertices = Array.Empty<float>();
                outIndices = Array.Empty<uint>();
                return;
            }

            foreach (var line in File.ReadAllLines(fullPath))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;
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
            if (vertices.Length == 0 || indices.Length == 0) return;
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        void CompileDefaultShader()
        {
            shaderProgram = CompileCustomShader(
               "#version 330 core\nlayout(location=0) in vec3 aPos; void main(){gl_Position=vec4(aPos,1.0);}",
               "#version 330 core\nout vec4 fColor; void main(){fColor=vec4(1.0, 0.5, 0.2, 1.0);}"
            );
        }

        int CompileCustomShader(string vSource, string fSource)
        {
            int v = GL.CreateShader(ShaderType.VertexShader); GL.ShaderSource(v, vSource); GL.CompileShader(v);
            int f = GL.CreateShader(ShaderType.FragmentShader); GL.ShaderSource(f, fSource); GL.CompileShader(f);
            int prog = GL.CreateProgram();
            GL.AttachShader(prog, v); GL.AttachShader(prog, f);
            GL.LinkProgram(prog);
            return prog;
        }
    }
}
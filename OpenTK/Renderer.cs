using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Base.Rendering
{
    public class Renderer : Behaviour
    {
        int vao, vbo, ebo;
        uint[] indices;
        float[] vertices;
        string fragShaderSource, vertShaderSource;

        int shaderProgram;
        int fragmentShader, vertexShader;
        public Renderer(float[] vertices, uint[] indices, string fragShaderSource, string vertShaderSource) : base()
        {
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
            this.fragShaderSource = fragShaderSource;
            this.vertShaderSource = vertShaderSource;
            this.indices = indices;
            this.vertices = vertices;
            GenerateMesh();
        }

        public Renderer(string fragShaderPath, string vertShaderPath, float[] vertices, uint[] indices) : base()
        {
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
            this.indices = indices;
            this.vertices = vertices;
            this.fragShaderSource = "";
            this.vertShaderSource = "";
            ReadShaderSources(fragShaderPath, vertShaderPath);
            GenerateMesh();
        }

        void ReadShaderSources(string fragShaderPath, string vertShaderPath)
        {
            if (File.Exists("Shaders/" + fragShaderPath) && File.Exists("Shaders/" + vertShaderPath))
            {
                fragShaderSource = File.ReadAllText("Shaders/" + fragShaderPath);
                vertShaderSource = File.ReadAllText("Shaders/" + vertShaderPath);
            }
            else
            {
                throw new Exception("Shader not found");
            }
        }

        void GenerateMesh()
        {
            GL.BindVertexArray(vao);

            shaderProgram = GL.CreateProgram();

            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            vertexShader = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(fragmentShader, fragShaderSource);
            GL.ShaderSource(vertexShader, vertShaderSource);

            GL.CompileShader(vertexShader);
            GL.CompileShader(fragmentShader);

            GL.AttachShader(shaderProgram, fragmentShader);
            GL.AttachShader(shaderProgram, vertexShader);

            GL.LinkProgram(shaderProgram);



            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indices.Length, indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);

        }

        public override void Update()
        {
            base.Update();
            GL.BindVertexArray(vao);
            GL.UseProgram(shaderProgram);
            GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }

}

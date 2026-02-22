using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;


using (Game game = new Game(800, 600, "LearnOpenTK"))
{
    game.Run();
}

public class Game : GameWindow
{
    // Constructor: Sets window size and title
    public Game(int width, int height, string title)
        : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
    {
    }

    // Runs once when the window starts
    protected override void OnLoad()
    {
        base.OnLoad();

        // Set the "clear color" (Background color)
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        uint[] indices = {
            0, 1, 2
        };

        float[] vertices = {
            -0.5f, -0.5f, 0.0f, // Sol Alt Köşe (Index 0)
             0.5f, -0.5f, 0.0f, // Sağ Alt Köşe (Index 1)
             0.0f,  0.5f, 0.0f  // Üst Orta Köşe (Index 2)
        };

        new MeshRenderer("testFrag", "testVert", vertices, indices);
    }

    // Runs every frame - put your rendering code here
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        // This is where you'd draw shapes!
        foreach(Behaviour b in Behaviour.behaviours)
        {
            b.Update();
        }
        // Swap the front and back buffers to display the image
        SwapBuffers();
    }

    // Handle input like the Escape key to close the window
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }
}

public class GameObject
{

}

public class Behaviour 
{
    public static List<Behaviour> behaviours = new List<Behaviour>();
    public Behaviour()
    {
        behaviours.Add(this);
    }

    public virtual void Update()
    {

    }
}

public class MeshRenderer : Behaviour
{
    int vao, vbo, ebo;
    uint[] indices;
    float[] vertices;
    string fragShaderSource, vertShaderSource;

    int shaderProgram;
    int fragmentShader, vertexShader;
    public MeshRenderer(float[] vertices, uint[] indices, string fragShaderSource, string vertShaderSource) : base()
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

    public MeshRenderer(string fragShaderPath, string vertShaderPath, float[] vertices, uint[] indices) : base()
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


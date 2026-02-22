using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Base.Rendering;

using (Base.Game game = new Base.Game(800, 600, "LearnOpenTK"))
{
    game.Run();
}

namespace Base
{
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

            new Renderer("testFrag", "testVert", vertices, indices);
        }

        // Runs every frame - put your rendering code here
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // This is where you'd draw shapes!
            foreach (Behaviour b in Behaviour.behaviours)
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
}
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vector2 = System.Numerics.Vector2; // Varsayılan olarak Numerics kullan

namespace Base
{
    public class ImGuiController : IDisposable
    {
        private bool _frameBegun;
        private int _vertexArray, _vertexBuffer, _indexBuffer, _shader, _fontTexture;
        private int _windowWidth, _windowHeight;

        public ImGuiController(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            CreateDeviceResources();

            // BURADAKİ NewFrame ve _frameBegun satırlarını sildik!
        }


        private void CreateDeviceResources()
        {
            _vertexArray = GL.GenVertexArray();
            _vertexBuffer = GL.GenBuffer();
            _indexBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 10000, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, 2000, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            string vSource = @"#version 330 core
                layout (location = 0) in vec2 Position;
                layout (location = 1) in vec2 UV;
                layout (location = 2) in vec4 Color;
                uniform mat4 ProjectionMatrix;
                out vec2 Frag_UV;
                out vec4 Frag_Color;
                void main() {
                    Frag_UV = UV;
                    Frag_Color = Color;
                    gl_Position = ProjectionMatrix * vec4(Position.xy, 0, 1);
                }";

            string fSource = @"#version 330 core
                in vec2 Frag_UV;
                in vec4 Frag_Color;
                uniform sampler2D Texture;
                layout (location = 0) out vec4 Out_Color;
                void main() {
                    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
                }";

            _shader = GL.CreateProgram();
            int v = GL.CreateShader(ShaderType.VertexShader); GL.ShaderSource(v, vSource); GL.CompileShader(v);
            int f = GL.CreateShader(ShaderType.FragmentShader); GL.ShaderSource(f, fSource); GL.CompileShader(f);
            GL.AttachShader(_shader, v); GL.AttachShader(_shader, f); GL.LinkProgram(_shader);

            GL.BindVertexArray(_vertexArray);
            int stride = Unsafe.SizeOf<ImDrawVert>();
            GL.EnableVertexAttribArray(0); GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1); GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.EnableVertexAttribArray(2); GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            // Font Texture
            ImGui.GetIO().Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int w, out int h, out _);
            _fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            ImGui.GetIO().Fonts.SetTexID((IntPtr)_fontTexture);
        }


        public void Update(GameWindow window, float delta)
        {
            var io = ImGui.GetIO();

            // Boyut kontrolü
            if (_windowWidth <= 0 || _windowHeight <= 0) return;

            io.DisplaySize = new System.Numerics.Vector2(_windowWidth, _windowHeight);
            io.DeltaTime = delta;

            // Mouse ve diğer input güncellemeleri...
            var mouse = window.MouseState;
            io.MousePos = new System.Numerics.Vector2(mouse.X, mouse.Y);
            io.MouseDown[0] = mouse.IsButtonDown(MouseButton.Left);
            io.MouseDown[1] = mouse.IsButtonDown(MouseButton.Right);

            // Eğer daha önce bir kare başladıysa onu sonlandır (Render et)
            if (_frameBegun) ImGui.Render();

            // Yeni kareyi güvenle başlat
            ImGui.NewFrame();
            _frameBegun = true;
        }

        public void Render()
        {
            if (!_frameBegun) return;
            _frameBegun = false;
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
        }

        private void RenderDrawData(ImDrawDataPtr data)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);

            var io = ImGui.GetIO();
            // Numerics Matrix'i OpenTK Matrix'e manuel çeviriyoruz
            Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(0.0f, io.DisplaySize.X, io.DisplaySize.Y, 0.0f, -1.0f, 1.0f);
            OpenTK.Mathematics.Matrix4 projection = new OpenTK.Mathematics.Matrix4(
                mvp.M11, mvp.M12, mvp.M13, mvp.M14,
                mvp.M21, mvp.M22, mvp.M23, mvp.M24,
                mvp.M31, mvp.M32, mvp.M33, mvp.M34,
                mvp.M41, mvp.M42, mvp.M43, mvp.M44
            );

            GL.UseProgram(_shader);
            GL.UniformMatrix4(GL.GetUniformLocation(_shader, "ProjectionMatrix"), false, ref projection);
            GL.BindVertexArray(_vertexArray);

            for (int i = 0; i < data.CmdListsCount; i++)
            {
                var list = data.CmdLists[i];
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), list.VtxBuffer.Data, BufferUsageHint.StreamDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, list.IdxBuffer.Size * sizeof(ushort), list.IdxBuffer.Data, BufferUsageHint.StreamDraw);

                for (int j = 0; j < list.CmdBuffer.Size; j++)
                {
                    var cmd = list.CmdBuffer[j];
                    GL.Scissor((int)cmd.ClipRect.X, _windowHeight - (int)cmd.ClipRect.W, (int)(cmd.ClipRect.Z - cmd.ClipRect.X), (int)(cmd.ClipRect.W - cmd.ClipRect.Y));
                    GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)cmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(cmd.IdxOffset * sizeof(ushort)), (int)cmd.VtxOffset);
                }
            }
            GL.Disable(EnableCap.ScissorTest);
        }

        public void WindowResized(int w, int h) { _windowWidth = w; _windowHeight = h; }
        public void PressChar(char c) => ImGui.GetIO().AddInputCharacter(c);
        public void OnKeyDown(Keys key, bool down, bool shift, bool ctrl, bool alt)
        {
            var io = ImGui.GetIO();
            // OpenTK Keys enumunu ImGuiKey'e çevirmemiz gerekiyor
            ImGuiKey imguiKey = TranslateKey(key);

            if (imguiKey != ImGuiKey.None)
            {
                io.AddKeyEvent(imguiKey, down);
                // Mod tuşlarını da bildir (Backspace + Ctrl kombinasyonları vb. için)
                io.AddKeyEvent(ImGuiKey.ModCtrl, ctrl);
                io.AddKeyEvent(ImGuiKey.ModShift, shift);
                io.AddKeyEvent(ImGuiKey.ModAlt, alt);
            }
        }

        private ImGuiKey TranslateKey(Keys key)
        {
            // OpenTK Keys -> ImGuiKey eşleştirmesi
            // En önemli olanları buraya ekliyoruz
            return key switch
            {
                Keys.Backspace => ImGuiKey.Backspace,
                Keys.Delete => ImGuiKey.Delete,
                Keys.Enter => ImGuiKey.Enter,
                Keys.Tab => ImGuiKey.Tab,
                Keys.Left => ImGuiKey.LeftArrow,
                Keys.Right => ImGuiKey.RightArrow,
                Keys.Up => ImGuiKey.UpArrow,
                Keys.Down => ImGuiKey.DownArrow,
                _ => ImGuiKey.None
            };
        }
        public void MouseScroll(OpenTK.Mathematics.Vector2 offset) => ImGui.GetIO().MouseWheel += offset.Y;

        public void Dispose()
        {
            GL.DeleteProgram(_shader);
            GL.DeleteTexture(_fontTexture);
            ImGui.DestroyContext();
        }
    }
}
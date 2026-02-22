using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Base.Rendering;
using ImGuiNET; // Doğrudan ImGui.NET kullanımı

using (Base.Game game = new Base.Game(800, 600, "LearnOpenTK"))
{
    game.Run();
}

namespace Base
{
    public class Game : GameWindow
    {
        // ImGuiController'ı buraya ekliyoruz
        private ImGuiController _controller;

        public Game(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // 1. ImGui Controller'ı başlat
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);

            uint[] indices = { 0, 1, 2 };
            float[] vertices = {
                -0.5f, -0.5f, 0.0f,
                 0.5f, -0.5f, 0.0f,
                 0.0f,  0.5f, 0.0f
            };

            Renderer r = new Renderer("testFrag", "testVert", "default.obj");
            GameObject obj = new GameObject();
            obj.components.Add(r);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            // Yeni eklediğimiz metodu çağırıyoruz
            _controller.OnKeyDown(e.Key, true, e.Shift, e.Control, e.Alt);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            _controller.OnKeyDown(e.Key, false, e.Shift, e.Control, e.Alt);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            // 2. Pencere boyutu değişince ImGui'yi bilgilendir
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // 3. Kareyi başlat
            _controller.Update(this, (float)e.Time);
            ImGui.Begin("Game Objects");
            foreach (GameObject b in GameObject.gameObjects)
            {
                b.StepUpdate();

                // TreeNode kullanmak hiyerarşi için daha sağlıklıdır
                if (ImGui.TreeNode(b.name + "##" + b.GetHashCode()))
                {
                    foreach (Behaviour c in b.components)
                    {
                        // Bileşen başlığı
                        ImGui.TextDisabled("Component: " + c.GetType().Name);

                        // Bileşen içeriği için bir Child alan (Hata burada düzeltildi)
                        if (ImGui.BeginChild(c.GetHashCode().ToString(), new System.Numerics.Vector2(0, 80)))
                        {
                            foreach (ImgUiElement ui in c.uiElements)
                            {
                                // ID çakışmasını önlemek için her elemente benzersiz etiket veriyoruz
                                string uniqueLabel = ui.content + "##" + ui.GetHashCode();

                                switch (ui.type)
                                {
                                    case UiElementType.Button:
                                        if (ImGui.Button(uniqueLabel)) ui.Interaction?.Invoke();
                                        break;
                                    case UiElementType.Text:
                                        ImGui.Text(ui.content);
                                        break;
                                    case UiElementType.CheckBox:
                                        if (ImGui.Checkbox(uniqueLabel, ref ui.c)) ui.Interaction?.Invoke();
                                        break;
                                    case UiElementType.InputText:
                                        if (ImGui.InputText(uniqueLabel, ref ui.inputValue, 100)) ui.Interaction?.Invoke();
                                        break;
                                }
                            }
                            ImGui.EndChild(); // Child'ı içeride kapatıyoruz
                        }
                        ImGui.Separator();
                    }
                    ImGui.TreePop(); // TreeNode'u kapatıyoruz
                }
            }
            ImGui.End();



            // 5. ImGui'yi en son çiz (üçgenin üstünde kalması için)
            _controller.Render();

            SwapBuffers();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            // Klavyeden yazı yazabilmek için gerekli
            _controller.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            // Mouse scroll desteği
            _controller.MouseScroll(e.Offset);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            // Belleği temizle
            _controller.Dispose();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
        }
    }

    // --- Mevcut Sınıfların ---
    public class GameObject {
        public static List<GameObject> gameObjects = new List<GameObject>();
        public List<Behaviour> components = new List<Behaviour>();
        public string name = "GameObject";

        public GameObject() {
            gameObjects.Add(this);
        }

        public GameObject(string name) {
            this.name = name;
            gameObjects.Add(this);
        }

        public void StepUpdate() {
            foreach (Behaviour b in components) {
                if (b.enabled)
                {
                    b.Update();
                }
            }
        }



    }
    public class Behaviour
    {
        public bool enabled = true;
        public static List<Behaviour> behaviours = new List<Behaviour>();
        public Behaviour() { behaviours.Add(this); }
        public virtual void Update() { }
        public List<ImgUiElement> uiElements = new List<ImgUiElement>();
    }

    public class ImgUiElement
    {
        public ImgUiElement(UiElementType typr, string content, Action act)
        {
            this.type = typr;
            this.content = content;
            this.Interaction = act;
        }
        public UiElementType type;
        public string content;
        public Action Interaction;
        public bool c; // CheckBox için
        public string inputValue = ""; // InputText için
    }

    public enum UiElementType
    {
        Button,
        Text,
        CheckBox,
        InputText
    }
}
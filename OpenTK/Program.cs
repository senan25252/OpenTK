using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Base.Rendering;
using ImGuiNET;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;

using (Base.Game game = new Base.Game(800, 600, "LearnOpenTK"))
{
    game.Run();
}

namespace Base
{
    public class Game : GameWindow
    {
        private ImGuiController _controller;

        public Game(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
            foreach(GameObject go in GameObject.gameObjects) go.OnEngineStart();
        }



        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            _controller.Update(this, (float)e.Time);

            Renderer.RenderAllEntites();

            if (ImGui.Begin("Game Objects"))
            {
                // 1. Koleksiyonu güvenli dönmek için for kullanıyoruz
                for (int i = 0; i < GameObject.gameObjects.Count; i++)
                {
                    GameObject b = GameObject.gameObjects[i];
                    b.StepUpdate();

                    // 2. TreeNode Başlat
                    if (ImGui.TreeNode($"{b.name}##{b.GetHashCode()}"))
                    {
                        for (int j = 0; j < b.components.Count; j++)
                        {
                            Behaviour c = b.components[j];
                            ImGui.TextDisabled($"Component: {c.GetType().Name}");

                            // 3. BeginChild - Hataların merkezi burasıdır.
                            // Child penceresi açılırken ID'nin benzersiz olduğundan emin oluyoruz.
                            bool childVisible = ImGui.BeginChild($"child_{c.GetHashCode()}", new System.Numerics.Vector2(0, 130));

                            if (childVisible)
                            {
                                foreach (ImgUiElement ui in c.uiElements)
                                {
                                    string uniqueLabel = $"{ui.content}##{ui.GetHashCode()}";

                                    switch (ui.type)
                                    {
                                        case UiElementType.Button:
                                            if (ImGui.Button(uniqueLabel))
                                            {
                                                // Field'dan Action nesnesini al
                                                Action action = (Action)ui.targetField?.GetValue(ui.targetObject);

                                                // Eğer içine bir metod atanmışsa çalıştır
                                                action?.Invoke();

                                                // Varsa özel etkileşim delegate'ini de çalıştır
                                                ui.Interaction?.Invoke();
                                            }
                                            break;
                                        case UiElementType.Text:
                                            ImGui.Text(ui.content);
                                            break;
                                        case UiElementType.InputText:
                                            if (ImGui.InputText(uniqueLabel, ref ui.inputValue, 100))
                                            {
                                                // UI'daki değişim anında sınıftaki gerçek değişkene (objPath vb.) yazılır
                                                ui.targetField?.SetValue(ui.targetObject, ui.inputValue);
                                                ui.Interaction?.Invoke();
                                            }
                                            break;

                                        case UiElementType.CheckBox:
                                            if (ImGui.Checkbox(uniqueLabel, ref ui.c))
                                            {
                                                ui.targetField?.SetValue(ui.targetObject, ui.c);
                                                ui.Interaction?.Invoke();
                                            }
                                            break;
                                        case UiElementType.DropDown:
                                            // Benzersiz ID için içeriği ve Hash'i kullan
                                            if (ImGui.Combo(uniqueLabel, ref ui.selectedIndex, ui.options, ui.options.Length))
                                            {
                                                // Seçilen değeri değişkene yaz
                                                string selectedValue = ui.options[ui.selectedIndex];
                                                ui.targetField?.SetValue(ui.targetObject, selectedValue);
                                                ui.inputValue = selectedValue; // Senkronizasyon için

                                                ui.Interaction?.Invoke();
                                            }
                                            break;
                                            
                                    }
                                }
                            }
                            // KRİTİK: BeginChild çağrıldıysa, sonuç ne olursa olsun EndChild() çağrılmalıdır.
                            // Ancak PopID hatası alıyorsan, BeginChild'ın içinde başka bir PopID çağrılmadığından emin ol.
                            ImGui.EndChild();
                            ImGui.Separator();
                        }

                        // --- COMPONENT EKLEME SİSTEMİ ---
                        if (ImGui.Button($"Add Component +##{b.GetHashCode()}"))
                        {
                            ImGui.OpenPopup($"add_comp_popup_{b.GetHashCode()}");
                        }

                        if (ImGui.BeginPopup($"add_comp_popup_{b.GetHashCode()}"))
                        {
                            // 1. Projedeki 'Behaviour' sınıfından türeyen tüm sınıfları otomatik bulur
                            var componentTypes = Assembly.GetExecutingAssembly().GetTypes()
                                .Where(t => t.IsSubclassOf(typeof(Behaviour)) && !t.IsAbstract);

                            foreach (var type in componentTypes)
                            {
                                // Listede her sınıfın adını göster (Renderer, PlayerMovement vb.)
                                if (ImGui.Selectable(type.Name))
                                {
                                    // 2. Seçilen sınıftan çalışma zamanında bir örnek (Instance) oluşturur
                                    Behaviour newComp = (Behaviour)Activator.CreateInstance(type);
                                    b.AddComponent(newComp);

                                    ImGui.CloseCurrentPopup();
                                }
                            }
                            ImGui.EndPopup();
                        }

                        ImGui.TreePop(); // TreeNode kapat
                    }
                }

                ImGui.Separator();
                ImGui.Text($"FPS: {(1f / e.Time):0.00}");
                if (ImGui.Button("Create Game Object", new System.Numerics.Vector2(-1, 0)))
                {
                    new GameObject("GameObject " + GameObject.gameObjects.Count);
                }

                ImGui.End(); // Game Objects Penceresini kapat
            }

            _controller.Render();
            SwapBuffers();
        }

        // --- Diğer Eventler ---
        protected override void OnKeyDown(KeyboardKeyEventArgs e) { base.OnKeyDown(e); _controller.OnKeyDown(e.Key, true, e.Shift, e.Control, e.Alt); }
        protected override void OnKeyUp(KeyboardKeyEventArgs e) { base.OnKeyUp(e); _controller.OnKeyDown(e.Key, false, e.Shift, e.Control, e.Alt); }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // 1. OpenGL çizim alanını yeni boyuta göre ayarla
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            // 2. ImGui kontrolcüsüne yeni boyutları bildir
            _controller.WindowResized(ClientSize.X, ClientSize.Y);

            // Not: Bazı ImGuiController sürümlerinde bu metod DisplaySize 
            // veya WindowResized olarak geçer. Parametrelerin doğruluğundan emin ol.
        }
        protected override void OnTextInput(TextInputEventArgs e) { base.OnTextInput(e); _controller.PressChar((char)e.Unicode); }
        protected override void OnMouseWheel(MouseWheelEventArgs e) { base.OnMouseWheel(e); _controller.MouseScroll(e.Offset); }
        protected override void OnUnload() { base.OnUnload(); _controller.Dispose(); }
        protected override void OnUpdateFrame(FrameEventArgs e) { base.OnUpdateFrame(e); if (KeyboardState.IsKeyDown(Keys.Escape)) Close(); }
    }

    public class GameObject
    {
        public static List<GameObject> gameObjects = new List<GameObject>();
        public List<Behaviour> components = new List<Behaviour>();
        public string name = "GameObject";
        public GameObject(string name = "GameObject") { this.name = name; gameObjects.Add(this); }
        public void StepUpdate() { foreach (Behaviour b in components) if (b.enabled) b.Update(); }
        public void OnEngineStart() { foreach (Behaviour b in components) b.OnEngineStart(); }
        public void AddComponent(Behaviour b) { components.Add(b); b.InitUI(); }
    }

    public class Behaviour
    {
        public bool enabled = true;
        public List<ImgUiElement> uiElements = new List<ImgUiElement>();
        public virtual void Update() 
        {
            
        }

        public virtual void OnEngineStart()
        {

        }

        public void InitUI()
        {
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                // Eğer zaten eklenmişse mükerrer kayıt yapma
                if (uiElements.Any(x => x.targetField == field)) continue;

                string label = field.Name;

                // --- ÖZEL DURUM: Shader ve Mesh Seçimi (DropDown) ---
                if (field.Name.Contains("ShaderPath") || field.Name.Contains("objPath"))
                {
                    Console.WriteLine("WARNING: Specific situation detected if you dont want this to happen please rename the object something diffrent from (shaderPath, objPath)");
                    var drop = new ImgUiElement(UiElementType.DropDown, label, this, field);

                    // Klasör tarama mantığını buraya veya bir yardımcı metoda alıyoruz
                    string folder = field.Name.Contains("obj") ? "Meshes" : "Shaders";
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);

                    if (Directory.Exists(fullPath))
                    {
                        drop.options = Directory.GetFiles(fullPath).Select(Path.GetFileName).ToArray();
                        drop.selectedIndex = Math.Max(0, Array.IndexOf(drop.options, field.GetValue(this).ToString()));
                    }
                    else
                    {
                        drop.options = new string[] { "Klasör Bulunamadı" };
                    }

                    uiElements.Add(drop);
                }
                // --- Standart Tipler ---
                else if (field.FieldType == typeof(bool))
                {
                    uiElements.Add(new ImgUiElement(UiElementType.CheckBox, label, this, field));
                }
                else if (field.FieldType == typeof(string))
                {
                    uiElements.Add(new ImgUiElement(UiElementType.InputText, label, this, field));
                }
                else if (field.FieldType == typeof(Action))
                {
                    uiElements.Add(new ImgUiElement(UiElementType.Button, label, this, field));
                }
            }
        }
    }

    public class ImgUiElement
    {
        public UiElementType type;
        public string content;
        public Action Interaction;

        // Referans takibi için eklenenler:
        public FieldInfo targetField;
        public object targetObject;

        public bool c;
        public string inputValue = "";
        public string[] options;
        public int selectedIndex = 0;

        public ImgUiElement(UiElementType type, string content, object target, FieldInfo field)
        {
            this.type = type;
            this.content = content;
            this.targetObject = target;
            this.targetField = field;

            // Değişkenin başlangıç değerini al
            if (field != null)
            {
                var val = field.GetValue(target);
                if (type == UiElementType.InputText) inputValue = val?.ToString() ?? "";
                if (type == UiElementType.CheckBox) c = (bool)val;
            }
        }
    }

    public enum UiElementType { Button, Text, CheckBox, InputText, DropDown }
}
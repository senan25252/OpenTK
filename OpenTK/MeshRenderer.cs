using Base;
using Base.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class MeshRenderer : Behaviour
{
    // İsimlendirme önemli: 'ShaderPath' veya 'objPath' içerdiği için 
    // otomatik olarak Dropdown olacaklar.
    public string objPath = "default.obj";
    public string vertShaderPath = "testVert";
    public string fragShaderPath = "testFrag";

    public Action ApplyChanges;
    private Renderer.RenderEntity entity;

    public MeshRenderer() : base()
    {
        // BÜTÜN UI otomatik oluşur (Dropdownlar dahil)
        InitUI();

        ApplyChanges = () => {
            Renderer.DeleteEntity(entity);
            Renderer.ReloadEverything(ref entity, vertShaderPath, fragShaderPath, objPath);
        };

        // Motoru başlat
        Renderer.ReloadEverything(ref entity, vertShaderPath, fragShaderPath, objPath);
    }

    public override void Update()
    {
        // 1. Shader'ı seç
        GL.UseProgram(entity.shaderProgram);

        // 2. Matrisi hesapla ve Shader'a gönder
        CalculateModelMatrix();
    }

    public void CalculateModelMatrix()
    {
        var pos = gameObject.transform.position;
        var rot = gameObject.transform.rotation;
        var scale = gameObject.transform.scale;

        // Dereceyi Radyana çeviriyoruz: MathHelper.DegreesToRadians
        // Doğru SRT Sırası
        Matrix4 model = Matrix4.CreateScale(scale.X, scale.Y, scale.Z) *
                        Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X)) *
                        Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y)) *
                        Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z)) *
                        Matrix4.CreateTranslation(pos.X, pos.Y, pos.Z);

        int modelLoc = GL.GetUniformLocation(entity.shaderProgram, "model");
        GL.UniformMatrix4(modelLoc, false, ref model);
    }
}
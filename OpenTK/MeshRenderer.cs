using Base;
using Base.Rendering;

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
}
using Base;
using Base.Rendering;
using OpenTK.Windowing.Common;
using OpenTK.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Input 
{
    public static bool GetKey(Keys key)
    {
        bool Input = Game.Instance.IsKeyDown((Keys)key);
        return Input;
    }
}

using Base;

public class TestScript2 : Behaviour
{
    public bool Test;
    public Action act = () => Console.WriteLine("Hello World");
    public enum TestEnum
    {
        Option1,
        Option2,
        Option3
    }

    public TestEnum testEnumValue;

    public override void Update()
    {
        if(Input.GetKey(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
        {
            gameObject.transform.position.X += 1f * (float)Game.Instance.UpdateTime;
        }
        if (Input.GetKey(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
        {
            gameObject.transform.position.X -= 1f * (float)Game.Instance.UpdateTime;
        }
        if (Input.GetKey(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
        {
            gameObject.transform.position.Y += 1f * (float)Game.Instance.UpdateTime;
        }
        if (Input.GetKey(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
        {
            gameObject.transform.position.Y -= 1f * (float)Game.Instance.UpdateTime;
        }
    }
}
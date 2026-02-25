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
    
    }
}
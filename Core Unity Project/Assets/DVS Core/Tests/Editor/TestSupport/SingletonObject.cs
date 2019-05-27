namespace Dvs.Core.IoC.Tests
{
    public interface ISingletonObject
    {
        string Text { get; set; }
    }
    public class SingletonObject : ISingletonObject
    {
        public const string TextDefault = "default2";
        public string Text { get; set; } = TextDefault;
    }
}

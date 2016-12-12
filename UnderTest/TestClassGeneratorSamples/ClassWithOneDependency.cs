namespace TestClassGeneratorSamples
{
    public class ClassWithOneDependency
    {
        IDependency _dependency;
        public ClassWithOneDependency(IDependency dependency)
        {
            _dependency = dependency;

            ClassWithOneDependency a;
            a = new ClassWithOneDependency(_dependency);
        }
    }

    public interface IDependency
    {
        string Value { get; set; }
    }
}

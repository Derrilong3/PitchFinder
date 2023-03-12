namespace PitchFinder.Models
{
    public interface IWindow
    {
        string Name { get; }
        string Description { get; }
        double Apply(int n, int size);
    }
}

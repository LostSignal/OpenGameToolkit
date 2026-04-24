
namespace OGT
{
    public interface IInputUpdater<T>
    {
        bool IsActive { get; }

        void Update(ref T input, float deltaTime);
    }
}

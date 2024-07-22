namespace Monstromatic.Data.Interfaces;

public interface IAppBaseDataStorage<T> : IBaseDataStorage<T>
{
    void ResetToDefault();
}
namespace Monstromatic.Data.Interfaces;

public interface IBaseDataStorage<T>
{
    T Read();
    void Save(T data);
}
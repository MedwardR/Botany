namespace Botany.Interfaces;

interface ISerializable<T>
{
    string Serialize();
    static abstract T Deserialize(string s);
}

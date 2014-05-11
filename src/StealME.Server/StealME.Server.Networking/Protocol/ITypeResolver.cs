using System;

namespace StealME.Networking.Protocol
{
    public interface ITypeResolver
    {
        Type[] GetTypes();
        Type GetTypeById(int id);
        int GetIdByType(Type type);
    }
}

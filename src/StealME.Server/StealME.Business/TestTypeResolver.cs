using System;
using System.Collections.Generic;
using System.Linq;
using StealME.Business.Requests;
using StealME.Business.Responses;
using StealME.Networking.Protocol;

namespace StealME.Business
{
    public class TestTypeResolver : ITypeResolver
    {
        private static Dictionary<int, Type> _typeDictionary = new Dictionary<int, Type>() {
                                                                                               { 0, typeof(TestRequest) },
                                                                                               { 1, typeof(TestResponse)}
                                                                                         };

        public Type[] GetTypes()
        {
            return _typeDictionary.Values.ToArray();
        }

        public Type GetTypeById(int id)
        {
            return _typeDictionary[id];
        }

        public int GetIdByType(Type type)
        {
            return _typeDictionary.First(a => a.Value == type).Key;
        }
    }
}

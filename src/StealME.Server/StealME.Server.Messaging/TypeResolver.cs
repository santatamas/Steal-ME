using System;
using System.Collections.Generic;
using System.Linq;
using StealME.Networking.Protocol;
using StealME.Server.Messaging.Messages;
using StealME.Server.Messaging.Requests;
using StealME.Server.Messaging.Responses;

namespace StealME.Server.Messaging
{
    public class TypeResolver : ITypeResolver
    {
        private static Dictionary<int, Type> _typeDictionary = new Dictionary<int, Type>() {
                                                                                               { 0,  typeof(TestRequest)    },
                                                                                               { 1,  typeof(TestResponse)   },

                                                                                               { 11, typeof(AuthRequest)    },
                                                                                               { 12, typeof(CommandRequest) },
                                                                                               { 13, typeof(GetRequest)     },
                                                                                               { 14, typeof(SetRequest)     },
                                                                                               { 15, typeof(PingRequest) },

                                                                                               { 21, typeof(ACKResponse)    },
                                                                                               { 22, typeof(AuthResponse)   },
                                                                                               { 23, typeof(StatusResponse) },
                                                                                               { 24, typeof(PingResponse) },
                                                                                               { 25, typeof(ProtocolVersionResponse) },

                                                                                               { 31, typeof(LocationMessage)},
                                                                                               { 32, typeof(TrackerStateMessage)},
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

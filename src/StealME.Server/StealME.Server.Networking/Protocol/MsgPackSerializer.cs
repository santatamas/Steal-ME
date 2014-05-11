using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MsgPack;
using MsgPack.Serialization;

namespace StealME.Networking.Protocol
{
    public class MsgPackSerializer : IMessageSerializer
    {
        public static SerializationContext Context = new SerializationContext();
        private ITypeResolver _typeResolver;
        private static ConcurrentDictionary<Type, IMessagePackSerializer> _serializers = new ConcurrentDictionary<Type, IMessagePackSerializer>();
        public MsgPackSerializer(ITypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
            Context.Serializers.Register(MessagePackSerializer.Create<TypedMessagePackObject>());
            foreach (var type in typeResolver.GetTypes().Where(type => !_serializers.ContainsKey(type)))
            {
                _serializers[type] = MessagePackSerializer.Create(type);
            }
        }

        public byte[] Serialize(object obj)
        {
            lock (_lockObjForSerializing)
            {
                // Serialize object
                var typedSerializer = _serializers[obj.GetType()];
                var typedStream = new MemoryStream();
                var typedPacker = Packer.Create(typedStream);
                typedSerializer.PackTo(typedPacker, obj);

                // Serialize object wrapper
                var objToSend = new TypedMessagePackObject
                {
                    InnerObjectTypeId = _typeResolver.GetIdByType(obj.GetType()),
                    InnerObject = typedStream.ToArray()
                };

                var serializer = Context.Serializers.Get<TypedMessagePackObject>(Context);
                var genStream = new MemoryStream();
                serializer.Pack(genStream, objToSend);

                return genStream.ToArray();
            }
        }

        //public T Deserialize<T>(byte[] raw)
        //{
        //    var message = new MemoryStream(raw);
        //    var serializer = MessagePackSerializer.Create<T>();
        //    var result = serializer.Unpack(message);
        //    return result;
        //}

        object _lockObjForDeserializing = new object();
        object _lockObjForSerializing = new object();

        public object Deserialize(byte[] raw)
        {
            lock (_lockObjForDeserializing)
            {
                var message = new MemoryStream(raw);
                var serializer = Context.Serializers.Get<TypedMessagePackObject>(Context);
                var typedWrapper = serializer.Unpack(message);
                var typedSerializer = _serializers[_typeResolver.GetTypeById(typedWrapper.InnerObjectTypeId)];
                var typedMessage = new MemoryStream(typedWrapper.InnerObject);
                using (var unpacker = Unpacker.Create(typedMessage))
                {
                    unpacker.Read();
                    object result = typedSerializer.UnpackFrom(unpacker);
                    return result;
                }
            }
        }
    }
}
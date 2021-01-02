using MessagePack;
using Nextwin.Client.Util;
using Nextwin.Protocol;

namespace Nextwin.Client.Protocol
{
    public class Serializer : Singleton<Serializer>, ISerializer
    {
        public T Deserialize<T>(byte[] bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        public byte[] Serialize<T>(T data)
        {
            return MessagePackSerializer.Serialize(data);
        }
    }
}

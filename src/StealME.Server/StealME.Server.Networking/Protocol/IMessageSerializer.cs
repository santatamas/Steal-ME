namespace StealME.Networking.Protocol
{
    public interface IMessageSerializer
    {
        byte[] Serialize(object obj);
        //T Deserialize<T>(byte[] raw);
        object Deserialize(byte[] raw);
    }
}

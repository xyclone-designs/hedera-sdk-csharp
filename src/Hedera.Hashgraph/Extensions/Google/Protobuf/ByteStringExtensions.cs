
namespace Google.Protobuf
{
    public static class ByteStringExtensions
	{
        public static ByteString Concat(this ByteString bytes, ByteString value) 
        {
            return ByteString.CopyFrom([.. bytes, ..value]);
        }
        public static ByteString Concat(this ByteString bytes, params byte[] value) 
        {
            return ByteString.CopyFrom([.. bytes, ..value]);
        }
	}
}

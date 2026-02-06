
using System.Text;

namespace Google.Protobuf
{
    public static class ByteStringExtensions
	{
        public static ByteString Copy(this ByteString bytes) 
        {
            return ByteString.CopyFrom(bytes.ToByteArray());
        }
        public static ByteString Concat(this ByteString bytes, ByteString value) 
        {
            return ByteString.CopyFrom([.. bytes, ..value]);
        }
        public static ByteString Concat(this ByteString bytes, params byte[] value) 
        {
            return ByteString.CopyFrom([.. bytes, ..value]);
        }
        
        public static ByteString Substring(this ByteString bytes, int startIndex, Encoding? encoding = null)
		{
            string substring = bytes.ToString(encoding ?? Encoding.UTF8)[startIndex..];

            return ByteString.CopyFrom(substring, encoding ?? Encoding.UTF8);
        }
		public static ByteString Substring(this ByteString bytes, int startIndex, int endIndex, Encoding? encoding = null)
		{
			string substring = bytes.ToString(encoding ?? Encoding.UTF8)[startIndex..endIndex];

			return ByteString.CopyFrom(substring, encoding ?? Encoding.UTF8);
		}

		public static bool EndsWith(this ByteString bytes, char value, Encoding? encoding = null)
		{
            return bytes.ToString(encoding ?? Encoding.UTF8).EndsWith(value);
		}
		public static bool EndsWith(this ByteString bytes, string value, Encoding? encoding = null)
		{
            return bytes.ToString(encoding ?? Encoding.UTF8).EndsWith(value);
		}
		public static bool EndsWith(this ByteString bytes, ByteString value, Encoding? encoding = null)
		{
			string _ = value.ToString(encoding ?? Encoding.UTF8);

			return bytes.ToString(encoding ?? Encoding.UTF8).EndsWith(_);
		}

		public static bool StartsWith(this ByteString bytes, char value, Encoding? encoding = null)
		{
            return bytes.ToString(encoding ?? Encoding.UTF8).StartsWith(value);
		}
		public static bool StartsWith(this ByteString bytes, string value, Encoding? encoding = null)
		{
            return bytes.ToString(encoding ?? Encoding.UTF8).StartsWith(value);
		}
		public static bool StartsWith(this ByteString bytes, ByteString value, Encoding? encoding = null)
		{
			string _ = value.ToString(encoding ?? Encoding.UTF8);

			return bytes.ToString(encoding ?? Encoding.UTF8).StartsWith(_);
		}
	}
}

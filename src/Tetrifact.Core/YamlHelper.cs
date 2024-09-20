using YamlDotNet.Serialization;

namespace Tetrifact.Core
{
    public class YmlHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IDeserializer GetDeserializer()
        {
            return new DeserializerBuilder()
                .WithNodeDeserializer(new KeyValueDeserializer())
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public static ISerializer GetSerializer()
        {
            return new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
        }
    }
}
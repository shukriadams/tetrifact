using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Tetrifact.Core
{
	public class KeyValueDeserializer : INodeDeserializer
	{
		public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
		{
			if (expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
			{
				Type[] pairArgs = expectedType.GetGenericArguments();
				object key = null;
				object val = null;

				parser.Consume<MappingStart>();

				// disable "obsolete" warning on this library, no documentation on how to replace the new method
#pragma warning disable 612, 618
				while (parser.Allow<MappingEnd>() == null)
#pragma warning restore 612, 618
				{
					Scalar keyName = parser.Consume<Scalar>();
					key = keyName.Value;
					val = nestedObjectDeserializer(parser, pairArgs[1]);
				}

				value = Activator.CreateInstance(expectedType, key, val);
				return true;
			}

			value = null;
			return false;
		}
	}
}
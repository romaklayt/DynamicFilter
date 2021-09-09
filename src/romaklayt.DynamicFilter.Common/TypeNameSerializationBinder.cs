using System;
using System.Runtime.Serialization;

namespace romaklayt.DynamicFilter.Common
{
    public class TypeNameSerializationBinder : SerializationBinder
    {
        public TypeNameSerializationBinder(string typeFormat)
        {
            TypeFormat = typeFormat;
        }

        public string TypeFormat { get; }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            var resolvedTypeName = string.Format(TypeFormat, typeName);

            return Type.GetType(resolvedTypeName, true);
        }
    }
}
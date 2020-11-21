//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.ComponentModel;
//using Microsoft.Xna.Framework;
//using System.Globalization;


//namespace HD
//{
//    public class Vector2Converter : ExpandableObjectConverter
//    {
//        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
//        {
//            return sourceType == typeof(String);
//        }

//        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
//        {
//            string sValue = value as string;
//            object retVal = null;

//            if (sValue != null)
//            {
//                sValue = sValue.Trim();

//                if (sValue.Length != 0)
//                {
//                    // Parse the string
//                    if (null == culture)
//                        culture = CultureInfo.CurrentCulture;

//                    // Split the string based on the cultures list separator
//                    string[] parms = sValue.Split(new char[] { culture.TextInfo.ListSeparator[0] });

//                    if (parms.Length == 2)
//                    {
//                        // Should have an integer and a string.
//                        float x = Convert.ToSingle(parms[0]);
//                        float y = Convert.ToSingle(parms[1]);

//                        // And finally create the object
//                        retVal = new Vector2(x, y);
//                    }
//                }
//            }
//            else
//                retVal = base.ConvertFrom(context, culture, value);

//            return retVal;

//        }

//        public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
//        {
//            return new Vector2((float)propertyValues["X"], (float)propertyValues["Y"]);
//        }

//        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
//        {
//            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(value, attributes);

//            string[] sortOrder = new string[2];

//            sortOrder[0] = "X";
//            sortOrder[1] = "Y";

//            // Return a sorted list of properties
//            return properties.Sort(sortOrder);
//        }

//        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
//        {
//            return true;
//        }

//        public static void Register<T, TC>() where TC : TypeConverter
//        {
//            Attribute[] attr = new Attribute[1];
//            TypeConverterAttribute vConv = new TypeConverterAttribute(typeof(TC));
//            attr[0] = vConv;
//            TypeDescriptor.AddAttributes(typeof(T), attr);
//        }
//    }
//}
using System;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace ProtoBuf.Serializers
{
    sealed class Vector2Serializer : IProtoSerializer
    {
        static readonly Type expectedType = typeof(Vector2);
        public Vector2Serializer(ProtoBuf.Meta.TypeModel model)
        {
        }
        public Type ExpectedType { get { return expectedType; } }

        bool IProtoSerializer.RequiresOldValue { get { return false; } }
        bool IProtoSerializer.ReturnsValue { get { return true; } }
        public object Read(object value, ProtoReader source)
        {
            var result = new Vector2();

            var bytes = BitConverter.GetBytes(source.ReadInt64());
            result.X = BitConverter.ToSingle(bytes, 0);
            result.Y = BitConverter.ToSingle(bytes, 4);

            return result;
        }
        public void Write(object value, ProtoWriter dest)
        {
            var vect = (Vector2)value;

            var bytes = new byte[8];
            Array.Copy(BitConverter.GetBytes(vect.X), bytes, 4);
            Array.Copy(BitConverter.GetBytes(vect.Y), 0, bytes, 4, 4);
            ProtoWriter.WriteInt64(BitConverter.ToInt64(bytes, 0), dest);
        }
    }
}

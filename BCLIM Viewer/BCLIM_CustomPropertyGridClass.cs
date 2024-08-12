using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace BCLIM_Viewer
{
	public class BCLIM_CustomPropertyGridClass
	{
        public class CustomSortTypeConverter : TypeConverter
        {
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                PropertyDescriptorCollection PDC = TypeDescriptor.GetProperties(value, attributes);

                Type type = value.GetType();

                List<string> list = type.GetProperties().Select(x => x.Name).ToList();

                return PDC.Sort(list.ToArray());
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class CustomExpandableObjectSortTypeConverter : ExpandableObjectConverter
        {
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                PropertyDescriptorCollection PDC = TypeDescriptor.GetProperties(value, attributes);

                Type type = value.GetType();

                List<string> list = type.GetProperties().Select(x => x.Name).ToList();

                return PDC.Sort(list.ToArray());
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }

        public class CustomListTypeConverter<T> : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                string type = value.GetType().GenericTypeArguments[0].Name;

                IList memberList = value as IList;

                if (memberList == null) return $"List<{type}>[NULL]";


                return $"List<{type}>[{memberList.Count}]";
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                List<PropertyDescriptor> PropDescList = new List<PropertyDescriptor>();

                IEnumerable memberList = value as IEnumerable;
                if (memberList != null)
                {
                    foreach (var m in memberList) PropDescList.Add(new CustomMemberDescriptor<T>(m, PropDescList.Count));
                }

                return new PropertyDescriptorCollection(PropDescList.ToArray());
            }

            class CustomMemberDescriptor<MemberType> : SimplePropertyDescriptor
            {
                public int Index { get; private set; }
                public MemberType Value { get; private set; }

                public CustomMemberDescriptor(object value, int index) : base(value.GetType(), $"{nameof(T)} [{index}]", typeof(T))
                {
                    Index = index;
                    Value = (MemberType)value;
                }

                public override object GetValue(object component)
                {
                    //Default
                    //object[] objects = ((IEnumerable)component).Cast<object>().ToArray();
                    T[] objectsT = ((IEnumerable)component).Cast<T>().ToArray();
                    return objectsT[Index];
                    //return Value;
                }

                public override void SetValue(object component, object value)
                {
                    //value(String)が空("")のとき、System.InvalidCastExceptionが発生する(?)
                    Value = (MemberType)value;
                }
            }
        }
    }
}

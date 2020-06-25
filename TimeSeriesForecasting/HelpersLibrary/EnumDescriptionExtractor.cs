using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesForecasting.HelpersLibrary
{
    //public class EnumDescriptionExtractor : EnumConverter
    //{
    //    private readonly Type _enumType;

    //    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
    //    {
    //        if (value == null) return "";

    //        var fieldInfo = _enumType.GetField(Enum.GetName(_enumType, value));

    //        if (Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) is DescriptionAttribute dna)
    //            return dna.Description;

    //        return value.ToString();
    //    }

    //    public EnumDescriptionExtractor(Type type) : base(type)
    //    {
    //        _enumType = type;
    //    }
    //}

    public class EnumDescriptionExtractor : EnumConverter
    {
        private readonly Type _enumType;

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (value == null) return "";

            var fieldInfo = _enumType.GetField(Enum.GetName(_enumType, value));

            if (Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) is DescriptionAttribute dna)
                return dna.Description;

            return value.ToString();
        }

        public EnumDescriptionExtractor(Type type) : base(type)
        {
            _enumType = type;
        }
    }
}

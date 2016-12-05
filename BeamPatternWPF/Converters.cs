using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace BeamPatternWPF
{
    internal abstract class ValueConverter : MarkupExtension, IValueConverter
    {
        protected static readonly TypeConverter sf_DoubleConverter = TypeDescriptor.GetConverter(typeof(double));

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        /// <inheritdoc />
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        /// <inheritdoc />
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }

    internal abstract class MultiValueConverter : MarkupExtension, IMultiValueConverter
    {
        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        /// <inheritdoc />
        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        /// <inheritdoc />
        public abstract object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);

    }

    internal class CompositeCollectionConverter : MultiValueConverter
    {
        /// <inheritdoc />
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = new CompositeCollection();
            foreach(var value in values)
            {
                var enumerable = value as IEnumerable;
                collection.Add(enumerable != null ? new CollectionContainer { Collection = enumerable } : value);
            }

            return collection;
        }

        /// <inheritdoc />
        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("CompositeCollectionConverter ony supports oneway bindings");
        }
    }

    internal class CommandsExtractor : ValueConverter
    {
        #region Overrides of ValueConverter

        /// <inheritdoc />
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null) return null;
            var t = value.GetType();
            var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(ICommand) || p.PropertyType.GetInterface(typeof(ICommand).FullName) != null);
            return properties.Select(p => p.GetValue(value)).OfType<ICommand>();
        }

        /// <inheritdoc />
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    internal class RoundConverter : ValueConverter
    {
        private static readonly TypeConverter sf_Converter = TypeDescriptor.GetConverter(typeof(int));

        #region Overrides of ValueConverter

        /// <inheritdoc />
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(parameter == null) return Math.Round((double)value);
            if(!sf_Converter.CanConvertFrom(parameter.GetType()))
                throw new ArgumentException("Тип параметра не может быть преобразован в int");
            return Math.Round((double)value, (int)sf_Converter.ConvertFrom(parameter));
        }

        /// <inheritdoc />
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;

        #endregion
    }

    internal class MultyplyConverter : ValueConverter
    {
        private double f_K;

        public MultyplyConverter() { f_K = double.NaN; }

        public MultyplyConverter(double K) { f_K = K; }

        #region Overrides of ValueConverter

        /// <inheritdoc />
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is double) && !sf_DoubleConverter.CanConvertFrom(value.GetType())) return double.NaN;
            var v = value as double? ?? (double)sf_DoubleConverter.ConvertFrom(value);
            if(!double.IsNaN(f_K)) return v * f_K;

            if(!(parameter is double) && parameter == null || !sf_DoubleConverter.CanConvertFrom(parameter.GetType())) return double.NaN;
            var k = parameter as double? ?? (double)sf_DoubleConverter.ConvertFrom(parameter);
            return v * k;
        }

        /// <inheritdoc />
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is double) && !sf_DoubleConverter.CanConvertFrom(value.GetType())) return double.NaN;
            var v = value as double? ?? (double)sf_DoubleConverter.ConvertFrom(value);
            if(!double.IsNaN(f_K)) return v / f_K;

            if(!(parameter is double) && parameter == null || !sf_DoubleConverter.CanConvertFrom(parameter.GetType())) return double.NaN;
            var k = parameter as double? ?? (double)sf_DoubleConverter.ConvertFrom(parameter);
            return v / k;
        }

        #endregion
    }
}
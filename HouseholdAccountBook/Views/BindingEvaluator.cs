using System.Windows;
using System.Windows.Data;

#nullable enable

namespace HouseholdAccountBook.Views
{
    public class BindingEvaluator : DependencyObject
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(object),
                typeof(BindingEvaluator),
                new PropertyMetadata(null));

        public object? Value {
            get => this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        public static object? Evaluate(object source, Binding binding)
        {
            BindingEvaluator evaluator = new();

            Binding clone = new() {
                Path = binding.Path,
                Converter = binding.Converter,
                ConverterCulture = binding.ConverterCulture,
                ConverterParameter = binding.ConverterParameter,
                StringFormat = binding.StringFormat,
                TargetNullValue = binding.TargetNullValue,
                FallbackValue = binding.FallbackValue,
                Mode = BindingMode.OneTime,
                Source = source
            };

            _ = BindingOperations.SetBinding(evaluator, ValueProperty, clone);

            return evaluator.Value;
        }
    }
}

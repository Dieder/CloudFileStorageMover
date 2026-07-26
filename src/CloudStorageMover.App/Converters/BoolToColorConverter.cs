using System.Globalization;

namespace CloudStorageMover.App.Converters;

public class BoolToColorConverter : IValueConverter
{
    public string TrueValue { get; set; } = "#512BD4";
    public string FalseValue { get; set; } = "#555";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string param && param.Contains('|'))
        {
            var parts = param.Split('|');
            var color = value is true ? parts[0] : parts[1];
            return Color.FromArgb(color);
        }
        var defaultColor = value is true ? TrueValue : FalseValue;
        return Color.FromArgb(defaultColor);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

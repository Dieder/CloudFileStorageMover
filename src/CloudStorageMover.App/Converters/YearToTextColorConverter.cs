using System.Globalization;

namespace CloudStorageMover.App.Converters;

public class YearToTextColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int selectedYear = 0;
        if (parameter is string paramStr && int.TryParse(paramStr, out var paramYear))
            selectedYear = paramYear;

        if (value is int year && year == selectedYear)
            return Colors.White;

        return Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.LightGray : Colors.DarkGray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

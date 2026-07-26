using System.Globalization;

namespace CloudStorageMover.App.Converters;

public class YearToColorConverter : IValueConverter
{
    private static readonly string ActiveColor = "#512BD4";
    private static readonly string InactiveLight = "#e0e0e0";
    private static readonly string InactiveDark = "#444";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int selectedYear = 0;
        if (parameter is string paramStr && int.TryParse(paramStr, out var paramYear))
            selectedYear = paramYear;

        if (value is int year)
        {
            bool isActive = year == selectedYear;
            return isActive ? ActiveColor : (Application.Current?.RequestedTheme == AppTheme.Dark ? InactiveDark : InactiveLight);
        }

        return Application.Current?.RequestedTheme == AppTheme.Dark ? InactiveDark : InactiveLight;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

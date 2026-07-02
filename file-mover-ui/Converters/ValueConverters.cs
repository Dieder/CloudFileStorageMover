using System.Globalization;

namespace FileMover.UI.Converters;

/// <summary>
/// Converts double value (0-100) to progress value (0-1)
/// </summary>
public class DoubleToProgressConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue / 100.0;
        }
        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue * 100.0;
        }
        return 0.0;
    }
}

/// <summary>
/// Converts null to boolean (null = false, not null = true)
/// </summary>
public class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts non-null to boolean (null = true, not null = false)
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts a boolean value
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return true;
    }
}

/// <summary>
/// Converts transfer status to color
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FileMover.UI.Models.TransferStatus status)
        {
            return status switch
            {
                FileMover.UI.Models.TransferStatus.Completed => Colors.Green,
                FileMover.UI.Models.TransferStatus.Failed => Colors.Red,
                FileMover.UI.Models.TransferStatus.Cancelled => Colors.Orange,
                FileMover.UI.Models.TransferStatus.InProgress => Colors.Blue,
                _ => Colors.Gray
            };
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

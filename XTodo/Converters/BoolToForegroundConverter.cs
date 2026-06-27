using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace XTodo.Converters;

public class BoolToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value
            ? new SolidColorBrush(Color.FromRgb(0xBD, 0xBD, 0xBD))
            : new SolidColorBrush(Color.FromRgb(0x3E, 0x27, 0x23));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

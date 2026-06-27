using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace XTodo.Converters;

public class BoolToSuccessForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value
            ? new SolidColorBrush(Color.FromRgb(0x66, 0xBB, 0x6A))
            : new SolidColorBrush(Color.FromRgb(0xBD, 0xBD, 0xBD));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

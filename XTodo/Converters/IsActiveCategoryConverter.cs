using System.Globalization;
using System.Windows.Data;

namespace XTodo.Converters;

public class IsActiveCategoryConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is string id && values[1] is string activeId)
            return id == activeId;
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

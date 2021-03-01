using System;
using System.Globalization;
using System.Windows.Data;
using AutoUpdaterDotNET;

namespace ValheimToolerLauncher.Converters
{
    public class UpdateInfoToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return "New update available: " + (value as UpdateInfoEventArgs).CurrentVersion;
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DocumentManagement.Launcher.Base
{
    public class BoolToVisuble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bvalue)
            {
                if (bvalue)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

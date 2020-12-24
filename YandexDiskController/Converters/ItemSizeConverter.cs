﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace MRS.DocumentManagement.Converters
{
    public class ItemSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || int.Parse(value.ToString()) == 0)
            {
                return "-";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
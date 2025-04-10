﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Converters;

public class FileEntryTypeVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (EntryType)value == EntryType.File ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
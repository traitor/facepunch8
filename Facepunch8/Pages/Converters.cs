using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Facepunch8.Pages
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan time = DateTime.Now - UnixTimeStampToDateTime((int)value);
            return GetDuration(time);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null; //One way _only_.
        }

        private static string GetDuration(TimeSpan time)
        {
            int years = time.Days / 365;
            if (years > 0)
                return years + " year" + (years > 1 ? "s" : "") + " ago";

            int months = time.Days / 30;
            if (months > 0)
                return months + " month" + (months > 1 ? "s" : "") + " ago";
            if (time.Days > 0)
                return time.Days + " day" + (time.Days > 1 ? "s" : "") + " ago";
            if (time.Hours > 0)
                return time.Hours + " hour" + (time.Hours > 1 ? "s" : "") + " ago";
            return time.Minutes + " minute" + (time.Minutes > 1 ? "s" : "") + " ago";
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public sealed class IntegerVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is int && (int)value != 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented.");
            //return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public sealed class ParentVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is int && (int)value == -1) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented.");
            //return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public sealed class ChildVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is int && (int)value != -1) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented.");
            //return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public sealed class PageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is int && (int)value != 1) ? ((int)value) + " pages" : ((int)value) + " page";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented.");
            //return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public sealed class NumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is int && (int)value != 1) ? ((int)value).ToString("N0") : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented.");
            //return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public sealed class ForumIDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is int && (int)value != 1) ? "/Assets/ForumIcons/" + ((int)value).ToString() + ".png" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented.");
            //return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public sealed class CapitalizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value is string) ? ((string)value).ToUpper() : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented.");
            //return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}

using System.Globalization;

namespace Coach_app.Core.Converters
{
    public class StringNullToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;

            // Si le chemin existe et n'est pas vide, on l'utilise
            if (!string.IsNullOrEmpty(path))
                return path;

            // Sinon, image par défaut dans Resources/Images
            return "lezardo.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
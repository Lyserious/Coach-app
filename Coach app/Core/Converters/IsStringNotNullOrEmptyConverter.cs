using System.Globalization;

namespace Coach_app.Core.Converters
{
    public class IsStringNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Renvoie TRUE si la chaîne n'est ni nulle ni vide (donc le message d'erreur est visible)
            // Renvoie FALSE si la chaîne est vide (donc le message est caché)
            string str = value as string;
            return !string.IsNullOrWhiteSpace(str);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
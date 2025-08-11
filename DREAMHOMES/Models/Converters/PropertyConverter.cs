using DREAMHOMES.Models.NonDBModels;

namespace DREAMHOMES.Models.Converters
{
    public static class PropertyConverter
    {
        /// <summary>
        /// Converts list of <see cref="Property"/> to string.
        /// </summary>
        /// <param name="properties">The list of <see cref="Property"/> to be converted.</param>
        /// <returns>converted string.</returns>
        public static string ConvertToString(List<Property> properties)
        {
            return string.Join(";", properties.Select(p => $"{p.Key}:{p.Value}"));
        }

        public static List<Property> ConvertToList(string propertiesString)
        {
            if (string.IsNullOrEmpty(propertiesString))
                return new List<Property>();

            return propertiesString.Split(';')
                                   .Select(pair => pair.Split(':'))
                                   .Select(p => new Property { Key = p[0], Value = p[1] })
                                   .ToList();
        }
    }
}

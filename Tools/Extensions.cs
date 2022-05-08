using System.Text;

namespace Point.Tools
{
    public static class Extensions
    {
        /// <summary>
        /// Capitalises first character of string
        /// </summary>
        /// <param name="source">The string to use</param>
        /// <returns>A string with a capitalised first letter</returns>
        public static string FirstCharToUpper(this string source)
            => source.Length > 0 ? string.Concat(source[0].ToString().ToUpper(), source.AsSpan(1)) : source;

        /// <summary>
        /// Write multiple lines to <paramref name="source"/>
        /// </summary>
        /// <param name="source">The StreamWriter to use</param>
        /// <param name="lines">The string array to write to <paramref name="source"/></param>
        public static void WriteLines(this StreamWriter source, string[] lines)
        {
            foreach (string line in lines) source.WriteLine(line);
        }

        /// <summary>
        /// Splits an IEnumerable
        /// </summary>
        /// <typeparam name="T">The type of collection</typeparam>
        /// <param name="source">The source collection</param>
        /// <param name="separator">The function that controls separation of <paramref name="source"/> when returns true</param>
        /// <returns>A IEnumerable of IEnumerables from <paramref name="source"/></returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, Func<T, bool> separator)
        {
            List<List<T>> dest = new();

            List<T> current = new();
            foreach (var chunk in source)
            {
                if (separator(chunk))
                {
                    dest.Add(current);
                    current = new();
                }
                else
                {
                    current.Add(chunk);
                }
            }

            if (current.Any())
                dest.Add(current);

            return dest;
        }

        /// <summary>
        /// Safely convert long to int and prevents overflow
        /// </summary>
        /// <param name="source">The long to convert</param>
        /// <returns>A safely converted interger</returns>
        public static int ToInt(this double source)
        {
            return (int)(source % int.MaxValue);
        }
        /// <summary>
        /// Repeats a string x amount of times
        /// </summary>
        /// <param name="source">The source string</param>
        /// <param name="multiplier">The amount of times to repeat <paramref name="source"/></param>
        /// <returns><paramref name="source"/> repeated <paramref name="multiplier"/> amount of times</returns>
        public static string Multiply(this string source, int multiplier)
        {
            StringBuilder sb = new(multiplier * source.Length);
            for (long i = 0; i < multiplier; i++)
            {
                sb.Append(source);
            }

            return sb.ToString();
        }
    }
}

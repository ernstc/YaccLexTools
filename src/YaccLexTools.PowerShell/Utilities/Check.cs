using System;


namespace YaccLexTools.PowerShell.Utilities
{

    internal static class Check
    {
        public static T NotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        public static T? NotNull<T>(T? value, string parameterName) where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        public static string NotEmpty(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                //throw new ArgumentException(Strings.ArgumentIsNullOrWhitespace(parameterName));
				throw new ArgumentException(String.Format("Argument is null or white space: \"{0}\"", parameterName));
            }

            return value;
        }
    }
}

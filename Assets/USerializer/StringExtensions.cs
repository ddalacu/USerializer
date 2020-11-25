
namespace USerialization
{
    public static class StringExtensions
    {
        public static long GetInt64Hash(this string text)
        {
            unchecked
            {
                long hash = 23;
                var textLength = text.Length;
                for (var index = 0; index < textLength; index++)
                    hash = hash * 31 + text[index];

                return hash;
            }
        }

        public static int GetInt32Hash(this string text)
        {
            unchecked
            {
                int hash = 23;
                var textLength = text.Length;
                for (var index = 0; index < textLength; index++)
                    hash = hash * 31 + text[index];

                return hash;
            }
        }
    }
}

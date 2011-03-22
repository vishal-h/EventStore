namespace EventStore.Persistence.AmazonPersistence
{
	using System.Globalization;

	internal static class StringExtensions
	{
		public static string FormatWith(this string format, params object[] values)
		{
			return string.Format(CultureInfo.InvariantCulture, format, values);
		}
	}
}
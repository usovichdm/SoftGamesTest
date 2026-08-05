namespace Common.Networking
{
	public static class NetworkTexts
	{
		public const string UnknownError = "Unknown error";
		public const string UrlEmpty = "URL is empty.";
		public const string EmptyResponseBody = "Response body is empty.";
		public const string NoInternet = "No internet connection.";
		public const string TimedOut = "Request timed out. Please try again.";
		public const string CouldNotReachServer = "Could not reach the server. Please try again.";
		public const string ServerErrorFormat = "Server error ({0}). Please try again.";
		public const string RequestFailedFormat = "Request failed ({0}).";
		public const string NetworkRequestFailed = "Network request failed.";
		public const string NetworkErrorPrefix = "Network error: ";
		public const string MalformedJsonPrefix = "Malformed JSON: ";
		public const string MalformedJsonNull = "Malformed JSON: parser returned null.";
	}
}


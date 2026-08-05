namespace Common.Networking
{
	public readonly struct HttpResult<T>
	{
		public readonly bool Success;
		public readonly T Value;
		public readonly string Error;

		private HttpResult(bool success, T value, string error)
		{
			Success = success;
			Value = value;
			Error = error;
		}

		public static HttpResult<T> Ok(T value)
		{
			return new HttpResult<T>(true, value, null);
		}

		public static HttpResult<T> Fail(string error)
		{
			return new HttpResult<T>(false, default, error ?? NetworkTexts.UnknownError);
		}
	}
}

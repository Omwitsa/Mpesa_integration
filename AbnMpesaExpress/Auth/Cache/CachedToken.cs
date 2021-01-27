using System;

namespace AbnMpesaExpress.Auth.Cache
{
	public class CachedToken
	{
		public string access_token { get; set; }
		public DateTime expires_at { get; set; }
		public bool is_valid { get; set; }
		public string consumer_key { get; set; }
	}
}

using AbnMpesaExpress.DataTransfer;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AbnMpesaExpress.Auth.Cache
{
	public class MemCacheService
	{
		private readonly IMemoryCache _cache;
		private const string _key = "ACCESS_TOKENS";
		public MemCacheService(IMemoryCache cache)
		{
			_cache = cache;
		}

		public CachedToken Store(AccessCredentialResponse response, string consumerKey)
		{
			var validity = int.Parse(response.expires_in) - 10;
			var token = new CachedToken
			{
				access_token = response.access_token,
				expires_at = DateTime.Now.AddSeconds(validity),
				consumer_key = consumerKey
			};
			var tokens = ReadAll();
			var cachedToken = tokens.FirstOrDefault(t => t.consumer_key.Equals(consumerKey));
			if (cachedToken != null)
				tokens.Remove(cachedToken);
			tokens.Add(token);
			_cache.Set(_key, JsonConvert.SerializeObject(tokens), new MemoryCacheEntryOptions
			{
				SlidingExpiration = TimeSpan.FromSeconds(validity)
			});
			token.is_valid = true;
			return token;
		}

		public CachedToken GetByKey(string consumerKey)
		{
			var res = new CachedToken();
			var tokens = ReadAll();
			var token = tokens.FirstOrDefault(t => t.consumer_key.Equals(consumerKey));
			if (token == null)
				return res;
			res.access_token = token.access_token;
			res.expires_at = token.expires_at;
			res.is_valid = token.expires_at >= DateTime.Now;
			return res;
		}

		public List<CachedToken> ReadAll()
		{
			var res = new List<CachedToken>();
			var hasValue = _cache.TryGetValue<string>(_key, out var tokens_data);
			if (!hasValue)
				return res;
			var tokens = JsonConvert.DeserializeObject<List<CachedToken>>(tokens_data);
			return tokens;
		}
	}
}

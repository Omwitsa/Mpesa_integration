using System;
using System.Net;
using System.Threading.Tasks;
using AbnMpesaExpress.Auth.Cache;
using AbnMpesaExpress.DataTransfer;
using AbnMpesaExpress.Log4Net;
using log4net;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace AbnMpesaExpress.Auth
{
	public class AuthService
	{
		private readonly ILog _logger;
		private readonly AbnLogManager _rLog = new AbnLogManager();
		private readonly MemCacheService _cacheService;

		public AuthService(IMemoryCache cache)
		{
			_logger = _rLog.Logger(typeof(AuthService));
			_cacheService = new MemCacheService(cache);
		}


		public async Task<AccessCredentialResponse> GenerateCredential(string consumerKey, string consumerSecret, string authUrl)
		{
			var result = new AccessCredentialResponse();
			try
			{
				_logger.Info($"AUTH: Check token from cache");
				var cacheRes = _cacheService.GetByKey(consumerKey);
				if (cacheRes.is_valid)
				{
					result.access_token = cacheRes.access_token;
					result.IsCompleted = true;
					result.expires_in = $"{(cacheRes.expires_at - DateTime.Now).TotalSeconds}";

					_logger.Info($"AUTH: Found token from cache. Expires in {result.expires_in}");
					return result;
				}

				_logger.Info($"AUTH: Requesting access Url {authUrl} ConsumerKey {consumerKey} and ConsumerSecret {consumerSecret}");
				var client = new RestClient(authUrl)
				{
					Authenticator = new HttpBasicAuthenticator(consumerKey, consumerSecret)
				};
				var request = new RestRequest("generate?grant_type=client_credentials", Method.GET) { RequestFormat = DataFormat.Json };

				var data = await client.ExecuteGetTaskAsync(request);
				var content = data.Content;
				if (string.IsNullOrEmpty(content))
				{
					result.ErrorCode = $"{data.StatusCode}";
					result.ErrorMessage = data.StatusDescription;
				}
				else
					result = JsonConvert.DeserializeObject<AccessCredentialResponse>(content);
				if (!data.StatusCode.Equals(HttpStatusCode.OK))
					_logger.Warn($"AUTH: Request not completed {data.StatusCode} - {data.StatusDescription} {data.Content}");
				else
				{
					result.IsCompleted = true;
					_cacheService.Store(result, consumerKey);
					_logger.Info($"AUTH: Request completed {content}");
				}
				return result;
			}
			catch (Exception e)
			{
				_logger.Error($"AUTH: Error. Request Failed {e.Message}");
				return result;
			}
		}

		public async Task<LipaResponse> StkPush(LipaNaMpesa lipa)
		{
			var result = new LipaResponse();
			try
			{
				_logger.Info($"STK: Requesting STK push payload {JsonConvert.SerializeObject(lipa)}");
				var credential = await GenerateCredential(lipa.ConsumerKey, lipa.ConsumerSecret, lipa.AuthUrl);
				if (credential.IsCompleted)
				{
					var client = new RestClient(lipa.StkUrl);
					client.AddDefaultHeader("Authorization", $"Bearer {credential.access_token}");
					var request = new RestRequest("processrequest", Method.POST) { RequestFormat = DataFormat.Json };
					request.AddJsonBody(lipa);

					var data = await client.ExecutePostTaskAsync(request);
					var content = data.Content;
					result = JsonConvert.DeserializeObject<LipaResponse>(content);
					if (data.StatusCode.Equals(HttpStatusCode.OK))
					{
						result.IsCompleted = true;
						_logger.Info($"STK: Request completed {content}");
						return result;
					}

					_logger.Warn($"STK: Request not completed {data.StatusCode} {data.StatusDescription} {data.Content}");
					return result;
				}

				_logger.Warn($"STK: Request AUTH Failed {JsonConvert.SerializeObject(credential)}");
				return result;
			}
			catch (Exception e)
			{
				_logger.Error($"STK: Error. Request Failed {e.Message}");
				return result;
			}
		}

		public async Task<LipaResponse> StkPaymentStatus(LipaNaMpesa lipa)
		{
			var result = new LipaResponse();
			try
			{
				_logger.Info($"STK_STATUS: Requesting STK payment status payload {JsonConvert.SerializeObject(lipa)}");
				var credential = await GenerateCredential(lipa.ConsumerKey, lipa.ConsumerSecret, lipa.AuthUrl);
				if (credential.IsCompleted)
				{
					var client = new RestClient(lipa.StkUrl);
					client.AddDefaultHeader("Authorization", $"Bearer {credential.access_token}");
					var request = new RestRequest("stkpushquery/v1/query", Method.POST) { RequestFormat = DataFormat.Json };
					request.AddJsonBody(lipa);

					var data = await client.ExecutePostTaskAsync(request);
					var content = data.Content;
					result = JsonConvert.DeserializeObject<LipaResponse>(content);
					if (data.StatusCode.Equals(HttpStatusCode.OK))
					{
						result.IsCompleted = true;
						_logger.Info($"STK_STATUS: Request completed {content}");
						return result;
					}
					_logger.Warn($"STK_STATUS: Request not completed {data.StatusCode} {data.StatusDescription} {data.Content}");
					return result;
				}
				result.ErrorCode = credential.ErrorCode;
				result.ErrorMessage = credential.ErrorMessage;
				_logger.Warn($"STK_STATUS: Request AUTH Failed {JsonConvert.SerializeObject(credential)}");
				return result;
			}
			catch (Exception e)
			{
				_logger.Error($"STK_STATUS: Error. Request Failed {e.Message}");
				return result;
			}
		}

		public async Task<RegisterUrlsResponse> RegisterUrl(RegisterUrlsRequest register)
		{
			var result = new RegisterUrlsResponse();
			try
			{
				_logger.Info($"REG:  Register Urls {JsonConvert.SerializeObject(register)}");

				var credential = await GenerateCredential(register.ConsumerKey, register.ConsumerSecret, register.AuthUrl);
				if (credential.IsCompleted)
				{
					var client = new RestClient(register.DarajaC2BUrl);
					client.AddDefaultHeader("Authorization", $"Bearer {credential.access_token}");
					var request = new RestRequest("registerurl", Method.POST) { RequestFormat = DataFormat.Json };
					request.AddJsonBody(register);

					var data = await client.ExecutePostTaskAsync(request);
					if (data.StatusCode.Equals(HttpStatusCode.OK))
					{
						var content = data.Content;
						result = JsonConvert.DeserializeObject<RegisterUrlsResponse>(content);
						result.IsCompleted = true;

						_logger.Info($"REG: Request completed {content}");
						return result;
					}

					_logger.Warn($"REG: Request not completed {data.StatusCode} {data.Content}");
					return result;
				}

				_logger.Warn($"REG: Request AUTH Failed {JsonConvert.SerializeObject(credential)}");
				return result;
			}
			catch (Exception e)
			{
				_logger.Error($"REG: Error. Request Failed {e.Message}");
				return result;
			}
		}

		public async Task<SimulatePaybillResponse> Simulate(SimulatePayBillRequest simulate)
		{
			var result = new SimulatePaybillResponse();
			try
			{
				_logger.Info($"SIM:  Simulate PayBill payment {JsonConvert.SerializeObject(simulate)}");

				var credential = await GenerateCredential(simulate.ConsumerKey, simulate.ConsumerSecret, simulate.AuthUrl);
				if (credential.IsCompleted)
				{
					var client = new RestClient(simulate.DarajaC2BUrl);
					client.AddDefaultHeader("Authorization", $"Bearer {credential.access_token}");
					var request = new RestRequest("simulate", Method.POST) { RequestFormat = DataFormat.Json };
					request.AddJsonBody(simulate);

					var data = await client.ExecutePostTaskAsync(request);
					if (data.StatusCode.Equals(HttpStatusCode.OK))
					{
						var content = data.Content;
						result = JsonConvert.DeserializeObject<SimulatePaybillResponse>(content);
						result.IsCompleted = true;

						_logger.Info($"SIM: Request completed {content}");
						return result;
					}

					_logger.Warn($"SIM: Request not completed {data.StatusCode} {data.Content}");
					return result;
				}

				_logger.Warn($"SIM: Request AUTH Failed {JsonConvert.SerializeObject(credential)}");
				return result;
			}
			catch (Exception e)
			{
				_logger.Error($"SIM: Error. Request Failed {e.Message}");
				return result;
			}
		}
	}
}

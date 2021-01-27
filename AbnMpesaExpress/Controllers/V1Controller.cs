using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AbnMpesaExpress.Auth;
using AbnMpesaExpress.DataTransfer;
using AbnMpesaExpress.Log4Net;
using AbnMpesaExpress.Soap;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace AbnMpesaExpress.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class V1Controller : Controller
	{
		private readonly AuthService _authService;
		private readonly ILog _logger;
		private readonly AbnLogManager _rLog = new AbnLogManager();
		private readonly string _apiKey;
		private readonly string _baseUrl;
		private readonly string _coreErp;

		public V1Controller(IConfiguration configuration, IMemoryCache cache)
		{
			_authService = new AuthService(cache);
			_logger = _rLog.Logger(typeof(V1Controller));
			_apiKey = configuration["ApiKey"];
			_baseUrl = configuration["ServiceBaseUrl"];
			_coreErp = configuration["CoreErp"];
		}

		[HttpGet]
		public JsonResult Get()
		{
			return Json(new { success = true, message = $"ABN Mpesa Express Service is running {DateTime.Now:F} " });
		}

		[HttpPost("[action]")]
		public async Task<JsonResult> Credentials([FromBody] AuthRequest auth)
		{
			if (!IsAllowedClient(auth.ClientId))
				return Json(new { success = false, message = $"ABNClientID {auth.ClientId} is not Allowed." });

			var data = await _authService.GenerateCredential(auth.ConsumerKey, auth.ConsumerSecret, auth.AuthUrl);
			var msg = data.IsCompleted ? "Completed" : "Failed";
			return Json(new { success = data.IsCompleted, message = msg, data });
		}

		[HttpPost("[action]")]
		public JsonResult Pay([FromBody] LipaRequest request)
		{
			if (!IsAllowedClient(request.ClientId))
				return Json(new { success = false, message = $"ABNClientID {request.ClientId} is not Allowed." });

			request.StkUrl = request.BusinessShortCode.Equals("174379") ?
				"https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/" : "https://api.safaricom.co.ke/mpesa/stkpush/v1/";
			request.AuthUrl = request.BusinessShortCode.Equals("174379") ?
				"https://sandbox.safaricom.co.ke/oauth/v1/" : "https://api.safaricom.co.ke/oauth/v1/";
			request.ConsumerKey = string.IsNullOrEmpty(request.ConsumerKey) ?
				"3oL8waCK2jckmZVnhxUkpFf26yQu4zwl" : request.ConsumerKey;
			request.ConsumerSecret = string.IsNullOrEmpty(request.ConsumerSecret) ?
				"xc9Oh9OyhLjujRnG" : request.ConsumerSecret;
			var lipa = new LipaNaMpesa(request);
			var response = _authService.StkPush(lipa);
			var data = response.Result;
			var msg = data.IsCompleted ? "Completed" : "Failed";
			return Json(new { success = data.IsCompleted, message = msg, data });
		}

		[HttpPost("[action]")]
		public JsonResult PayStatus([FromBody] PayStatusRequest request)
		{
			if (!IsAllowedClient(request.ClientId))
				return Json(new { success = false, message = $"ABNClientID {request.ClientId} is not Allowed." });

			var lipa = new LipaNaMpesa(request);
			var response = _authService.StkPaymentStatus(lipa);
			var data = response.Result;
			var msg = data.IsCompleted ? "Completed" : "Failed";
			return Json(new { success = data.IsCompleted, message = msg, data });
		}

		[HttpPost("[action]")]
		public JsonResult Simulate([FromBody] SimulatePayBillRequest request)
		{
			if (!IsAllowedClient(request.ClientId))
				return Json(new { success = false, message = $"ABNClientID {request.ClientId} is not Allowed." });

			var response = _authService.Simulate(request);
			var data = response.Result;
			var msg = data.IsCompleted ? "Completed" : "Failed";
			return Json(new { success = data.IsCompleted, message = msg, data });
		}

		[HttpPost("[action]")]
		public async Task<JsonResult> Receive()
		{
			var data = new PayResponse();
			try
			{
				string body;
				using (var stream = new StreamReader(HttpContext.Request.Body))
				{
					body = await stream.ReadToEndAsync();
				}

				_logger.Info($"RCV: Received from callback  Payload {body}");
				var stkPayResponse = JsonConvert.DeserializeObject<StkPayResponse>(body);

				var stkCallback = stkPayResponse.Body?.stkCallback;
				if (stkCallback == null)
				{
					return Json(new { success = false, message = "Received data. Callback is Null", data = body });
				}
				data.CheckoutRequestID = stkCallback.CheckoutRequestID;
				data.MerchantRequestID = stkCallback.MerchantRequestID;
				data.ResultCode = stkCallback.ResultCode;
				data.ResultDesc = stkCallback.ResultDesc;
				data.IsCompleted = stkCallback.ResultCode.Equals(0);

				var items = stkCallback.CallbackMetadata?.Item;
				if (items == null)
				{
					_logger.Warn($"RCV: Failed to Process STK callback. No metadata data  {JsonConvert.SerializeObject(data)}");
					return Json(new { success = false, message = "Received data", data });
				}
				for (var i = 0; i < items.Count; i++)
				{
					switch (i)
					{
						case 0:
							data.Amount = items[i]?.Value;
							break;
						case 1:
							data.MpesaReceiptNumber = items[i]?.Value;
							break;
						case 3:
							data.TransactionDate = items[i]?.Value;
							break;
						case 4:
							data.PhoneNumber = items[i]?.Value;
							break;
					}
				}
				_logger.Info($"RCV: Processed STK callback  Data {JsonConvert.SerializeObject(data)}");
				return Json(new { success = true, message = "Received data", data });
			}
			catch (Exception e)
			{
				_logger.Error($"RCV: Error. Processing STK callback  {e.Message}");
				return Json(new { success = false, message = "Failed to process STK data", e.Message });
			}
		}

		[HttpPost("[action]")]
		public JsonResult RegisterUrls([FromBody] RegisterUrlsRequest request)
		{
			if (!IsAllowedClient(request.ClientId))
				return Json(new { success = false, message = $"ABNClientID {request.ClientId} is not Allowed." });

			var response = _authService.RegisterUrl(request);
			var data = response.Result;
			var msg = data.IsCompleted ? "Completed" : "Failed";
			return Json(new { success = data.IsCompleted, message = msg, data });
		}

		[HttpPost("[action]")]
		public async Task<JsonResult> Verify([FromBody] VerificationResponse verifyResponse)
		{
			string msg;
			try
			{
				if (verifyResponse != null)
				{
					_logger.Info($"VER: {verifyResponse.TransID} Amount: {verifyResponse.TransAmount} Account: {verifyResponse.BillRefNumber} Phone: {verifyResponse.MSISDN}");
					var hasUserName = Request.Headers.TryGetValue("Username", out var userName);
					var hasPassword = Request.Headers.TryGetValue("Password", out var password);
					if (!hasPassword || !hasUserName)
					{
						msg = "Username and Password is Required";
						return Json(new DarajaResponse { ResultCode = "1", ResultDesc = msg });
					}
					var soap = new SoapService(userName, password, _baseUrl);
					if (_coreErp.ToLower().Equals("unisol"))
					{
						var studentRes = await soap.IsBonafideStudent(verifyResponse.BillRefNumber);
						var sCode = studentRes.Success ? 0 : 1;
						msg = studentRes.Message;
						if (studentRes.Success)
							msg += $" :: {studentRes.Data.StrStudName}";
						_logger.Info($"VER: {msg}");
						return Json(new DarajaResponse { ResultCode = $"{sCode}", ResultDesc = msg });
					}
					var customerRes = await soap.IsValidCustomer(verifyResponse.BillRefNumber);
					var cCode = customerRes.Success ? 0 : 1;
					msg = customerRes.Message;
					if (customerRes.Success)
						msg += $" :: {customerRes.Data.StrCustName}";
					_logger.Info($"CONF: {msg}");
					return Json(new DarajaResponse { ResultCode = $"{cCode}", ResultDesc = msg });
				}
				msg = "Payload is Empty";
				_logger.Info($"VER: {msg}");
				return Json(new DarajaResponse { ResultCode = "1", ResultDesc = msg });
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				_logger.Error($"VER: Failed to verify request :- {e.Message}.");
				return Json(new DarajaResponse { ResultCode = "1", ResultDesc = "Error occured. Try later" });
			}
		}

		[HttpPost("[action]")]
		public async Task<JsonResult> Confirm([FromBody] VerificationResponse confirmResponse)
		{
			string msg;
			try
			{
				if (confirmResponse != null)
				{
					_logger.Info($"CONF: {confirmResponse.TransID} Amount: {confirmResponse.TransAmount} Account: {confirmResponse.BillRefNumber} Phone: {confirmResponse.MSISDN}");
					var hasUserName = Request.Headers.TryGetValue("Username", out var userName);
					var hasPassword = Request.Headers.TryGetValue("Password", out var password);
					if (!hasPassword || !hasUserName)
					{
						msg = "Username and Password is Required";
						_logger.Info($"CONF: {msg}");
						return Json(new DarajaResponse { ResultCode = "1", ResultDesc = msg });
					}
					var soap = new SoapService(userName, password, _baseUrl);
					if (_coreErp.ToLower().Equals("unisol"))
					{
						var studentRes = await soap.ProcessStudentPayment(confirmResponse.BillRefNumber, confirmResponse.TransAmount, confirmResponse.TransID);
						var sCode = studentRes.Success ? 0 : 1;
						msg = studentRes.Message;
						if (studentRes.Success)
							msg += $" :: {studentRes.Data.StrRcptNo}";
						_logger.Info($"CONF: {msg}");
						return Json(new DarajaResponse { ResultCode = $"{sCode}", ResultDesc = msg });
					}
					var customerRes = await soap.ProcessCustomerPayment(confirmResponse.BillRefNumber, confirmResponse.TransAmount, confirmResponse.TransID);
					var cCode = customerRes.Success ? 0 : 1;
					msg = customerRes.Message;
					if (customerRes.Success)
						msg += $" :: {customerRes.Data.StrRcptNo}";
					_logger.Info($"CONF: {msg}");
					return Json(new DarajaResponse { ResultCode = $"{cCode}", ResultDesc = msg });
				}
				msg = "Payload is Empty";
				_logger.Info($"CONF: {msg}");
				return Json(new DarajaResponse { ResultCode = "1", ResultDesc = msg });
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				msg = "Error Occurred. Try later";
				_logger.Error($"CONF: Failed to Confirm request : {msg}");
				return Json(new DarajaResponse { ResultCode = "1", ResultDesc = msg });
			}
		}

		private static async Task Forward(string url, PayResponse data)
		{
			var client = new RestClient(url);
			var request = new RestRequest("", Method.POST) { RequestFormat = DataFormat.Json };
			request.AddJsonBody(data);
			await client.ExecutePostTaskAsync(request);
		}

		private bool IsAllowedClient(string clientId)
		{
			return _apiKey.Equals(clientId);
		}
	}
}

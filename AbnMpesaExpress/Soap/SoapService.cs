using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
namespace AbnMpesaExpress.Soap
{
	public class SoapService
	{
		private readonly TimeSpan _timeout = TimeSpan.FromMinutes(6);
		private readonly GenerateXml _generateXml;
		public string _apiUrl;

		public SoapService(string userName, string password, string apiUrl)
		{
			_generateXml = new GenerateXml(userName, password);
			_apiUrl = apiUrl;
		}

		private async Task<BaseSoapResponse<XDocument>> SendAsync(string soapRequest, string action)
		{
			var res = new BaseSoapResponse<XDocument>();
			try
			{
				using (var client = new HttpClient(new HttpClientHandler
				{ AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip })
				{ Timeout = _timeout })
				{
					var request = new HttpRequestMessage
					{
						RequestUri = new Uri(_apiUrl),
						Method = HttpMethod.Post,
						Content = new StringContent(soapRequest, Encoding.UTF8, "text/xml")
					};
					request.Headers.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
					request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
					request.Headers.Add("SOAPAction", $"{_generateXml.myns}/{action}");

					var response = await client.SendAsync(request);
					if (!response.IsSuccessStatusCode)
					{
						res.Message = $"FAILED : Remote server exception- {response.StatusCode}";
						return res;
					}

					var streamTask = response.Content.ReadAsStreamAsync();
					var stream = streamTask.Result;
					var sr = new StreamReader(stream);
					var soapResponse = XDocument.Load(sr);

					res.Success = true;
					res.Message = "OK. Executed";
					res.Data = soapResponse;
					return res;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				res.Message = "ERROR. Server Error Occured";
				res.ErrorMessage = ex.Message;
				return res;
			}
		}

		public async Task<BaseSoapResponse<IsValidCustomerResult>> IsValidCustomer(string custRef)
		{
			var res = new BaseSoapResponse<IsValidCustomerResult>();
			try
			{
				var paymentRequest = _generateXml.IsValidCustomerRequest(custRef);
				var response = await SendAsync(paymentRequest.ToString(), "IsValidCustomer");
				if (!response.Success)
				{
					res.Message = response.Message;
					res.ErrorMessage = response.ErrorMessage;
					return res;
				}
				var xml = response.Data.Descendants(_generateXml.myns + "IsValidCustomerResult").FirstOrDefault()?.ToString();
				var isValidCustomerResult = Deserialize<IsValidCustomerResult>(xml);
				res.Success = isValidCustomerResult.StrStatus.Equals("OK");
				res.Data = isValidCustomerResult;
				res.Message = isValidCustomerResult.StrMsg;
				return res;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				res.ErrorMessage = e.Message;
				res.Message = "Error occured";
				return res;
			}
		}

		public async Task<BaseSoapResponse<ProcessCustomerPaymentResult>> ProcessCustomerPayment(string custRef, double amount, string transNo)
		{
			var res = new BaseSoapResponse<ProcessCustomerPaymentResult>();
			try
			{
				var paymentRequest = _generateXml.ProcessCustomerPaymentRequest(custRef, amount, transNo);
				var response = await SendAsync(paymentRequest.ToString(), "ProcessPayment");
				if (!response.Success)
				{
					res.Message = response.Message;
					res.ErrorMessage = response.ErrorMessage;
					return res;
				}
				var xml = response.Data.Descendants(_generateXml.myns + "ProcessPaymentResult").FirstOrDefault()?.ToString();
				var paymentResult = Deserialize<ProcessCustomerPaymentResult>(xml);
				res.Success = paymentResult.StrStatus.Equals("OK");
				res.Data = paymentResult;
				res.Message = paymentResult.StrMsg;
				return res;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				res.ErrorMessage = e.Message;
				res.Message = "Error occured";
				return res;
			}
		}

		public async Task<BaseSoapResponse<IsBonafideStudentResult>> IsBonafideStudent(string regNo)
		{
			var res = new BaseSoapResponse<IsBonafideStudentResult>();
			try
			{
				var isBonfideRequest = _generateXml.IsThisBonaFideStudentRequest(regNo);
				var response = await SendAsync(isBonfideRequest.ToString(), "IsThisBonaFideStudent");
				if (!response.Success)
				{
					res.Message = response.Message;
					res.ErrorMessage = response.ErrorMessage;
					return res;
				}
				var xml = response.Data.Descendants(_generateXml.myns + "IsThisBonaFideStudentResult").FirstOrDefault()?.ToString();
				var isBonafideStudentResult = Deserialize<IsBonafideStudentResult>(xml);
				res.Success = isBonafideStudentResult.StrStatus.Equals("OK");
				res.Data = isBonafideStudentResult;
				res.Message = isBonafideStudentResult.StrMsg;
				return res;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				res.ErrorMessage = e.Message;
				res.Message = "Error occured";
				return res;
			}
		}

		public async Task<BaseSoapResponse<ProcessStudentFeesResult>> ProcessStudentPayment(string regNo, double amount, string transNo)
		{
			var res = new BaseSoapResponse<ProcessStudentFeesResult>();
			try
			{
				var paymentRequest = _generateXml.ProcessStudentPaymentRequest(regNo, amount, transNo);
				var response = await SendAsync(paymentRequest.ToString(), "ProcessStudentFees");
				if (!response.Success)
				{
					res.Message = response.Message;
					res.ErrorMessage = response.ErrorMessage;
					return res;
				}
				var xml = response.Data.Descendants(_generateXml.myns + "ProcessStudentFeesResult").FirstOrDefault()?.ToString();
				var paymentResult = Deserialize<ProcessStudentFeesResult>(xml);
				res.Success = paymentResult.StrStatus.Equals("OK");
				res.Data = paymentResult;
				res.Message = paymentResult.StrMsg;
				return res;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				res.ErrorMessage = e.Message;
				res.Message = "Error occured";
				return res;
			}
		}


		public static T Deserialize<T>(string xmlStr)
		{
			var serializer = new XmlSerializer(typeof(T));
			T result;
			using (TextReader reader = new StringReader(xmlStr))
			{
				result = (T)serializer.Deserialize(reader);
			}
			return result;
		}
	}
}



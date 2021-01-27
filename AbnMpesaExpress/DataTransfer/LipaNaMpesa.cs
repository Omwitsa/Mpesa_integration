using System;
using System.Text;

namespace AbnMpesaExpress.DataTransfer
{
	public class LipaNaMpesa : AuthRequest
	{
		public LipaNaMpesa(LipaRequest request)
		{
			Timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			TransactionType = "CustomerPayBillOnline";
			PassKey = request.PassKey;
			PartyA = request.PhoneNumber;
			PartyB = request.BusinessShortCode;
			Amount = request.Amount;
			BusinessShortCode = request.BusinessShortCode;
			PhoneNumber = request.PhoneNumber;
			CallBackURL = request.CallBackURL;
			AccountReference = request.AccountReference;
			TransactionDesc = request.TransactionDesc;
			Password = GeneratePassword();
			AuthUrl = request.AuthUrl;
			StkUrl = request.StkUrl;
			ConsumerKey = request.ConsumerKey;
			ConsumerSecret = request.ConsumerSecret;
			ClientId = request.ClientId;
		}

		public LipaNaMpesa(PayStatusRequest request)
		{
			Timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			PassKey = request.PassKey;
			BusinessShortCode = request.BusinessShortCode;
			Password = GeneratePassword();
			CheckoutRequestID = request.CheckoutRequestID;
			AuthUrl = request.AuthUrl;
			StkUrl = request.StkUrl;
			ConsumerKey = request.ConsumerKey;
			ConsumerSecret = request.ConsumerSecret;
			ClientId = request.ClientId;
		}

		public string BusinessShortCode { get; set; }
		public string Amount { get; set; }
		public string Password { get; set; }
		public string Timestamp { get; set; }
		public string TransactionType { get; set; }
		public string PartyA { get; set; }
		public string PartyB { get; set; }
		public string PhoneNumber { get; set; }
		public string CallBackURL { get; set; }
		public string AccountReference { get; set; }
		public string TransactionDesc { get; set; }
		public string PassKey { get; set; }
		public string CheckoutRequestID { get; set; }

		private string GeneratePassword()
		{
			var pass = $"{BusinessShortCode}{PassKey}{Timestamp}";
			var bytes = Encoding.UTF8.GetBytes(pass);
			return Convert.ToBase64String(bytes);
		}
	}
}

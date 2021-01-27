namespace AbnMpesaExpress.DataTransfer
{
	public class PayStatusRequest : AuthRequest
	{
		public string BusinessShortCode { get; set; }
		public string PassKey { get; set; }
		public string CheckoutRequestID { get; set; }
	}
}

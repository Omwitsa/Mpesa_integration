namespace AbnMpesaExpress.DataTransfer
{
	public class LipaResponse : BaseErrorReponse
	{
		public LipaResponse()
		{
			IsCompleted = false;
		}
		public bool IsCompleted { get; set; }
		public string MerchantRequestID { get; set; }
		public string CheckoutRequestID { get; set; }
		public string ResponseCode { get; set; }
		public string ResponseDescription { get; set; }
		public string CustomerMessage { get; set; }
	}
}

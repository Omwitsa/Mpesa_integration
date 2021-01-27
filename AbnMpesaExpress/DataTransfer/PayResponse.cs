namespace AbnMpesaExpress.DataTransfer
{
	public class PayResponse : BaseErrorReponse
	{
		public bool IsCompleted { get; set; }
		public string MerchantRequestID { get; set; }
		public string CheckoutRequestID { get; set; }
		public int ResultCode { get; set; }
		public string ResultDesc { get; set; }
		public string Amount { get; set; }
		public string Balance { get; set; }
		public string MpesaReceiptNumber { get; set; }
		public string TransactionDate { get; set; }
		public string PhoneNumber { get; set; }
	}
}

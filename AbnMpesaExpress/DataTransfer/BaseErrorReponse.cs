namespace AbnMpesaExpress.DataTransfer
{
	public abstract class BaseErrorReponse
	{
		public string RequestId { get; set; }
		public string ErrorCode { get; set; }
		public string ErrorMessage { get; set; }
	}
}

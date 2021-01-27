namespace AbnMpesaExpress.DataTransfer
{
	public class AccessCredentialResponse : BaseErrorReponse
	{
		public AccessCredentialResponse()
		{
			IsCompleted = false;
		}
		public bool IsCompleted { get; set; }
		public string access_token { get; set; }
		public string expires_in { get; set; }
	}
}

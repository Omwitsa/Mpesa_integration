using System;

namespace AbnMpesaExpress.Soap
{
	public class BaseSoapResponse<T>
	{
		public BaseSoapResponse()
		{
			Success = false;
			TimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
		}
		public bool Success { get; set; }
		public string Message { get; set; }
		public T Data { get; set; }
		public string TimeStamp { get; set; }
		public string ErrorMessage { get; set; }
	}
}

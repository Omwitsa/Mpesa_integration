namespace AbnMpesaExpress.DataTransfer
{
    public class RegisterUrlsRequest : AuthRequest
    {
        public RegisterUrlsRequest()
        {
            ResponseType = DefaultResponseType.Completed.ToString();
        }
        public string ShortCode { get; set; }
        public string ValidationURL { get; set; }
        public string ConfirmationURL { get; set; }
        public string ResponseType { get; set; }
    }

    public enum DefaultResponseType
    {
        Completed,
        Canceled
    }
}

namespace AbnMpesaExpress.DataTransfer
{
    public class LipaRequest : AuthRequest
    {
        public string PhoneNumber { get; set; }
        public string CallBackURL { get; set; }
        public string AccountReference { get; set; }
        public string TransactionDesc { get; set; }
        public string PassKey { get; set; }
        public string BusinessShortCode { get; set; }
        public string Amount { get; set; }
    }
}

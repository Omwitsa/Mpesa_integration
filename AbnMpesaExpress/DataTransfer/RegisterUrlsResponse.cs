namespace AbnMpesaExpress.DataTransfer
{
    public class RegisterUrlsResponse
    {
        public RegisterUrlsResponse()
        {
            IsCompleted = false;
        }
        public bool IsCompleted { get; set; }
        public string ConversationID { get; set; }
        public string OriginatorConversationID { get; set; }
        public string ResponseDescription { get; set; }
    }
}

namespace AbnMpesaExpress.DataTransfer
{
    public class SimulatePaybillResponse
    {
        public bool IsCompleted { get; set; }
        public string ConversationID { get; set; }
        public string OriginatorCoversationID { get; set; }
        public string ResponseDescription { get; set; }
    }
}

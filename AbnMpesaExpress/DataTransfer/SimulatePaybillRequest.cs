namespace AbnMpesaExpress.DataTransfer
{
    public class SimulatePayBillRequest : AuthRequest
    {
        public SimulatePayBillRequest()
        {
            CommandID = "CustomerPayBillOnline";
        }
        public string ShortCode { get; set; }
        public string CommandID { get; set; }
        public string Amount { get; set; }
        public string Msisdn { get; set; }
        public string BillRefNumber { get; set; }
    }
}

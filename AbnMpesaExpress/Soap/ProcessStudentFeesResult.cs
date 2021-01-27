using System.Xml.Serialization;

namespace AbnMpesaExpress.Soap
{
	[XmlRoot(ElementName = "ProcessStudentFeesResult", Namespace = "unisol:bankapi")]
	public class ProcessStudentFeesResult
	{
		[XmlElement(ElementName = "strStatus", Namespace = "unisol:bankapi")]
		public string StrStatus { get; set; }
		[XmlElement(ElementName = "strMsg", Namespace = "unisol:bankapi")]
		public string StrMsg { get; set; }
		[XmlElement(ElementName = "strRegNo", Namespace = "unisol:bankapi")]
		public string StrRegNo { get; set; }
		[XmlElement(ElementName = "strTransNo", Namespace = "unisol:bankapi")]
		public string StrTransNo { get; set; }
		[XmlElement(ElementName = "strUN", Namespace = "unisol:bankapi")]
		public string StrUN { get; set; }
		[XmlElement(ElementName = "strRcptNo", Namespace = "unisol:bankapi")]
		public string StrRcptNo { get; set; }
	}
}

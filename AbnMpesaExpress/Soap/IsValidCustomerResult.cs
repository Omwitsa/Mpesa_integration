using System.Xml.Serialization;
namespace AbnMpesaExpress.Soap
{
	[XmlRoot(ElementName = "IsValidCustomerResult", Namespace = "unisol:bankapi")]
	public class IsValidCustomerResult
	{

		[XmlElement(ElementName = "strStatus", Namespace = "unisol:bankapi")]
		public string StrStatus { get; set; }
		[XmlElement(ElementName = "strMsg", Namespace = "unisol:bankapi")]
		public string StrMsg { get; set; }
		[XmlElement(ElementName = "strCustRef", Namespace = "unisol:bankapi")]
		public string StrCustRef { get; set; }
		[XmlElement(ElementName = "strCustName", Namespace = "unisol:bankapi")]
		public string StrCustName { get; set; }
		[XmlElement(ElementName = "strUN", Namespace = "unisol:bankapi")]
		public string StrUN { get; set; }
	}
}

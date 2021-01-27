using System.Xml.Serialization;

namespace AbnMpesaExpress.Soap
{
	[XmlRoot(ElementName = "IsThisBonaFideStudentResult", Namespace = "unisol:bankapi")]
	public class IsBonafideStudentResult
	{
		[XmlElement(ElementName = "strStatus", Namespace = "unisol:bankapi")]
		public string StrStatus { get; set; }
		[XmlElement(ElementName = "strMsg", Namespace = "unisol:bankapi")]
		public string StrMsg { get; set; }
		[XmlElement(ElementName = "strRegNo ", Namespace = "unisol:bankapi")]
		public string StrRegNo { get; set; }
		[XmlElement(ElementName = "strStudName", Namespace = "unisol:bankapi")]
		public string StrStudName { get; set; }
		[XmlElement(ElementName = "strUN", Namespace = "unisol:bankapi")]
		public string StrUN { get; set; }
	}
}

using System;
using System.Xml.Linq;

namespace AbnMpesaExpress.Soap
{
	public class GenerateXml
	{
		private readonly XNamespace ns = "http://schemas.xmlsoap.org/soap/envelope/";
		public readonly XNamespace myns = "unisol:bankapi";
		private readonly XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
		private readonly XNamespace xsd = "http://www.w3.org/2001/XMLSchema";

		private readonly string _userName;
		private readonly string _password;

		public GenerateXml(string userName, string password)
		{
			_userName = userName;
			_password = password;
		}
		public XDocument IsValidCustomerRequest(string custRef)
		{
			var soapRequest = new XDocument(
				new XDeclaration("1.0", "UTF-8", "no"),
				new XElement(ns + "Envelope",
					new XAttribute(XNamespace.Xmlns + "xsi", xsi),
					new XAttribute(XNamespace.Xmlns + "xsd", xsd),
					new XAttribute(XNamespace.Xmlns + "soap", ns),
					new XElement(ns + "Body",
						new XElement(myns + "IsValidCustomer",
								new XElement(myns + "strUN", _userName),
								new XElement(myns + "strPWD", _password),
								new XElement(myns + "strCustRef", custRef)
						)
					)
				));
			return soapRequest;
		}

		public XDocument IsThisBonaFideStudentRequest(string regNo)
		{
			var soapRequest = new XDocument(
				new XDeclaration("1.0", "UTF-8", "no"),
				new XElement(ns + "Envelope",
					new XAttribute(XNamespace.Xmlns + "xsi", xsi),
					new XAttribute(XNamespace.Xmlns + "xsd", xsd),
					new XAttribute(XNamespace.Xmlns + "soap", ns),
					new XElement(ns + "Body",
						new XElement(myns + "IsThisBonaFideStudent",
								new XElement(myns + "strUN", _userName),
								new XElement(myns + "strPWD", _password),
								new XElement(myns + "strRegNo", regNo)
						)
					)
				));
			return soapRequest;
		}

		public XDocument ProcessCustomerPaymentRequest(string custRef, double amount, string strTransNo)
		{
			var sDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
			var soapRequest = new XDocument(
				new XDeclaration("1.0", "UTF-8", "no"),
				new XElement(ns + "Envelope",
					new XAttribute(XNamespace.Xmlns + "xsi", xsi),
					new XAttribute(XNamespace.Xmlns + "xsd", xsd),
					new XAttribute(XNamespace.Xmlns + "soap", ns),
					new XElement(ns + "Body",
						new XElement(myns + "ProcessPayment",
							new XElement(myns + "strUN", _userName),
							new XElement(myns + "strPWD", _password),
							new XElement(myns + "strCustRef", custRef),
							new XElement(myns + "dblAmount", amount),
							new XElement(myns + "strTransNo", strTransNo),
							new XElement(myns + "dtTransDate", sDate)
						)
					)
				));
			return soapRequest;
		}

		public XDocument ProcessStudentPaymentRequest(string regNo, double amount, string strTransNo)
		{
			var sDate = DateTime.Now.ToString("yyyy-MM-dd");
			var soapRequest = new XDocument(
				new XDeclaration("1.0", "UTF-8", "no"),
				new XElement(ns + "Envelope",
					new XAttribute(XNamespace.Xmlns + "xsi", xsi),
					new XAttribute(XNamespace.Xmlns + "xsd", xsd),
					new XAttribute(XNamespace.Xmlns + "soap", ns),
					new XElement(ns + "Body",
						new XElement(myns + "ProcessStudentFees",
							new XElement(myns + "strUN", _userName),
							new XElement(myns + "strPWD", _password),
							new XElement(myns + "strRegNo", regNo),
							new XElement(myns + "dblAmount", amount),
							new XElement(myns + "strTransNo", strTransNo),
							new XElement(myns + "dtTransDate", sDate)
						)
					)
				));
			return soapRequest;
		}

		public XDocument CustomerStatementRequest(string custRef, string currency, DateTime date)
		{
			var rDate = date.ToString("yyyy-MM-dd");
			var soapRequest = new XDocument(
				new XDeclaration("1.0", "UTF-8", "no"),
				new XElement(ns + "Envelope",
					new XAttribute(XNamespace.Xmlns + "xsi", xsi),
					new XAttribute(XNamespace.Xmlns + "xsd", xsd),
					new XAttribute(XNamespace.Xmlns + "soap", ns),
					new XElement(ns + "Body",
						new XElement(myns + "GetCustomerStatement",
							new XElement(myns + "strUN", _userName),
							new XElement(myns + "strPWD", _password),
							new XElement(myns + "strCustRef", custRef),
							new XElement(myns + "strCurr", currency),
							new XElement(myns + "dtDueDate", rDate)
						)
					)
				));
			return soapRequest;
		}
	}
}
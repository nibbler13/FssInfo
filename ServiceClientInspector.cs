using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FssInfo {
	class ServiceClientInspector : IClientMessageInspector {
		public void AfterReceiveReply(ref Message reply, object correlationState) {
			string messageRef = MessageString(ref reply);
			string response = CryptoTools.DecryptXml(messageRef, WsdlServiceHandle.userCert).Replace("ns1:", "").Replace(":ns1", "");

			if (response.Contains("getLNListByDateResponse"))
				WsdlServiceHandle.Instance.ParseGetLNListByDateResponse(response);
			else if (response.Contains("getLNDataResponse"))
				WsdlServiceHandle.Instance.ParseGetLnDataResponse(response);

			reply = CreateMessageFromString(response, reply.Version);
		}

		public object BeforeSendRequest(ref Message request, IClientChannel channel) {
			string messageRef = MessageString(ref request);
			XmlDocument docRequest = new XmlDocument();
			docRequest.LoadXml(messageRef);

			WsdlServiceHandle.RequestType type;
			DateTime? dateTime = null;
			string lnNum = string.Empty;
			string lnSnils = string.Empty;

			if (messageRef.Contains("getLNListByDate")) {
				type = WsdlServiceHandle.RequestType.GetLnListByDate;

				XmlNodeList nodeDate = docRequest.GetElementsByTagName("date");
				if (nodeDate != null && nodeDate.Count == 1)
					dateTime = DateTime.Parse(nodeDate[0].InnerText);

			} else if (messageRef.Contains("getLNData")) {
				type = WsdlServiceHandle.RequestType.GetLnData;

				XmlNodeList nodeLnCode = docRequest.GetElementsByTagName("lnCode");
				if (nodeLnCode != null && nodeLnCode.Count == 1)
					lnNum = nodeLnCode[0].InnerText;

				XmlNodeList nodeLnSnils = docRequest.GetElementsByTagName("snils");
				if (nodeLnSnils != null && nodeLnCode.Count == 1)
					lnSnils = nodeLnSnils[0].InnerText;

			} else
				throw new Exception("Неподдерживаемый тип запроса");

			XmlDocument docCustomSigned = WsdlServiceHandle.GetSignedRequestXml(type, dateTime, lnNum, lnSnils);
			XmlDocument docCustomEncrypted = 
				CryptoTools.EncryptionXML(docCustomSigned, WsdlServiceHandle.fssCert, WsdlServiceHandle.userCert);

			request = CreateMessageFromString(docCustomEncrypted.OuterXml, request.Version);




			//var _url = "https://docs.fss.ru/WSLnCryptoV11/FileOperationsLnPort?WSDL";
			//var _action = "http://ru/ibs/fss/ln/ws/FileOperationsLn.wsdl/getLNListByDate";

			////XmlDocument soapEnvelopeXml = CreateSoapEnvelope();
			//HttpWebRequest webRequest = CreateWebRequest(_url, _action);
			//InsertSoapEnvelopeIntoWebRequest(docCustomEncrypted, webRequest);

			//// begin async call to web request.
			//IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

			//// suspend this thread until call is complete. You might want to
			//// do something usefull here like update your UI.
			//asyncResult.AsyncWaitHandle.WaitOne();

			//// get the response from the completed web request.
			//string soapResult;
			//using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult)) {
			//	using (StreamReader rd = new StreamReader(webResponse.GetResponseStream())) {
			//		soapResult = rd.ReadToEnd();
			//	}
			//	Console.Write(soapResult);
			//}




			return request;
		}



		private static HttpWebRequest CreateWebRequest(string url, string action) {
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
			webRequest.Headers.Add("SOAPAction", action);
			webRequest.ContentType = "text/xml;charset=\"utf-8\"";
			webRequest.Accept = "text/xml";
			webRequest.Method = "POST";
			return webRequest;
		}

		private static XmlDocument CreateSoapEnvelope() {
			XmlDocument soapEnvelopeDocument = new XmlDocument();
			soapEnvelopeDocument.LoadXml(@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/1999/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/1999/XMLSchema""><SOAP-ENV:Body><HelloWorld xmlns=""http://tempuri.org/"" SOAP-ENV:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/""><int1 xsi:type=""xsd:integer"">12</int1><int2 xsi:type=""xsd:integer"">32</int2></HelloWorld></SOAP-ENV:Body></SOAP-ENV:Envelope>");
			return soapEnvelopeDocument;
		}

		private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest) {
			using (Stream stream = webRequest.GetRequestStream()) {
				soapEnvelopeXml.Save(stream);
			}
		}






		static string MessageString(ref Message m) {
			MessageBuffer mb = m.CreateBufferedCopy(int.MaxValue);
			m = mb.CreateMessage();
			MemoryStream s = new MemoryStream();
			XmlWriter xw = XmlWriter.Create(s);
			m.WriteMessage(xw);
			xw.Flush();
			s.Position = 0;

			var bXml = new byte[s.Length];
			s.Read(bXml, 0, (int)s.Length);

			return bXml[0] != (byte)'<'
				? Encoding.UTF8.GetString(bXml, 3, bXml.Length - 3)
				: Encoding.UTF8.GetString(bXml, 0, bXml.Length);
		}

		static Message CreateMessageFromString(string xml, MessageVersion ver) {
			return Message.CreateMessage(XmlReaderFromString(xml), int.MaxValue, ver);
		}

		static XmlReader XmlReaderFromString(string stringValue) {
			return XmlReader.Create(new StringReader(stringValue));
		}
	}
}

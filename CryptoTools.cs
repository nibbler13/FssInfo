using CryptoPro.Sharpei;
using CryptoPro.Sharpei.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using GostCryptography;
using GostCryptography.Xml;

namespace FssInfo {
	class CryptoTools {
		public static XmlElement GenerateSecurity(XmlDocument document,
											X509Certificate2 certificate,
											string prefix,
											string id,
											string iteration = "") {
			XmlNodeList nodeList = null;
			XmlElement elemSecurity = null;
			FSSSignedXml signer = new FSSSignedXml(document);
			signer.SigningKey = certificate.PrivateKey;

			KeyInfo keyInfo = new KeyInfo();
			XmlElement keySecurityTokenReference =
				document.CreateElement("wsse", "SecurityTokenReference", WsdlServiceHandle.xmlns_wsse);
			XmlElement keyReference = document.CreateElement("wsse", "Reference", WsdlServiceHandle.xmlns_wsse);
			keyReference.SetAttribute("URI", $"#http://eln.fss.ru/actor/mo/{WsdlServiceHandle.ogrn}");
			keySecurityTokenReference.AppendChild(keyReference);
			KeyInfoNode keyInfoData = new KeyInfoNode(keySecurityTokenReference);
			keyInfo.AddClause(keyInfoData);
			signer.KeyInfo = keyInfo;

			Reference reference =
				new Reference($"#{prefix}_{id}{(String.IsNullOrEmpty(iteration) ? "" : $"_{iteration}")}");
			reference.DigestMethod = "urn:ietf:params:xml:ns:cpxmlsec:algorithms:gostr34112012-256"; //new!
			//reference.DigestMethod = "http://www.w3.org/2001/04/xmldsig-more#gostr3411";
			reference.AddTransform(new XmlDsigC14NTransform());
			signer.AddReference(reference);
			signer.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
#pragma warning disable CS0612
			signer.SignedInfo.SignatureMethod = CPSignedXml.XmlDsigGost3410_2012_256Url;
			//signer.SignedInfo.SignatureMethod = CPSignedXml.XmlDsigGost3410UrlObsolete;
#pragma warning restore CS0612
			signer.ComputeSignature();

			XmlElement xmlDigitalSignature = signer.GetXml();
			nodeList = document.GetElementsByTagName("Header", WsdlServiceHandle.xmlns_soapenv);
			if (nodeList != null && nodeList.Count == 1) {
				elemSecurity = document.CreateElement("wsse", "Security", WsdlServiceHandle.xmlns_wsse);
				elemSecurity.SetAttribute(
					"actor", WsdlServiceHandle.xmlns_soapenv, $"http://eln.fss.ru/actor/mo/{WsdlServiceHandle.ogrn}");
				elemSecurity.SetAttribute("xmlns:wsu", WsdlServiceHandle.xmlns_wsu);
				elemSecurity.SetAttribute("xmlns:ds", WsdlServiceHandle.xmlns_ds);
				nodeList[0].AppendChild(elemSecurity);
			}

			XmlNode nodeSecurity = (XmlNode)elemSecurity;
			nodeSecurity.AppendChild(xmlDigitalSignature);
			XmlElement elemBinarySecurityToken =
				document.CreateElement("wsse", "BinarySecurityToken", WsdlServiceHandle.xmlns_wsse);
			elemBinarySecurityToken.SetAttribute(
				"EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
			elemBinarySecurityToken.SetAttribute(
				"ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
			elemBinarySecurityToken.SetAttribute(
				"Id", WsdlServiceHandle.xmlns_wsu, $"http://eln.fss.ru/actor/mo/{WsdlServiceHandle.ogrn}");
			elemBinarySecurityToken.InnerText = Convert.ToBase64String(certificate.Export(X509ContentType.Cert));
			nodeSecurity.AppendChild(elemBinarySecurityToken);

			return elemSecurity;
		}

		public static XmlDocument EncryptionXML(XmlDocument document,
										  X509Certificate2 certificateEncryption,
										  X509Certificate2 certificateOpen) {
			XmlNode elementBody = document.GetElementsByTagName("Envelope", WsdlServiceHandle.xmlns_soapenv)[0];

			XmlDocument doc = new XmlDocument();
			XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
			ns.AddNamespace("soapenv", WsdlServiceHandle.xmlns_soapenv);
			ns.AddNamespace("xenc", WsdlServiceHandle.xmlns_xenc);
			ns.AddNamespace("ds", WsdlServiceHandle.xmlns_ds);
			ns.AddNamespace("sch", WsdlServiceHandle.xmlns_sch);
			ns.AddNamespace("wsse", WsdlServiceHandle.xmlns_wsse);
			ns.AddNamespace("wsu", WsdlServiceHandle.xmlns_wsu);

			MemoryStream newRequestStream = new MemoryStream();
			XmlWriter writer = XmlWriter.Create(
				newRequestStream, new XmlWriterSettings { Encoding = Encoding.UTF8 });
			writer.WriteStartDocument();
			writer.WriteStartElement("soapenv", "Envelope", WsdlServiceHandle.xmlns_soapenv);
			writer.WriteStartElement("soapenv", "Header", WsdlServiceHandle.xmlns_soapenv);
			writer.WriteEndElement();
			writer.WriteStartElement("soapenv", "Body", WsdlServiceHandle.xmlns_soapenv);
			writer.WriteRaw(elementBody.OuterXml);
			writer.WriteEndElement();
			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();

			string xmlText = Encoding.GetEncoding("UTF-8").GetString(newRequestStream.ToArray());
			XmlDocument xml = new XmlDocument();
			newRequestStream.Position = 0;
			xml.Load(newRequestStream);
			writer.Close();

			XmlElement elementToEncrypt =
				xml.GetElementsByTagName("Envelope", WsdlServiceHandle.xmlns_soapenv)[1] as XmlElement;
			EncryptedData edElement = new EncryptedData();
			edElement.Type = EncryptedXml.XmlEncElementUrl;
			edElement.EncryptionMethod = new EncryptionMethod(GostEncryptedXml.XmlEncGostNamespaceUrl + "gost28147"); //was XmlEncGost28147Url
			edElement.KeyInfo = new KeyInfo();

			using (Gost28147CryptoServiceProvider sessionKey = new Gost28147CryptoServiceProvider()) {
				sessionKey.CipherOid = "1.2.643.7.1.2.5.1.1"; //new

				EncryptedXml eXml = new EncryptedXml();
				byte[] encryptedElement = eXml.EncryptData(elementToEncrypt, sessionKey, false);
				EncryptedKey ek = new EncryptedKey();
				byte[] encryptedKey = CPEncryptedXml.EncryptKey(
					sessionKey, (Gost3410_2012_256)certificateEncryption.PublicKey.Key,
					GostKeyWrapMethod.CryptoProKeyWrap); //was GOST3410 //was without method

				ek.CipherData = new CipherData(encryptedKey);
				ek.EncryptionMethod = new EncryptionMethod(GostEncryptedXml.XmlEncGostNamespaceUrl + "transport-gost2001"); //was CPEncryptedXml.XmlEncGostKeyTransportUrl
				KeyInfoX509Data data = new KeyInfoX509Data(certificateOpen);
				ek.KeyInfo.AddClause(data);
				edElement.KeyInfo.AddClause(new KeyInfoEncryptedKey(ek));
				edElement.CipherData.CipherValue = encryptedElement;
			}

			EncryptedXml.ReplaceElement(elementToEncrypt, edElement, false);

			return xml;
		}

		public static string DecryptXml(string document, X509Certificate2 cert) {
			XmlDocument xmlDoc = new XmlDocument();

			xmlDoc.PreserveWhitespace = true;
			xmlDoc.LoadXml(document);

			XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
			XmlNodeList list = xmlDoc.SelectNodes("//enc:EncryptedData", nsmgr);

			EncryptedXml exml = new EncryptedXml(xmlDoc);

			if (list != null) {
				foreach (XmlNode node in list) {
					XmlElement element = node as XmlElement;
					EncryptedData encryptedData = new EncryptedData();
					encryptedData.LoadXml(element);

					SymmetricAlgorithm decryptionKey = GetDecryptionKey(exml, encryptedData, cert);

					if (decryptionKey == null)
						throw new Exception("Ключ для расшифрования сообщения не найден.");

					byte[] decryptedData = exml.DecryptData(encryptedData, decryptionKey);
					exml.ReplaceData(element, decryptedData);
				}
			}

			return xmlDoc.OuterXml;
		}

		private static SymmetricAlgorithm GetDecryptionKey(EncryptedXml exml, EncryptedData encryptedData, X509Certificate2 cert) {
			IEnumerator encryptedKeyEnumerator = encryptedData.KeyInfo.GetEnumerator();

			while (encryptedKeyEnumerator.MoveNext()) {
				KeyInfoEncryptedKey current = encryptedKeyEnumerator.Current as KeyInfoEncryptedKey;
				if (current == null)
					continue;

				EncryptedKey encryptedKey = current.EncryptedKey;
				if (encryptedKey == null)
					continue;

				KeyInfo keyInfo = encryptedKey.KeyInfo;
				IEnumerator srcKeyEnumerator = keyInfo.GetEnumerator();

				while (srcKeyEnumerator.MoveNext()) {
					KeyInfoX509Data keyInfoCert = srcKeyEnumerator.Current as KeyInfoX509Data;
					if (keyInfoCert == null)
						continue;

					AsymmetricAlgorithm alg = cert.PrivateKey;
					Gost3410_2012_256 myKey = alg as Gost3410_2012_256;
					//Gost3410 myKey = alg as Gost3410;
					if (myKey == null)
						continue;

					return CPEncryptedXml.DecryptKeyClass(
                        encryptedKey.CipherData.CipherValue, 
                        myKey, 
                        encryptedData.EncryptionMethod.KeyAlgorithm);
				}
			}

			return null;
		}

		public static X509Certificate2 GetCertificate(StoreLocation storeLocation,
												StoreName storeName,
												string subjectDN) {
			X509Store store = new X509Store(storeName, storeLocation);
			store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

			X509Certificate2Collection found =
				store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, subjectDN, false);

			if (found.Count == 0)
				throw new Exception("!!! Сертификат не найден: " + subjectDN);

			if (found.Count > 1)
				throw new Exception("!!! Найдено больше одного сертификата по имени: " + subjectDN);

			return found[0];
		}

		public static List<string> GetStoreSertificatesList(StoreLocation location, StoreName name) {
			X509Store store = new X509Store(name, location);
			store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

			List<string> certs = new List<string>();
			foreach (X509Certificate2 cert in store.Certificates)
				certs.Add(cert.Subject);

			return certs;
		}
	}
}

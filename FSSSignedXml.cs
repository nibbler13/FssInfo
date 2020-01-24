using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FssInfo {
	class FSSSignedXml : SignedXml {
		public FSSSignedXml(XmlDocument document) : base(document) { }

		public override XmlElement GetIdElement(XmlDocument document, string idValue) {
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
			nsmgr.AddNamespace("wsu", WsdlServiceHandle.xmlns_wsu);
			XmlElement xmlElement =
				document.SelectSingleNode($"//*[@wsu:Id=\"{idValue}\"]", nsmgr) as XmlElement;

			return xmlElement;
		}
	}
}

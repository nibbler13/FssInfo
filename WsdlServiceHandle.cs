using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml;

namespace FssInfo {
	public class WsdlServiceHandle {
		private static WsdlServiceHandle instance = null;
		private static readonly object padlock = new object();

		public static WsdlServiceHandle Instance {
			get {
				lock (padlock) {
					if (instance == null)
						instance = new WsdlServiceHandle();

					return instance;
				}
			}
		}

		public const string xmlns_soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
		public const string xmlns_xsd = "http://www.w3.org/2001/XMLSchema";
		public const string xmlns_xsi = "http://www.w3.org/2001/XMLSchema-instance";
		public const string xmlns_wsse =
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
		public const string xmlns_wsu =
			"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
		public const string xmlns_ds = "http://www.w3.org/2000/09/xmldsig#";
		public const string xmlns_eln = "http://ru/ibs/fss/ln/ws/FileOperationsLn.wsdl";
		public const string xmlns_xenc = "http://www.w3.org/2001/04/xmlenc#";
		public const string xmlns_sch = "http://gost34.ibs.ru/WrapperService/Schema";
		public const string wsdl = "http://ru/ibs/fss/ln/ws/FileOperationsLn.wsdl";
		public enum RequestType {
			GetLnListByDate,
			GetLnData
		}

		private const string templateSoapMessageGetLnData =
			"PFM6RW52ZWxvcGUgeG1sbnM6Uz0iaHR0cDovL3NjaGVtYXMueG1sc" +
			"29hcC5vcmcvc29hcC9lbnZlbG9wZS8iIHhtbG5zOmRzPSJodHRwOi" +
			"8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjIiB4bWxuczp3c3N" +
			"lPSJodHRwOi8vZG9jcy5vYXNpcy1vcGVuLm9yZy93c3MvMjAwNC8w" +
			"MS9vYXNpcy0yMDA0MDEtd3NzLXdzc2VjdXJpdHktc2VjZXh0LTEuM" +
			"C54c2QiIHhtbG5zOndzdT0iaHR0cDovL2RvY3Mub2FzaXMtb3Blbi" +
			"5vcmcvd3NzLzIwMDQvMDEvb2FzaXMtMjAwNDAxLXdzcy13c3NlY3V" +
			"yaXR5LXV0aWxpdHktMS4wLnhzZCI+PFM6SGVhZGVyPjwvUzpIZWFk" +
			"ZXI+PFM6Qm9keT48bnMxOmdldExORGF0YSB4bWxuczpuczE9Imh0d" +
			"HA6Ly9ydS9pYnMvZnNzL2xuL3dzL0ZpbGVPcGVyYXRpb25zTG4ud3" +
			"NkbCI+PG5zMTpvZ3JuLz48bnMxOmxuQ29kZS8+PG5zMTpzbmlscy8" +
			"+PC9uczE6Z2V0TE5EYXRhPjwvUzpCb2R5PjwvUzpFbnZlbG9wZT4=";

		private const string templateSoapMessageGetLnListByDate =
			"PFM6RW52ZWxvcGUgeG1sbnM6Uz0iaHR0cDovL3NjaGVtYXMueG1sc" +
			"29hcC5vcmcvc29hcC9lbnZlbG9wZS8iIHhtbG5zOmRzPSJodHRwOi" +
			"8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjIiB4bWxuczp3c3N" +
			"lPSJodHRwOi8vZG9jcy5vYXNpcy1vcGVuLm9yZy93c3MvMjAwNC8w" +
			"MS9vYXNpcy0yMDA0MDEtd3NzLXdzc2VjdXJpdHktc2VjZXh0LTEuM" +
			"C54c2QiIHhtbG5zOndzdT0iaHR0cDovL2RvY3Mub2FzaXMtb3Blbi" +
			"5vcmcvd3NzLzIwMDQvMDEvb2FzaXMtMjAwNDAxLXdzcy13c3NlY3V" +
			"yaXR5LXV0aWxpdHktMS4wLnhzZCI+PFM6SGVhZGVyPjwvUzpIZWFk" +
			"ZXI+PFM6Qm9keT48bnMxOmdldExOTGlzdEJ5RGF0ZSB4bWxuczpuc" +
			"zE9Imh0dHA6Ly9ydS9pYnMvZnNzL2xuL3dzL0ZpbGVPcGVyYXRpb2" +
			"5zTG4ud3NkbCI+PG5zMTpvZ3JuIC8+PG5zMTpkYXRlIC8+PC9uczE" +
			"6Z2V0TE5MaXN0QnlEYXRlPjwvUzpCb2R5PjwvUzpFbnZlbG9wZT4=";

		public const string ogrn = "1057746061262";

		public static X509Certificate2 userCert;
		public static X509Certificate2 fssCert;
		private BackgroundWorker bworker;
		public DateTime DateBegin { get; private set; }
		public DateTime DateEnd { get; private set; }

		private double progressCurrent;

		public Dictionary<long, ItemDisabilitySheet> DisabilitySheets { get; set; } =
			new Dictionary<long, ItemDisabilitySheet>();

		public void Initialize(DateTime dateBegin,
						 DateTime dateEnd,
						 X509Certificate2 userCert,
						 X509Certificate2 fssCert) {
			DateBegin = dateBegin;
			DateEnd = dateEnd;

			WsdlServiceHandle.userCert = userCert;
			WsdlServiceHandle.fssCert = fssCert;

			DisabilitySheets.Clear();

			progressCurrent = 0;
		}

		public void LoadDisabilitySheetList(BackgroundWorker bworker, bool updateOpened = false) {
			this.bworker = bworker;

            if (updateOpened)
                bworker.ReportProgress((int)progressCurrent, 
                    "Получение информации по больничным листам со статусом 'ЭЛН открыт'");
			else
                bworker.ReportProgress((int)progressCurrent,
				    "Получения информации о больничных листах за период: " +
				    DateBegin.ToShortDateString() + " - " + DateEnd.ToShortDateString());

			double iteration = (DateEnd - DateBegin).TotalDays + 1;
			double progressStep = 30.0d / iteration;

			bworker.ReportProgress((int)progressCurrent, "Создание подключения к сервису ФСС");
			FssWsdlService.FileOperationsLnClient client = new FssWsdlService.FileOperationsLnClient();

			ServiceBehavior behavior = new ServiceBehavior(new ServiceClientInspector());
			client.Endpoint.EndpointBehaviors.Add(behavior);

            if (!updateOpened) {
                string daysLoadedFolder = Path.Combine(Logging.ASSEMBLY_DIRECTORY, "DaysLoaded");
                string daysLoadedPath = Path.Combine(Directory.GetCurrentDirectory(), daysLoadedFolder);
                if (!Directory.Exists(daysLoadedPath))
                    Directory.CreateDirectory(daysLoadedPath);

                for (DateTime date = DateBegin; date <= DateEnd; date = date.AddDays(1)) {
                    InsertDelimiterToBworker();
                    bworker.ReportProgress((int)progressCurrent, 
                        "Запрос списка больничных от: " + date.ToShortDateString());

                    string dayFile = Path.Combine(daysLoadedPath, date.ToString("yyyy_MM_dd"));
                    if (File.Exists(dayFile)) {
                        bworker.ReportProgress((int)progressCurrent, "Данный день был выгружен ранее, пропуск");
                        continue;
                    }

                    for (int i = 0; i < 10; i++) {
                        if (i > 0)
                            bworker.ReportProgress((int)progressCurrent, "Попытка: " + (i + 1));

                        try {
                            client.getLNListByDate(date, ogrn);
                            File.Create(dayFile);
                            break;
                        } catch (Exception e) {
                            Thread.Sleep(1000);
                            if (i == 9)
                                bworker.ReportProgress((int)progressCurrent, "!!! Ошибка: " + e.Message);
                        }
                    }

                    progressCurrent += progressStep;
                }
            }

			progressStep = 60.0d / (double)DisabilitySheets.Count;
			int currentItem = 1;
			foreach (ItemDisabilitySheet item in DisabilitySheets.Values) {
				InsertDelimiterToBworker();

				for (int i = 0; i < 10; i++) {
					string attempt = string.Empty;
					if (i > 0)
						attempt = ", попытка: " + (i + 1);

					bworker.ReportProgress((int)progressCurrent, 
                        "Запрос детализации по листу: " + item.LN_CODE + attempt + " (" + currentItem++ + " / " + DisabilitySheets.Count + ")");

					try {
						client.getLNData(ogrn, item.LN_CODE.ToString(), item.SNILS);
						break;
					} catch (Exception e) {
                        Thread.Sleep(1000);
                        if (i == 9)
							bworker.ReportProgress((int)progressCurrent, "!!! Ошибка: " + e.Message);
					}
				}

				progressCurrent += progressStep;
			}

			InsertDelimiterToBworker();
			bworker.ReportProgress(90, "Получено больничных листов: " + DisabilitySheets.Count);
			InsertDelimiterToBworker();
			return;
		}

		private void InsertDelimiterToBworker() {
			bworker.ReportProgress((int)progressCurrent, new string('-', 40));
		}

		public void ParseGetLnDataResponse(string response) {
			bworker.ReportProgress((int)progressCurrent, "Разбор полученных данных");

			XmlDocument xlDoc = new XmlDocument();
			xlDoc.LoadXml(response);

			XmlNodeList root = xlDoc.GetElementsByTagName("FileOperationsLnUserGetLNDataOut");
			if (root == null || root.Count == 0) {
				bworker.ReportProgress((int)progressCurrent, 
                    "Не удается найти базовый элемент - FileOperationsLnUserGetLNDataOut");
				return;
			}

			XmlElement rootElement = root[0] as XmlElement;
			XmlNodeList status = rootElement.GetElementsByTagName("STATUS");
			if (status == null && status.Count != 1) {
				bworker.ReportProgress((int)progressCurrent, 
                    "Не удается найти элемент - STATUS");
				return;
			}

			string statusValue = status[0].InnerText;
			if (!statusValue.Equals("1")) {
				XmlNodeList requestId = rootElement.GetElementsByTagName("REQUEST_ID");
				string requestIdValue = string.Empty;
				if (requestId != null && requestId.Count > 0)
					requestIdValue = requestId[0].InnerText;

				XmlNodeList mess = rootElement.GetElementsByTagName("MESS");
				string messValue = string.Empty;
				if (mess != null && mess.Count > 0)
					messValue = mess[0].InnerText;

				bworker.ReportProgress((int)progressCurrent, 
                    "Статус выполнения запроса - метод отработал с ошибками");
				bworker.ReportProgress((int)progressCurrent, 
                    "REQUEST_ID: " + requestIdValue + "; STATUS: " + statusValue + "; MESS: " + messValue);
				return;
			}

			XmlNodeList rows = rootElement.GetElementsByTagName("ROW");
			if (rows == null || rows.Count == 0) {
				bworker.ReportProgress((int)progressCurrent, 
                    "Отсутствуют данные о больничном листе");
				return;
			}

			XmlElement data = rows[0] as XmlElement;
			XmlNodeList lnCode = data.GetElementsByTagName("LN_CODE");
			ItemDisabilitySheet item = null;

			if (lnCode != null || lnCode.Count == 1)
				if (DisabilitySheets.ContainsKey(Convert.ToInt64(lnCode[0].InnerText)))
					item = DisabilitySheets[Convert.ToInt64(lnCode[0].InnerText)];

			if (item == null) {
				bworker.ReportProgress((int)progressCurrent, 
                    "Не удалось найти больничный лист в списке по номеру: " + lnCode);
				return;
			}

			XmlNodeList searchList = null;
			searchList = data.GetElementsByTagName("SURNAME");
			if (searchList != null && searchList.Count == 1)
				item.SURNAME = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("NAME");
			if (searchList != null && searchList.Count == 1)
				item.NAME = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("PATRONIMIC");
			if (searchList != null && searchList.Count == 1)
				item.PATRONIMIC = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("BOZ_FLAG");
			if (searchList != null && searchList.Count == 1)
				item.BOZ_FLAG = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("LPU_EMPLOYER");
			if (searchList != null && searchList.Count == 1)
				item.LPU_EMPLOYER = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("LPU_EMPL_FLAG");
			if (searchList != null && searchList.Count == 1)
				item.LPU_EMPL_FLAG = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("PREV_LN_CODE");
			if (searchList != null && searchList.Count == 1)
                if (long.TryParse(searchList[0].InnerText, out long prevLnCode))
                    item.PREV_LN_CODE = prevLnCode;

			searchList = data.GetElementsByTagName("PRIMARY_FLAG");
			if (searchList != null && searchList.Count == 1)
				item.PRIMARY_FLAG = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("DUPLICATE_FLAG");
			if (searchList != null && searchList.Count == 1)
				item.DUPLICATE_FLAG = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("LN_DATE");
			if (searchList != null && searchList.Count == 1)
                if (DateTime.TryParse(searchList[0].InnerText, out DateTime lnDate))
                    item.LN_DATE = lnDate;

			searchList = data.GetElementsByTagName("LPU_NAME");
			if (searchList != null && searchList.Count == 1)
				item.LPU_NAME = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("LPU_ADDRESS");
			if (searchList != null && searchList.Count == 1)
				item.LPU_ADDRESS = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("LPU_OGRN");
			if (searchList != null && searchList.Count == 1)
                if (long.TryParse(searchList[0].InnerText, out long lpuOgrn))
				    item.LPU_OGRN = lpuOgrn;

			searchList = data.GetElementsByTagName("BIRTHDAY");
			if (searchList != null && searchList.Count == 1)
                if (DateTime.TryParse(searchList[0].InnerText, out DateTime birthday))
				    item.BIRTHDAY = birthday;

			searchList = data.GetElementsByTagName("GENDER");
			if (searchList != null && searchList.Count == 1)
				item.GENDER = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("REASON1");
			if (searchList != null && searchList.Count == 1)
				item.REASON1 = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("REASON2");
			if (searchList != null && searchList.Count == 1)
				item.REASON2 = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("REASON3");
			if (searchList != null && searchList.Count == 1)
				item.REASON3 = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("DIAGNOS");
			if (searchList != null && searchList.Count == 1)
				item.DIAGNOS = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("PARENT_CODE");
			if (searchList != null && searchList.Count == 1)
                if (long.TryParse(searchList[0].InnerText, out long parentCode))
				    item.PARENT_CODE = parentCode;

			searchList = data.GetElementsByTagName("DATE1");
			if (searchList != null && searchList.Count == 1)
                if (DateTime.TryParse(searchList[0].InnerText, out DateTime date1))
				    item.DATE1 = date1;

			searchList = data.GetElementsByTagName("DATE2");
			if (searchList != null && searchList.Count == 1)
                if (DateTime.TryParse(searchList[0].InnerText, out DateTime date2))
				    item.DATE2 = date2;

			searchList = data.GetElementsByTagName("VOUCHER_NO");
			if (searchList != null && searchList.Count == 1)
                if (long.TryParse(searchList[0].InnerText, out long voucherNo))
				    item.VOUCHER_NO = voucherNo;

			searchList = data.GetElementsByTagName("VOUCHER_OGRN");
			if (searchList != null && searchList.Count == 1)
                if (long.TryParse(searchList[0].InnerText, out long voucherOgrn))
				    item.VOUCHER_OGRN = voucherOgrn;

			searchList = data.GetElementsByTagName("SERV1_AGE");
			if (searchList != null && searchList.Count == 1)
                if (long.TryParse(searchList[0].InnerText, out long serv1Age))
				    item.SERV1_AGE = serv1Age;

			searchList = data.GetElementsByTagName("SERV1_MM");
			if (searchList != null && searchList.Count == 1)
                if (long.TryParse(searchList[0].InnerText, out long serv1Mm))
				    item.SERV1_MM = serv1Mm;

			searchList = data.GetElementsByTagName("SERV1_RELATION_CODE");
			if (searchList != null && searchList.Count == 1)
				item.SERV1_RELATION_CODE = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("SERV1_FIO");
			if (searchList != null && searchList.Count == 1)
				item.SERV1_FIO = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("SERV2_AGE");
			if (searchList != null && searchList.Count == 1)
                if (long.TryParse(searchList[0].InnerText, out long serv2Age))
				    item.SERV2_AGE = serv2Age;

			searchList = data.GetElementsByTagName("SERV2_MM");
			if (searchList != null && searchList.Count == 1)
                if (long.TryParse(searchList[0].InnerText, out long serv2Mm))
				    item.SERV2_MM = serv2Mm;

			searchList = data.GetElementsByTagName("SERV2_RELATION_CODE");
			if (searchList != null && searchList.Count == 1)
				item.SERV2_RELATION_CODE = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("SERV2_FIO");
			if (searchList != null && searchList.Count == 1)
				item.SERV2_FIO = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("PREGN12W_FLAG");
			if (searchList != null && searchList.Count == 1)
				item.PREGN12W_FLAG = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("HOSPITAL_DT1");
			if (searchList != null && searchList.Count == 1)
                if (DateTime.TryParse(searchList[0].InnerText, out DateTime hospitalDt1))
				    item.HOSPITAL_DT1 = hospitalDt1;

			searchList = data.GetElementsByTagName("HOSPITAL_DT2");
			if (searchList != null && searchList.Count == 1)
                if (DateTime.TryParse(searchList[0].InnerText, out DateTime hospitalDt2))
				    item.HOSPITAL_DT2 = hospitalDt2;

			searchList = data.GetElementsByTagName("MSE_DT1");
			if (searchList != null && searchList.Count == 1)
                if (DateTime.TryParse(searchList[0].InnerText, out DateTime mseDt1))
				    item.MSE_DT1 = mseDt1;

			searchList = data.GetElementsByTagName("MSE_DT2");
			if (searchList != null && searchList.Count == 1)
                if (DateTime.TryParse(searchList[0].InnerText, out DateTime mseDt2))
				    item.MSE_DT2 = mseDt2;

			searchList = data.GetElementsByTagName("MSE_DT3");
			if (searchList != null && searchList.Count == 1)
                if (DateTime.TryParse(searchList[0].InnerText, out DateTime mseDt3))
				    item.MSE_DT3 = mseDt3;

			searchList = data.GetElementsByTagName("MSE_INVALID_GROUP");
			if (searchList != null && searchList.Count == 1)
				item.MSE_INVALID_GROUP = searchList[0].InnerText;

			searchList = data.GetElementsByTagName("LN_HASH");
			if (searchList != null && searchList.Count == 1)
				item.LN_HASH = searchList[0].InnerText;

            searchList = data.GetElementsByTagName("LN_STATE");
            if (searchList != null && searchList.Count == 1)
                item.LN_STATE = searchList[0].InnerText;

            searchList = data.GetElementsByTagName("HOSPITAL_BREACH");
			if (searchList != null)
				foreach (XmlElement breachElement in searchList) {
					ItemDisabilitySheet.HospitalBreach breach = new ItemDisabilitySheet.HospitalBreach();

					XmlNodeList breachSearchList = breachElement.GetElementsByTagName("HOSPITAL_BREACH_CODE");
					if (breachSearchList != null && breachSearchList.Count == 1)
						breach.HOSPITAL_BREACH_CODE = breachSearchList[0].InnerText;

					breachSearchList = breachElement.GetElementsByTagName("HOSPITAL_BREACH_DT");
					if (breachSearchList != null && breachSearchList.Count == 1)
						breach.HOSPITAL_BREACH_DT = breachSearchList[0].InnerText;

					item.HospitalBreachs.Add(breach);
				}

			searchList = data.GetElementsByTagName("LN_RESULT");
			if (searchList != null)
				foreach (XmlElement resultElement in searchList) {
					ItemDisabilitySheet.LNResult result = new ItemDisabilitySheet.LNResult();

					XmlNodeList resultSearchList = resultElement.GetElementsByTagName("MSE_RESULT");
					if (resultSearchList != null && resultSearchList.Count == 1)
						result.MSE_RESULT = resultSearchList[0].InnerText;

					resultSearchList = resultElement.GetElementsByTagName("OTHER_STATE_DT");
					if (resultSearchList != null && resultSearchList.Count == 1)
						result.OTHER_STATE_DT = resultSearchList[0].InnerText;

					resultSearchList = resultElement.GetElementsByTagName("RETURN_DATE_LPU");
					if (resultSearchList != null && resultSearchList.Count == 1)
						result.RETURN_DATE_LPU = resultSearchList[0].InnerText;

					resultSearchList = resultElement.GetElementsByTagName("NEXT_LN_CODE");
					if (resultSearchList != null && resultSearchList.Count == 1)
						result.NEXT_LN_CODE = resultSearchList[0].InnerText;

					item.LNResults.Add(result);
				}

			searchList = data.GetElementsByTagName("TREAT_FULL_PERIOD");
			if (searchList != null)
				foreach (XmlElement fullPeriodElement in searchList) {
					ItemDisabilitySheet.TreatFullPeriod fullPeriod = new ItemDisabilitySheet.TreatFullPeriod();

					XmlNodeList fullPeriodSearchList = fullPeriodElement.GetElementsByTagName("TREAT_CHAIRMAN_ROLE");
					if (fullPeriodSearchList != null && fullPeriodSearchList.Count == 1)
						fullPeriod.TREAT_CHAIRMAN_ROLE = fullPeriodSearchList[0].InnerText;

					fullPeriodSearchList = fullPeriodElement.GetElementsByTagName("TREAT_CHAIRMAN");
					if (fullPeriodSearchList != null && fullPeriodSearchList.Count == 1)
						fullPeriod.TREAT_CHAIRMAN = fullPeriodSearchList[0].InnerText;


					XmlNodeList periodsSearchList = fullPeriodElement.GetElementsByTagName("TREAT_PERIOD");
					if (periodsSearchList != null)
						foreach (XmlElement period in periodsSearchList) {
							ItemDisabilitySheet.TreatFullPeriod.TreatPeriod treatPeriod = 
								new ItemDisabilitySheet.TreatFullPeriod.TreatPeriod();

							XmlNodeList periodList = period.GetElementsByTagName("TREAT_DT1");
							if (periodList != null && periodList.Count == 1)
								treatPeriod.TREAT_DT1 = periodList[0].InnerText;

							periodList = period.GetElementsByTagName("TREAT_DT2");
							if (periodList != null && periodList.Count == 1)
								treatPeriod.TREAT_DT2 = periodList[0].InnerText;

							periodList = period.GetElementsByTagName("TREAT_DOCTOR_ROLE");
							if (periodList != null && periodList.Count == 1)
								treatPeriod.TREAT_DOCTOR_ROLE = periodList[0].InnerText;

							periodList = period.GetElementsByTagName("TREAT_DOCTOR");
							if (periodList != null && periodList.Count == 1)
								treatPeriod.TREAT_DOCTOR = periodList[0].InnerText;

							fullPeriod.TreatPeriods.Add(treatPeriod);
						}

					item.TreatFullPeriods.Add(fullPeriod);
				}
		}

		public void ParseGetLNListByDateResponse(string response) {
			bworker.ReportProgress((int)progressCurrent, "Разбор полученных данных");

			XmlDocument xlDoc = new XmlDocument();
			xlDoc.LoadXml(response);

			XmlNodeList root = xlDoc.GetElementsByTagName("FileOperationsLnUserGetLNListByDateOut");
			if (root == null || root.Count == 0) {
				bworker.ReportProgress((int)progressCurrent, "!!! Не удается найти базовый элемент - FileOperationsLnUserGetLNListByDateOut");
				return;
			}

			XmlElement rootElement = root[0] as XmlElement;
			XmlNodeList status = rootElement.GetElementsByTagName("STATUS");
			if (status == null && status.Count != 1) {
				bworker.ReportProgress((int)progressCurrent, "!!! Не удается найти элемент - STATUS");
				return;
			}

			string statusValue = status[0].InnerText;
			if (!statusValue.Equals("1")) {
				XmlNodeList requestId = rootElement.GetElementsByTagName("REQUEST_ID");
				string requestIdValue = string.Empty;
				if (requestId != null && requestId.Count > 0)
					requestIdValue = requestId[0].InnerText;

				XmlNodeList mess = rootElement.GetElementsByTagName("MESS");
				string messValue = string.Empty;
				if (mess != null && mess.Count > 0)
					messValue = mess[0].InnerText;

				bworker.ReportProgress((int)progressCurrent, "!!! Статус выполнения запроса - метод отработал с ошибками");
				bworker.ReportProgress((int)progressCurrent, "REQUEST_ID: " + requestIdValue + "; STATUS: " + statusValue + "; MESS: " + messValue);
				return;
			}

			XmlNodeList rows = rootElement.GetElementsByTagName("RowLNbyDate");
			if (rows == null || rows.Count == 0) {
				bworker.ReportProgress((int)progressCurrent, "Отсутствуют данные о больничных листах");
				return;
			}

			bworker.ReportProgress((int)progressCurrent, "Получено строк: " + rows.Count);

			foreach (XmlElement row in rows) {
				XmlNodeList lnCode = row.GetElementsByTagName("LN_CODE");
				XmlNodeList lnState = row.GetElementsByTagName("LN_STATE");
				XmlNodeList snils = row.GetElementsByTagName("SNILS");

				if (lnCode == null || lnState == null || snils == null) {
					bworker.ReportProgress((int)progressCurrent, "!!! Пропуск обработки строки, незвестный формат");
					continue;
				}

				ItemDisabilitySheet item = new ItemDisabilitySheet {
					LN_CODE = Convert.ToInt64(lnCode[0].InnerText),
					LN_STATE = lnState[0].InnerText,
					SNILS = snils[0].InnerText
				};

				if (!DisabilitySheets.ContainsKey(item.LN_CODE))
					DisabilitySheets.Add(item.LN_CODE, item);
			}
		}


		public static XmlDocument GetSignedRequestXml(RequestType type,
												DateTime? dateTime = null,
												string lnNum = "",
												string lnSnils = "") {
			XmlDocument doc = new XmlDocument();
			XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
			ns.AddNamespace("soapenv", xmlns_soapenv);
			ns.AddNamespace("ds", xmlns_ds);
			ns.AddNamespace("wsse", xmlns_wsse);
			ns.AddNamespace("wsu", xmlns_xsd);
			ns.AddNamespace("xsd", xmlns_xsd);
			ns.AddNamespace("xsi", xmlns_xsi);
			string xmlTemplate;

			if (type == RequestType.GetLnListByDate) 
				xmlTemplate = templateSoapMessageGetLnListByDate;
			else
				xmlTemplate = templateSoapMessageGetLnData;
			

			doc.LoadXml(Encoding.UTF8.GetString(Convert.FromBase64String(xmlTemplate)));

			XmlNodeList ogrnNode = 
				doc.GetElementsByTagName("ogrn", wsdl);

			if (ogrnNode != null && ogrnNode.Count == 1)
				ogrnNode[0].InnerText = ogrn;

			if (type == RequestType.GetLnListByDate) {
				XmlNodeList date =
					doc.GetElementsByTagName("date", wsdl);

				if (date != null && date.Count == 1)
					date[0].InnerText = dateTime.Value.ToString("yyyy-MM-dd");
			} else {
				XmlNodeList lnCode =
					doc.GetElementsByTagName("lnCode", wsdl);

				if (lnCode != null && lnCode.Count == 1)
					lnCode[0].InnerText = lnNum;

				XmlNodeList snils =
					doc.GetElementsByTagName("snils", wsdl);

				if (snils != null && snils.Count == 1)
					snils[0].InnerText = lnSnils;

			}

			XmlNodeList bodyNode = doc.GetElementsByTagName("Body", xmlns_soapenv);
			if (bodyNode != null && bodyNode.Count == 1) {
				XmlElement body = bodyNode[0] as XmlElement;
				body.SetAttribute("xmlns:wsu", xmlns_wsu);
				body.SetAttribute("Id", xmlns_wsu, $"OGRN_{ogrn}");
				CryptoTools.GenerateSecurity(doc, userCert, "OGRN", ogrn);
			}

			return doc;
		}
	}
}

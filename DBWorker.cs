using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FssInfo {
    static class DBWorker {
        static public void LoadDataAndWriteToDb(DateTime dateBegin, DateTime dateEnd, bool updateOpened = false) {
            string message = string.Empty;
            string tmp = string.Empty;

            if (updateOpened) {
                tmp = "---Обновление информации по открытым больничным листам в БД";
                message += tmp + Environment.NewLine;
                Logging.ToLog(tmp);
            } else {
                tmp = "---Загрузка данных по больничным листам в БД за период: с " +
                    dateBegin.ToShortDateString() + " по " + dateEnd.ToShortDateString();
                message += tmp + Environment.NewLine;
                Logging.ToLog(tmp);
            }

			X509Certificate2 userCertificate = GetCertificate(true);
			X509Certificate2 fssCertificate = GetCertificate(false);

			if (userCertificate == null || fssCertificate == null) {
                tmp = "!!! Не удалось определить сертификат(ы)";
                message += tmp + Environment.NewLine;
                Logging.ToLog(tmp);
			} else {
                WsdlServiceHandle.Instance.Initialize(
                    dateBegin,
                    dateEnd,
                    userCertificate,
                    fssCertificate);

                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += (s, e) => {
                    if (e.UserState != null) {
                        tmp = e.UserState.ToString();
                        message += tmp + Environment.NewLine;
                        Logging.ToLog(tmp);
                    }
                };

                FirebirdClient firebirdClient = new FirebirdClient(
                    Properties.Settings.Default.MisDbAddress,
                    Properties.Settings.Default.MisDbName,
                    Properties.Settings.Default.MisDbUser,
                    Properties.Settings.Default.MisDbPassword);

                if (firebirdClient.IsConnectionOpened()) {
                    if (updateOpened) {
                        tmp = "Получение списка больничных листов из БД со статусом 'ЭЛН открыт'";
                        message += tmp + Environment.NewLine;
                        Logging.ToLog(tmp);

                        try {
                            string queryGetOpened = Properties.Settings.Default.SqlSelectOpened;
                            DataTable dt = firebirdClient.GetDataTable(queryGetOpened, new Dictionary<string, object>());
                            tmp = "Получено строк из БД: " + dt.Rows.Count;
                            message += tmp + Environment.NewLine;
                            Logging.ToLog(tmp);

                            if (dt.Rows.Count > 0)
                                foreach (DataRow row in dt.Rows) {
                                    ItemDisabilitySheet item = new ItemDisabilitySheet() {
                                        LN_CODE = Convert.ToInt64(row[0].ToString()),
                                        SNILS = row[1].ToString()
                                    };

                                    WsdlServiceHandle.Instance.DisabilitySheets.Add(item.LN_CODE, item);
                                }
                        } catch (Exception e) {
                            tmp = "!!! " + e.Message + Environment.NewLine + e.StackTrace;
                            message += tmp + Environment.NewLine;
                            Logging.ToLog(tmp);
                        }
                    }

                    WsdlServiceHandle.Instance.LoadDisabilitySheetList(bw, updateOpened);

                    if (WsdlServiceHandle.Instance.DisabilitySheets.Count == 0) {
                        tmp = "Нет записей за указанный период, завершение работы";
                        message += tmp + Environment.NewLine;
                        Logging.ToLog(tmp);
                    } else {
                        tmp = "Запись полученных данных в БД";
                        message += tmp;
                        Logging.ToLog(tmp);

                        try {
                            string queryUpdate = Properties.Settings.Default.SqlUpdateRow;
                            foreach (ItemDisabilitySheet item in WsdlServiceHandle.Instance.DisabilitySheets.Values)
                                firebirdClient.ExecuteUpdateQuery(queryUpdate, item.GetDictionary());
                        } catch (Exception e) {
                            tmp = "!!! " + e.Message + Environment.NewLine + e.StackTrace;
                            message += tmp + Environment.NewLine;
                            Logging.ToLog(tmp);
                        }
                    }
                } else {
                    tmp = "!!! Не удалось открыть подключение к БД: " +
                        Properties.Settings.Default.MisDbAddress + ":" +
                        Properties.Settings.Default.MisDbName;
                    message += tmp + Environment.NewLine;
                    Logging.ToLog(tmp);
                }
            }

            string subject = message.Contains("!!!") ? "!!! Выполнено с ошибками" : "Выполнено успешно";
            string body = message;
            string receiver = Properties.Settings.Default.MailTo;

            SystemMail.SendMail(subject, body, receiver);
        }

        static private X509Certificate2 GetCertificate(bool isUserCertificate) {
			string storeLocation;
			string storeName;
			string subjectDN;

			if (isUserCertificate) {
				storeLocation = Properties.Settings.Default.CertificateUserStoreLocation;
				storeName = Properties.Settings.Default.CertificateUserStoreName;
				subjectDN = Properties.Settings.Default.CertificateUserSubjectDN;
			} else {
				storeLocation = Properties.Settings.Default.CertificateFssStoreLocation;
				storeName = Properties.Settings.Default.CertificateFssStoreName;
				subjectDN = Properties.Settings.Default.CertificateFssSubjectDN;
			}

			bool isStoreLocationOK = Enum.TryParse(storeLocation, out StoreLocation storeLocationEnum);

            if (!isStoreLocationOK) {
                Logging.ToLog("!!! Не удалось определить расположение хранилища для параметра: " + storeLocation);
                return null;
            }

            bool isStoreNameOK = Enum.TryParse(storeName, out StoreName storeNameEnum);

			if (!isStoreNameOK) {
				Logging.ToLog("!!! Не удалось определить имя хранилища для параметра: " + storeName);
				return null;
			}

			try {
				return CryptoTools.GetCertificate(storeLocationEnum, storeNameEnum, subjectDN);
			} catch (Exception e) {
				Logging.ToLog("!!! Не удалось найти сертификат: " + Environment.NewLine +
					e.Message + Environment.NewLine + e.StackTrace);
				return null;
			}
        }
    }
}

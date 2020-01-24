using System;
using System.Data;
using System.Collections.Generic;
using FirebirdSql.Data.FirebirdClient;
using System.Windows;

namespace FssInfo {
	public class FirebirdClient {
		private FbConnection connection;

		public FirebirdClient(string ipAddress, string baseName, string user, string pass) {
			FbConnectionStringBuilder cs = new FbConnectionStringBuilder {
				DataSource = ipAddress,
				Database = baseName,
				UserID = user,
				Password = pass,
				Charset = "NONE",
				Pooling = false
			};

			connection = new FbConnection(cs.ToString());
			IsConnectionOpened();
		}

		public void Close() {
			connection.Close();
		}

		public bool IsConnectionOpened() {
			if (connection.State != ConnectionState.Open) {
				try {
					connection.Open();
				} catch (Exception e) {
					string subject = "Ошибка подключения к БД";
					string body = e.Message + Environment.NewLine + e.StackTrace;
					Logging.ToLog(subject + Environment.NewLine + body);
				}
			}

			return connection.State == ConnectionState.Open;
		}

		public DataTable GetDataTable(string query, Dictionary<string, object> parameters) {
			DataTable dataTable = new DataTable();

            if (!IsConnectionOpened())
                throw new Exception("DB connection is not opened");

            using (FbCommand command = new FbCommand(query, connection)) {
                if (parameters.Count > 0)
                    foreach (KeyValuePair<string, object> parameter in parameters)
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);

                using (FbDataAdapter fbDataAdapter = new FbDataAdapter(command))
                    fbDataAdapter.Fill(dataTable);
            }

            return dataTable;
		}

		public bool ExecuteUpdateQuery(string query, Dictionary<string, object> parameters) {
			if (!IsConnectionOpened())
                throw new Exception("DB connection is not opened");

            FbCommand update = new FbCommand(query, connection);

            if (parameters.Count > 0)
                foreach (KeyValuePair<string, object> parameter in parameters)
                    update.Parameters.AddWithValue(parameter.Key, parameter.Value);

            return update.ExecuteNonQuery() > 0 ? true : false;
		}
	}
}

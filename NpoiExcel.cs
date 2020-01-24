using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FssInfo {
	class NpoiExcel {
		private BackgroundWorker bworker;
		private double progressCurrent;

		public static string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";

		public NpoiExcel(BackgroundWorker bworker) {
			this.bworker = bworker;
		}

		private  bool CreateNewIWorkbook(string resultFilePrefix, string templateFileName,
			out IWorkbook workbook, out ISheet sheet, out string resultFile, string sheetName) {
			workbook = null;
			sheet = null;
			resultFile = string.Empty;

			try {
				if (!GetTemplateFilePath(ref templateFileName))
					return false;

				string resultPath = GetResultFilePath(resultFilePrefix, templateFileName);

				using (FileStream stream = new FileStream(templateFileName, FileMode.Open, FileAccess.Read))
					workbook = new XSSFWorkbook(stream);

				if (string.IsNullOrEmpty(sheetName))
					sheetName = "Данные";

				sheet = workbook.GetSheet(sheetName);
				resultFile = resultPath;

				return true;
			} catch (Exception e) {
				bworker.ReportProgress((int)progressCurrent, e.Message + Environment.NewLine + e.StackTrace);
				return false;
			}
		}

		protected bool GetTemplateFilePath(ref string templateFileName) {
			templateFileName = Path.Combine(AssemblyDirectory, templateFileName);

			if (!File.Exists(templateFileName)) {
				bworker.ReportProgress((int)progressCurrent, "Не удалось найти файл шаблона: " + templateFileName);
				return false;
			}

			return true;
		}

		protected static string GetResultFilePath(string resultFilePrefix, string templateFileName, bool isPlainText = false) {
			string resultPath = Path.Combine(AssemblyDirectory, "Results");
			if (!Directory.Exists(resultPath))
				Directory.CreateDirectory(resultPath);

			foreach (char item in Path.GetInvalidFileNameChars())
				resultFilePrefix = resultFilePrefix.Replace(item, '-');

			string fileEnding = ".xlsx";
			if (isPlainText)
				fileEnding = ".txt";

			string resultFile = Path.Combine(resultPath, resultFilePrefix + " " + DateTime.Now.ToString("yyyyMMdd_HHmmss") + fileEnding);
			if (isPlainText)
				File.Copy(templateFileName, resultFile, true);

			return resultFile;
		}

		protected bool SaveAndCloseIWorkbook(IWorkbook workbook, string resultFile) {
			try {
				using (FileStream stream = new FileStream(resultFile, FileMode.Create, FileAccess.Write))
					workbook.Write(stream);

				workbook.Close();

				return true;
			} catch (Exception e) {
				bworker.ReportProgress((int)progressCurrent, e.Message + Environment.NewLine + e.StackTrace);
				return false;
			}
		}
		public string WriteDisabilitySheetsToExcel(double progressStart, double progressEnd) {
			IWorkbook workbook = null;
			ISheet sheet = null;
			string resultFile = string.Empty;

			string resultFilePrefix = "Отчет по больничным листам c " + 
				WsdlServiceHandle.Instance.DateBegin.ToShortDateString() + " по " + 
				WsdlServiceHandle.Instance.DateEnd.ToShortDateString();
			string sheetName = "Данные";
			string templateFileName = "Template.xlsx";
			if (!CreateNewIWorkbook(resultFilePrefix, templateFileName,
				out workbook, out sheet, out resultFile, sheetName))
				return string.Empty;

			int rowNumber = 4;
			int columnNumber = 0;

			progressCurrent = progressStart;
			double progressStep = (progressEnd - progressStart) / WsdlServiceHandle.Instance.DisabilitySheets.Count;
			bworker.ReportProgress((int)progressCurrent, "Запись информации в книгу Excel");

			IDataFormat dataFormat = workbook.CreateDataFormat();

			foreach (ItemDisabilitySheet item in WsdlServiceHandle.Instance.DisabilitySheets.Values) {
				IRow row = null;
				try { row = sheet.GetRow(rowNumber); } catch (Exception) { }

				if (row == null)
					row = sheet.CreateRow(rowNumber);

				string[] values = item.GetValueArray();

				foreach (string value in values) {
					ICell cell = null;
					try { cell = row.GetCell(columnNumber); } catch (Exception) { }

					if (cell == null)
						cell = row.CreateCell(columnNumber);

					if (value != null) {
						if (double.TryParse(value, out double result)) {
							cell.SetCellValue("'" + result);
						} else if (DateTime.TryParse(value, out DateTime date)) {
							cell.SetCellValue(date);
							cell.CellStyle.DataFormat = dataFormat.GetFormat("yyyy.MM.dd");
						} else {
							cell.SetCellValue(value);
						}
					}

					columnNumber++;
				}

				columnNumber = 0;
				rowNumber++;
				progressCurrent += progressStep;
				bworker.ReportProgress((int)progressCurrent);
			}

			if (!SaveAndCloseIWorkbook(workbook, resultFile))
				return string.Empty;

			return resultFile;
		}
	}
}

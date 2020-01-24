using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FssInfo {
	/// <summary>
	/// Interaction logic for WindowFssList.xaml
	/// </summary>
	public partial class WindowFssList : Window {
		public WindowFssList() {
			InitializeComponent();

			BackgroundWorker bworker = new BackgroundWorker();
			bworker.WorkerReportsProgress = true;
			bworker.ProgressChanged += Bw_ProgressChanged;
			bworker.RunWorkerCompleted += Bw_RunWorkerCompleted;
			bworker.DoWork += Bw_DoWork;
			bworker.RunWorkerAsync();
		}

		private void Bw_DoWork(object sender, DoWorkEventArgs e) {
			BackgroundWorker bworker = sender as BackgroundWorker;
			WsdlServiceHandle.Instance.LoadDisabilitySheetList(bworker);

			if (WsdlServiceHandle.Instance.DisabilitySheets.Count > 0) {
				NpoiExcel npoiExcel = new NpoiExcel(bworker);
				string result = npoiExcel.WriteDisabilitySheetsToExcel(90, 100);
				if (string.IsNullOrEmpty(result))
					bworker.ReportProgress(100, "Не удалось записать данные в файл");
				else {
					bworker.ReportProgress(100, "Данные записаны в файл: " + result);
					Process.Start(result);
				}
			}
		}

		private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Error != null) {
				TextBoxProgress.Text += e.Error.Message + Environment.NewLine + e.Error.StackTrace;
				MessageBox.Show(this,
					"Во время выполнения запроса возникла ошибка",
					"",
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}

			MessageBox.Show(this, "Выполнение завершено", "", MessageBoxButton.OK, MessageBoxImage.Information);

			ButtonClose.IsEnabled = true;
		}

		private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			ProgressBarMain.Value = e.ProgressPercentage;

			if (e.UserState != null) {
				TextBoxProgress.AppendText(e.UserState.ToString() + Environment.NewLine);
				TextBoxProgress.ScrollToEnd();
			}
		}

		private void ButtonClose_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void ButtonExportToExcel_Click(object sender, RoutedEventArgs e) {

		}
	}
}

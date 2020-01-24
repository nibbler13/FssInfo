using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FssInfo {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			ComboBoxUserCertStoreLocation.ItemsSource = Enum.GetValues(typeof(StoreLocation)).Cast<StoreLocation>();
			ComboBoxUserCertStoreName.ItemsSource = Enum.GetValues(typeof(StoreName)).Cast<StoreName>();
			ComboBoxUserCertStoreLocation.SelectionChanged += ComboBoxUserCert_SelectionChanged;
			ComboBoxUserCertStoreName.SelectionChanged += ComboBoxUserCert_SelectionChanged;
			ComboBoxUserCertStoreLocation.SelectedItem = StoreLocation.CurrentUser;
			ComboBoxUserCertStoreName.SelectedItem = StoreName.My;

			ComboBoxFssCertStoreLocation.ItemsSource = Enum.GetValues(typeof(StoreLocation)).Cast<StoreLocation>();
			ComboBoxFssCertStoreName.ItemsSource = Enum.GetValues(typeof(StoreName)).Cast<StoreName>();
			ComboBoxFssCertStoreLocation.SelectionChanged += ComboBoxFssCert_SelectionChanged;
			ComboBoxFssCertStoreName.SelectionChanged += ComboBoxFssCert_SelectionChanged;
			ComboBoxFssCertStoreLocation.SelectedItem = StoreLocation.CurrentUser;
			ComboBoxFssCertStoreName.SelectedItem = StoreName.AddressBook;

			DatePickerBegin.SelectedDate = DateTime.Now;
			DatePickerEnd.SelectedDate = DateTime.Now;

			if (Debugger.IsAttached) {
				ListBoxUserCerts.SelectedIndex = 0;
				ListBoxFssCerts.SelectedIndex = 0;
			}
		}

		private void ComboBoxUserCert_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ListBoxUserCerts.Items.Clear();

			object location = ComboBoxUserCertStoreLocation.SelectedItem;
			object name = ComboBoxUserCertStoreName.SelectedItem;
			if (location == null || name == null)
				return;

			foreach (string str in CryptoTools.GetStoreSertificatesList((StoreLocation)location, (StoreName)name))
				ListBoxUserCerts.Items.Add(str);
		}

		private void ComboBoxFssCert_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ListBoxFssCerts.Items.Clear();

			object location = ComboBoxFssCertStoreLocation.SelectedItem;
			object name = ComboBoxFssCertStoreName.SelectedItem;
			if (location == null || name == null)
				return;

			foreach (string str in CryptoTools.GetStoreSertificatesList((StoreLocation)location, (StoreName)name))
				ListBoxFssCerts.Items.Add(str);
		}

		private void ButtonGetInfo_Click(object sender, RoutedEventArgs e) {
			object userCertSubjectName = ListBoxUserCerts.SelectedItem;
			object fssCertSubjectName = ListBoxFssCerts.SelectedItem;
			if (userCertSubjectName == null ||
				fssCertSubjectName == null) {
				MessageBox.Show(this,
					"Необходимо выбрать сертификаты для продолжения",
					"",
					MessageBoxButton.OK,
					MessageBoxImage.Information);
				return;
			}

			DateTime dateBegin = DatePickerBegin.SelectedDate.Value.Date;
			DateTime dateEnd = DatePickerEnd.SelectedDate.Value.Date;
			if (dateBegin > dateEnd) {
				MessageBox.Show(this,
					"Дата начала выбранного периода не может быть больше даты окончания",
					"",
					MessageBoxButton.OK,
					MessageBoxImage.Information);
				return;
			}

			X509Certificate2 userCert = CryptoTools.GetCertificate(
				(StoreLocation)ComboBoxUserCertStoreLocation.SelectedItem,
				(StoreName)ComboBoxUserCertStoreName.SelectedItem,
				(string)userCertSubjectName);

			X509Certificate2 fssCert = CryptoTools.GetCertificate(
				(StoreLocation)ComboBoxFssCertStoreLocation.SelectedItem,
				(StoreName)ComboBoxFssCertStoreName.SelectedItem,
				(string)fssCertSubjectName);

			WsdlServiceHandle.Instance.Initialize(
				dateBegin,
				dateEnd,
				userCert,
				fssCert);

			WindowFssList windowFssList = new WindowFssList();
			windowFssList.Owner = this;
			windowFssList.ShowDialog();
		}
	}
}

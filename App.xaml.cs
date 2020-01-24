using FssInfo.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FssInfo {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (e.Args.Length == 0) {
                MainWindow window = new MainWindow();
                window.Show();
                return;
            }

			Logging.ToLog("Переданные параметры: " + string.Join(";", e.Args));

            if (e.Args.Length == 1 && e.Args[0].ToLower().Equals("previousday")) {
                DateTime dateBegin = DateTime.Now.Date.AddDays(-1);
                DateTime dateEnd = dateBegin;
                DBWorker.LoadDataAndWriteToDb(dateBegin, dateEnd);
            } else if (e.Args.Length == 1 && e.Args[0].ToLower().Equals("all")) {
                DateTime dateBegin = new DateTime(2017, 1, 1);
                DateTime dateEnd = DateTime.Now.Date.AddDays(-1);
                DBWorker.LoadDataAndWriteToDb(dateBegin, dateEnd);
            } else if (e.Args.Length == 1 && DateTime.TryParse(e.Args[0], out DateTime singleDate)) {
                DBWorker.LoadDataAndWriteToDb(singleDate, singleDate);
            } else if (e.Args.Length == 2 &&
                DateTime.TryParse(e.Args[0], out DateTime dateBegin) &&
                DateTime.TryParse(e.Args[1], out DateTime dateEnd)) {
                DBWorker.LoadDataAndWriteToDb(dateBegin, dateEnd);
            } else if (e.Args.Length == 1 && e.Args[0].ToLower().Equals("updateopened")) {
                DBWorker.LoadDataAndWriteToDb(DateTime.Now, DateTime.Now, true);
            } else {
                Console.WriteLine(
                    "Неизвестный параметр. " + Environment.NewLine +
                    "Для обновление данных по больничным листам за предыдущий день используйте параметр 'previousday'." + Environment.NewLine +
                    "Для получения всего списка больничных листов используйте параметр 'all'" + Environment.NewLine +
                    "Для задания конкретной даты - укажите ее в формате дд.ММ.гггг" + Environment.NewLine +
                    "Для задания диапазона - укажите даты в формате дата_начала дата_окончания, формат дд.ММ.гггг" + Environment.NewLine +
                    "Для обновления данных по открытым больничным листам используйте параметр updateopened");
            }

            Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            HandleException(e.ExceptionObject as Exception);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            HandleException(e.Exception);
        }

        private void HandleException(Exception exception) {
            if (exception != null) {
                Logging.ToLog(exception.Message + Environment.NewLine + exception.StackTrace);
                SystemMail.SendMail(
                    "Необработанное исключение",
                    exception.Message + Environment.NewLine + exception.StackTrace,
                    Settings.Default.MailTo);
            }

            Logging.ToLog("!!!App - Аварийное завершение работы");
            Process.GetCurrentProcess().Kill();
        }
    }
}

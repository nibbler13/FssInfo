﻿using System.Net.Mail;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Net.Mime;

namespace FssInfo {
	public class SystemMail {
		public static void SendMail (string subject, string body, string receiver) {
			Logging.ToLog("Отправка сообщения, тема: " + subject + ", текст: " + body);
			Logging.ToLog("Получатели: " + receiver);

			if (string.IsNullOrEmpty(receiver))
				return;

			try {
				string appName = Assembly.GetExecutingAssembly().GetName().Name;

				MailAddress from = new MailAddress(
					Properties.Settings.Default.MailUser + "@" + 
					Properties.Settings.Default.MailDomain, appName);

				List<MailAddress> mailAddressesTo = new List<MailAddress>();

				if (receiver.Contains(";")) {
					string[] receivers = receiver.Split(';');
					foreach (string address in receivers)
						mailAddressesTo.Add(new MailAddress(address));
				} else
					mailAddressesTo.Add(new MailAddress(receiver));
				
				body += Environment.NewLine + Environment.NewLine + 
					"___________________________________________" + Environment.NewLine +
					"Это автоматически сгенерированное сообщение" + Environment.NewLine + 
					"Просьба не отвечать на него" + Environment.NewLine +
 					"Имя системы: " + Environment.MachineName;

				MailMessage message = new MailMessage();

				foreach (MailAddress mailAddress in mailAddressesTo)
					message.To.Add(mailAddress);

				message.IsBodyHtml = body.Contains("<") && body.Contains(">");

				if (message.IsBodyHtml)
					body = body.Replace(Environment.NewLine, "<br>");
				
				message.From = from;
				message.Subject = subject;
				message.Body = body;

				if (!string.IsNullOrEmpty(Properties.Settings.Default.MailCopy))
					message.CC.Add(Properties.Settings.Default.MailCopy);

				SmtpClient client = new SmtpClient(Properties.Settings.Default.MailSmtpServer, 25);
				client.UseDefaultCredentials = false;
				client.Credentials = new System.Net.NetworkCredential(
					Properties.Settings.Default.MailUser,
					Properties.Settings.Default.MailPassword,
					Properties.Settings.Default.MailDomain);

				client.Send(message);
				client.Dispose();
				message.Dispose();
				Logging.ToLog("Письмо отправлено успешно");
			} catch (Exception e) {
				Logging.ToLog("SendMail exception: " + e.Message + Environment.NewLine + e.StackTrace);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FssInfo {
	public class ItemDisabilitySheet {
		private string ln_state;
		public string LN_STATE {
			get {
				switch (ln_state) {
					case "010":
						return "ЭЛН открыт";
					case "020":
						return "ЭЛН продлен";
					case "030":
						return "ЭЛН закрыт";
					case "040":
						return "ЭЛН направление на МСЭ";
					case "050":
						return "ЭЛН дополнен данными МСЭ";
					case "060":
						return "ЭЛН заполнен Страхователем";
					case "070":
						return "ЭЛН заполнен Страхователем  (реестр ПВСО)";
					case "080":
						return "Пособие выплачено";
					case "090":
						return "Действия прекращены";
					default:
						return ln_state;
				}
			} set {
				if (value != ln_state)
					ln_state = value;
			}
		}


		public Int64 LN_CODE { get; set; }
		public string SNILS { get; set; }
		public string SURNAME { get; set; }
		public string NAME { get; set; }
		public string PATRONIMIC { get; set; }

		private string boz_flag;
		public string BOZ_FLAG {
			get {
				switch (boz_flag) {
					case "0":
						return "нет";
					case "1":
						return "состоит";
					default:
						return boz_flag;
				}
			} set {
				if (value != boz_flag)
					boz_flag = value;
			}
		}
		public string LPU_EMPLOYER { get; set; }

		private string lpu_empl_flag;
		public string LPU_EMPL_FLAG {
			get {
				switch (lpu_empl_flag) {
					case "0":
						return "по совместительству";
					case "1":
						return "основное";
					default:
						return lpu_empl_flag;
				}
			} set {
				if (value != lpu_empl_flag)
					lpu_empl_flag = value;
			}
		}
		public Int64 PREV_LN_CODE { get; set; }

		private string primary_flag;
		public string PRIMARY_FLAG {
			get {
				switch (primary_flag) {
					case "0":
						return "продолжение";
					case "1":
						return "первичный";
					default:
						return primary_flag;
				}
			} set {
				if (value != primary_flag)
					primary_flag = value;
			}
		}

		private string duplicate_flag;
		public string DUPLICATE_FLAG {
			get {
				switch (duplicate_flag) {
					case "0":
						return "оригинал";
					case "1":
						return "дубликат";
					default:
						return duplicate_flag;
				}
			} set {
				if (value != duplicate_flag)
					duplicate_flag = value;
			}
		}
		public DateTime? LN_DATE { get; set; }
		public string LPU_NAME { get; set; }
		public string LPU_ADDRESS { get; set; }
		public Int64 LPU_OGRN { get; set; }
		public DateTime? BIRTHDAY { get; set; }

		private string gender;
		public string GENDER {
			get {
				switch (gender) {
					case "0":
						return "мужской";
					case "1":
						return "женский";
					default:
						return gender;
				}
			}
			set {
				if (value != gender)
					gender = value;
			}
		}

		private string reason1;
		public string REASON1 {
			get {
				switch (reason1) {
					case "01":
						return "заболевание";
					case "02":
						return "травма";
					case "03":
						return "карантин";
					case "04":
						return "несчастный случай на производстве или его последствия";
					case "05":
						return "отпуск по беременности и родам";
					case "06":
						return "протезирование в стационаре";
					case "07":
						return "профессиональное заболевание или его обострение";
					case "08":
						return "долечивание в санатории";
					case "09":
						return "уход за больным членом семьи";
					case "10":
						return "иное состояние (отравление, проведение манипуляций и др.)";
					case "11":
						return "заболевание туберкулезом";
					case "12":
						return "в случае заболевания ребенка, включенного в перечень заболеваний определяемых Минздравсоцразвития России";
					case "13":
						return "ребенок-инвалид";
					case "14":
						return "поствакцинальное осложнение или злокачественное новообразование у ребенка";
					case "15":
						return "ВИЧ-инфицированный ребенок";
					default:
						return reason1;
				}
			} set {
				if (value != reason1)
					reason1 = value;
			}
		}

		private string reason2;
		public string REASON2 {
			get {
				switch (reason2) {
					case "17":
						return "лечение в специализированном санатории";
					case "18":
						return "санаторно-курортное лечение в связи с несчастным случаем на производстве в период временной нетрудоспособности (до направления на МСЭ)";
					case "19":
						return "лечение в клинике научно-исследовательского учреждения (института) курортологии, физиотерапии и реабилитации";
					case "20":
						return "дополнительный отпуск по беременности и родам";
					case "21":
						return "заболевание или травма, наступившие вследствие алкогольного, наркотического, токсического опьянения или действий, связанных с таким опьянением";
					default:
						return reason2;
				}
			} set {
				if (value != reason2)
					reason2 = value;
			}
		}

		private string reason3;
		public string REASON3 {
			get {
				switch (reason3) {
					case "01":
						return "заболевание";
					case "02":
						return "травма";
					case "03":
						return "карантин";
					case "04":
						return "несчастный случай на производстве или его последствия";
					case "05":
						return "отпуск по беременности и родам";
					case "06":
						return "протезирование в стационаре";
					case "07":
						return "профессиональное заболевание или его обострение";
					case "08":
						return "долечивание в санатории";
					case "09":
						return "уход за больным членом семьи";
					case "10":
						return "иное состояние (отравление, проведение манипуляций и др.)";
					case "11":
						return "заболевание туберкулезом";
					case "12":
						return "в случае заболевания ребенка, включенного в перечень заболеваний определяемых Минздравсоцразвития России";
					case "13":
						return "ребенок-инвалид";
					case "14":
						return "поствакцинальное осложнение или злокачественное новообразование у ребенка";
					case "15":
						return "ВИЧ-инфицированный ребенок";
					default:
						return reason3;
				}
			}
			set {
				if (value != reason3)
					reason3 = value;
			}
		}
		public string DIAGNOS { get; set; }
		public Int64 PARENT_CODE { get; set; }
		public DateTime? DATE1 { get; set; }
		public DateTime? DATE2 { get; set; }
		public Int64 VOUCHER_NO { get; set; }
		public Int64 VOUCHER_OGRN { get; set; }
		public Int64 SERV1_AGE { get; set; }
		public Int64 SERV1_MM { get; set; }

		private string serv1_relation_code;
		public string SERV1_RELATION_CODE {
			get {
				switch (serv1_relation_code) {
					case "38":
						return "мать";
					case "39":
						return "отец";
					case "40":
						return "опекун";
					case "41":
						return "попечитель";
					case "42":
						return "иной родственник, фактически осуществляющий уход за ребенком";
					default:
						return serv1_relation_code;
				}
			} set {
				if (value != serv1_relation_code)
					serv1_relation_code = value;
			}
		}
		public string SERV1_FIO { get; set; }
		public Int64 SERV2_AGE { get; set; }
		public Int64 SERV2_MM { get; set; }

		private string serv2_relation_code;
		public string SERV2_RELATION_CODE {
			get {
				switch (serv2_relation_code) {
					case "38":
						return "мать";
					case "39":
						return "отец";
					case "40":
						return "опекун";
					case "41":
						return "попечитель";
					case "42":
						return "иной родственник, фактически осуществляющий уход за ребенком";
					default:
						return serv2_relation_code;
				}
			} set {
				if (value != serv2_relation_code)
					serv2_relation_code = value;
			}
		}
		public string SERV2_FIO { get; set; }

		private string pregn12w_flag;
		public string PREGN12W_FLAG {
			get {
				switch (pregn12w_flag) {
					case "0":
						return "нет";
					case "1":
						return "поставлена";
					default:
						return pregn12w_flag;
				}
			} set {
				if (value != pregn12w_flag)
					pregn12w_flag = value;
			}
		}
		public DateTime? HOSPITAL_DT1 { get; set; }
		public DateTime? HOSPITAL_DT2 { get; set; }
		public DateTime? MSE_DT1 { get; set; }
		public DateTime? MSE_DT2 { get; set; }
		public DateTime? MSE_DT3 { get; set; }

		private string mse_invalid_group;
		public string MSE_INVALID_GROUP {
			get {
				switch (mse_invalid_group) {
					case "1":
						return "первая группа";
					case "2":
						return "вторая группа";
					case "3":
						return "третья группа";
					default:
						return mse_invalid_group;
				}
			} set {
				if (value != mse_invalid_group)
					mse_invalid_group = value;
			}
		}
		public string LN_HASH { get; set; }

		public List<TreatFullPeriod> TreatFullPeriods { get; set; } =
			new List<TreatFullPeriod>();

		public List<LNResult> LNResults { get; set; } =
			new List<LNResult>();

		public List<HospitalBreach> HospitalBreachs { get; set; } =
			new List<HospitalBreach>();

		public class HospitalBreach {
			private string hospital_breach_code;
			public string HOSPITAL_BREACH_CODE {
				get {
					switch (hospital_breach_code) {
						case "23":
							return "несоблюдение предписанного режима, самовольный уход из стационара, выезд на лечение в другой административный район без разрешения лечащего врача";
						case "24":
							return "несвоевременная явка на прием к врачу";
						case "25":
							return "выход на работу без выписки";
						case "26":
							return "отказ от направления в учреждение медико-социальной экспертизы";
						case "27":
							return "несвоевременная явка в учреждение медико-социальной экспертизы";
						case "28":
							return "другие нарушения";
						default:
							return hospital_breach_code;
					}
				} set {
					if (value != hospital_breach_code)
						hospital_breach_code = value;
				}
			}
			public string HOSPITAL_BREACH_DT { get; set; }
		}

		public class LNResult {
			private string mse_result;
			public string MSE_RESULT {
				get {
					switch (mse_result) {
						case "31":
							return "продолжает болеть";
						case "32":
							return "установлена инвалидность";
						case "33":
							return "изменена группа инвалидности";
						case "34":
							return "умер";
						case "35":
							return "отказ от проведения медико-социальной экспертизы";
						case "36":
							return "явился трудоспособным";
						case "37":
							return "долечивание";
						default:
							return mse_result;
					}
				} set {
					if (value != mse_result)
						mse_result = value;
				}
			}
			public string OTHER_STATE_DT { get; set; }
			public string RETURN_DATE_LPU { get; set; }
			public string NEXT_LN_CODE { get; set; }
		}

		public class TreatFullPeriod {
			public string TREAT_CHAIRMAN_ROLE { get; set; }
			public string TREAT_CHAIRMAN { get; set; }

			public List<TreatPeriod> TreatPeriods =
				new List<TreatPeriod>();

			public class TreatPeriod {
				public string TREAT_DT1 { get; set; }
				public string TREAT_DT2 { get; set; }
				public string TREAT_DOCTOR_ROLE { get; set; }
				public string TREAT_DOCTOR { get; set; }
			}
		}

		public Dictionary<string, object> GetDictionary() {
			List<string> TREAT_PERIOD_TREAT_DT1 = new List<string>();
			List<string> TREAT_PERIOD_TREAT_DT2 = new List<string>();
			List<string> TREAT_PERIOD_TREAT_DOCTOR_ROLE = new List<string>();
			List<string> TREAT_PERIOD_TREAT_DOCTOR = new List<string>();

			foreach (ItemDisabilitySheet.TreatFullPeriod fullPeriod in TreatFullPeriods) {
				TREAT_PERIOD_TREAT_DT1.AddRange(fullPeriod.TreatPeriods.Select(x => x.TREAT_DT1).ToList());
				TREAT_PERIOD_TREAT_DT2.AddRange(fullPeriod.TreatPeriods.Select(x => x.TREAT_DT2).ToList());
				TREAT_PERIOD_TREAT_DOCTOR_ROLE.AddRange(fullPeriod.TreatPeriods.Select(x => x.TREAT_DOCTOR_ROLE).ToList());
				TREAT_PERIOD_TREAT_DOCTOR.AddRange(fullPeriod.TreatPeriods.Select(x => x.TREAT_DOCTOR).ToList());
			}

			Dictionary<string, object> dictionary = new Dictionary<string, object>() {
				{ "@SNILS", SNILS },
				{ "@SURNAME", SURNAME },
				{ "@NAME", NAME },
				{ "@PATRONIMIC", PATRONIMIC },
				{ "@BOZ_FLAG", BOZ_FLAG },
				{ "@LPU_EMPLOYER", LPU_EMPLOYER },
				{ "@LPU_EMPL_FLAG", LPU_EMPL_FLAG },
				{ "@LN_CODE", LN_CODE },
				{ "@PREV_LN_CODE", PREV_LN_CODE },
				{ "@PRIMARY_FLAG", PRIMARY_FLAG },
				{ "@DUPLICATE_FLAG", DUPLICATE_FLAG },
				{ "@LN_DATE", LN_DATE },
				{ "@LPU_NAME", LPU_NAME },
				{ "@LPU_ADDRESS", LPU_ADDRESS },
				{ "@LPU_OGRN", LPU_OGRN },
				{ "@BIRTHDAY", BIRTHDAY },
				{ "@GENDER", GENDER },
				{ "@REASON1", REASON1 },
				{ "@REASON2", REASON2 },
				{ "@REASON3", REASON3 },
				{ "@DIAGNOS", DIAGNOS },
				{ "@PARENT_CODE", PARENT_CODE },
				{ "@DATE1", DATE1 },
				{ "@DATE2", DATE2 },
				{ "@VOUCHER_NO", VOUCHER_NO },
				{ "@VOUCHER_OGRN", VOUCHER_OGRN },
				{ "@SERV1_AGE", SERV1_AGE },
				{ "@SERV1_MM", SERV1_MM },
				{ "@SERV1_RELATION_CODE", SERV1_RELATION_CODE },
				{ "@SERV1_FIO", SERV1_FIO },
				{ "@SERV2_AGE", SERV2_AGE },
				{ "@SERV2_MM", SERV2_MM },
				{ "@SERV2_RELATION_CODE", SERV2_RELATION_CODE },
				{ "@SERV2_FIO", SERV2_FIO },
				{ "@PREGN12W_FLAG", PREGN12W_FLAG },
				{ "@HOSPITAL_DT1", HOSPITAL_DT1 },
				{ "@HOSPITAL_DT2", HOSPITAL_DT2 },
				{ "@HOSPITAL_BREACH_CODE", string.Join(";", HospitalBreachs.Select(x => x.HOSPITAL_BREACH_CODE).ToList()) },
				{ "@HOSPITAL_BREACH_DT", string.Join(";", HospitalBreachs.Select(x => x.HOSPITAL_BREACH_DT).ToList()) },
				{ "@MSE_DT1", MSE_DT1 },
				{ "@MSE_DT2", MSE_DT2 },
				{ "@MSE_DT3", MSE_DT3 },
				{ "@MSE_INVALID_GROUP", MSE_INVALID_GROUP },
				{ "@MSE_RESULT", string.Join(";", LNResults.Select(x => x.MSE_RESULT).ToList()) },
				{ "@OTHER_STATE_DT", string.Join(";", LNResults.Select(x => x.OTHER_STATE_DT).ToList()) },
				{ "@RETURN_DATE_LPU", string.Join(";", LNResults.Select(x => x.RETURN_DATE_LPU).ToList()) },
				{ "@NEXT_LN_CODE", string.Join(";", LNResults.Select(x => x.NEXT_LN_CODE).ToList()) },
				{ "@LN_STATE", LN_STATE },
				{ "@LN_HASH", LN_HASH },
				{ "@TREAT_CHAIRMAN_ROLE", string.Join(";", TreatFullPeriods.Select(x => x.TREAT_CHAIRMAN_ROLE).ToList()) },
				{ "@TREAT_CHAIRMAN", string.Join(";", TreatFullPeriods.Select(x => x.TREAT_CHAIRMAN).ToList()) },
				{ "@TREAT_DT1", string.Join(";", TREAT_PERIOD_TREAT_DT1) },
				{ "@TREAT_DT2", string.Join(";", TREAT_PERIOD_TREAT_DT2) },
				{ "@TREAT_DOCTOR_ROLE", string.Join(";", TREAT_PERIOD_TREAT_DOCTOR_ROLE) },
				{ "@TREAT_DOCTOR", string.Join("; ", TREAT_PERIOD_TREAT_DOCTOR) }
			};

			return dictionary;
		}

		public string[] GetValueArray() {
			Dictionary<string, object> dict = GetDictionary();
			List<string> list = new List<string>();
			foreach (object item in dict.Values)
				if (item == null)
					list.Add(string.Empty);
				else
					list.Add(item.ToString());

			return list.ToArray();
		}
	}
}

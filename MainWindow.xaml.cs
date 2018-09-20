using System;
using System.Windows;
using JPKFVHurt.CSharp;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace JPKFVHurt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] cele = { "Po raz pierwszy", "Korekta" };
        private JPKFA.JPKFA fa;
        private Config.ConfigWin configWindow;
        private byte koniec = 0; //0 - trwa, 1 - powodzenie, 2 - niepowodzenie

        public MainWindow()
        {
            InitializeComponent();
            bool isLic = true;
            try
            {
                configWindow = new Config.ConfigWin();
                Cb_cel.ItemsSource = cele;
                Cb_cel.SelectedItem = cele[0];
                Cb_kodUrzedu.ItemsSource = Const.urzedy;
                Cb_kodUrzedu.SelectedItem = Const.urzedy[0];
                Cb_waluta.ItemsSource = Enum.GetNames(typeof(CurrCode_Type));
                Cb_waluta.SelectedItem = Cb_waluta.Items[(int)CurrCode_Type.PLN];

                Cb_country.ItemsSource = Enum.GetNames(typeof(TKodKraju));
                Cb_country.SelectedItem = Cb_country.Items[(int)TKodKraju.PL];

                Cb_woj.ItemsSource = Const.wojewodztwaTab;
                Cb_woj.SelectedItem = Const.wojewodztwaTab[0];

                Dp_dateCr.SelectedDate = DateTime.Today;
                Dp_dateFrom.SelectedDate = MainStatic.FirstDayOfMonth(DateTime.Today.AddMonths(-1));
                Dp_dateTo.SelectedDate = MainStatic.LastDayOfMonth(DateTime.Today.AddMonths(-1));
                SetData();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Brak licencji")
                {
                    isLic = false;
                }
                else
                {
                    MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            if (!isLic)
            {
                TabFA.IsEnabled = false;

                MessageBoxResult res = MessageBox.Show("Brak licencji.\nSkontaktuj się z firmą Logotech AA", "Brak licencji", MessageBoxButton.OK, MessageBoxImage.Error);
                if (res == MessageBoxResult.Cancel || res == MessageBoxResult.OK) Close();
            }
        }

        #region Generowanie JPK_FA

        private bool CheckAddress(TAdresPolski address)
        {
            return !(string.IsNullOrEmpty(address.KodPocztowy) ||
                string.IsNullOrEmpty(address.Miejscowosc) ||
                string.IsNullOrEmpty(address.NrDomu) ||
                string.IsNullOrEmpty(address.Poczta) ||
                string.IsNullOrEmpty(address.Powiat) ||
                string.IsNullOrEmpty(address.Ulica) ||
                string.IsNullOrEmpty(address.Wojewodztwo) ||
                string.IsNullOrEmpty(address.Gmina));
        }

        private void B_create_Click(object sender, RoutedEventArgs e)
        {
            l_kodForm.Content = "JPK_FA (1)";
            l_warForm.Content = "1-0";

            if (MainStatic.PathIsOK(configWindow.ConfField.AppPath, JPKGenerator.GetCode(configWindow.ConfField.CodeName)))
            {
                string uzupelnij = "";
                if (Cb_kodUrzedu.SelectedItem == null)
                { uzupelnij += "\nKod urzędu"; }
                if (Cb_waluta.SelectedItem == null)
                { uzupelnij += "\nDomyślny kod waluty"; }
                if (Dp_dateFrom.SelectedDate == null)
                { uzupelnij += "\nOkres od"; }
                if (Dp_dateTo.SelectedDate == null)
                { uzupelnij += "\nOkres do"; }
                if (Dp_dateCr.SelectedDate == null)
                { uzupelnij += "\nData utworzenia"; }
                if (uzupelnij.Length != 0)
                {
                    lsb.Content = "Nie wszystkie pola zostały uzupełnione.";
                    MessageBox.Show("Uzupełnij brakujące pola:" + uzupelnij, "Brak danych", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    JPKPodmiot1 podmiot = GetPodmiot();
                    if (!CheckAddress(podmiot.AdresPodmiotu))
                    {
                        MessageBox.Show("Uzupełnij adres", "Brak danych", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        try
                        {
                            fa = new JPKFA.JPKFA()
                            {
                                //Naglowek
                                CelZlozenia = Cb_cel.SelectedItem.ToString() == cele[0] ? (sbyte)1 : (sbyte)2,
                                Kodus = (TKodUS)Cb_kodUrzedu.SelectedValue,
                                DateFrom = (DateTime)Dp_dateFrom.SelectedDate,
                                DateTo = (DateTime)Dp_dateTo.SelectedDate,
                                CodingName = JPKGenerator.GetCode(configWindow.ConfField.CodeName),
                                DbPath = configWindow.ConfField.AppPath,
                            };
                            DateTime now = (DateTime)Dp_dateCr.SelectedDate;
                            fa.CrDate = new DateTime(now.Year, now.Month, now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                            fa.Jpk.Podmiot1 = podmiot;

                            System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
                            worker.RunWorkerCompleted += Worker_RunWorkerCompletedFA;
                            worker.WorkerReportsProgress = true;
                            worker.DoWork += Worker_DoWorkFA;
                            worker.ProgressChanged += Worker_ProgressChangedFA;
                            worker.RunWorkerAsync();
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "Obiekt dopuszczający wartość pustą musi mieć wartość.")
                                MessageBox.Show("Wybierz datę", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                            else MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }

            }
            else
            {
                MessageBox.Show("Błędna ścieżka programu Hurt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                lsb.Content = "Błędna ścieżka programu Hurt.";
            }
        }

        private void Worker_ProgressChangedFA(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            TabFA.IsEnabled = false;
            lsb.Content = (string)e.UserState;
        }

        private void Worker_DoWorkFA(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                var worker = sender as System.ComponentModel.BackgroundWorker;
                worker.ReportProgress(0, string.Format("Generowanie pliku JPK"));
                fa.Create(worker);
                koniec = 1;
            }
            catch (Exception ex)
            {
                MainStatic.ShowException(ex, "Woker_DoWorkFA()");
                koniec = 2;
            }
        }

        private void Worker_RunWorkerCompletedFA(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            TabFA.IsEnabled = koniec != 0;
        }

        #endregion
        private void Dp_dateTo_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if ((DateTime)e.Source <= Dp_dateFrom.SelectedDate)
                {
                    Dp_dateTo.SelectedDate = (DateTime)e.OriginalSource;
                    MainStatic.ShowException(new Exception("Data do nie może być wcześniejsza niż data od"), "Błąd");
                }
            }
            catch { }
        }

        private void Dp_dateFrom_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if ((DateTime)e.Source >= Dp_dateTo.SelectedDate)
                {
                    Dp_dateFrom.SelectedDate = (DateTime)e.OriginalSource;
                    MainStatic.ShowException(new Exception("Data do nie może być późniejsza niż data do"), "Błąd");
                }
            }
            catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Application.Current.Shutdown();

         private void Tb_REGON_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tb_REGON.Text.Length != 0)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(Tb_REGON.Text, @"\d{9}") || System.Text.RegularExpressions.Regex.IsMatch(Tb_REGON.Text, @"\d{14}"))
                {
                    if (!MainStatic.Cyfrakontrolna(Tb_REGON.Text))
                        MessageBox.Show("Niepoprawny REGON ", "Błędny REGON", MessageBoxButton.OK, MessageBoxImage.Warning);
                    try
                    {
                        if (fa != null) fa.Jpk.Podmiot1.IdentyfikatorPodmiotu.REGON = Tb_REGON.Text;
                    }
                    catch { }
                }
                else
                {
                    MessageBox.Show("REGON musi mieć 9 lub 14 cyfr", "Błędny REGON", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Tb_REGON.Text = "";
                    Tb_REGON.BorderBrush = System.Windows.Media.Brushes.DarkRed;
                }
            }
        }

        private void Cb_kodUrzedu_PatternChanged(object sender, cb_AutoComplete.AutoCompleteArgs args)
        {
            if (string.IsNullOrEmpty(args.Pattern))
                args.CancelBinding = true;
            else
                args.DataSource = GetUrzedy(args.Pattern);
        }

        private static ObservableCollection<US> GetUrzedy(string pattern)
        {
            List<US> listTemp = new List<US>();
            foreach (US urzad in Const.urzedy)
                if (urzad.Nazwa.ToLower().Contains(pattern.ToLower()))
                    listTemp.Add(urzad);
            return new ObservableCollection<US>(listTemp);
        }

        private void Cb_woj_PatternChanged(object sender, cb_AutoComplete.AutoCompleteArgs args)
        {
            if (string.IsNullOrEmpty(args.Pattern))
                args.CancelBinding = true;
            else
                args.DataSource = GetWojewodztwa(args.Pattern);
        }

        private static ObservableCollection<Wojewodztwo> GetWojewodztwa(string Pattern)
        {
            List<Wojewodztwo> listaTemp = new List<Wojewodztwo>();
            foreach (Wojewodztwo woj in Const.wojewodztwaTab)
                if (woj.Nazwa.ToLower().Contains(Pattern.ToLower()))
                    listaTemp.Add(woj);

            return new ObservableCollection<Wojewodztwo>(listaTemp);
        }

        private void B_saveCompData_Click(object sender, RoutedEventArgs e)
        {
            Config.Podmiot newPodmiot = new Config.Podmiot
            {
                KodUS = (TKodUS)Cb_kodUrzedu.SelectedValue,
                Id = new Config.ID
                {
                    NIP = (string)Tb_NIP.Content,
                    PelnaNazwa = tb_fullName.Text,
                    REGON = Tb_REGON.Text
                },
                Adres = new Config.Adres
                {
                    Gmina = tb_gmina.Text,
                    KodPocztowy = string.Format("{0}-{1}", tb_kodP1.Text, tb_kodP2.Text),
                    Miejscowosc = tb_city.Text,
                    NrDomu = tb_nrDom.Text,
                    NrLokalu = tb_nrLok.Text,
                    Poczta = tb_poczta.Text,
                    Powiat = tb_powiat.Text,
                    Ulica = tb_ulica.Text,
                    Wojewodztwo = Cb_woj.SelectedValue.ToString()
                }
            };
            configWindow.SetDanePodmiotu(newPodmiot);            
        }

        private void SetData()
        {
            if (configWindow.ConfField.AppPath != "")
            {
                try
                {
                    fa = new JPKFA.JPKFA
                    {
                        DbPath = configWindow.ConfField.AppPath,
                        CodingName = JPKGenerator.GetCode(configWindow.ConfField.CodeName),
                        Jpk = new JPKFA.JPK()
                    };
                    Cb_kodUrzedu.SelectedValue = configWindow.ConfField.CompanyData.KodUS;
                    fa.Jpk.Podmiot1 = configWindow.ConfField.CompanyData.ToJPKPodmiot1();
                    FillSender(fa.Jpk.Podmiot1);
                                        
                    Cb_kodUrzedu.SelectedValue = configWindow.ConfField.CompanyData.KodUS;                   

                    B_create.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void FillSender(JPKPodmiot1 jpkP = null)
        {
            if (jpkP == null)
            {
                jpkP = new JPKPodmiot1
                {
                    IdentyfikatorPodmiotu = new TIdentyfikatorOsobyNiefizycznej(),
                    AdresPodmiotu = new TAdresPolski()
                };
            }
            tb_fullName.Text = jpkP.IdentyfikatorPodmiotu.PelnaNazwa;
            Tb_REGON.Text = jpkP.IdentyfikatorPodmiotu.REGON ?? "";
            tb_ulica.Text = jpkP.AdresPodmiotu.Ulica;
            tb_nrDom.Text = jpkP.AdresPodmiotu.NrDomu;
            tb_nrLok.Text = jpkP.AdresPodmiotu.NrLokalu ?? "";
            tb_kodP1.Text = jpkP.AdresPodmiotu.KodPocztowy.Split('-')[0];
            tb_kodP2.Text = tb_kodP1.Text == "" ? "" : jpkP.AdresPodmiotu.KodPocztowy.Split('-')[1];
            tb_city.Text = jpkP.AdresPodmiotu.Miejscowosc;
            tb_poczta.Text = jpkP.AdresPodmiotu.Poczta;
            tb_gmina.Text = jpkP.AdresPodmiotu.Gmina;
            tb_powiat.Text = jpkP.AdresPodmiotu.Powiat;
            try
            {
                Cb_woj.Text = jpkP.AdresPodmiotu.Wojewodztwo.ToLower();
                Cb_woj.SelectedValue = jpkP.AdresPodmiotu.Wojewodztwo.ToLower();
            }
            catch { }
            Cb_country.SelectedItem = Cb_country.Items[(int)jpkP.AdresPodmiotu.KodKraju];
        }

        private JPKPodmiot1 GetPodmiot()
        {
            return new JPKPodmiot1()
            {
                IdentyfikatorPodmiotu = new TIdentyfikatorOsobyNiefizycznej()
                {
                    NIP = (string)Tb_NIP.Content,
                    PelnaNazwa = tb_fullName.Text,
                    REGON = Tb_REGON.Text
                },
                AdresPodmiotu = new TAdresPolski()
                {
                    Ulica = tb_ulica.Text,
                    NrDomu = tb_nrDom.Text,
                    NrLokalu = tb_nrLok.Text == "" ? null : tb_nrLok.Text,
                    KodPocztowy = tb_kodP1.Text + "-" + tb_kodP2.Text,
                    Miejscowosc = tb_city.Text,
                    Poczta = tb_poczta.Text,
                    Gmina = tb_gmina.Text,
                    Powiat = tb_powiat.Text,
                    Wojewodztwo = Cb_woj.SelectedValue as string
                }
            };
        }       
    }
}

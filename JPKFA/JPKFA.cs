using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using JPKFVHurt.CSharp;
using SocialExplorer.IO.FastDBF; /* biblioteka FastDBF jest to biblioteka udostępniona na zasadach licencji openSource i
                                    służy zarządzniu DBFami.*/

namespace JPKFVHurt.JPKFA
{
    class JPKFA : JPKMethods
    {
        public string Xml { get; set; }
        public bool isMK = false; //metoda kasowa
        public JPK Jpk;
        public Encoding CodingName;
        public string DbPath;
        public TKodUS Kodus;
        public sbyte CelZlozenia;
        /// <summary>
        /// Data - zakres od
        /// </summary>
        public DateTime DateFrom;
        /// <summary>
        /// Data - zakres do
        /// </summary>
        public DateTime DateTo;
        /// <summary>
        /// Data wytworzenia JPK
        /// </summary>
        public DateTime CrDate;

        public JPKFA()
        {
            Jpk = new JPK()
            {
                FakturaCtrl = new JPKFakturaCtrl(),
                FakturaWierszCtrl = new JPKFakturaWierszCtrl(),                
                Podmiot1 = JPKBase.CreatePodmiot(),
            };
        }

        /// <summary>
        /// Tworzenie JPK
        /// </summary>
        /// <param name="worker">Pasek ładownia</param>
        public void Create(BackgroundWorker worker)
        {
            Jpk.Naglowek = CreateNaglowek();
            try { Jpkinit(worker); } catch (Exception ex) { throw ex; }
            Xml = CreateXLS(Jpk, typeof(JPK), NameSpacesFA.Default);
        }

        private void Jpkinit(BackgroundWorker worker)
        {
            try
            {
                worker.ReportProgress(0, string.Format("Generowanie nagłówka"));
                Jpk.Faktura = CreateFaktura(worker);
                Jpk.StawkiPodatku = new JPKStawkiPodatku();
                
                worker.ReportProgress(100, string.Format("Jednolity plik kontrolny został wygenerowany."));
            }
            catch (Exception ex)
            {
                worker.ReportProgress(100, string.Format("Jednolity plik kontrolny NIE został wygenerowany."));
                throw ex;
            }
        }

        private List<JPKFakturaWiersz> CreateFakturaWiersz(BackgroundWorker worker, ref List<JPKFaktura> tabDoc,  List<Product> products)
        {
            List<JPKFakturaWiersz> tab = new List<JPKFakturaWiersz>();
            
            var db = new DbfFile(CodingName);
            int wzSize = 0;

            string fail = "";
            try
            {
                double index = 0;
                db.Open(Path.Combine(DbPath, "WZA.DBF"), FileMode.Open);
                JPKGenerator.SizeDB(db, ref wzSize);
                var dbRow = new DbfRecord(db.Header);

                if (tab.Count == 0)
                {
                    db.Read(0, dbRow);
                    do
                    {
                        string failNr = dbRow["RF"].Trim() + "/" + dbRow["NRDOK"].Trim();
                        try
                        {
                            if (tabDoc.Exists(d => d.P_2A == failNr) && !dbRow.IsDeleted)
                            {
                                int indexDoc = tabDoc.FindIndex(d => d.P_2A == failNr);
                                JPKFakturaWiersz row = new JPKFakturaWiersz();
                                fail = "P_2B";
                                row.P_2B = failNr; // numer faktury
                                fail = "P_12";
                                row.P_12 = JPKGenerator.GetVat
                                    (dbRow["PTU"]); // stawka vat
                                fail = "P_7";
                                Product prod = products.Find(pr => pr.Code == dbRow["SYMB"].Trim());
                                row.P_7 = prod.Name;
                                fail = "P_8A";
                                row.P_8A = prod.Quantity;
                                fail = "P_8B";
                                row.P_8B = JPKGenerator.GetDecimal(dbRow["ILOSC"]); // ilosc

                                decimal brutto = JPKGenerator.GetDecimal(dbRow["WARTOSC"]);
                                decimal netto = 0;
                                decimal vat = 0;

                                if (dbRow["RF"].Trim() == "F") // od netto
                                {
                                    row.P_9A = JPKGenerator.GetDecimal(dbRow["CENA"]); // cena jednostkowa netto
                                    netto = decimal.Parse((row.P_9A * row.P_8B).ToString("0.00"));
                                    vat = brutto - netto;
                                }
                                else // od brutto
                                {
                                    decimal stawkaVat = 0;
                                    try { stawkaVat = JPKGenerator.GetDecimal(dbRow["PTU"]); } catch { }
                                    netto = stawkaVat != 0 ? (brutto * 100) / (stawkaVat + 100) : brutto;
                                    if (netto != 0) row.P_9A = netto / row.P_8B;
                                    vat = brutto - netto;
                                }
                                fail = "P_11";
                                row.P_11 = row.P_8B * row.P_9A;
                                if (dbRow["RF"].Trim() != "F") row.P_11A = brutto; // wartosc brutto, gdy faktura liczona jest od brutto

                                fail = "vaty";
                                switch (row.P_12)
                                {
                                    case JPKFakturaWierszP_12.Item22:
                                    case JPKFakturaWierszP_12.Item23:
                                        tabDoc[indexDoc].P_13_1 = tabDoc[indexDoc].P_13_1 + netto;
                                        tabDoc[indexDoc].P_14_1 = tabDoc[indexDoc].P_14_1 + vat;
                                        break;
                                    case JPKFakturaWierszP_12.Item8:
                                    case JPKFakturaWierszP_12.Item7:
                                        tabDoc[indexDoc].P_13_2 = tabDoc[indexDoc].P_13_2 + netto;
                                        tabDoc[indexDoc].P_14_2 = tabDoc[indexDoc].P_14_1 + vat;
                                        break;
                                    case JPKFakturaWierszP_12.Item5:
                                        tabDoc[indexDoc].P_13_3 = tabDoc[indexDoc].P_13_3 + netto;
                                        tabDoc[indexDoc].P_14_3 = tabDoc[indexDoc].P_14_3 + vat;
                                        break;
                                    case JPKFakturaWierszP_12.Item0:
                                        tabDoc[indexDoc].P_13_6 = tabDoc[indexDoc].P_13_6 + netto;
                                        break;
                                    case JPKFakturaWierszP_12.zw:
                                        tabDoc[indexDoc].P_13_7 = tabDoc[indexDoc].P_13_7 + netto;
                                        tabDoc[indexDoc].P_19 = true;
                                        tabDoc[indexDoc].P_19A = @"Ustawa z dnia 11.03.2004 o podatku od towarów i usług, art. 43 ust. 1";
                                        break;
                                    case JPKFakturaWierszP_12.np:
                                        tabDoc[indexDoc].P_13_4 = tabDoc[indexDoc].P_13_4 + netto;
                                        tabDoc[indexDoc].P_18 = true;
                                        break;
                                }

                                tab.Add(row);
                            }
                        }
                        catch (Exception ex)
                        {
                            MainStatic.ShowException(ex, "Bład pozycji faktury nr: " + failNr + "\n" + fail);
                        }
                        index++;
                        worker.ReportProgress((int)Math.Ceiling((index / wzSize) * 100), string.Format("Przetwarzanie pozycji faktur: {0}%", (int)Math.Ceiling((index / wzSize) * 100)));
                    }
                    while (db.ReadNext(dbRow));
                }                
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Błędne kodowanie")) throw new Exception("Zmień kodowanie");
                throw ex;
            }
            finally
            {
                db.Close();
            }

            return tab;
        }

        private void CreateFakturaWierszKorekta(BackgroundWorker worker, List<Product> products, ref List<JPKFaktura> tabDoc, ref List<JPKFakturaWiersz> tab)
        {
            var db = new DbfFile(CodingName);
            int wzSize = 0;
            
            string fail = "";
            try
            {
                double index = 0;
                db.Open(Path.Combine(DbPath, "ZWA.DBF"), FileMode.Open);
                JPKGenerator.SizeDB(db, ref wzSize);
                var dbRow = new DbfRecord(db.Header);

                db.Read(0, dbRow);
                do
                {
                    string failNr = "";
                    try
                    {
                        if (tabDoc.Exists(f => f.P_2A == dbRow["NRDOK"].Trim() && f.RodzajFaktury == JPKFakturaRodzajFaktury.KOREKTA) && !dbRow.IsDeleted)
                        {
                            failNr = dbRow["NRDOK"].Trim();
                            int indexKor = tabDoc.FindIndex(f => f.P_2A == dbRow["NRDOK"].Trim() && f.RodzajFaktury == JPKFakturaRodzajFaktury.KOREKTA);
                            int typ = int.Parse(dbRow["TYP"].Trim());
                            JPKFakturaWiersz row = new JPKFakturaWiersz();
                            fail = "P_2B";
                            row.P_2B = dbRow["NRDOK"].Trim(); // numer faktury
                            fail = "P_12";
                            row.P_12 = JPKGenerator.GetVat(dbRow["PTU"].Trim()); // stawka vat
                            fail = "P_7";
                            Product prod = products.Find(pr => pr.Code == dbRow["SYMB"].Trim()); // towar / usługa
                            row.P_7 = prod.Name;
                            fail = "P_8A";
                            row.P_8A = prod.Quantity;
                            fail = "ilosc";
                            row.P_8B = JPKGenerator.GetDecimal(dbRow["ILOSC"]); // ilosc

                            fail = "nettoU";
                            row.P_9A = JPKGenerator.GetDecimal(dbRow["CENA"]); // cena jednostkowa netto
                            fail = "stawkaVAT";
                            decimal stawkaVat = 0;
                            try { stawkaVat = JPKGenerator.GetDecimal(dbRow["PTU"]); } catch { }
                            if (dbRow["RF"].Trim() != "F") row.P_9A = (stawkaVat != 0 ? (row.P_9A * 100) / (stawkaVat + 100) : row.P_9A) / row.P_8B;

                            switch (typ)
                            {
                                case 1:
                                    fail = "typ1";
                                    row.P_8B = -row.P_8B;
                                    row.P_11 = row.P_9A * -row.P_8B;
                                    if (dbRow["RF"].Trim() != "F") row.P_11A = JPKGenerator.GetDecimal(dbRow["WARTOSC"]);
                                    break;
                                case 2:
                                    fail = "typ2";
                                    if ((decimal.Parse(dbRow["WARTKOR"].Trim().Replace(".", ",")) * stawkaVat * (decimal)0.01).ToString("0.00").Replace(",", ".") == dbRow["WARTKORV"].Trim())
                                    {
                                        row.P_11 = -JPKGenerator.GetDecimal(dbRow["WARTKOR"]);
                                        if (dbRow["RF"].Trim() != "F") row.P_11A = -JPKGenerator.GetDecimal(dbRow["WARTKOR"]) + JPKGenerator.GetDecimal(dbRow["WARTKORV"]);
                                    }
                                    else
                                    {
                                        row.P_11 = -JPKGenerator.GetDecimal(dbRow["WARTKOR"]) - JPKGenerator.GetDecimal(dbRow["WARTKORV"]);
                                        if (dbRow["RF"].Trim() != "F") row.P_11A = -JPKGenerator.GetDecimal(dbRow["WARTKOR"]);
                                    }
                                    break;
                                case 3:
                                    fail = "typ3";
                                    if (JPKGenerator.GetDecimal(dbRow["WARTKOR"]) * stawkaVat * (decimal)0.01 == JPKGenerator.GetDecimal(dbRow["WARTKORV"]))
                                    {
                                        row.P_11 = JPKGenerator.GetDecimal(dbRow["WARTKOR"]);
                                        if (dbRow["RF"].Trim() != "F") row.P_11A = JPKGenerator.GetDecimal(dbRow["WARTKOR"]) + JPKGenerator.GetDecimal(dbRow["WARTKORV"]);
                                    }
                                    else
                                    {
                                        row.P_11 = JPKGenerator.GetDecimal(dbRow["WARTKOR"]) - JPKGenerator.GetDecimal(dbRow["WARTKORV"]);
                                        row.P_11A = JPKGenerator.GetDecimal(dbRow["WARTKOR"]);
                                    }
                                    break;
                                case 4:
                                    fail = "typ4";
                                    row.P_11 = 0;
                                    break;
                            }
                            fail = "stawka";
                            switch (row.P_12)
                            {
                                case JPKFakturaWierszP_12.Item22:
                                case JPKFakturaWierszP_12.Item23:
                                    tabDoc[indexKor].P_13_1 = tabDoc[indexKor].P_13_1 + row.P_11;
                                    tabDoc[indexKor].P_14_1 = row.P_11 * JPKGenerator.GetVatValue(row.P_12);
                                    break;
                                case JPKFakturaWierszP_12.Item8:
                                case JPKFakturaWierszP_12.Item7:
                                    tabDoc[indexKor].P_13_2 = tabDoc[indexKor].P_13_2 + row.P_11;
                                    tabDoc[indexKor].P_14_1 = row.P_11 * JPKGenerator.GetVatValue(row.P_12);
                                    break;
                                case JPKFakturaWierszP_12.Item5:
                                    tabDoc[indexKor].P_13_3 = tabDoc[indexKor].P_13_3 + row.P_11;
                                    tabDoc[indexKor].P_14_1 = row.P_11 * JPKGenerator.GetVatValue(row.P_12);
                                    break;
                                case JPKFakturaWierszP_12.Item0:
                                    tabDoc[indexKor].P_13_6 = tabDoc[indexKor].P_13_6 + row.P_11;
                                    break;
                                case JPKFakturaWierszP_12.zw:
                                    tabDoc[indexKor].P_13_7 = tabDoc[indexKor].P_13_7 + row.P_11;
                                    tabDoc[indexKor].P_19 = true;
                                    tabDoc[indexKor].P_19A = @"Ustawa z dnia 11.03.2004 o podatku od towarów i usług, art. 43 ust. 1";
                                    break;
                            }
                            tab.Add(row);

                        }
                    }
                    catch (Exception ex)
                    {
                        MainStatic.ShowException(ex, "Bład pozycji faktury nr: " + failNr + "\n" + fail);
                    }
                    index++;
                    worker.ReportProgress((int)Math.Ceiling((index / wzSize) * 100), string.Format("Przetwarzanie pozycji faktur: {0} %", (int)Math.Ceiling((index / wzSize) * 100)));
                }
                while (db.ReadNext(dbRow));
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Błędne kodowanie"))
                {
                    throw new Exception("Zmień kodowanie", ex);
                }
                throw ex;
            }
            finally
            {
                db.Close();
            }
        }

        private JPKFaktura[] CreateFaktura(BackgroundWorker worker)
        {
            try
            {
                /// FV
                List<Product> products = GetProducts(DbPath, CodingName);
                if (products.Count == 0) throw new Exception("Wystąpił błąd podczas analizowania danych. Uruchom program ponownie.");
                Dictionary<string, Config.Podmiot> castomers = GetCustomers(worker, DbPath, CodingName);
                if (castomers.Count == 0) throw new Exception("Wystąpił błąd podczas analizowania danych. Uruchom program ponownie.");
                List<JPKFaktura> tabFaktura = new List<JPKFaktura>();
                GetFA(worker, castomers, ref tabFaktura);
                List<JPKFakturaWiersz> tabFakturaWiersz = CreateFakturaWiersz(worker, ref tabFaktura, products);

                /// KFV

                Jpk.FakturaWierszCtrl.LiczbaWierszyFaktur = tabFakturaWiersz.Count.ToString();
                GetKFA(worker, castomers, ref tabFaktura);
                CreateFakturaWierszKorekta(worker, products, ref tabFaktura, ref tabFakturaWiersz);


                foreach (JPKFakturaWiersz w in tabFakturaWiersz)
                {
                    Jpk.FakturaWierszCtrl.WartoscWierszyFaktur += w.P_11;
                }
                Jpk.FakturaWierszCtrl.LiczbaWierszyFaktur = tabFakturaWiersz.Count.ToString();

                foreach (JPKFaktura f in tabFaktura)
                {
                    Jpk.FakturaCtrl.WartoscFaktur += f.P_15;
                }
                Jpk.FakturaCtrl.LiczbaFaktur = tabFaktura.Count.ToString();

                Jpk.FakturaWiersz = tabFakturaWiersz.ToArray();
                return tabFaktura.ToArray();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void GetFA(BackgroundWorker worker, Dictionary<string, Config.Podmiot> castomers, ref List<JPKFaktura> tab)
        {
            var db = new DbfFile(CodingName);
            int wzSize = 0;
            string fail = "";
            try
            {
                double index = 0;
                db.Open(Path.Combine(DbPath, "WZ.DBF"), FileMode.Open);
                JPKGenerator.SizeDB(db, ref wzSize);
                var dbRow = new DbfRecord(db.Header);

                db.Read(0, dbRow);
                do
                {
                    string failNr = dbRow["RF"].Trim() + "/" + dbRow["NRDOK"].Trim();

                    if (JPKGenerator.GetDate(dbRow["DATA"]) >= DateFrom && JPKGenerator.GetDate(dbRow["DATA"]) <= DateTo && !dbRow.IsDeleted && failNr.ToLower()[0] != 'w')
                    {
                        try
                        {
                            JPKFaktura row = new JPKFaktura();
                            fail = "P_1";
                            row.P_1 = dbRow["DATA"];
                            fail = "P_2";
                            row.P_2A = failNr;
                            fail = "P_3A";
                            Config.Podmiot p = castomers[dbRow["KOD_K"].Trim()];
                            row.P_3A = p.Id.PelnaNazwa;
                            fail = "P_3B";
                            row.P_3B = string.Format("{0}, {1} {2}", p.Adres.Ulica, p.Adres.KodPocztowy, p.Adres.Miejscowosc);
                            row.P_3C = Jpk.Podmiot1.IdentyfikatorPodmiotu.PelnaNazwa;
                            row.P_3D = Jpk.Podmiot1.AdresPodmiotu.Ulica + " " + Jpk.Podmiot1.AdresPodmiotu.NrDomu;
                            row.P_3D += (string.IsNullOrEmpty(Jpk.Podmiot1.AdresPodmiotu.NrLokalu) ? " " : "/" + Jpk.Podmiot1.AdresPodmiotu.NrLokalu) + " " + Jpk.Podmiot1.AdresPodmiotu.Miejscowosc;
                            fail = "P_4B";
                            row.P_4B = Jpk.Podmiot1.IdentyfikatorPodmiotu.NIP;
                            if (!System.Text.RegularExpressions.Regex.IsMatch(p.Id.NIP, @"^\d"))
                            {
                                fail = "P_5A";
                                try
                                {
                                    row.P_5A = (MSCountryCode_Type)Enum.Parse(typeof(MSCountryCode_Type), p.Id.NIP.Substring(0, 2));
                                }
                                catch { }
                            }
                            else
                            {
                                fail = "P_5B";
                                row.P_5B = p.Id.NIP;
                            }
                            fail = "P_6";
                            row.P_6 = row.P_1;
                            fail = "P_13_1";
                            fail = "stawka";
                            row.P_15 = JPKGenerator.GetDecimal(dbRow["WARTOSC"]);
                            row.P_16 = isMK; // metoda kasowa
                            row.P_17 = false; //samofakturowanie
                            row.P_18 = false; // odwrotne obciążenie
                            fail = "P_19";
                            row.P_20 = false;
                            row.P_21 = false;
                            row.P_23 = false;
                            row.P_106E_2 = false;
                            row.P_106E_3 = false;
                            row.RodzajFaktury = JPKFakturaRodzajFaktury.VAT;
                            if (dbRow[39].Trim().Length > 1)
                            {
                                if (dbRow[39].Trim()[0] == 'Z')
                                {
                                    row.RodzajFaktury = JPKFakturaRodzajFaktury.ZAL;
                                    row.ZALZaplata = JPKGenerator.GetDecimal(dbRow["WARTOSC"]);
                                    row.ZALPodatek = JPKGenerator.GetDecimal(dbRow["WARTVAT"]);
                                }
                            }

                            tab.Add(row);
                        }
                        catch (Exception ex)
                        {

                            MainStatic.ShowException(ex, "Bład faktury nr: " + failNr + " \n" + fail);
                        }
                    }
                    index++;
                    worker.ReportProgress((int)Math.Ceiling((index / wzSize) * 100), string.Format("Przetwarzanie danych z faktur: {0} %", (int)Math.Ceiling((index / wzSize) * 100)));

                }
                while (db.ReadNext(dbRow));
            }
            catch (Exception ex)
            {
                throw new Exception("Zmień kodowanie", ex);
            }
            finally
            {
                db.Close();
            }
        }
    }
}

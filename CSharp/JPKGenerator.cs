using JPKFVHurt.JPKFA;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace JPKFVHurt.CSharp
{
    class JPKGenerator
    {
        public static void AddDataToDataGrid(object[] o, System.Windows.Controls.DataGrid dataGrid)
        {
            try
            {
                try { dataGrid.Items.Clear(); } catch { }
                ObservableCollection<object> oc = new ObservableCollection<object>(o);
                dataGrid.ItemsSource = oc;
                dataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Wystąpił błąd podczas dodawania danych do tabeli: {0}", ex.Message));
            }
        }

        public static void CleanDataGrid(System.Windows.Controls.DataGrid dataGrid)
        {
            try
            {
                ObservableCollection<object> oc = new ObservableCollection<object>();
                dataGrid.ItemsSource = oc;
                dataGrid.Items.Refresh();
            }
            catch (Exception ex) { throw new Exception("Wystąpił błąd podczas wczytywania danych", ex); }
        }

        public static void SaveXML(object jpk, Type T, string title, string company, string namespaceHtml, XmlSerializerNamespaces namespaces)
        {
            SaveFileDialog f = new SaveFileDialog();
            DateTime d = DateTime.Now;
            string name = string.Format("{0}_{1}_{2}", title, company,
                d.ToString("yyMMdd"));
            name = name.Replace("\"", "");
            f.FileName = name;
            f.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            f.Filter = "Pliki xml (xml)|*.xml";
            if (f.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream fs = new FileStream(string.Format(@"{0}", f.FileName), FileMode.Create))
                    {
                        XmlSerializerNamespaces ns = namespaces;
                        XmlSerializer ser = new XmlSerializer(T, namespaceHtml);
                        ser.Serialize(fs, jpk, ns);
                    }
                    MessageBox.Show("Zapis zakończony powodzeniem", "Zapis do pliku", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
        }

        public static string GetTXT(object jpk, Type T)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                XmlSerializer ser = new XmlSerializer(T);
                ser.Serialize(sw, jpk, ns);
                return sw.ToString();
            }
        }

        public static bool ReadXML(ref object jpk, Type T, ref string txt)
        {
            OpenFileDialog f = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "Pliki xml (xml)|*.xml"
            };
            if (f.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream fs = new FileStream(string.Format(@"{0}", f.FileName), FileMode.Open))
                    {
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        XmlSerializer ser = new XmlSerializer(T);
                        jpk = ser.Deserialize(fs);
                    }
                    txt = GetTXT(jpk, T);
                    return true;
                }

                catch (Exception e)
                {
                    throw e;
                }
            }
            return false;
        }

        public static decimal GetDecimal(string s)
        {
            s = s.Trim();
            if (string.IsNullOrEmpty(s)) return 0;
            s = s.Replace('.', ',');
            int mnoznik = 1;
            if (s[0] == '-')
            {
                s = s.Replace("-", "");
                mnoznik = -1;
            }
            return decimal.Parse(s) * mnoznik;
        }
        
        /// <summary>
        /// Konwertuje stringa w postaci rrrrmmdd na datę
        /// </summary>
        /// <param name="s">Postać łańcucha znaków: yyyymmdd</param>
        /// <returns></returns>
        public static DateTime GetDate(string s)
        {
            CultureInfo provider = CultureInfo.CurrentCulture;
            s.Trim();
            DateTime res = new DateTime();
            if (DateTime.TryParse(s, out res)) return res;
            string[] sTab = s.Split('.');
            if (sTab.Length == 4)
            {
                return new DateTime (int.Parse(sTab[0]), int.Parse(sTab[1]), int.Parse(sTab[2]));
            }
            return new DateTime(int.Parse(s.Substring(0,4)), int.Parse(s.Substring(4,2)), int.Parse(s.Substring(6,2)));

        }

        public static void SizeDB(SocialExplorer.IO.FastDBF.DbfFile odbf, ref int res)
        {
            var rec = new SocialExplorer.IO.FastDBF.DbfRecord(odbf.Header);
            odbf.Read(0, rec);
            do res++;
            while (odbf.ReadNext(rec));
        }

        public static Code[] codes = new Code[]
        {
            new Code { Name = "Mazovia", Encode = new MazoviaEncoding() },
            new Code { Name = "IBM852", Encode = Encoding.GetEncoding(852)},
            new Code { Name = "Latin 2", Encode = Encoding.GetEncoding(870) },
            new Code { Name = "Windows 1250", Encode = Encoding.GetEncoding(1250) }
        };

        public static Encoding GetCode(string value) => codes.Where(kod => (kod.Name == value)).First().Encode;

        public static JPKFakturaWierszP_12 GetVat(string s)
        {
            s = s.Trim();
            switch (s)
            {
                case "23":
                    return JPKFakturaWierszP_12.Item23;
                case "22":
                    return JPKFakturaWierszP_12.Item22;
                case "8":
                    return JPKFakturaWierszP_12.Item8;
                case "7":
                    return JPKFakturaWierszP_12.Item7;
                case "5":
                    return JPKFakturaWierszP_12.Item5;
                case "3":
                    return JPKFakturaWierszP_12.Item3;
                case "0":
                    return JPKFakturaWierszP_12.Item0;
                case "zw":
                    return JPKFakturaWierszP_12.zw;
                case "np":
                    return JPKFakturaWierszP_12.np;
                default:
                    throw new Exception("Błędne kodowanie lub brak stawki VAT");
            }
        }

        public static decimal GetVatValue(JPKFakturaWierszP_12 s)
        {
            try { return decimal.Parse(s.ToString().Replace("Item", "")); } catch { return 0; }
        }

        public static decimal GetVatValue(string s)
        {
            try { return decimal.Parse(s); } catch { return 0; }
        }
    }

    public class Code
    {
        public string Name { get; set; }
        public Encoding Encode { get; set; }
    }

    public class US
    {
        public string Nazwa { get; set; }
        public TKodUS KodUS { get; set; }
    }

    public class Wojewodztwo
    {
        public string Nazwa { get; set; }
    }

    public class Product
    {
        public string Code = "";
        public string Name = "";
        public string Quantity = "";
    }
}
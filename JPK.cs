using JPKFVHurt.CSharp;
using SocialExplorer.IO.FastDBF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace JPKFVHurt
{
    public partial class JPKBase : JPKMethods
    {
        public TNaglowek Naglowek;
        public JPKPodmiot1 Podmiot1;
    }
    public partial class JPKMethods
    {
        public static JPKPodmiot1 CreatePodmiot() => new JPKPodmiot1()
        {
            AdresPodmiotu = CreateAdresPodmiotu(),
            IdentyfikatorPodmiotu = CreateIdentyfikatorPodmiotu()
        };

        private static TIdentyfikatorOsobyNiefizycznej CreateIdentyfikatorPodmiotu() => new TIdentyfikatorOsobyNiefizycznej()
        {
            NIP = "",
            PelnaNazwa = "",
            REGON = "",
        };

        private static TAdresPolski CreateAdresPodmiotu() => new TAdresPolski()
        {
            Gmina = "",
            KodKraju = TKodKraju.PL,
            KodPocztowy = "",
            Miejscowosc = "",
            NrDomu = "",
            NrLokalu = "",
            Poczta = "",
            Powiat = "",
            Ulica = "",
            Wojewodztwo = "",
        };

        public static string CreateXLS(object jpk, Type T, string namespaceHtml)
        {
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    XmlSerializerNamespaces ns = NameSpaces.XmlSerializerNamespaces;
                    XmlSerializer ser = new XmlSerializer(T, namespaceHtml);
                    ser.Serialize(sw, jpk, ns);
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlsn
        {
            get => NameSpaces.XmlSerializerNamespaces; set { }
        }

        public static List<Product> GetProducts(string dbPath, System.Text.Encoding codingName)
        {
            List<Product> tab = new List<Product>();
            Dictionary<string, string> quantities = GetQunatities(dbPath, codingName);
            var db = new DbfFile(codingName);
            string fail = "";
            try
            {
                db.Open(Path.Combine(dbPath, "WYROP.DBF"), FileMode.Open);
                var dbRow = new DbfRecord(db.Header);

                db.Read(0, dbRow);
                do
                {
                    Product row = new Product();
                    fail = "Kod produktu";
                    row.Code = dbRow["SYMB"].Trim();
                    fail = "Nazwa produktu";
                    row.Name = string.Format("{0} {1}", dbRow["NAZWA"].Trim(), dbRow["NAZWA1"].Trim()).Trim();
                    fail = "Jednostka";
                    try
                    {
                        row.Quantity = quantities[dbRow["JM"].Trim()];
                    }
                    catch { row.Quantity = dbRow["JM"].Trim(); }
                    tab.Add(row);
                }
                while (db.ReadNext(dbRow));

            }
            catch (Exception ex)
            {
                MainStatic.ShowException(ex, "GetProducts()" + fail);
            }
            finally
            {
                db.Close();
            }
            return tab;
        }

        public static Dictionary<string, Config.Podmiot> GetCustomers(BackgroundWorker worker, string DbPath, System.Text.Encoding codingName)
        {
            Dictionary<string, Config.Podmiot> tab = new Dictionary<string, Config.Podmiot>();
            string fail = "";
            string failNip = "";
            var db = new DbfFile(codingName);
            try
            {
                int customersSize = 0;
                double index = 0;
                db.Open(Path.Combine(DbPath, "KONTRAH.DBF"), FileMode.Open);
                JPKGenerator.SizeDB(db, ref customersSize);
                var dbRow = new DbfRecord(db.Header);

                db.Read(0, dbRow);
                do
                {
                    failNip = dbRow["KOD_K"].Trim();
                    Config.Podmiot row = new Config.Podmiot
                    {
                        Id = new Config.ID(),
                        Adres = new Config.Adres()
                    };
                    fail = "PelnaNazwa";
                    try
                    {
                        row.Id.PelnaNazwa = string.Format("{0} {1}", dbRow["NAZWA_K"].Trim(), dbRow["NAZWA_1"].Trim()).Trim();
                    }
                    catch
                    {
                        row.Id.PelnaNazwa = string.Format("{0} {1}", dbRow["NAZWA_K"].Trim(), dbRow["NAZWA1"].Trim()).Trim();
                    }

                    fail = "NIP";
                    row.Id.NIP = dbRow["NIP"].Trim().Replace("-", "").Replace(" ", "");
                    fail = "KodPocztowy";
                    row.Adres.KodPocztowy = dbRow["PNA"].Trim();
                    fail = "Miejscowosc";
                    row.Adres.Miejscowosc = dbRow["MIASTO"].Trim();
                    fail = "Ulica";
                    row.Adres.Ulica = dbRow["ULICA"].Trim();

                    tab[failNip] = row;
                    index++;
                    worker.ReportProgress((int)Math.Ceiling((index / customersSize) * 100), string.Format("Pobieranie kontrahentów: {0} %", (int)Math.Ceiling((index / customersSize) * 100)));
                }
                while (db.ReadNext(dbRow));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Wystąpił błąd podczas pobierania kontrahentów. Błąd: {0}. Kod kontrahenta: {1}", fail, failNip), ex);
            }
            finally
            {
                db.Close();
            }

            return tab;
        }

        private static Dictionary<string, string> GetQunatities(string dbPath, System.Text.Encoding codingName)
        {
            Dictionary<string, string> tab = new Dictionary<string, string>();
            var db = new DbfFile(codingName);
            try
            {
                db.Open(Path.Combine(dbPath, "TJM.DBF"), FileMode.Open);
                var dbRow = new DbfRecord(db.Header);

                db.Read(0, dbRow);
                do
                {
                    tab[dbRow["KOD"].Trim()] = dbRow["NAZWA"].Trim();
                }
                while (db.ReadNext(dbRow));
            }
            catch (Exception ex)
            {
                MainStatic.ShowException(ex, "GetQunatities()");
            }
            finally
            {
                db.Close();
            }
            return tab;
        }
    }

    public partial class TNaglowek
    {
        private DateTime dataWytworzeniaJPKField;
        private DateTime dataOdField;
        private DateTime dataDoField;

        [XmlElement(Order = 0)]
        public TNaglowekKodFormularza KodFormularza;
        [XmlElement(Order = 1)]
        public sbyte WariantFormularza;
        [XmlElement(Order = 2)]
        public sbyte CelZlozenia;
        [XmlElement(Order = 3)]
        public string DataWytworzeniaJPK { get => dataWytworzeniaJPKField.ToString("yyyy-MM-ddThh:mm:ss"); set => dataWytworzeniaJPKField = DateTime.Parse(value); }

        [XmlElement(Order = 4)]
        public string DataOd { get => dataOdField.ToString("yyyy-MM-dd"); set => dataOdField = DateTime.Parse(value); }
        [XmlElement(Order = 5)]
        public string DataDo { get => dataDoField.ToString("yyyy-MM-dd"); set => dataDoField = DateTime.Parse(value); }
        [XmlElement(Order = 6)]
        public CurrCode_Type DomyslnyKodWaluty;
        [XmlElement(Order = 7)]
        public TKodUS KodUrzedu;
    }

    public partial class TNaglowekKodFormularza
    {
        [XmlAttribute()]
        public string kodSystemowy;
        [XmlAttribute()]
        public string wersjaSchemy;
        [XmlText()]
        public TKodFormularza value;
    }

    public partial class TAdresPolski
    {
        [XmlElement("KodKraju", Namespace = NameSpaces.Etd)]
        public TKodKraju KodKraju = TKodKraju.PL;

        [XmlElement("Wojewodztwo", Namespace = NameSpaces.Etd)]
        public string Wojewodztwo;

        [XmlElement("Powiat", Namespace = NameSpaces.Etd)]
        public string Powiat;

        [XmlElement("Gmina", Namespace = NameSpaces.Etd)]
        public string Gmina;

        [XmlElement("Ulica", Namespace = NameSpaces.Etd)]
        public string Ulica;

        [XmlElement("NrDomu", Namespace = NameSpaces.Etd)]
        public string NrDomu;

        [XmlElement("NrLokalu", Namespace = NameSpaces.Etd)]
        public string NrLokalu;

        [XmlElement("Miejscowosc", Namespace = NameSpaces.Etd)]
        public string Miejscowosc;

        [XmlElement("KodPocztowy", Namespace = NameSpaces.Etd)]
        public string KodPocztowy;

        [XmlElement("Poczta", Namespace = NameSpaces.Etd)]
        public string Poczta;
    }

    public partial class TIdentyfikatorOsobyNiefizycznej
    {
        [XmlElement("NIP", Namespace = NameSpaces.Etd)]
        [DefaultValue("")]
        public string NIP;

        [XmlElement("PelnaNazwa", Namespace = NameSpaces.Etd)]
        [DefaultValue("")]
        public string PelnaNazwa;

        [XmlElement("REGON", Namespace = NameSpaces.Etd)]
        [DefaultValue("")]
        public string REGON;
    }

    public partial class JPKPodmiot1
    {
        public TIdentyfikatorOsobyNiefizycznej IdentyfikatorPodmiotu;
        public TAdresPolski AdresPodmiotu;
    }

    public class NameSpaces
    {
        static NameSpaces()
        {
            XmlSerializerNamespaces = new XmlSerializerNamespaces();            
            XmlSerializerNamespaces.Add("xsi", Xsi);
            XmlSerializerNamespaces.Add("etd", Etd);
        }

        public static XmlSerializerNamespaces XmlSerializerNamespaces { get; private set; }
        public const string Etd = "http://crd.gov.pl/xml/schematy/dziedzinowe/mf/2016/01/25/eD/DefinicjeTypy/";
        public const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";
    }

    public enum TKodFormularza { JPK_MAG, JPK_FA, DEFAULT }

    public enum TKodKraju
    {
        AF,
        AX,
        AL,
        DZ,
        AD,
        AO,
        AI,
        AQ,
        AG,
        AN,
        SA,
        AR,
        AM,
        AW,
        AU,
        AT,
        AZ,
        BS,
        BH,
        BD,
        BB,
        BE,
        BZ,
        BJ,
        BM,
        BT,
        BY,
        BO,
        BA,
        BW,
        BR,
        BN,
        IO,
        BG,
        BF,
        BI,
        XC,
        CL,
        CN,
        HR,
        CY,
        TD,
        ME,
        DK,
        DM,
        DO,
        DJ,
        EG,
        EC,
        ER,
        EE,
        ET,
        FK,
        FJ,
        PH,
        FI,
        FR,
        TF,
        GA,
        GM,
        GH,
        GI,
        GR,
        GD,
        GL,
        GE,
        GU,
        GG,
        GY,
        GF,
        GP,
        GT,
        GN,
        GQ,
        GW,
        HT,
        ES,
        HN,
        HK,
        IN,
        ID,
        IQ,
        IR,
        IE,
        IS,
        IL,
        JM,
        JP,
        YE,
        JE,
        JO,
        KY,
        KH,
        CM,
        CA,
        QA,
        KZ,
        KE,
        KG,
        KI,
        CO,
        KM,
        CG,
        CD,
        KP,
        XK,
        CR,
        CU,
        KW,
        LA,
        LS,
        LB,
        LR,
        LY,
        LI,
        LT,
        LV,
        LU,
        MK,
        MG,
        YT,
        MO,
        MW,
        MV,
        MY,
        ML,
        MT,
        MP,
        MA,
        MQ,
        MR,
        MU,
        MX,
        XL,
        FM,
        UM,
        MD,
        MC,
        MN,
        MS,
        MZ,
        MM,
        NA,
        NR,
        NP,
        NL,
        DE,
        NE,
        NG,
        NI,
        NU,
        NF,
        NO,
        NC,
        NZ,
        PS,
        OM,
        PK,
        PW,
        PA,
        PG,
        PY,
        PE,
        PN,
        PF,
        PL,
        GS,
        PT,
        PR,
        CF,
        CZ,
        KR,
        ZA,
        RE,
        RU,
        RO,
        RW,
        EH,
        BL,
        KN,
        LC,
        MF,
        VC,
        SV,
        WS,
        AS,
        SM,
        SN,
        RS,
        SC,
        SL,
        SG,
        SK,
        SI,
        SO,
        LK,
        PM,
        US,
        SZ,
        SD,
        SR,
        SJ,
        SH,
        SY,
        CH,
        SE,
        TJ,
        TH,
        TW,
        TZ,
        TG,
        TK,
        TO,
        TT,
        TN,
        TR,
        TM,
        TV,
        UG,
        UA,
        UY,
        UZ,
        VU,
        WF,
        VA,
        HU,
        VE,
        GB,
        VN,
        IT,
        TL,
        CI,
        BV,
        CX,
        IM,
        CK,
        VI,
        VG,
        HM,
        CC,
        MH,
        FO,
        SB,
        ST,
        TC,
        ZM,
        CV,
        ZW,
        AE,
    }

    public enum CurrCode_Type
    {
        AED,
        AFN,
        ALL,
        AMD,
        ANG,
        AOA,
        ARS,
        AUD,
        AWG,
        AZN,
        BAM,
        BBD,
        BDT,
        BGN,
        BHD,
        BIF,
        BMD,
        BND,
        BOB,
        BOV,
        BRL,
        BSD,
        BTN,
        BWP,
        BYR,
        BZD,
        CAD,
        CDF,
        CHF,
        CLF,
        CLP,
        CNY,
        COP,
        COU,
        CRC,
        CUC,
        CUP,
        CVE,
        CZK,
        DJF,
        DKK,
        DOP,
        DZD,
        EEK,
        EGP,
        ERN,
        ETB,
        EUR,
        FJD,
        FKP,
        GBP,
        GEL,
        GHS,
        GIP,
        GMD,
        GNF,
        GTQ,
        GWP,
        GYD,
        HKD,
        HNL,
        HRK,
        HTG,
        HUF,
        IDR,
        ILS,
        INR,
        IQD,
        IRR,
        ISK,
        JMD,
        JOD,
        JPY,
        KES,
        KGS,
        KHR,
        KMF,
        KPW,
        KRW,
        KWD,
        KYD,
        KZT,
        LAK,
        LBP,
        LKR,
        LRD,
        LSL,
        LTL,
        LVL,
        LYD,
        MAD,
        MDL,
        MGA,
        MKD,
        MMK,
        MNT,
        MOP,
        MRO,
        MUR,
        MVR,
        MWK,
        MXN,
        MXV,
        MYR,
        MZN,
        NAD,
        NGN,
        NIO,
        NOK,
        NPR,
        NZD,
        OMR,
        PAB,
        PEN,
        PGK,
        PHP,
        PKR,
        PLN,
        PYG,
        QAR,
        RON,
        RSD,
        RUB,
        RWF,
        SAR,
        SBD,
        SCR,
        SDG,
        SEK,
        SGD,
        SHP,
        SLL,
        SOS,
        SRD,
        STD,
        SVC,
        SYP,
        SZL,
        THB,
        TJS,
        TMT,
        TND,
        TOP,
        TRY,
        TTD,
        TVD,
        TWD,
        TZS,
        UAH,
        UGX,
        USD,
        UYU,
        UZS,
        VEF,
        VND,
        VUV,
        WST,
        XAF,
        XCD,
        XOF,
        XPD,
        XPF,
        YER,
        ZAR,
        ZMK,
        ZWL,
    }

    public enum TKodUS
    {
        [XmlEnum("0202")]
        Item0202,
        [XmlEnum("0203")]
        Item0203,
        [XmlEnum("0204")]
        Item0204,
        [XmlEnum("0205")]
        Item0205,
        [XmlEnum("0206")]
        Item0206,
        [XmlEnum("0207")]
        Item0207,
        [XmlEnum("0208")]
        Item0208,
        [XmlEnum("0209")]
        Item0209,
        [XmlEnum("0210")]
        Item0210,
        [XmlEnum("0211")]
        Item0211,
        [XmlEnum("0212")]
        Item0212,
        [XmlEnum("0213")]
        Item0213,
        [XmlEnum("0214")]
        Item0214,
        [XmlEnum("0215")]
        Item0215,
        [XmlEnum("0216")]
        Item0216,
        [XmlEnum("0217")]
        Item0217,
        [XmlEnum("0218")]
        Item0218,
        [XmlEnum("0219")]
        Item0219,
        [XmlEnum("0220")]
        Item0220,
        [XmlEnum("0221")]
        Item0221,
        [XmlEnum("0222")]
        Item0222,
        [XmlEnum("0223")]
        Item0223,
        [XmlEnum("0224")]
        Item0224,
        [XmlEnum("0225")]
        Item0225,
        [XmlEnum("0226")]
        Item0226,
        [XmlEnum("0227")]
        Item0227,
        [XmlEnum("0228")]
        Item0228,
        [XmlEnum("0229")]
        Item0229,
        [XmlEnum("0230")]
        Item0230,
        [XmlEnum("0231")]
        Item0231,
        [XmlEnum("0232")]
        Item0232,
        [XmlEnum("0233")]
        Item0233,
        [XmlEnum("0234")]
        Item0234,
        [XmlEnum("0271")]
        Item0271,
        [XmlEnum("0402")]
        Item0402,
        [XmlEnum("0403")]
        Item0403,
        [XmlEnum("0404")]
        Item0404,
        [XmlEnum("0405")]
        Item0405,
        [XmlEnum("0406")]
        Item0406,
        [XmlEnum("0407")]
        Item0407,
        [XmlEnum("0408")]
        Item0408,
        [XmlEnum("0409")]
        Item0409,
        [XmlEnum("0410")]
        Item0410,
        [XmlEnum("0411")]
        Item0411,
        [XmlEnum("0412")]
        Item0412,
        [XmlEnum("0413")]
        Item0413,
        [XmlEnum("0414")]
        Item0414,
        [XmlEnum("0415")]
        Item0415,
        [XmlEnum("0416")]
        Item0416,
        [XmlEnum("0417")]
        Item0417,
        [XmlEnum("0418")]
        Item0418,
        [XmlEnum("0419")]
        Item0419,
        [XmlEnum("0420")]
        Item0420,
        [XmlEnum("0421")]
        Item0421,
        [XmlEnum("0422")]
        Item0422,
        [XmlEnum("0423")]
        Item0423,
        [XmlEnum("0471")]
        Item0471,
        [XmlEnum("0602")]
        Item0602,
        [XmlEnum("0603")]
        Item0603,
        [XmlEnum("0604")]
        Item0604,
        [XmlEnum("0605")]
        Item0605,
        [XmlEnum("0606")]
        Item0606,
        [XmlEnum("0607")]
        Item0607,
        [XmlEnum("0608")]
        Item0608,
        [XmlEnum("0609")]
        Item0609,
        [XmlEnum("0610")]
        Item0610,
        [XmlEnum("0611")]
        Item0611,
        [XmlEnum("0612")]
        Item0612,
        [XmlEnum("0613")]
        Item0613,
        [XmlEnum("0614")]
        Item0614,
        [XmlEnum("0615")]
        Item0615,
        [XmlEnum("0616")]
        Item0616,
        [XmlEnum("0617")]
        Item0617,
        [XmlEnum("0618")]
        Item0618,
        [XmlEnum("0619")]
        Item0619,
        [XmlEnum("0620")]
        Item0620,
        [XmlEnum("0621")]
        Item0621,
        [XmlEnum("0622")]
        Item0622,
        [XmlEnum("0671")]
        Item0671,
        [XmlEnum("0802")]
        Item0802,
        [XmlEnum("0803")]
        Item0803,
        [XmlEnum("0804")]
        Item0804,
        [XmlEnum("0805")]
        Item0805,
        [XmlEnum("0806")]
        Item0806,
        [XmlEnum("0807")]
        Item0807,
        [XmlEnum("0808")]
        Item0808,
        [XmlEnum("0809")]
        Item0809,
        [XmlEnum("0810")]
        Item0810,
        [XmlEnum("0811")]
        Item0811,
        [XmlEnum("0812")]
        Item0812,
        [XmlEnum("0813")]
        Item0813,
        [XmlEnum("0814")]
        Item0814,
        [XmlEnum("0871")]
        Item0871,
        [XmlEnum("1002")]
        Item1002,
        [XmlEnum("1003")]
        Item1003,
        [XmlEnum("1004")]
        Item1004,
        [XmlEnum("1005")]
        Item1005,
        [XmlEnum("1006")]
        Item1006,
        [XmlEnum("1007")]
        Item1007,
        [XmlEnum("1008")]
        Item1008,
        [XmlEnum("1009")]
        Item1009,
        [XmlEnum("1010")]
        Item1010,
        [XmlEnum("1011")]
        Item1011,
        [XmlEnum("1012")]
        Item1012,
        [XmlEnum("1013")]
        Item1013,
        [XmlEnum("1014")]
        Item1014,
        [XmlEnum("1015")]
        Item1015,
        [XmlEnum("1016")]
        Item1016,
        [XmlEnum("1017")]
        Item1017,
        [XmlEnum("1018")]
        Item1018,
        [XmlEnum("1019")]
        Item1019,
        [XmlEnum("1020")]
        Item1020,
        [XmlEnum("1021")]
        Item1021,
        [XmlEnum("1022")]
        Item1022,
        [XmlEnum("1023")]
        Item1023,
        [XmlEnum("1024")]
        Item1024,
        [XmlEnum("1025")]
        Item1025,
        [XmlEnum("1026")]
        Item1026,
        [XmlEnum("1027")]
        Item1027,
        [XmlEnum("1028")]
        Item1028,
        [XmlEnum("1029")]
        Item1029,
        [XmlEnum("1071")]
        Item1071,
        [XmlEnum("1202")]
        Item1202,
        [XmlEnum("1203")]
        Item1203,
        [XmlEnum("1204")]
        Item1204,
        [XmlEnum("1205")]
        Item1205,
        [XmlEnum("1206")]
        Item1206,
        [XmlEnum("1207")]
        Item1207,
        [XmlEnum("1208")]
        Item1208,
        [XmlEnum("1209")]
        Item1209,
        [XmlEnum("1210")]
        Item1210,
        [XmlEnum("1211")]
        Item1211,
        [XmlEnum("1212")]
        Item1212,
        [XmlEnum("1213")]
        Item1213,
        [XmlEnum("1214")]
        Item1214,
        [XmlEnum("1215")]
        Item1215,
        [XmlEnum("1216")]
        Item1216,
        [XmlEnum("1217")]
        Item1217,
        [XmlEnum("1218")]
        Item1218,
        [XmlEnum("1219")]
        Item1219,
        [XmlEnum("1220")]
        Item1220,
        [XmlEnum("1221")]
        Item1221,
        [XmlEnum("1222")]
        Item1222,
        [XmlEnum("1223")]
        Item1223,
        [XmlEnum("1224")]
        Item1224,
        [XmlEnum("1225")]
        Item1225,
        [XmlEnum("1226")]
        Item1226,
        [XmlEnum("1227")]
        Item1227,
        [XmlEnum("1228")]
        Item1228,
        [XmlEnum("1271")]
        Item1271,
        [XmlEnum("1402")]
        Item1402,
        [XmlEnum("1403")]
        Item1403,
        [XmlEnum("1404")]
        Item1404,
        [XmlEnum("1405")]
        Item1405,
        [XmlEnum("1406")]
        Item1406,
        [XmlEnum("1407")]
        Item1407,
        [XmlEnum("1408")]
        Item1408,
        [XmlEnum("1409")]
        Item1409,
        [XmlEnum("1410")]
        Item1410,
        [XmlEnum("1411")]
        Item1411,
        [XmlEnum("1412")]
        Item1412,
        [XmlEnum("1413")]
        Item1413,
        [XmlEnum("1414")]
        Item1414,
        [XmlEnum("1415")]
        Item1415,
        [XmlEnum("1416")]
        Item1416,
        [XmlEnum("1417")]
        Item1417,
        [XmlEnum("1418")]
        Item1418,
        [XmlEnum("1419")]
        Item1419,
        [XmlEnum("1420")]
        Item1420,
        [XmlEnum("1421")]
        Item1421,
        [XmlEnum("1422")]
        Item1422,
        [XmlEnum("1423")]
        Item1423,
        [XmlEnum("1424")]
        Item1424,
        [XmlEnum("1425")]
        Item1425,
        [XmlEnum("1426")]
        Item1426,
        [XmlEnum("1427")]
        Item1427,
        [XmlEnum("1428")]
        Item1428,
        [XmlEnum("1429")]
        Item1429,
        [XmlEnum("1430")]
        Item1430,
        [XmlEnum("1431")]
        Item1431,
        [XmlEnum("1432")]
        Item1432,
        [XmlEnum("1433")]
        Item1433,
        [XmlEnum("1434")]
        Item1434,
        [XmlEnum("1435")]
        Item1435,
        [XmlEnum("1436")]
        Item1436,
        [XmlEnum("1437")]
        Item1437,
        [XmlEnum("1438")]
        Item1438,
        [XmlEnum("1439")]
        Item1439,
        [XmlEnum("1440")]
        Item1440,
        [XmlEnum("1441")]
        Item1441,
        [XmlEnum("1442")]
        Item1442,
        [XmlEnum("1443")]
        Item1443,
        [XmlEnum("1444")]
        Item1444,
        [XmlEnum("1445")]
        Item1445,
        [XmlEnum("1446")]
        Item1446,
        [XmlEnum("1447")]
        Item1447,
        [XmlEnum("1448")]
        Item1448,
        [XmlEnum("1449")]
        Item1449,
        [XmlEnum("1471")]
        Item1471,
        [XmlEnum("1472")]
        Item1472,
        [XmlEnum("1473")]
        Item1473,
        [XmlEnum("1602")]
        Item1602,
        [XmlEnum("1603")]
        Item1603,
        [XmlEnum("1604")]
        Item1604,
        [XmlEnum("1605")]
        Item1605,
        [XmlEnum("1606")]
        Item1606,
        [XmlEnum("1607")]
        Item1607,
        [XmlEnum("1608")]
        Item1608,
        [XmlEnum("1609")]
        Item1609,
        [XmlEnum("1610")]
        Item1610,
        [XmlEnum("1611")]
        Item1611,
        [XmlEnum("1612")]
        Item1612,
        [XmlEnum("1613")]
        Item1613,
        [XmlEnum("1671")]
        Item1671,
        [XmlEnum("1802")]
        Item1802,
        [XmlEnum("1803")]
        Item1803,
        [XmlEnum("1804")]
        Item1804,
        [XmlEnum("1805")]
        Item1805,
        [XmlEnum("1806")]
        Item1806,
        [XmlEnum("1807")]
        Item1807,
        [XmlEnum("1808")]
        Item1808,
        [XmlEnum("1809")]
        Item1809,
        [XmlEnum("1810")]
        Item1810,
        [XmlEnum("1811")]
        Item1811,
        [XmlEnum("1812")]
        Item1812,
        [XmlEnum("1813")]
        Item1813,
        [XmlEnum("1814")]
        Item1814,
        [XmlEnum("1815")]
        Item1815,
        [XmlEnum("1816")]
        Item1816,
        [XmlEnum("1817")]
        Item1817,
        [XmlEnum("1818")]
        Item1818,
        [XmlEnum("1819")]
        Item1819,
        [XmlEnum("1820")]
        Item1820,
        [XmlEnum("1821")]
        Item1821,
        [XmlEnum("1822")]
        Item1822,
        [XmlEnum("1823")]
        Item1823,
        [XmlEnum("1871")]
        Item1871,
        [XmlEnum("2002")]
        Item2002,
        [XmlEnum("2003")]
        Item2003,
        [XmlEnum("2004")]
        Item2004,
        [XmlEnum("2005")]
        Item2005,
        [XmlEnum("2006")]
        Item2006,
        [XmlEnum("2007")]
        Item2007,
        [XmlEnum("2008")]
        Item2008,
        [XmlEnum("2009")]
        Item2009,
        [XmlEnum("2010")]
        Item2010,
        [XmlEnum("2011")]
        Item2011,
        [XmlEnum("2012")]
        Item2012,
        [XmlEnum("2013")]
        Item2013,
        [XmlEnum("2014")]
        Item2014,
        [XmlEnum("2015")]
        Item2015,
        [XmlEnum("2071")]
        Item2071,
        [XmlEnum("2202")]
        Item2202,
        [XmlEnum("2203")]
        Item2203,
        [XmlEnum("2204")]
        Item2204,
        [XmlEnum("2205")]
        Item2205,
        [XmlEnum("2206")]
        Item2206,
        [XmlEnum("2207")]
        Item2207,
        [XmlEnum("2208")]
        Item2208,
        [XmlEnum("2209")]
        Item2209,
        [XmlEnum("2210")]
        Item2210,
        [XmlEnum("2211")]
        Item2211,
        [XmlEnum("2212")]
        Item2212,
        [XmlEnum("2213")]
        Item2213,
        [XmlEnum("2214")]
        Item2214,
        [XmlEnum("2215")]
        Item2215,
        [XmlEnum("2216")]
        Item2216,
        [XmlEnum("2217")]
        Item2217,
        [XmlEnum("2218")]
        Item2218,
        [XmlEnum("2219")]
        Item2219,
        [XmlEnum("2220")]
        Item2220,
        [XmlEnum("2221")]
        Item2221,
        [XmlEnum("2271")]
        Item2271,
        [XmlEnum("2402")]
        Item2402,
        [XmlEnum("2403")]
        Item2403,
        [XmlEnum("2404")]
        Item2404,
        [XmlEnum("2405")]
        Item2405,
        [XmlEnum("2406")]
        Item2406,
        [XmlEnum("2407")]
        Item2407,
        [XmlEnum("2408")]
        Item2408,
        [XmlEnum("2409")]
        Item2409,
        [XmlEnum("2410")]
        Item2410,
        [XmlEnum("2411")]
        Item2411,
        [XmlEnum("2412")]
        Item2412,
        [XmlEnum("2413")]
        Item2413,
        [XmlEnum("2414")]
        Item2414,
        [XmlEnum("2415")]
        Item2415,
        [XmlEnum("2416")]
        Item2416,
        [XmlEnum("2417")]
        Item2417,
        [XmlEnum("2418")]
        Item2418,
        [XmlEnum("2419")]
        Item2419,
        [XmlEnum("2420")]
        Item2420,
        [XmlEnum("2421")]
        Item2421,
        [XmlEnum("2422")]
        Item2422,
        [XmlEnum("2423")]
        Item2423,
        [XmlEnum("2424")]
        Item2424,
        [XmlEnum("2425")]
        Item2425,
        [XmlEnum("2426")]
        Item2426,
        [XmlEnum("2427")]
        Item2427,
        [XmlEnum("2428")]
        Item2428,
        [XmlEnum("2429")]
        Item2429,
        [XmlEnum("2430")]
        Item2430,
        [XmlEnum("2431")]
        Item2431,
        [XmlEnum("2432")]
        Item2432,
        [XmlEnum("2433")]
        Item2433,
        [XmlEnum("2434")]
        Item2434,
        [XmlEnum("2435")]
        Item2435,
        [XmlEnum("2436")]
        Item2436,
        [XmlEnum("2471")]
        Item2471,
        [XmlEnum("2472")]
        Item2472,
        [XmlEnum("2602")]
        Item2602,
        [XmlEnum("2603")]
        Item2603,
        [XmlEnum("2604")]
        Item2604,
        [XmlEnum("2605")]
        Item2605,
        [XmlEnum("2606")]
        Item2606,
        [XmlEnum("2607")]
        Item2607,
        [XmlEnum("2608")]
        Item2608,
        [XmlEnum("2609")]
        Item2609,
        [XmlEnum("2610")]
        Item2610,
        [XmlEnum("2611")]
        Item2611,
        [XmlEnum("2612")]
        Item2612,
        [XmlEnum("2613")]
        Item2613,
        [XmlEnum("2614")]
        Item2614,
        [XmlEnum("2615")]
        Item2615,
        [XmlEnum("2671")]
        Item2671,
        [XmlEnum("2802")]
        Item2802,
        [XmlEnum("2803")]
        Item2803,
        [XmlEnum("2804")]
        Item2804,
        [XmlEnum("2805")]
        Item2805,
        [XmlEnum("2806")]
        Item2806,
        [XmlEnum("2807")]
        Item2807,
        [XmlEnum("2808")]
        Item2808,
        [XmlEnum("2809")]
        Item2809,
        [XmlEnum("2810")]
        Item2810,
        [XmlEnum("2811")]
        Item2811,
        [XmlEnum("2812")]
        Item2812,
        [XmlEnum("2813")]
        Item2813,
        [XmlEnum("2814")]
        Item2814,
        [XmlEnum("2815")]
        Item2815,
        [XmlEnum("2816")]
        Item2816,
        [XmlEnum("2871")]
        Item2871,
        [XmlEnum("3002")]
        Item3002,
        [XmlEnum("3003")]
        Item3003,
        [XmlEnum("3004")]
        Item3004,
        [XmlEnum("3005")]
        Item3005,
        [XmlEnum("3006")]
        Item3006,
        [XmlEnum("3007")]
        Item3007,
        [XmlEnum("3008")]
        Item3008,
        [XmlEnum("3009")]
        Item3009,
        [XmlEnum("3010")]
        Item3010,
        [XmlEnum("3011")]
        Item3011,
        [XmlEnum("3012")]
        Item3012,
        [XmlEnum("3013")]
        Item3013,
        [XmlEnum("3014")]
        Item3014,
        [XmlEnum("3015")]
        Item3015,
        [XmlEnum("3016")]
        Item3016,
        [XmlEnum("3017")]
        Item3017,
        [XmlEnum("3018")]
        Item3018,
        [XmlEnum("3019")]
        Item3019,
        [XmlEnum("3020")]
        Item3020,
        [XmlEnum("3021")]
        Item3021,
        [XmlEnum("3022")]
        Item3022,
        [XmlEnum("3023")]
        Item3023,
        [XmlEnum("3025")]
        Item3025,
        [XmlEnum("3026")]
        Item3026,
        [XmlEnum("3027")]
        Item3027,
        [XmlEnum("3028")]
        Item3028,
        [XmlEnum("3029")]
        Item3029,
        [XmlEnum("3030")]
        Item3030,
        [XmlEnum("3031")]
        Item3031,
        [XmlEnum("3032")]
        Item3032,
        [XmlEnum("3033")]
        Item3033,
        [XmlEnum("3034")]
        Item3034,
        [XmlEnum("3035")]
        Item3035,
        [XmlEnum("3036")]
        Item3036,
        [XmlEnum("3037")]
        Item3037,
        [XmlEnum("3038")]
        Item3038,
        [XmlEnum("3039")]
        Item3039,
        [XmlEnum("3071")]
        Item3071,
        [XmlEnum("3072")]
        Item3072,
        [XmlEnum("3202")]
        Item3202,
        [XmlEnum("3203")]
        Item3203,
        [XmlEnum("3204")]
        Item3204,
        [XmlEnum("3205")]
        Item3205,
        [XmlEnum("3206")]
        Item3206,
        [XmlEnum("3207")]
        Item3207,
        [XmlEnum("3208")]
        Item3208,
        [XmlEnum("3209")]
        Item3209,
        [XmlEnum("3210")]
        Item3210,
        [XmlEnum("3211")]
        Item3211,
        [XmlEnum("3212")]
        Item3212,
        [XmlEnum("3213")]
        Item3213,
        [XmlEnum("3214")]
        Item3214,
        [XmlEnum("3215")]
        Item3215,
        [XmlEnum("3216")]
        Item3216,
        [XmlEnum("3217")]
        Item3217,
        [XmlEnum("3218")]
        Item3218,
        [XmlEnum("3219")]
        Item3219,
        [XmlEnum("3220")]
        Item3220,
        [XmlEnum("3271")]
        Item3271,
    }

    public enum MSCountryCode_Type
    {
        AT,
        BE,
        BG,
        CY,
        CZ,
        DK,
        EE,
        FI,
        FR,
        DE,
        EL,
        HR,
        HU,
        IE,
        IT,
        LV,
        LT,
        LU,
        MT,
        NL,
        PL,
        PT,
        RO,
        SK,
        SI,
        ES,
        SE,
        GB,
        IC,
        XI,
        XJ,
        MC,
    }    
}
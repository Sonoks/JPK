using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace JPKFVHurt.JPKFA
{
    public class NameSpacesFA : NameSpaces
    {
        static NameSpacesFA()
        {
            XmlSerializerNamespaces.Add("", Default);
        }

        public const string Default = "http://jpk.mf.gov.pl/wzor/2016/03/09/03095/";
        public const string SchemaLocation = "http://jpk.mf.gov.pl/wzor/2016/03/09/03095/ https://www.mf.gov.pl/documents/764034/5134536/Schemat_JPK_FA%281%29_v1-0.xsd";
    }

    [XmlRoot("JPK", Namespace = NameSpacesFA.Default)]
    public partial class JPK : JPKBase
    {
        [XmlElement("Faktura")]
        public JPKFaktura[] Faktura;
        public JPKFakturaCtrl FakturaCtrl;
        public JPKStawkiPodatku StawkiPodatku;

        [XmlElement("FakturaWiersz")]
        public JPKFakturaWiersz[] FakturaWiersz;
        public JPKFakturaWierszCtrl FakturaWierszCtrl;

        [XmlAttribute("schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string XSDSchemaLocation
        {
            get => NameSpacesFA.SchemaLocation; set { }
        }
    }

    public partial class JPKFaktura
    {
        private DateTime p_1Field;
        private DateTime p_6Field;

        /// <summary>
        /// Data wystawienia
        /// </summary>
        [XmlElement(Order = 0)]
        public string P_1 { get => p_1Field.ToString("yyyy-MM-dd"); set => p_1Field = CSharp.JPKGenerator.GetDate(value); }

        /// <summary>
        /// Numer faktury
        /// </summary>
        [XmlElement(DataType = "token", Order = 1)]
        public string P_2A;

        [XmlElement(DataType = "token", Order = 2)]
        public string P_3A;

        [XmlElement(DataType = "token", Order = 3)]
        public string P_3B;

        [XmlElement(DataType = "token", Order = 4)]
        public string P_3C;

        [XmlElement(DataType = "token", Order = 5)]
        public string P_3D;

        [XmlElement(Order = 6)]
        public MSCountryCode_Type P_4A;
        [XmlIgnore()]
        public bool P_4ASpecified;
        [XmlElement(Order = 7)]
        public string P_4B;

        [XmlElement("P_5A", Order = 8)]
        [DefaultValue(MSCountryCode_Type.PL)]
        public MSCountryCode_Type P_5A = MSCountryCode_Type.PL;

        [XmlIgnore()]
        public bool P_5ASpecified;

        [XmlElement("P_5B", Order = 9)]
        [DefaultValue("")]
        public string P_5B = "";

        [XmlElement("P_6", Order = 10)]
        public string P_6 { get => p_6Field.ToString("yyyy-MM-dd"); set => p_6Field = CSharp.JPKGenerator.GetDate(value); }

        [XmlIgnore()]
        public decimal P_13_1 = 0;
        [XmlIgnore()]
        public decimal P_14_1 = 0;
        [XmlIgnore()]
        public decimal P_13_2 = 0;
        [XmlIgnore()]
        public decimal P_14_2 = 0;
        [XmlIgnore()]
        public decimal P_13_3 = 0;
        [XmlIgnore()]
        public decimal P_14_3 = 0;
        [XmlIgnore()]
        public decimal P_13_4 = 0;
        [XmlIgnore()]
        public decimal P_14_4 = 0;
        [XmlIgnore()]
        public decimal P_13_5 = 0;
        [XmlIgnore()]
        public decimal P_14_5 = 0;
        [XmlIgnore()]
        public decimal P_13_6 = 0;


        [XmlElement("P_13_1", Order = 11)]
        public string P_13_1Field { get => P_13_1.ToString("0.00").Replace(",", "."); set => P_13_1 = decimal.Parse(value); }
        [XmlElement("P_14_1", Order = 12)]
        public string P_14_1Field { get => P_14_1.ToString("0.00").Replace(",", "."); set => P_14_1 = decimal.Parse(value); }
        [XmlElement("P_13_2", Order = 13)]
        public string P_13_2Field { get => P_13_2.ToString("0.00").Replace(",", "."); set => P_13_2 = decimal.Parse(value); }
        [XmlElement("P_14_2", Order = 14)]
        public string P_14_2Field { get => P_14_2.ToString("0.00").Replace(",", "."); set => P_14_2 = decimal.Parse(value); }
        [XmlElement("P_13_3", Order = 15)]
        public string P_13_3Field { get => P_13_3.ToString("0.00").Replace(",", "."); set => P_13_3 = decimal.Parse(value); }
        [XmlElement("P_14_3", Order = 16)]
        public string P_14_3Field { get => P_14_3.ToString("0.00").Replace(",", "."); set => P_14_3 = decimal.Parse(value); }
        [XmlElement("P_13_4", Order = 17)]
        [DefaultValue("0.00")]
        public string P_13_4Field { get => P_13_4.ToString("0.00").Replace(",", "."); set => P_13_4 = decimal.Parse(value); }
        [XmlElement("P_14_4", Order = 18)]
        [DefaultValue("0.00")]
        public string P_14_4Field { get => P_14_4.ToString("0.00").Replace(",", "."); set => P_14_4 = decimal.Parse(value); }
        [XmlElement("P_13_5", Order = 19)]
        [DefaultValue("0.00")]
        public string P_13_5Field { get => P_13_5.ToString("0.00").Replace(",", "."); set => P_13_5 = decimal.Parse(value); }
        [XmlElement("P_14_5", Order = 20)]
        [DefaultValue("0.00")]
        public string P_14_5Field { get => P_14_5.ToString("0.00").Replace(",", "."); set => P_14_5 = decimal.Parse(value); }
        [XmlElement("P_13_6", Order = 21)]
        public string P_13_6Field { get => P_13_6.ToString("0.00").Replace(",", "."); set => P_13_6 = decimal.Parse(value); }

        [XmlIgnore()]
        public decimal P_13_7;

        [XmlElement("P_13_7", Order = 22)]
        public string P_13_7Field { get => P_13_7.ToString("0.00").Replace(",", "."); set => P_13_7 = decimal.Parse(value); }

        /// <summary>
        /// Brutto
        /// </summary>
        [XmlIgnore()]
        public decimal P_15;

        [XmlElement("P_15", Order = 23)]
        public string P_15Field { get => P_15.ToString("0.00").Replace(",", "."); set => P_15 = decimal.Parse(value); }

        [XmlElement(Order = 24)]
        public bool P_16 = false;
        [XmlElement(Order = 25)]
        public bool P_17 = false;
        [XmlElement(Order = 26)]
        public bool P_18 = false;
        [XmlElement(Order = 27)]
        public bool P_19 = false;

        [XmlElement(DataType = "token", Order = 28)]
        [DefaultValue("false")]
        public string P_19A = "false";

        [XmlElement(DataType = "token", Order = 29)]
        [DefaultValue("false")]
        public string P_19B = "false";

        [XmlElement(DataType = "token", Order = 30)]
        [DefaultValue("false")]
        public string P_19C = "false";

        [XmlElement(Order = 31)]
        public bool P_20 = false;

        [XmlElement(DataType = "token", Order = 32)]
        public string P_20A;

        [XmlElement(DataType = "token", Order = 33)]
        public string P_20B;
        [XmlElement(Order = 34)]
        public bool P_21 = false;

        [XmlElement(DataType = "token", Order = 35)]
        [DefaultValue("false")]
        public string P_21A = "false";

        [XmlElement(DataType = "token", Order = 36)]
        [DefaultValue("false")]
        public string P_21B = "false";

        [DefaultValue("false")]
        [XmlElement(DataType = "token", Order = 37)]
        public string P_21C = "false";

        [XmlElement(Order = 38)]
        public bool P_23 = false;
        [XmlElement(Order = 39)]
        public bool P_106E_2 = false;
        [XmlElement(Order = 40)]
        public bool P_106E_3 = false;

        [XmlElement(DataType = "token", Order = 41)]
        [DefaultValue("false")]
        public string P_106E_3A = "false";

        [XmlElement(Order = 42)]
        public JPKFakturaRodzajFaktury RodzajFaktury;

        [XmlElement("PrzyczynaKorekty", Order = 43)]
        [DefaultValue("")]
        public string PrzyczynaKorekty = "";

        [XmlElement("NrFaKorygowanej", Order = 44)]
        [DefaultValue("")]
        public string NrFaKorygowanej = "";

        [XmlElement("OkresFaKorygowanej", Order = 45)]
        [DefaultValue("")]
        public string OkresFaKorygowanej = "";

        [XmlIgnore]
        public decimal ZALZaplata;
        [XmlElement("ZALZaplata", Order = 46)]
        [DefaultValue("0.00")]
        public string ZALZaplataField { get => ZALZaplata.ToString("0.00").Replace(",", "."); set => ZALZaplata = decimal.Parse(value); }
        [XmlIgnore]
        public decimal ZALPodatek;
        [XmlElement("ZALPodatek", Order = 47)]
        [DefaultValue("0.00")]
        public string ZALPodatekField{ get => ZALPodatek.ToString("0.00").Replace(",", "."); set => ZALPodatek = decimal.Parse(value); }

        [XmlAttribute()]
        public string typ = "G";
    }

    public enum JPKFakturaRodzajFaktury
    {
        VAT,
        KOREKTA,
        ZAL,
        POZ,
    }

    public partial class JPKFakturaCtrl
    {
        [XmlElement(DataType = "nonNegativeInteger")]
        public string LiczbaFaktur;

        [XmlIgnore]
        public decimal WartoscFaktur;

        [XmlElement("WartoscFaktur")]
        public string WartoscFakturField { get => WartoscFaktur.ToString("0.00").Replace(",","."); set => WartoscFaktur = decimal.Parse(value); }
    }

    public partial class JPKStawkiPodatku
    {
        public decimal Stawka1 = 0.23m;
        public decimal Stawka2 = 0.08m;
        public decimal Stawka3 = 0.05m;
        public decimal Stawka4 = 0.00m;
        public decimal Stawka5 = 0.00m;
    }

    public partial class JPKFakturaWiersz
    {
        /// <summary>
        /// Numer faktury
        /// </summary>
        [XmlElement("P_2B", Order = 0)]
        public string P_2B;

        /// <summary>
        /// Nazwa towaru, usługi
        /// </summary>
        [XmlElement("P_7", Order = 1)]
        public string P_7;

        /// <summary>
        /// Miara dostarczonych towarów lub zakres wykonanych usług
        /// </summary>
        [XmlElement("P_8A", Order = 2)]
        public string P_8A;

        /// <summary>
        /// Ilość towarów
        /// </summary>
        [XmlIgnore]
        public decimal P_8B;
        [XmlElement("P_8B", Order = 3)]
        public string P_8BField { get => P_8B.ToString("0.000000").Replace(",", "."); set => P_8B = decimal.Parse(value); }

        /// <summary>
        /// Cena jednostkowa
        /// </summary>
        [XmlIgnore]
        public decimal P_9A;
        [XmlElement("P_9A", Order = 4)]
        public string P_9AField { get => P_9A.ToString("0.00").Replace(",", "."); set => P_9A = decimal.Parse(value); }

        /// <summary>
        /// W przypadku zastosowania art.106e ustawy, cena wraz z kwotą podatku (Cena jednostkowa brutto)
        /// </summary>
        [XmlIgnore]
        public decimal P_9B;
        [XmlElement("P_9B", Order = 5)]
        [DefaultValue("0.00")]
        public string P_9BField { get => P_9B.ToString("0.00").Replace(",", "."); set => P_9B = decimal.Parse(value); }

        /// <summary>
        /// Rabat
        /// </summary>
        [XmlIgnore]
        public decimal P_10;
        [XmlElement("P_10", Order = 6)]
        [DefaultValue("0.00")]
        public string P_10Field { get => P_10.ToString("0.00").Replace(",", "."); set => P_10 = decimal.Parse(value); }

        /// <summary>
        /// Wartość sprzedaży netto
        /// </summary>
        [XmlIgnore]
        public decimal P_11;
        [XmlElement("P_11", Order = 7)]
        public string P_11Field { get => P_11.ToString("0.00").Replace(",", "."); set => P_11 = decimal.Parse(value); }

        /// <summary>
        /// W przypadku zastosowania art. 106e ust.7 i 8 ustawy, wartość sprzedaży brutto
        /// </summary>
        [XmlIgnore]
        public decimal P_11A;
        [XmlElement("P_11A", Order = 8)]
        [DefaultValue("0.00")]
        public string P_11AField { get => P_11A.ToString("0.00").Replace(",", "."); set => P_11A = decimal.Parse(value); }

        /// <summary>
        /// Stawka VAT
        /// </summary>
        [XmlElement("P_12", Order = 9)]
        public JPKFakturaWierszP_12 P_12;

        [XmlAttribute()]
        public string typ = "G";
    }

    public enum JPKFakturaWierszP_12
    {
        [XmlEnum("23")]
        Item23,
        [XmlEnum("22")]
        Item22,
        [XmlEnum("8")]
        Item8,
        [XmlEnum("7")]
        Item7,
        [XmlEnum("5")]
        Item5,
        [XmlEnum("3")]
        Item3,
        [XmlEnum("0")]
        Item0,
        zw,
        np,
    }

    public partial class JPKFakturaWierszCtrl
    {
        [XmlElement(DataType = "nonNegativeInteger")]
        public string LiczbaWierszyFaktur = "";

        [XmlIgnore]
        public decimal WartoscWierszyFaktur = 0;
        [XmlElement("WartoscWierszyFaktur")]
        public string WartoscWierszyFakturField { get => WartoscWierszyFaktur.ToString("0.00").Replace(",","."); set => WartoscWierszyFaktur = Convert.ToDecimal(value); }
    }
}
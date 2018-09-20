namespace JPKFVHurt.Config
{
    public class Podmiot
    {
        public ID Id = new ID();
        public Adres Adres = new Adres();
        public TKodUS KodUS = TKodUS.Item0202;

        public JPKPodmiot1 ToJPKPodmiot1()
        {
            return new JPKPodmiot1()
            {
                IdentyfikatorPodmiotu = Id?.MSIdentyfikatorPodmiotu(),
                AdresPodmiotu = Adres?.MSTAdresPolski()
            };
        }

        public static Podmiot JPKPodmiot1Parse(JPKPodmiot1 p)
        {
            return new Podmiot
            {
                Id = ID.TIdentyfikatorOsobyNiefizycznejParse(p.IdentyfikatorPodmiotu),
                Adres = Adres.TAdresPolskiParse(p.AdresPodmiotu)
            };
        }
    }

    public partial class ID
    {
        public string NIP = "";
        public string PelnaNazwa = "";
        public string REGON = "";

        public TIdentyfikatorOsobyNiefizycznej MSIdentyfikatorPodmiotu()
        {
            return new TIdentyfikatorOsobyNiefizycznej()
            {
                //NIP = nIPField,
                PelnaNazwa = PelnaNazwa,
                REGON = REGON
            };
        }

        public static ID TIdentyfikatorOsobyNiefizycznejParse(TIdentyfikatorOsobyNiefizycznej t)
        {
            return new ID()
            {
                NIP = t.NIP,
                PelnaNazwa = t.PelnaNazwa,
                REGON = t.REGON
            };
        }
    }

    public partial class Adres
    {
        public TKodKraju KodKraju = TKodKraju.PL;
        public string Wojewodztwo = "";
        public string Powiat = "";
        public string Gmina = "";
        public string Ulica = "";
        public string NrDomu = "";
        public string NrLokalu = "";
        public string Miejscowosc = "";
        public string KodPocztowy = "";
        public string Poczta = "";

        public TAdresPolski MSTAdresPolski()
        {
            return new TAdresPolski()
            {
                KodKraju = KodKraju,
                Wojewodztwo = Wojewodztwo,
                Powiat = Powiat,
                Gmina = Gmina,
                Ulica = Ulica,
                NrDomu = NrDomu,
                NrLokalu = NrLokalu,
                Miejscowosc = Miejscowosc,
                KodPocztowy = KodPocztowy,
                Poczta = Poczta
            };
        }

        public static Adres TAdresPolskiParse(TAdresPolski t)
        {
            return new Adres()
            {
                KodKraju = t.KodKraju,
                Wojewodztwo = t.Wojewodztwo,
                Powiat = t.Powiat,
                Gmina = t.Gmina,
                Ulica = t.Ulica,
                NrDomu = t.NrDomu,
                NrLokalu = t.NrLokalu,
                Miejscowosc = t.Miejscowosc,
                KodPocztowy = t.KodPocztowy,
                Poczta = t.Poczta
            };
        }
    }
}

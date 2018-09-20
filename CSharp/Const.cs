using FV = JPKFVHurt;

namespace JPKFVHurt.CSharp
{
    class Const
    {
        #region urzedy
        public static readonly US[] urzedy = new US[]
        {
            new US{Nazwa= "URZĄD SKARBOWY W BOLESŁAWCU", KodUS = TKodUS.Item0202 },
            new US{Nazwa="URZĄD SKARBOWY W BYSTRZYCY KŁODZKIEJ", KodUS= TKodUS.Item0203 },
        };
        #endregion

        #region wojewowdztwa
        public static readonly Wojewodztwo[] wojewodztwaTab = new Wojewodztwo[]
        {
            new Wojewodztwo { Nazwa = "dolnośląskie" },
            new Wojewodztwo { Nazwa = "kujawsko-pomorskie" },
        };
        #endregion
    }
}

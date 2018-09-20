using System;

namespace JPKFVHurt.CSharp
{
    class MainStatic
    {
        public static DateTime LastDayOfMonth(DateTime d) => new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));
        public static DateTime FirstDayOfMonth(DateTime d) => new DateTime(d.Year, d.Month, 1);

        /// <summary>
        /// Sprawdzanie, czy podana sciezka zawiera pliki FKi
        /// </summary>
        /// <param name="p">sciezka programu</param>
        /// <param name="encode">kodowanie plików</param>
        /// <returns></returns>
        public static bool PathIsOK(string p, System.Text.Encoding encode)
        {
            var db = new SocialExplorer.IO.FastDBF.DbfFile(encode);
            try
            {
                db.Open(System.IO.Path.Combine(p, "WZ.DBF"), System.IO.FileMode.Open);
                db.Close();
                db.Open(System.IO.Path.Combine(p, "WZA.DBF"), System.IO.FileMode.Open);
                db.Close();
                db.Open(System.IO.Path.Combine(p, "PZ.DBF"), System.IO.FileMode.Open);
                db.Close();
                db.Open(System.IO.Path.Combine(p, "ZAL.DBF"), System.IO.FileMode.Open);
                db.Close();
                db.Open(System.IO.Path.Combine(p, "TSZ.DBF"), System.IO.FileMode.Open);
                db.Close();
                db.Open(System.IO.Path.Combine(p, "KONTRAH.DBF"), System.IO.FileMode.Open);
                db.Close();
                return true;
            }
            catch { return false; }
            finally { db.Close(); }
        }

        /// <summary>
        /// Obliczanie cyfry kontrolnej w NIPie i w REGONie
        /// </summary>
        /// <param name="s">Numer do sprawdzenia</param>
        /// <returns></returns>
        public static bool Cyfrakontrolna(string s)
        {
            System.Collections.Generic.List<int> wagi;
            int result = 0;

            switch (s.Length)
            {
                case 9:
                    wagi = new System.Collections.Generic.List<int> { 8, 9, 2, 3, 4, 5, 6, 7 };
                    break;
                case 14:
                    wagi = new System.Collections.Generic.List<int> { 2, 4, 8, 5, 0, 9, 7, 3, 6, 1, 2, 4, 8 };
                    break;
                case 10:
                    wagi = new System.Collections.Generic.List<int> { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
                    break;
                default:
                    wagi = new System.Collections.Generic.List<int>();
                    break;
            }
            if (wagi.Count != 0)
            {
                for (int i = 0; i < s.Length - 1; i++)
                    result += wagi[i] * (s[i] - 48);
                if (result % 11 == (s[s.Length - 1] - 48) ||
                    (result % 11 == 10 && (s[s.Length - 1] - 48) == 0))
                    return true;
            }
            return false;
        }

        public static string GetNip(string nip)
        {
            if (nip[0] == 'P')
            {
                nip = nip.Remove(0, 2);
            }
            return nip.Replace("-", "");
        }

        public static void ShowException(Exception ex, string title)
        {
            string msg = "";
            do
            {
                msg += "\n" + ex.Message;
                ex = ex.InnerException;
            }
            while (ex != null);
            System.Windows.MessageBox.Show(msg, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}

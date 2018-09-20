using System;
using System.Windows;

namespace JPKFVHurt.Config
{
    /// <summary>
    /// Interaction logic for ConfigWin.xaml
    /// </summary>
    public partial class ConfigWin : Window
    {
        private string homePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public void SetDanePodmiotu(Podmiot p) => ConfField.CompanyData = p;
        public Config ConfField { get; private set; } = new Config();
    }
}

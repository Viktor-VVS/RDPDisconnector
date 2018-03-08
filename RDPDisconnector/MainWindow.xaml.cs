using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RDPDisconnector
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RDPmonitoring rdpMonitor;
        public MainWindow()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.CanMinimize;
            rdpMonitor = new RDPmonitoring();
        }
        /// <summary>
        /// Загрузка настроек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings_Content.Text = File.ReadAllText(Environment.CurrentDirectory + "\\settings.ini");
                MessageBox.Show("Success Load !");
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Сохранение настроек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(Settings_Content.Text) == false)
                {
                    File.WriteAllText(Environment.CurrentDirectory + "\\settings.ini", Settings_Content.Text);
                    MessageBox.Show("Success Save !");
                }
                else
                {
                    MessageBox.Show("Can't Save empty file!");
                }

            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// Старт мониторинга активности RDP пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Load.IsEnabled = false;
            Save.IsEnabled = false;
            Start.IsEnabled = false;
            Stop.IsEnabled = true;
            //запуск в отдельном потоке отслеживание по таймауту активной сессии RDP 
            rdpMonitor.MonitoringThreadStart(Log);
        }
        /// <summary>
        /// Стоп мониторингу активности RDP пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Utils_.ActionWithGuiThreadInvoke(Log, () =>
            {
                Log.Text += "\r\n Stop monitoring";
            });
            Load.IsEnabled = true;
            Save.IsEnabled = true;
            Start.IsEnabled = true;
            Stop.IsEnabled = false;
            rdpMonitor.MonitoringThreadStop();
        }
        /// <summary>
        /// Очистить лог
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Log.Clear();
        }

       

      

    }
}

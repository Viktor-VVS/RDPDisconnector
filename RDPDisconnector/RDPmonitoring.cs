using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RDPDisconnector
{
    class RDPmonitoring
    {
        bool globalStopThread = false;//стоп потоку отслеживания RDP пользователя
        AutoResetEvent globalStopEvent = new AutoResetEvent(false);

        public void MonitoringThreadStart(TextBox Log)
        {
            globalStopThread = false;
            //запуск в отдельном потоке отслеживание по таймауту активной сессии RDP 
            Utils_.ActionInThread(() =>
            {
                try
                {
                    IntPtr pDll = kernel32.LoadLibraryW("sessionhelper.dll");
                    IntPtr pAddressOfFunctionToCall = kernel32.GetProcAddress(pDll, "IsUserSessionActive");
                    sessionhelper.IsUserSessionActive pIsUserSessionActive = (sessionhelper.IsUserSessionActive)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(sessionhelper.IsUserSessionActive));

                    IntPtr pDll2 = kernel32.LoadLibraryW("sessionhelper.dll");
                    IntPtr pAddressOfFunctionToCall2 = kernel32.GetProcAddress(pDll2, "DisconnectUser");
                    sessionhelper.DisconnectUser pDisconnectUser = (sessionhelper.DisconnectUser)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall2, typeof(sessionhelper.DisconnectUser));

                    int timeout_second = Convert.ToInt32(Extract.Between(File.ReadAllText(Environment.CurrentDirectory + "\\settings.ini"), "#monitor_timeout_second=", ";"));
                    while (true)
                    {
                        if (globalStopThread == true)
                            break;
                        //вывод на форму из рабочего потока.
                        Utils_.ActionWithGuiThreadInvoke(Log, () =>
                        {
                            Log.Text += "\r\nStart monitoring....";
                        });
                        //проверка наступил ли период запрета на соединение по RDP
                        if (CheckTimetoDisconnect() == true)
                        {
                            //Узнаем имя пользователя которое нужно отследить
                            string UserName = GetUserName();
                            if (String.IsNullOrEmpty(UserName))
                                throw new Exception("Error UserName is Empty or invalid.");
                            //проверка подключен ли пользователь по RDP
                            if (pIsUserSessionActive(UserName) == true)
                            {
                                Utils_.ActionWithGuiThreadInvoke(Log, () =>
                                {
                                    Log.Text += "\r\nDisconnect time is NOW!";
                                    Log.Text += "\r\n" + UserName + " session is active!...start to disconnect";
                                });
                                //если подключен то дисконнектим его
                                if (pDisconnectUser(UserName) == true)
                                {
                                    Utils_.ActionWithGuiThreadInvoke(Log, () =>
                                    {
                                        Log.Text += "\r\n" + UserName + " Disconnected!";
                                    });
                                }
                            }
                        }
                        //считаем таймаут и снова в цикле.
                        globalStopEvent.WaitOne(timeout_second * 1000);
                    }

                }
                catch (Exception ec)
                {
                    Utils_.ActionWithGuiThreadInvoke(Log, () =>
                    {
                        Log.Text += "\r\nException from another thread :" + ec.Message;
                    });
                }


            });
        }

        public void MonitoringThreadStop()
        {
            globalStopThread = true;
            globalStopEvent.Set();
        }

        /// <summary>
        /// Проверка,сейчас время работы или запрет и дисконнект.
        /// </summary>
        /// <returns></returns>
        private bool CheckTimetoDisconnect()
        {
            bool Res = false;
            string content = File.ReadAllText(Environment.CurrentDirectory + "\\settings.ini");
            string MinTime = Extract.Between(content, "#disconnect_time=", "-");
            string MaxTime = Extract.Between(content, MinTime + "-", ";");
            int MinMinutes = (Convert.ToInt32(Extract.BetweenStart(MinTime, ":")) * 60) + Convert.ToInt32(Extract.BetweenEnd(MinTime, ":"));
            int MaxMinutes = (Convert.ToInt32(Extract.BetweenStart(MaxTime, ":")) * 60) + Convert.ToInt32(Extract.BetweenEnd(MaxTime, ":"));

            string CurHour = DateTime.Now.ToShortTimeString();
            int CurMinutes = (Convert.ToInt32(Extract.BetweenStart(CurHour, ":")) * 60) + Convert.ToInt32(Extract.BetweenEnd(CurHour, ":"));
            if (CurMinutes > MinMinutes && CurMinutes < MaxMinutes)
            {
                Res = true;
            }
            return Res;
        }
        /// <summary>
        /// Получение Имени пользователя RDP для отслеживания его активности.
        /// </summary>
        /// <returns></returns>
        private string GetUserName()
        {
            return Extract.Between(File.ReadAllText(Environment.CurrentDirectory + "\\settings.ini"), "#rdp_user=", ";");
        }

    }

    /// <summary>
    /// Класс для работы с WinApi через p/invoke
    /// </summary>
    static class kernel32
    {
        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibraryW(string libFilename);

        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryA", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr LoadLibraryA(string libFilename);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
    }
    /// <summary>
    /// Класс описывающий экспортируемые функции Dll-ки 
    /// </summary>
    static class sessionhelper
    {

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate bool DisconnectUser(string UserName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate bool IsUserSessionActive(string UserName);
    }
}

using AutoUpdaterDotNET;
using Microsoft.Extensions.Options;
using PasswordManager.RestApiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Forms;

namespace PasswordManager.Services
{
    public class AutoUpdaterService
    {
        private readonly ApiConfig _config;
        public AutoUpdaterService(IOptions<ApiConfig> options)
        {
            _config = options.Value;
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            string version = fvi.FileVersion;
            //label1.Text = "Phiên bản: " + version;
            AutoUpdater.DownloadPath = "update";
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 15 * 60 * 1000
            };
            timer.Elapsed += delegate
            {
                AutoUpdater.Start(_config.baseUrl+"/Updater/update.xml");
            };
            timer.Start();
        }
        public void update()
        {
            AutoUpdater.Start(_config.baseUrl + "/Updater/update.xml");
        }
        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.IsUpdateAvailable)
            {
                DialogResult dialogResult;
                dialogResult =
                        MessageBox.Show(
                            $@"New update released {args.CurrentVersion}, You are using passman version {args.InstalledVersion}. Stay safe and secured by getting the lastest update of ours software ?", @"Update",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                {
                    try
                    {
                        if (AutoUpdater.DownloadUpdate(args))
                        {
                            Application.Exit();
                            System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                            System.Windows.Application.Current.Shutdown();
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                        System.Windows.Application.Current.Shutdown();
                    }
                }
                else
                {
                    if (args.Mandatory.Value)
                    {
                        System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                        System.Windows.Application.Current.Shutdown();
                    }
                }
            }
            else
            {
                //MessageBox.Show(@"Phiên bản bạn đang sử dụng đã được cập nhật mới nhất.", @"Cập nhật phần mềm",
                    //MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

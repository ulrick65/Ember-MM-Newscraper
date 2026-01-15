using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace XBMCRPC
{

    public partial class Client : IDisposable
    {
        internal IPlatformServices PlatformServices { get; set; }
        public readonly ConnectionSettings _settings;
        private uint JsonRpcId = 0;

        public Methods.Addons Addons { get; private set; }
        public Methods.Application Application { get; private set; }
        public Methods.AudioLibrary AudioLibrary { get; private set; }
        public Methods.Favourites Favourites { get; private set; }
        public Methods.Files Files { get; private set; }
        public Methods.GUI GUI { get; private set; }
        public Methods.Input Input { get; private set; }
        public Methods.JSONRPC JSONRPC { get; private set; }
        public Methods.Player Player { get; private set; }
        public Methods.Playlist Playlist { get; private set; }
        public Methods.Profiles Profiles { get; private set; }
        public Methods.PVR PVR { get; private set; }
        public Methods.Settings Settings { get; private set; }
        public Methods.System System { get; private set; }
        public Methods.Textures Textures { get; private set; }
        public Methods.VideoLibrary VideoLibrary { get; private set; }
        public Methods.XBMC XBMC { get; private set; }

        public Client(ConnectionSettings settings, IPlatformServices platformServices)
        {
            PlatformServices = platformServices;
            Serializer = new JsonSerializer();
            Serializer.Converters.Add(new StringEnumConverter());
            _settings = settings;
            Addons = new Methods.Addons(this);
            Application = new Methods.Application(this);
            AudioLibrary = new Methods.AudioLibrary(this);
            Favourites = new Methods.Favourites(this);
            Files = new Methods.Files(this);
            GUI = new Methods.GUI(this);
            Input = new Methods.Input(this);
            JSONRPC = new Methods.JSONRPC(this);
            Player = new Methods.Player(this);
            Playlist = new Methods.Playlist(this);
            Profiles = new Methods.Profiles(this);
            PVR = new Methods.PVR(this);
            Settings = new Methods.Settings(this);
            System = new Methods.System(this);
            Textures = new Methods.Textures(this);
            VideoLibrary = new Methods.VideoLibrary(this);
            XBMC = new Methods.XBMC(this);
        }

        internal JsonSerializer Serializer { get; private set; }

        async internal Task<T> GetData<T>(string method, object args)
        {
            var request = WebRequest.Create(_settings.JsonInterfaceAddress);
            request.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
            request.ContentType = "application/json";
            request.Method = "POST";

            using (var postStream = await request.GetRequestStreamAsync())
            {
                var requestId = JsonRpcId++;
                var jsonRequest = BuildJsonPost(method, args, requestId);
                byte[] postData = Encoding.UTF8.GetBytes(jsonRequest);
                postStream.Write(postData, 0, postData.Length);
            }

            string responseData;
            using (var response = await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                {
                    throw new InvalidOperationException("No response stream received from Kodi API");
                }

                using (var streamReader = new StreamReader(responseStream))
                {
                    responseData = streamReader.ReadToEnd();
                }
            }

            JObject query = JObject.Parse(responseData);

            var error = query["error"];
            if (error != null)
            {
                throw new Exception(error.ToString());
            }

            var resultToken = query["result"];
            if (resultToken == null)
            {
                throw new InvalidOperationException("Kodi API response missing 'result' field");
            }

            return resultToken.ToObject<T>(Serializer);
        }

        private static string BuildJsonPost(string method, object args, uint id)
        {
            var jsonPost = new JObject { new JProperty("jsonrpc", "2.0"), new JProperty("method", method) };
            if (args != null)
            {
                jsonPost.Add(new JProperty("params", args));
            }
            jsonPost.Add(new JProperty("id", id));

            return jsonPost.ToString();
        }


        private ISocket _clientSocket;

        public async Task StartNotificationListener()
        {
            _clientSocket = PlatformServices.SocketFactory.GetSocket();
            await _clientSocket.ConnectAsync(_settings.Host, _settings.TcpPort);

            var stream = _clientSocket.GetInputStream();

            ListenForNotifications(stream);
        }

        private async Task ListenForNotifications(Stream stream)
        {
            var socketState = new NotificationListenerSocketState();
            try
            {
                while (_clientSocket != null)
                {
                    var receivedDataLength =
                        await stream.ReadAsync(socketState.Buffer, 0, NotificationListenerSocketState.BufferSize);

                    var receivedDataJson = Encoding.UTF8.GetString(socketState.Buffer, 0, receivedDataLength);

                    socketState.Builder.Append(receivedDataJson);

                    JObject jObject;
                    if (TryParseObject(socketState.Builder.ToString(), out jObject))
                    {
                        ParseNotification(jObject);

                        socketState = new NotificationListenerSocketState();
                    }
                    else
                    {
                        // Begin listening for remainder of announcement using same socket state
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static bool TryParseObject(string announcementJson, out JObject jObject)
        {
            jObject = null;
            try
            {
                jObject = JObject.Parse(announcementJson);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        private void ParseNotification(JObject jObject)
        {
            var methodToken = jObject["method"];
            if (methodToken == null)
                return;

            var paramsToken = jObject["params"];
            if (paramsToken == null)
                return;

            var senderToken = paramsToken["sender"];
            var dataToken = paramsToken["data"];
            if (senderToken == null)
                return;

            string method = methodToken.ToString();
            switch (method)
            {
                case "Application.OnVolumeChanged":
                    Application.RaiseOnVolumeChanged(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Application.OnVolumeChanged_data>(Serializer)
                    );
                    break;
                case "AudioLibrary.OnCleanFinished":
                    AudioLibrary.RaiseOnCleanFinished(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "AudioLibrary.OnCleanStarted":
                    AudioLibrary.RaiseOnCleanStarted(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "AudioLibrary.OnRemove":
                    AudioLibrary.RaiseOnRemove(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.AudioLibrary.OnRemove_data>(Serializer)
                    );
                    break;
                case "AudioLibrary.OnScanFinished":
                    AudioLibrary.RaiseOnScanFinished(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "AudioLibrary.OnScanStarted":
                    AudioLibrary.RaiseOnScanStarted(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "AudioLibrary.OnUpdate":
                    AudioLibrary.RaiseOnUpdate(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.AudioLibrary.OnUpdate_data>(Serializer)
                    );
                    break;
                case "GUI.OnScreensaverActivated":
                    GUI.RaiseOnScreensaverActivated(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "GUI.OnScreensaverDeactivated":
                    GUI.RaiseOnScreensaverDeactivated(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "Input.OnInputFinished":
                    Input.RaiseOnInputFinished(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "Input.OnInputRequested":
                    Input.RaiseOnInputRequested(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Input.OnInputRequested_data>(Serializer)
                    );
                    break;
                case "Player.OnPause":
                    Player.RaiseOnPause(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Player.Notifications.Data>(Serializer)
                    );
                    break;
                case "Player.OnPlay":
                    Player.RaiseOnPlay(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Player.Notifications.Data>(Serializer)
                    );
                    break;
                case "Player.OnPropertyChanged":
                    Player.RaiseOnPropertyChanged(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Player.OnPropertyChanged_data>(Serializer)
                    );
                    break;
                case "Player.OnSeek":
                    Player.RaiseOnSeek(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Player.OnSeek_data>(Serializer)
                    );
                    break;
                case "Player.OnSpeedChanged":
                    Player.RaiseOnSpeedChanged(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Player.Notifications.Data>(Serializer)
                    );
                    break;
                case "Player.OnStop":
                    Player.RaiseOnStop(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Player.OnStop_data>(Serializer)
                    );
                    break;
                case "Playlist.OnAdd":
                    Playlist.RaiseOnAdd(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Playlist.OnAdd_data>(Serializer)
                    );
                    break;
                case "Playlist.OnClear":
                    Playlist.RaiseOnClear(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Playlist.OnClear_data>(Serializer)
                    );
                    break;
                case "Playlist.OnRemove":
                    Playlist.RaiseOnRemove(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.Playlist.OnRemove_data>(Serializer)
                    );
                    break;
                case "System.OnLowBattery":
                    System.RaiseOnLowBattery(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "System.OnQuit":
                    System.RaiseOnQuit(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "System.OnRestart":
                    System.RaiseOnRestart(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "System.OnSleep":
                    System.RaiseOnSleep(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "System.OnWake":
                    System.RaiseOnWake(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "VideoLibrary.OnCleanFinished":
                    VideoLibrary.RaiseOnCleanFinished(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "VideoLibrary.OnCleanStarted":
                    VideoLibrary.RaiseOnCleanStarted(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "VideoLibrary.OnRemove":
                    VideoLibrary.RaiseOnRemove(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.VideoLibrary.OnRemove_data>(Serializer)
                    );
                    break;
                case "VideoLibrary.OnScanFinished":
                    VideoLibrary.RaiseOnScanFinished(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "VideoLibrary.OnScanStarted":
                    VideoLibrary.RaiseOnScanStarted(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<object>(Serializer)
                    );
                    break;
                case "VideoLibrary.OnUpdate":
                    VideoLibrary.RaiseOnUpdate(
                        senderToken.ToObject<string>(Serializer),
                        dataToken?.ToObject<XBMCRPC.VideoLibrary.OnUpdate_data>(Serializer)
                    );
                    break;
            }
        }

        public void Dispose()
        {
            var socket = _clientSocket;
            _clientSocket = null;
            if (socket != null)
            {
                socket.Dispose();
            }
        }

    }
}
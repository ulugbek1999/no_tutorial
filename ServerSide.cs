private int _localPortNumber;
private Thread _listenerThread;
private bool _listenerStarted;
private TcpListener _tcpListener;


private void Form_Load(object sender, EventArgs e)
        {
            new DAL().CreateDB();
            CheckForIllegalCrossThreadCalls = false;

            //get local address information
            _localPortNumber = 8080;
            YOUR_IP_LABEL.Text = $"{GetLocalIP()}:{_localPortNumber}";

            _listenerStarted = true;
            _tcpListener = new TcpListener(IPAddress.Any, _localPortNumber);

            //separate thread to handle incoming requests
            _listenerThread = new Thread(ListenerThreadMethod);
            _listenerStarted = true;
            _tcpListener.Start();
            _listenerThread.Start();

        }
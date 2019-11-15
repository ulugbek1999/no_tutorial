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


private string GetLocalIP()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            return ip.ToString();
        }
    }
    return "?";
}








private void ListenerThreadMethod()
{
    while (_listenerStarted)
    {
        try
        {
            var handlerSocket = _tcpListener.AcceptSocket();
            if (handlerSocket.Connected)
            {
                ThreadPool.QueueUserWorkItem(HandleConnection, handlerSocket);
            }

        }
        catch (Exception)
        {
        }
    }
}






private void HandleConnection(object socketToProcess)
{
    if (socketToProcess is Socket handlerSocket)
    {
        var networkStream = new NetworkStream(handlerSocket);// get stream from the socket

        //find out what is wanted
        using (var reader = new BinaryReader(networkStream))
        {
            var clientIdLength = reader.ReadInt32();
            var clientId = new string(reader.ReadChars(clientIdLength));
            var cost = reader.ReadDecimal();
            var duration = reader.ReadDecimal();
            var callDateLength = reader.ReadInt32();
            var callDate = new string(reader.ReadChars(callDateLength));

            YOUR_LISTBOX.Items.Add($"Client id: {clientId}. Cost: {cost}. Duration: {duration}. Call date: {callDate}");
            var error = new YOUR_CUSTOM_NAME_DAL().YOUR_CUSTOM_INSERT_FUNCTION_NAME(clientId, cost, duration, callDate);

            if (error)
            {
                SendMessage("Error!");
                YOUR_LISTBOX.Items.Add("Error has occured!");
            }
            else
            {
                SendMessage("Success!");
                YOUR_LISTBOX.Items.Add("Successfully added!");
            }

            using (var writer = new BinaryWriter(networkStream))
            {
                writer.Write(true); //indicates that the message was received                        writer.Flush();
            }
        }

        handlerSocket = null; // nullify socket
    }
}





private void Form_FormClosing(object sender, FormClosingEventArgs e)
{
    StopListening();
}

private void StopListening()
{
    _listenerStarted = false;
    _tcpListener.Stop();
    _listenerThread.Interrupt();
    _listenerThread.Abort();
}




private Task SendMessage(string status)
{
    return Task.Run(() =>
    {
        try
        {

            var client = new TcpClient();
            client.Connect(GetLocalIP(), 8000);
            using (var writer = new BinaryWriter(client.GetStream()))
            {
                writer.Write(status.Length);
                writer.Write(status);
                writer.Flush();
            }

        }
        catch (Exception)
        {
        }
    });
}


// DAL

private string _c = Properties.Settings.Default.ConnectionString;

public void CreateDB()
{
    SqlCeEngine engine = new SqlCeEngine(_c);
    engine.CreateDatabase();
}

public bool YOUR_CUSTOM_INSERT_FUNCTION_NAME()
{
    bool error = false;
    try
    {
        _con = new SqlCeConnection(_c);
        _con.Open();
        _cmd = new SqlCeCommand($"INSERT INTO info (client_id, cost, duration, call_date) VALUES ('{}')", _con);
        _cmd.ExecuteNonQuery();
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message);
        error = true;
    }
    return error;
}
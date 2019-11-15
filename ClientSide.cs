private int _localPortNumber;
private Thread _listenerThread;
private bool _listenerStarted;
private TcpListener _tcpListener;





private void Form_Load(object sender, EventArgs e)
{
    CheckForIllegalCrossThreadCalls = false;

    //get local address information
    _localPortNumber = 8000;
    YOUR_IP_LABEL.Text = $"{GetLocalIP()}:{_localPortNumber}";

    //separate thread to handle incoming requests
    _listenerStarted = true;
    _tcpListener = new TcpListener(IPAddress.Any, _localPortNumber);

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
        var networkStream = new NetworkStream(handlerSocket);

        //find out what is wanted
        using (var reader = new BinaryReader(networkStream))
        {
            var statusLength = reader.ReadInt32();
            var status = new string(reader.ReadChars(statusLength));

            MessageBox.Show(status);

            using (var writer = new BinaryWriter(networkStream))
            {
                writer.Write(true);
            }
        }

        handlerSocket = null;
    }
}





private async void YOUR_SAVE_BTN_Click(object sender, EventArgs e)
{
    try
    {
        await SendMessage();
    }
    catch (Exception)
    {
    }
}





private Task SendMessage()
{
    return Task.Run(() =>
    {
        try
        {

            var client = new TcpClient();
            client.Connect(GetLocalIP(), 8080);
            using (var writer = new BinaryWriter(client.GetStream()))
            {
                writer.Write(YOUR_CLIENTID_TBX.Text.Length);
                writer.Write(YOUR_CLIENTID_TBX.Text.ToCharArray());
                writer.Write(YOUR_COST_NUD.Value);
                writer.Write(YOUR_DURATION_NUD.Value);
                writer.Write(YOUR_CALLDATE_DTP.Value.ToString().Length);
                writer.Write(YOUR_CALLDATE_DTP.Value.ToString().ToCharArray());
                writer.Flush();

                //get response
                using (var reader = new BinaryReader(client.GetStream()))
                {
                    if (reader.ReadBoolean())
                    {
                        YOUR_CLIENTID_TBX.Text = string.Empty;
                        YOUR_COST_NUD.Value = 0;
                        YOUR_DURATION_NUD.Value = 0;
                    }
                    else
                    {
                        MessageBox.Show("Sending failed");
                    }
                }
            }

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    });
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
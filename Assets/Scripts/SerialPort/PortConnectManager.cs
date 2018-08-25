using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System;

public class PortConnectManager
{
    /*************** listener ***************/
    public interface Listener
    {
        //connecting: do not connect
        void OnPortConnecting(string portName);

        void OnPortConnectStarted(string portName);

        void OnPortConnected(string portName);

        void OnPortDisconnect(string portName);

        void OnPortConnectTimeOut(string portName);

        void OnPortInsert(string portName);

        void OnPortPullOut(string portName);

        void OnPortError(string portName, string errorMsg);

        void OnPortReceive(string data);

        void OnPortOpenSucceed(string portName);

        void OnPortOpenFailed(string portName, string reason);
    }

    private List<Listener> _listenerList = new List<Listener>();

    public void AddListener(Listener listener)
    {
        if (!_listenerList.Contains(listener))
        {
            _listenerList.Add(listener);
        }
    }

    /*************** others ***************/
    private bool _isConnected;
    private bool _isConnecting;

    //if larger than COM9,for example COM13 _portName still is COM13,_port.PortName is //./COM13
    private SerialPort _port;
    private string _portName;

    public Thread _readPortThread;
    private bool _keepReading;

    private Timer _insertAndPullOutMonitorTimer;
    private Timer _connectTimeOutMonitorTimer;

    private List<string> _lastPortNameList;

    /*************** single ***************/
    private static PortConnectManager _INSTANCE;

    private PortConnectManager()
    {
    }

    public static PortConnectManager GetInstance()
    {
        if (_INSTANCE == null)
        {
            _INSTANCE = new PortConnectManager();

            _INSTANCE._isConnected = false;
            _INSTANCE._isConnecting = false;
            //_INSTANCE._port = new SerialPort();

            _INSTANCE._keepReading = false;

            _INSTANCE.startInsertMonitorTimer();
            _INSTANCE._lastPortNameList = GetAllPortNames();
        }

        return _INSTANCE;
    }

    /*************** main method ***************/
    public void Connect(string portName)
    {
        if (_port != null && _port.IsOpen)
        {
            if (_isConnected)
            {
                Debug.LogWarning("Has connected : " + _portName + "\n");
                return;
            }
            if (_isConnecting)
            {
                Debug.LogWarning("Is connecting : " + _portName + "\n");

                foreach (Listener listener in _listenerList)
                {
                    listener.OnPortConnecting(_portName);
                }

                return;
            }
        }

        int portNumber = -1;
        if (Global.GetInstance().GetOSPlatform() == Global.OS_Platform_Enum.Win_64 || Global.GetInstance().GetOSPlatform() == Global.OS_Platform_Enum.Win_32)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(portName.ToUpper(), "^COM\\d*$"))
            {
                portNumber = int.Parse(portName.Remove(0, 3));
            }
        }

        _portName = portName;

        if (portNumber > 9)
        {
            _port = new SerialPort(@"\\.\" + _portName);
        }else
        {
            _port = new SerialPort(_portName);
        }
   
        //  _port.PortName = portName;
        _port.BaudRate = 115200;
        _port.DataBits = 8;
        _port.StopBits = StopBits.One;
        _port.ReadTimeout = SerialPort.InfiniteTimeout;
        _port.DtrEnable = true;
        //_port.NewLine = "\n";

        try
        {
            _port.Open();
            if (_port.IsOpen)
            {

                Debug.Log("Open succeed : " + portName + "\n");

                foreach (Listener listener in _listenerList)
                {
                    listener.OnPortOpenSucceed(_portName);
                }

                //open port succeed, port will be reseted and i will receive data
                //parse data by _readPortThread
                _isConnecting = true;
                _isConnected = false;

                startReadThread();

                foreach (Listener listener in _listenerList)
                {
                    listener.OnPortConnectStarted(_portName);
                }

                //send 'M105' per 1 second 8 repeats,
                //if receive temperature info ---> connected
                //else if not receive temperature info ---> connect time out
                startConnectTimeOutMonitorTimer();

                Debug.Log("Try to connect : " + _portName + "\n");
            }
        }
        catch (IOException e)
        {

            //_port is busy, _port name is wrong, _port is not inserted
            Debug.LogWarning("Open failed : " + e.ToString() + "\n");
            tellListeners_OpenFailed(_portName, e.ToString());
        }
    }

    public void Disconnect()
    {
        if(_port == null)
        {
            return;
        }
            if (!_port.IsOpen)
        {
            Debug.LogWarning("port not open" + "\n");
            return;
        }

        if (_isConnecting)
        {
            Debug.LogWarning("Is connecting " + "\n");
            return;
        }

        if (!_isConnected)
        {
            Debug.LogWarning("Have disconnected " + "\n");
            return;
        }

        tellListeners_Disconnect(_portName);
    }

    public void WriteCmd(string cmd)
    {
        if (!_port.IsOpen)
        {
            Debug.LogWarning("port not open" + "\n");
            return;
        }

        _port.WriteLine(cmd);
        //Debug.Log("" + cmd + "\n");
    }

    public void ReleaseAll()
    {
        if (_isConnected)
        {
            tellListeners_Disconnect(_portName);
        }

        _isConnected = false;
        _isConnecting = false;

        if (_port != null)
        {
            _port.Dispose();
            if (_port.IsOpen)
            {
                _port.Close();
            }
        }
        
        _portName = null;
        stopReadThread();

        _insertAndPullOutMonitorTimer.Dispose();

        if (_connectTimeOutMonitorTimer != null)
        {
            _connectTimeOutMonitorTimer.Dispose();
        }

        _INSTANCE = null;
    }

    public bool IsConnected()
    {
        return _isConnected;
    }

    public bool IsConnecting()
    {
        return _isConnecting;
    }

    /*************** timer and thread ***************/
    private void startReadThread()
    {
        _keepReading = true;
        _readPortThread = new Thread(readPort);
        _readPortThread.Start();
    }

    public void stopReadThread()
    {
        if (_readPortThread != null && _readPortThread.IsAlive)
        {
            Debug.Log("Stop read port thread" + "\n");
            _keepReading = false;
            //		_readThread.Abort ();
        }
    }

    int _count = 0;
    private void startConnectTimeOutMonitorTimer()
    {_count = 0;
        _connectTimeOutMonitorTimer = new Timer(new TimerCallback(timerCall_connectTimeOut), null, 1500, 1000);
    }

    private void timerCall_connectTimeOut(object obj)
    {
        Debug.Log("timerCall_connectTimeOut");
        if (++_count >= 8)
        {
            tellListeners_ConnectTimeOut(_portName);
            _connectTimeOutMonitorTimer.Dispose();
            return;
        }

        if (_isConnected)
        {
            _connectTimeOutMonitorTimer.Dispose();
            return;
        }

        WriteCmd("M105");
    }

    private void startInsertMonitorTimer()
    {
        _insertAndPullOutMonitorTimer = new Timer(new TimerCallback(timerCall_insert), null, 0, 100);
    }

    private void timerCall_insert(object obj)
    {
        if (_lastPortNameList.Count != GetAllPortNames().Count)
        {
            //todo:get port name
            string portName = null;
            if (_lastPortNameList.Count > GetAllPortNames().Count)
            {
                foreach (string temp in _lastPortNameList)
                {
                    if (!GetAllPortNames().Contains(temp))
                    {
                        portName = temp;
                        tellListeners_PullOut(portName);
                        break;
                    }
                }
            }
            else
            {
                foreach (string temp in GetAllPortNames())
                {
                    if (!_lastPortNameList.Contains(temp))
                    {
                        portName = temp;
                        tellListeners_Insert(portName);
                        break;
                    }
                }
            }
            _lastPortNameList = GetAllPortNames();
        }
    }

    /*************** get port name ***************/
    public static List<string> GetAllPortNames()
    {
        switch (Global.GetInstance().GetOSPlatform())
        {
            case Global.OS_Platform_Enum.Mac:
                {
                    return getPortNamesOfUnix();
                }
            case Global.OS_Platform_Enum.Win_64:
            case Global.OS_Platform_Enum.Win_32:
                {
                    string[] nameArray = SerialPort.GetPortNames();
                    return new List<string>(nameArray);
                }
            case Global.OS_Platform_Enum.Linux:
                //todo
                break;
            case Global.OS_Platform_Enum.Unsupport:
                break;
        }
        return null;
    }

    //hack : http://answers.unity3d.com/questions/643078/serialportsgetportnames-error.html
    //SerialPort.GetPortNames() return empty on Mac of liuming
    private static List<string> getPortNamesOfUnix()
    {
        int p = (int)Environment.OSVersion.Platform;
        List<string> serial_ports = new List<string>();

        // Are we on Unix?
        if (p == 4 || p == 128 || p == 6)
        {
            string[] ttys = Directory.GetFiles("/dev/", "tty.*");
            foreach (string dev in ttys)
            {
                if (dev.StartsWith("/dev/tty."))
                {
                    serial_ports.Add(dev);
                }
            }
        }
        return serial_ports;
    }

    /*************** tell all listeners ***************/
    private void tellListeners_Disconnect(string portName)
    {
        Debug.Log("!! OnPortDisconnect : " + portName + "\n");
        _isConnecting = false;
        _isConnected = false;

        stopReadThread();

        _connectTimeOutMonitorTimer.Dispose();

        _port.Dispose();
        if (_port.IsOpen)
        {
            _port.Close();
        }

        foreach (Listener listener in _listenerList)
        {
            listener.OnPortDisconnect(portName);
        }
    }

    private void tellListeners_Connect(string portName)
    {
        Debug.Log("@@ OnPortConnect : " + portName + "\n");

        _isConnecting = false;
        _isConnected = true;
        _connectTimeOutMonitorTimer.Dispose();

        foreach (Listener listener in _listenerList)
        {
            listener.OnPortConnected(portName);
        }
    }

    private void tellListeners_ConnectTimeOut(string portName)
    {
        Debug.Log("## OnPortConnectTimeOut : " + portName + "\n");

        _isConnecting = false;
        _isConnected = false;

        stopReadThread();

        _port.Dispose();
        if (_port.IsOpen)
        {
            _port.Close();
        }

        foreach (Listener listener in _listenerList)
        {
            listener.OnPortConnectTimeOut(portName);
        }
    }

    private void tellListeners_Error(string portName,string errorMsg)
    {
        Debug.Log("$$ OnPortError : " + portName + "\n");

        ReleaseAll();

        foreach (Listener listener in _listenerList)
        {
            listener.OnPortError(portName,  errorMsg);
        }
    }

    private void tellListeners_Receive(string data)
    {
        Debug.Log("%% OnPortReceive : " + data + "\n");

        foreach (Listener listener in _listenerList)
        {
            listener.OnPortReceive(data);
        }
    }

    private void tellListeners_OpenFailed(string portName, string reason)
    {
        Debug.Log("^^ OnPortOpenFailed : " + portName + " " + reason + "\n");

        _isConnecting = false;
        _isConnected = false;

        _port.Dispose();

        foreach (Listener listener in _listenerList)
        {
            listener.OnPortOpenFailed(portName, reason);
        }
    }

    private void tellListeners_Insert(string portName)
    {
        Debug.Log("&& OnPortInsert : " + portName + "\n");

        foreach (Listener listener in _listenerList)
        {
            listener.OnPortInsert(portName);
        }
    }

    private void tellListeners_PullOut(string portName)
    {
        Debug.Log("** OnPortPullOut : " + portName + "\n");
        if (portName == _portName)
        {
            tellListeners_Disconnect(portName);
        }
        foreach (Listener listener in _listenerList)
        {
            listener.OnPortPullOut(portName);
        }
    }

    /*************** tell all listeners ***************/
    List<byte> listReceive = new List<byte>();

    private void readPort()
    {
        while (_keepReading)
        {

            if (_port.IsOpen)
            {
                string line = null;
                try
                {
                    byte b = Convert.ToByte(_port.ReadByte());
                    //Debug.LogError("# "+b+" #"+"\n" );
                    if (b == ('\0') || b == ('\n'))
                    {
                        line = System.Text.Encoding.Default.GetString(listReceive.ToArray());
                        //hack for win: .net2.0 for unity, lost data.
                        //expect data: "ok T:XXX "    received data: "k T:XXX"
                        if (line.StartsWith("k T:"))
                        {
                            line = "o" + line;
                        }
                        //for(int k=0; k<listReceive.Count ;k++){
                        //string s = string.Format("{0:x}", listReceive[k])+"#";
                        //UnityEngine.Debug.Log("listReceive["+k+"] ="+  s+"\n");
                        //}
                        listReceive.Clear();
                    }
                    else
                    {
                        listReceive.Add(b);
                    }
                }
                catch (ThreadAbortException e)
                {
                    Debug.LogError("Exception ReadLine : " + e.ToString() + "\n");
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception ReadLine : " + e.ToString() + "\n");
                    //hack:Windows, catch time-out when exception execute '_port.ReadByte()'
                    //if return from thread, get no data.
                    //return;
                }

                if (line != null)
                {

                    if (line.ToLower().Contains("error"))
                    {
                        Debug.LogError("Error : " + line + "\n");
                        tellListeners_Error(_portName, line);
                        return;
                    }

                    if(!_isConnected){
                        if (line.Trim().StartsWith("T:") || line.Trim().StartsWith("ok T:"))
                        {
                            tellListeners_Connect(_portName);
                        }
                    }

                    tellListeners_Receive(line);
                }
            }
        }
    }
}

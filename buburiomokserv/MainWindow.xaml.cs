using System.Collections.ObjectModel;
using System.Configuration;
using System.Net; //TCP
using System.Net.Sockets; //TCP
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Xml.XPath;

namespace buburichesserv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class AsyncObject 
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[BufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }
    }

    public partial class MainWindow : Window
    {
        Dictionary<string, Socket> userdata =new Dictionary<string, Socket>();
        Dictionary<int, Socket> userdataint = new Dictionary<int, Socket>();
        Dictionary<string, string> userstatus = new Dictionary<string, string>();
        Dictionary<Socket, Socket> userversus = new Dictionary<Socket, Socket>();
        TcpListener server;
        Socket mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        
        public MainWindow()
        {
            InitializeComponent();
            string bindIP = "10.10.20.120";
            const int bindPort = 9190;
            try
            {
                IPEndPoint localAdr = new IPEndPoint(IPAddress.Parse(bindIP), bindPort);//주소 정보 설정
                mainSock.Bind(localAdr);
                mainSock.Listen(5);
                mainSock.BeginAccept(AcceptCallback, null);
            }
            catch (SocketException err) //소켓 오류 날때 예외처리
            {
                MessageBox.Show(err.ToString());
            }
            finally
            {
                //server.Stop(); //서버 종료
               
            }
        }
        
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket client = mainSock.EndAccept(ar);
            mainSock.BeginAccept(AcceptCallback, null);
            AsyncObject obj = new AsyncObject(4096);
            obj.WorkingSocket = client;
            client.BeginReceive(obj.Buffer, 0, 4096, 0, Getdata,obj);
            
        }
        int i = 1;
        Socket randombl;
        private void Getdata(IAsyncResult ar)
        {
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            Socket fromcli = obj.WorkingSocket;
            string fromclimsg = Encoding.Default.GetString(obj.Buffer);
            string selfunc = fromclimsg.Split('^')[0];
            string climsg = fromclimsg.Split('^')[1];
            string tocli = null;
            byte[] sendcli = new byte[16];
            //MessageBox.Show(climsg);
            if (selfunc == "1")
            {
                //selfunc가 1이면 닉네임중복검사
                if (userdata.ContainsKey(climsg))
                {
                    tocli = "no^";
                    sendcli = Encoding.Default.GetBytes(tocli);
                    fromcli.Send(sendcli);
                }
                else
                {
                    tocli = "ok^";
                    sendcli = Encoding.Default.GetBytes(tocli);
                    fromcli.Send(sendcli);
                    userdata.Add(climsg, fromcli);
                    userstatus.Add(climsg, "NotPlaying");
                }
            }
            else if (selfunc == "2")
            {

                if (userdata.TryGetValue(climsg, out Socket sendclisoc))
                {
                    //MessageBox.Show("찾는친구: " + climsg);

                    if (userstatus[climsg] == "NotPlaying")
                    {
                        //MessageBox.Show("어어그래 친구초대보내볼게");
                        tocli = "invite";
                        sendcli = Encoding.Default.GetBytes(tocli);
                        sendclisoc.Send(sendcli);
                        userversus.Add(sendclisoc,fromcli);
                        userversus.Add(fromcli, sendclisoc);
                    }
                    else
                    {
                        MessageBox.Show("어어그래 친구초대실패할게");
                        tocli = "이미게임중입니다";
                        sendcli = Encoding.Default.GetBytes(tocli);
                        fromcli.Send(sendcli);
                    }
                }
                else
                {
                    MessageBox.Show("어어그래 친구찾지도못해볼게");
                    tocli = "no";
                    sendcli = Encoding.Default.GetBytes(tocli);
                    fromcli.Send(sendcli);
                }
            }
            else if (selfunc == "2-2")
            {
                if (climsg=="yes")
                {
                    tocli = "check";
                    sendcli= Encoding.Default.GetBytes(tocli);
                    fromcli.Send(sendcli);
                    userversus[fromcli].Send(sendcli);
                    string vsnick = null;
                    string mynick = null;
                    mynick = userdata.FirstOrDefault(x => x.Value == fromcli).Key;
                    vsnick = userdata.FirstOrDefault(x => x.Value == userversus[fromcli]).Key;
                    userstatus[mynick] = "white";
                    userstatus[vsnick] = "black";
                }
                else
                {
                    tocli = "시러시러잉^";
                    sendcli = Encoding.Default.GetBytes(tocli);
                    
                    userversus[fromcli].Send(sendcli);
                }
            }
            else if(selfunc=="2-3")
            {
                i++;
                if (i == 2)
                {
                    string vsnick = null;
                    string mynick = null;
                    mynick = userdata.FirstOrDefault(x => x.Value == fromcli).Key;
                    vsnick = userdata.FirstOrDefault(x => x.Value == userversus[fromcli]).Key;
                    tocli = mynick + "^" +userstatus[mynick] + "^" + vsnick + "^" + userstatus[vsnick] + "^" ;
                    sendcli = Encoding.Default.GetBytes(tocli);
                    fromcli.Send(sendcli);
                    tocli = vsnick + "^" + userstatus[vsnick] + "^" + mynick + "^" + userstatus[mynick] + "^";
                    sendcli = Encoding.Default.GetBytes(tocli);
                    userversus[fromcli].Send(sendcli);
                    i = 0;
                }
            }
            
            else if (selfunc == "3")
            {
                
                if (i == 2)
                {
                    userstatus[climsg] = "white";//NotPlaying 에서 백돌게임중으로 변경
                    sendcli = Encoding.Default.GetBytes("random" + "^");
                    fromcli.Send(sendcli);
                    randombl.Send(sendcli);
                    userversus.Add(fromcli, randombl);
                    userversus.Add(randombl, fromcli);
                    i = 0;
                }
                else
                {
                    userstatus[climsg] = "black";//NotPlaying 에서 흑돌게임중으로 변경
                    sendcli = Encoding.Default.GetBytes(userstatus[climsg] + "^");
                    randombl = fromcli;
                    //fromcli.Send(sendcli);
                    i++;
                }
            }
            else if (selfunc == "4")
            {
                //selfunc4의 경우 게임진행
                string xpos = fromclimsg.Split('^')[2];// 좌표 받음
                string ypos = fromclimsg.Split('^')[3];
                string sendxypos = "4^" + xpos + "^" + ypos + "^";//xy 좌표 받아서 보낼준비
                byte[] xypos = new byte[sendxypos.Length];
                xypos = Encoding.Default.GetBytes(sendxypos);
                fromcli.Send(xypos);
                userversus[fromcli].Send(xypos);
            }
            else if (selfunc == "5")
            {
                //selfucn5는 채팅
                //MessageBox.Show(climsg);
                tocli = "5^"+userdata.FirstOrDefault(x => x.Value == fromcli).Key+": "+climsg+"^";
                sendcli= Encoding.Default.GetBytes(tocli);
                foreach (Socket Value in userdata.Values)
                {
                    Value.Send(sendcli);
                }
            }
            obj.ClearBuffer();
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, Getdata, obj);
        }
    }
}

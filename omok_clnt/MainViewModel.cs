using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// 추가한 헤더
using System.Net.Sockets;
using System.Windows;

namespace chessclnt
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public TcpClient client { get; private set; }
        public NetworkStream stream { get; private set; }

        public MainViewModel()
        {
            ConnectToServer();
        }
        public async Task ConnectToServer() // 서버 연결 함수
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync("10.10.20.120", 9190);
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                MessageBox.Show("서버연결 안됨");
            }
        }

        public string username;
        public string Nickname { get { return username; } }

        public string posMsg;
        public string posGet { get { return posMsg; } }

        public string oppname;
        public string stonecolor1; // 내 돌색
        public string stonecolor2; // 상대방 돌색

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

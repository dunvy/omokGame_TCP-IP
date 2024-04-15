using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//추가한 헤더
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace chessclnt
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Window1 : Window
    {
        private MainViewModel viewModel;

        public Window1(MainViewModel mainViewModel)
        {
            InitializeComponent();
            viewModel = mainViewModel;
            DataContext = viewModel;
            checkserver();
        }

        private void checkserver()
        {
            if (viewModel.client != null)
            {
                MessageBox.Show("서버 이미 연결");
            }
            else
            {
                MessageBox.Show("연결안됨 퉤퉤");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) // 초대하기 버튼 클릭했을 때
        {
            var mainViewModel = (MainViewModel)DataContext;
            // 초대할 친구 닉네임 보내기
            Byte[] friendname = Encoding.UTF8.GetBytes("2^"+invite.Text);
            mainViewModel.stream.Write(friendname, 0, friendname.Length);

            if (mainViewModel.stream.CanRead)
            {
                string receivemsg;

                byte[] receivebytes = new byte[mainViewModel.client.ReceiveBufferSize];
                mainViewModel.client.GetStream().Read(receivebytes, 0, receivebytes.Length);

                receivemsg = Encoding.UTF8.GetString(receivebytes);

                if(receivemsg.Contains("no") == true)
                {
                    MessageBox.Show("친구 닉네임을 다시 입력해주세요.");
                }
            }
        }
    }
}

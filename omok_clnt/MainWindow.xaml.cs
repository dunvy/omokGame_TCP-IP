//추가한 헤더
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows;


namespace chessclnt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainViewModel viewModel = new MainViewModel();
            DataContext = viewModel;
            Readdata();
            
            random.Visibility = Visibility.Hidden;
            invite.Visibility = Visibility.Hidden;
            freind.Visibility = Visibility.Hidden;

        }

        private void Button_Click(object sender, RoutedEventArgs e) //접속하기 버튼 클릭하기
        {
            var mainViewModel = (MainViewModel)DataContext;
            if (mainViewModel != null && mainViewModel.stream != null)
            {
                //닉네임 전송 코드
                Byte[] sendnickname = Encoding.UTF8.GetBytes("1^" + nickname.Text + "^");
                mainViewModel.stream.Write(sendnickname, 0, sendnickname.Length);
                mainViewModel.username = nickname.Text;
            }
        }

        private async void Readdata()
        {
            MainViewModel mainViewModel = (MainViewModel)this.DataContext;
            await Task.Run(async() =>
            {
                while (true)
                {
                    if (mainViewModel != null && mainViewModel.stream != null)
                    {
                        string receivemsg;
                        byte[] receivebytes = new byte[mainViewModel.client.ReceiveBufferSize];
                        mainViewModel.client.GetStream().Read(receivebytes, 0, receivebytes.Length);
                        receivemsg = Encoding.UTF8.GetString(receivebytes);

                        mainViewModel.posMsg = receivemsg;

                        MessageBox.Show(receivemsg);
                        if (receivemsg.Contains("ok") == true)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                info.Content = nickname.Text + "님 접속을 환영합니다!";
                                nickname.Visibility = Visibility.Hidden;
                                enter.Visibility = Visibility.Hidden;

                                random.Visibility = Visibility.Visible;
                                invite.Visibility = Visibility.Visible;
                            });
                        }
                        else if (receivemsg.Contains("random") == true) // 랜덤매칭 받앗을때
                        {
                            mainViewModel.posMsg = receivemsg;
                            if (mainViewModel != null && mainViewModel.stream != null && mainViewModel.client != null)
                            {
                                string tmp = "2-3^";
                                Byte[] request = Encoding.UTF8.GetBytes(tmp);
                                mainViewModel.stream.Write(request, 0, request.Length);

                            }
                        }
                        else if (receivemsg.Split('^')[0] == mainViewModel.username)
                        {
                            //MessageBox.Show("여기왜안들어와 샹");
                            // 화면전환 코드
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Uri uri = new Uri("Page1.xaml", UriKind.Relative);
                                Page1 page = new Page1(mainViewModel);
                                this.Content = page;
                            });
                        }
                        //else if (receivemsg.Contains("white") == true) // 후 입장 백
                        //{
                        //    mainViewModel.posMsg = receivemsg;
                        //    if (mainViewModel != null && mainViewModel.stream != null && mainViewModel.client != null)
                        //    {
                        //        // 화면전환 코드
                        //        Application.Current.Dispatcher.Invoke(() =>
                        //        {
                        //            Uri uri = new Uri("Page1.xaml", UriKind.Relative);
                        //            Page1 page = new Page1(mainViewModel);
                        //            this.Content = page;
                        //        });
                        //    }
                        //}
                        else if (receivemsg.Contains("invite") == true)
                        {
                            if (MessageBox.Show("초대 요청이 왔습니다.\n수락하시겠습니까?", "Yes-No", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                if (mainViewModel != null && mainViewModel.stream != null) // 이게 Yes 눌렀을때
                                {
                                    string tmp = "2-2^yes^";
                                    Byte[] request = Encoding.UTF8.GetBytes(tmp);
                                    mainViewModel.stream.Write(request, 0, request.Length);
                                }
                            }
                        }
                        else if (receivemsg.Contains("check") == true)
                        {
                            mainViewModel.posMsg = receivemsg;
                            if (mainViewModel != null && mainViewModel.stream != null && mainViewModel.client != null)
                            {
                                string tmp = "2-3^";
                                Byte[] request = Encoding.UTF8.GetBytes(tmp);
                                mainViewModel.stream.Write(request, 0, request.Length);

                            }
                            //if (mainViewModel != null && mainViewModel.stream != null && mainViewModel.client != null)
                            //{
                            //    // 화면전환 코드
                            //    try
                            //    {
                            //        Application.Current.Dispatcher.Invoke(() =>
                            //        {
                            //            Uri uri = new Uri("Page1.xaml", UriKind.Relative);
                            //            Page1 page = new Page1(mainViewModel);
                            //            this.Content = page;
                            //        });
                            //    }
                            //    catch
                            //    {
                            //        MessageBox.Show("예외발생?");
                            //    }
                            //}
                        }
                        receivemsg = "";
                    }                    
                }
            });
        }

        private void random_Click(object sender, RoutedEventArgs e) // 랜덤매칭
        {
            var mainViewModel = (MainViewModel)DataContext;
            string tmp = "3^"+nickname.Text+"^";
            Byte[] request = Encoding.UTF8.GetBytes(tmp);
            mainViewModel.stream.Write(request, 0, request.Length);

            random.Content = "매칭중...";
            random.IsEnabled = false;
            invite.IsEnabled = false;
        }

        private void invite_Click(object sender, RoutedEventArgs e) // 초대하기 버튼 눌렀을때
        {
            var mainViewModel = (MainViewModel)DataContext;
            info.Content = "초대할 유저 닉네임을 입력해주세요.";
            nickname.Text = ""; //닉네임 창 클리어해줌
            nickname.Visibility = Visibility.Visible;
            freind.Visibility = Visibility.Visible;
            random.Visibility = Visibility.Hidden;
            invite.Visibility = Visibility.Hidden;
        }

        private void freind_Click(object sender, RoutedEventArgs e) // 초대 눌렀을때
        {
            var mainViewModel = (MainViewModel)DataContext;
            string tmp = "2^" + nickname.Text+"^";
            Byte[] request = Encoding.UTF8.GetBytes(tmp);
            mainViewModel.stream.Write(request, 0, request.Length);
        }
    }
}
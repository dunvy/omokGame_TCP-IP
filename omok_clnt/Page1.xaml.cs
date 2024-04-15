using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace chessclnt
{
    /// <summary>
    /// Page1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Page1 : Page
    {
        private MainViewModel viewModel;


        private int marginX = 30;
        private int marginY = 20;

        private int houseSize = 40;     // 눈 사이즈
        private int stoneSize = 38;     // 돌 사이즈
        private int flowerSize = 10;    // 화점 사이즈
        private bool color = false;     // false = 흑돌, true = 백돌

        private string start;           // 선
        private string myColor;         // 내가 부여받은 색
        private int turn;               // 턴제

        // 좌표
        int[] xpos = new int[19];
        int[] ypos = new int[19];

        int[,] STONE = new int[19, 19];

        private ObservableCollection<string> chatLog = new ObservableCollection<string>();


        public static BitmapImage one = new BitmapImage(new Uri("/Resources/1.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage two = new BitmapImage(new Uri("/Resources/2.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage three = new BitmapImage(new Uri("/Resources/3.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage four = new BitmapImage(new Uri("/Resources/4.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage five = new BitmapImage(new Uri("/Resources/5.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage six = new BitmapImage(new Uri("/Resources/6.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage seven = new BitmapImage(new Uri("/Resources/7.png", UriKind.RelativeOrAbsolute));
        public static BitmapImage eight = new BitmapImage(new Uri("/Resources/8.png", UriKind.RelativeOrAbsolute));

        List<BitmapImage> monster = new List<BitmapImage> { one, two, three, four, five, six, seven, eight };

        public Page1(MainViewModel mainViewModel)
        {
            InitializeComponent();
            viewModel = mainViewModel;
            DataContext = viewModel;
            
            log.ItemsSource = chatLog;

            DrawBoard();

            user1name.Content = mainViewModel.posMsg.Split('^')[0]; //내 닉네임 띄우기
            user2name.Content = mainViewModel.posMsg.Split('^')[2]; //상대방 닉네임 띄우기
            stonecolor1.Content = mainViewModel.posMsg.Split('^')[1]; //내 돌색
            stonecolor2.Content = mainViewModel.posMsg.Split('^')[3]; // 상대방 돌색

            Random rand = new Random();
            int randnum = rand.Next(0, 7);
            int randnum2 = rand.Next(0, 7);

            user1.Source = monster[randnum];
            user2.Source = monster[randnum2];

            myColor = mainViewModel.posMsg.Split('^')[1];

            MessageBox.Show(myColor);

            start = "black";

            ReadMsg();
        }

        // 게임판 그리기
        private void DrawBoard()
        {
            for (int i = 0; i < 19; i++)
            {
                for (int j = 0; j < 19; j++)
                {
                    STONE[i, j] = 0;
                }
            }

            // 가로선
            for (int i = 0; i < 19; i++)
            {
                drawLine(marginX, marginY + houseSize * i, marginX + houseSize * 18, marginY + houseSize * i);
                if (i == 0)
                    xpos[i] += marginX + houseSize / 2;
                else
                    xpos[i] = xpos[i - 1] + houseSize;
            }
            // 세로선
            for (int i = 0; i < 19; i++)
            {
                drawLine(marginX + houseSize * i, marginY, marginX + houseSize * i, marginY + houseSize * 18);
                if (i == 0)
                    ypos[i] += marginY + houseSize / 2;
                else
                    ypos[i] = ypos[i - 1] + houseSize;
            }
            // 화점
            for (int i = 3; i < 16; i += 6)
                for (int j = 3; j < 16; j += 6)
                    drawCircle(marginX + houseSize * i - flowerSize / 2, marginY + houseSize * j - flowerSize / 2, flowerSize, flowerSize);
        }

        private void drawLine(int x1, int y1, int x2, int y2)
        {
            // 라인의 속성에 line 하나 만듦
            Line line = new Line();
            line.X1 = x1;               // 선의 시작 X 좌표
            line.Y1 = y1;               // 선의 시작 Y 좌표
            line.X2 = x2;               // 선의 종료 X 좌표
            line.Y2 = y2;               // 선의 종료 Y 좌표

            // line 색
            line.Stroke = Brushes.Black;

            // 캔버스에 바둑판 라인 집어넣기
            canvas1.Children.Add(line);
        }

        private void drawCircle(int x, int y, int h, int w)
        {
            Ellipse e = new Ellipse();

            // 화점 크기
            e.Width = w;
            e.Height = h;

            // 화점 위치
            e.Margin = new Thickness(x, y, 0, 0);

            // 화점 색
            e.Fill = Brushes.Black;

            // 캔버스에 화점 넣기
            canvas1.Children.Add(e);
        }


        // 상대방 돌 띄우기
        private async void ReadMsg()
        {
            MainViewModel mainViewModel = (MainViewModel)this.DataContext;
            var closestX = 0;
            var closestY = 0;

            await Task.Run(async () =>
            {
                while(true)
                {
                    var splitMsg = mainViewModel.posMsg.Split('^')[0];

                    if (splitMsg == "4")
                    {
                        if (mainViewModel != null && mainViewModel.stream != null)
                        {
                            closestX = Convert.ToInt32(mainViewModel.posMsg.Split('^')[1]);
                            closestY = Convert.ToInt32(mainViewModel.posMsg.Split('^')[2]);

                            int xcount = 0;
                            int ycount = 0;

                            for (int i = 0; i < 19; i++)
                            {
                                if (closestX == xpos[i])
                                    break;
                                else
                                    xcount++;
                            }
                            
                            for (int i = 0; i < 19; i++)
                            {
                                if (closestY == ypos[i])
                                    break;
                                else
                                    ycount++;
                            }
                            try
                            {
                                if (STONE[xcount, ycount] == 0)
                                {

                                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                                    {
                                        // 스톤 그리기
                                        Ellipse stone2 = new Ellipse();
                                        stone2.Width = stoneSize;
                                        stone2.Height = stoneSize;
                                        if (color)
                                        {
                                            color = false;
                                            stone2.Fill = Brushes.White;

                                            // 돌 위치 저장
                                            STONE[xcount, ycount] = 1;
                                        }
                                        else
                                        {
                                            color = true;
                                            stone2.Fill = Brushes.Black;

                                            // 돌 위치 저장
                                            STONE[xcount, ycount] = 2;
                                        }

                                        // 돌 그릴 위치
                                        stone2.Margin = new Thickness(closestX, closestY, 0, 0);

                                        // 돌 집어넣기
                                        canvas1.Children.Add(stone2);

                                        turn++;
                                        if (turn % 2 == 0)
                                            start = "black";
                                        else
                                            start = "white";

                                        //MessageBox.Show(turn.ToString());

                                        // 흰색일 때, 검정색 턴임
                                        // start는 true고, color는 false
                                        // 검정색이 뒀음
                                        // start는 false, color는 true가 되어야함


                                        GameOver(xcount, ycount);
                                    }));
                                }
                                else
                                {
                                    //MessageBox.Show("ㄴㄴ");
                                    mainViewModel.posMsg = "";
                                }

                            }
                            catch
                            {
                                MessageBox.Show("게임 종료");
                                break;
                            }
                        }
                    }
                    else if (splitMsg == "5")
                    {
                        string logMsg = mainViewModel.posMsg.Split('^')[1];

                        //MessageBox.Show(logMsg);

                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            chatLog.Add(logMsg);

                            // 스크롤 아래로 내리기
                            if (VisualTreeHelper.GetChildrenCount(log) > 0)
                            {
                                Border border = (Border)VisualTreeHelper.GetChild(log, 0);
                                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                                scrollViewer.ScrollToBottom();
                            }
                        }));

                        mainViewModel.posMsg = "";
                    }
                }
            });
        }

        private async void canvas1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainViewModel mainViewModel = (MainViewModel)this.DataContext;

            if (myColor == start)
            {
                // 클릭 위치 구하기
                Point ClickPos = e.GetPosition((IInputElement)sender);

                int ClickX = (int)ClickPos.X;
                int ClickY = (int)ClickPos.Y;

                // 현 위치에 돌이 있느냐 없느냐 체크

                // 현 위치에 돌이 겹치느냐 안겹치느냐
                // 반지름 정도로 계산해서?
                // 현 위치에서 공백 빼고, 

                // 가운데 잡으려고
                int mx = ClickX - marginX / 2;
                int my = ClickY - marginY / 2;

                // 좌표에서 클릭한 좌표 뺀 최소값 찾고
                var minX = xpos.Min(x => Math.Abs(x - mx));
                var minY = ypos.Min(x => Math.Abs(x - my));

                // 근삿값 찾기 (지정된 조건에 맞는 첫 번째 요소 반환)
                var closestX = xpos.First(y => Math.Abs(y - mx) == minX);
                var closestY = ypos.First(y => Math.Abs(y - my) == minY);

                //MessageBox.Show("내가 찍은 좌표: " + closestX.ToString() + "^" + closestY.ToString());
                
                if (mainViewModel != null && mainViewModel.stream != null)
                {
                    Byte[] pos = Encoding.UTF8.GetBytes("4^" + mainViewModel.Nickname + "^" + closestX + "^" + closestY + "^");
                    mainViewModel.stream.Write(pos, 0, pos.Length);

                    //MessageBox.Show("posMsg: " + mainViewModel.posMsg);
                    var splitMsg = mainViewModel.posMsg.Split('^')[0];
                    //MessageBox.Show("spliMsg: " + splitMsg.ToString());

                    if (splitMsg == "4")
                    {
                        var closestXX = Convert.ToInt32(mainViewModel.posMsg.Split('^')[1]);
                        var closestYY = Convert.ToInt32(mainViewModel.posMsg.Split('^')[2]);

                        int xcount = 0;
                        int ycount = 0;

                        for (int i = 0; i < 19; i++)
                        {
                            if (closestXX == xpos[i])
                                break;
                            else
                                xcount++;
                        }

                        for (int i = 0; i < 19; i++)
                        {
                            if (closestYY == ypos[i])
                                break;
                            else
                                ycount++;
                        }

                        if (STONE[xcount, ycount] == 0)
                        {
                            // 스톤 그리기
                            Ellipse stone = new Ellipse();
                            stone.Width = stoneSize;
                            stone.Height = stoneSize;
                            if (color)
                            {
                                color = false;
                                stone.Fill = Brushes.White;

                                // 돌 위치 저장
                                STONE[xcount, ycount] = 1;
                            }
                            else
                            {
                                color = true;
                                stone.Fill = Brushes.Black;

                                // 돌 위치 저장
                                STONE[xcount, ycount] = 2;
                            }

                            // 돌 그릴 위치
                            stone.Margin = new Thickness(closestXX, closestYY, 0, 0);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                // 돌 집어넣기
                                canvas1.Children.Add(stone);
                            });

                            turn++;
                            if (turn % 2 == 0)
                                start = "black";
                            else
                                start = "white";

                            GameOver(xcount, ycount);
                        }
                        else
                        {
                            //MessageBox.Show("이미있찌롱 꺼지세용!");
                        }

                    }
                }
            }
            else
            {
                //MessageBox.Show("님 차례 아님;");
            }
        }


        private void GameOver(int xcount, int ycount)
        {
            int cnt = 1;

            // 오른쪽 방향
            for (int i = xcount + 1; i < 17; i++)
            {
                if (STONE[i, ycount] == STONE[xcount, ycount])
                    cnt++;
                else
                    break;
            }
            // 왼쪽 방향
            for (int i = xcount - 1; i >= 0; i--)
            {
                if (STONE[i, ycount] == STONE[xcount, ycount])
                    cnt++;
                else
                    break;
            }
            if (cnt >= 5)
            {
                GameRE();
                return;
            }

            cnt = 1;
            for (int i = ycount + 1; i < 17; i++)
            {
                if (STONE[xcount, i] == STONE[xcount, ycount])
                    cnt++;
                else
                    break;
            }
            // 위 방향
            for (int i = ycount - 1; i >= 0; i--)
            {
                if (STONE[xcount, i] == STONE[xcount, ycount])
                    cnt++;
                else
                    break;
            }
            if (cnt >= 5)
            {
                GameRE();
                return;
            }

            cnt = 1;
            // 대각 오른쪽 위
            for (int i = xcount + 1, j = ycount - 1; i < 17 && j >= 0; i++, j--)
            {
                if (STONE[i, j] == STONE[xcount, ycount])
                    cnt++;
                else
                    break;
            }
            // 대각 왼쪽 아래
            for (int i = xcount - 1, j = ycount + 1; i >= 0 && j < 17; i--, j++)
            {
                if (STONE[i, j] == STONE[xcount, ycount])
                    cnt++;
                else
                    break;
            }
            if (cnt >= 5)
            {
                GameRE();
                return;
            }

            cnt = 1;
            // 대각 왼쪽 위
            for (int i = xcount - 1, j = ycount - 1; i >= 0 && j >= 0; i--, j--)
            {
                if (STONE[i, j] == STONE[xcount, ycount])
                    cnt++;
                else
                    break;
            }
            // 대각 오른쪽 아래
            for (int i = xcount + 1, j = ycount + 1; i < 17 && j < 17; i++, j++)
            {
                if (STONE[i, j] == STONE[xcount, ycount])
                    cnt++;
                else
                    break;
            }
            if (cnt >= 5)
            {
                GameRE();
                return;
            }
        }

        private void GameRE()
        {
            if (!color)
            {
                MessageBox.Show("white 승");
            }
            else
            {
                MessageBox.Show("black 승");
            }
            color = false;
            canvas1.Children.Clear();
            DrawBoard();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mainViewModel = (MainViewModel)this.DataContext;

            string chatMsg = chat.Text;

            Byte[] cMsg = Encoding.UTF8.GetBytes("5^" + chatMsg + "^");
            mainViewModel.stream.Write(cMsg, 0, cMsg.Length);

            chat.Clear();
        }
    }
}
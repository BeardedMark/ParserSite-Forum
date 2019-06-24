using AngleSharp;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PoEParser
{
    public partial class MainWindow : Window
    {
        static IConfiguration config = Configuration.Default.WithDefaultLoader();
        IBrowsingContext context = BrowsingContext.New(config);

        //Файловые переменные
        string file_url = "urls.txt";
        string file_settings = "settings.txt";
        string[] settings;

        //Лист хранения классов от ссылок
        List<SiteBlock> listclass = new List<SiteBlock>();
        public DispatcherTimer dispatcherTimer = new DispatcherTimer();

        //Инициализация
        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(file_settings))
            {
                settings = File.ReadAllLines(file_settings, Encoding.UTF8);
            }
            else
            {
                settings = new string[] { "30", "https://www.pathofexile.com", "0", "1" };
            }

            //Settings set = new Settings();
            //set.Show();
            //Close();

            dispatcherTimer.Tick += new EventHandler(Run);
            dispatcherTimer.Interval = new TimeSpan(0, 0, Convert.ToInt32(settings[0]));
            dispatcherTimer.Start();

            listclass.Clear();
            CreateClass();
            mainstack.Children.Clear();
            MainNews();
            OnDisplay();
            Resize();
        }

        //Создание класса сайта по ссылкам и чтение ссылок
        void CreateClass()
        {
            string[] readurl = File.ReadAllLines(file_url, Encoding.UTF8);
            foreach (string str in readurl)
            {
                listclass.Add(new SiteBlock(str));
            }
        }

        //Заполнение классов данными парсинга
        void Filling()
        {
            foreach(SiteBlock block in listclass)
            {
                //Узнаем сайт
                string[] urlsplit = block.link.Split('/');
                foreach (string str in urlsplit)
                {
                    if (str.Contains("pathofexile"))
                    {
                        block.mainsite = str;
                        break;
                    }
                }

                // Определили первую страницу темы
                char[] number = { '/', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                char[] page = { 'p', 'a', 'g', 'e' };
                if (block.link.Contains("/page/"))
                {
                    block.link = (block.link.TrimEnd(number)).TrimEnd(page);
                }

                //Подключение AngleSharp 
                var docurl = BrowsingContext.New(config).OpenAsync(block.link).GetAwaiter().GetResult();

                //Узнали время публикации темы
                block.title = docurl.QuerySelector("h1.topBar.last.layoutBoxTitle").TextContent;
                block.autor = docurl.QuerySelector("span.profile-link.post_by_account").TextContent;
                block.postdate = docurl.QuerySelector("span.post_date").TextContent;

                //Предыдущая страница
                var tree = docurl.QuerySelector("div.breadcrumb");
                var branch = tree.QuerySelectorAll("a");
                block.backpage = "https://" + block.mainsite + branch[branch.Length - 2].GetAttribute("href");

                //Создаем новый фаил для парса на основе backpage
                var docbackpage = BrowsingContext.New(config).OpenAsync(block.backpage).GetAwaiter().GetResult();

                //Сохраняем старые значения просмотров и коментариев
                block.pastviews = block.views;
                block.pastcomments = block.comments;

                //Определяем просмотры и комментарии
                var themes = docbackpage.QuerySelectorAll("tr");
                foreach (var theme in themes)
                {
                    if (theme.TextContent.Contains(block.postdate))
                    {
                        block.views = Convert.ToInt32(theme.QuerySelector("div.post-stat span").TextContent);
                        block.comments = Convert.ToInt32(theme.QuerySelector("td.views div span").TextContent);
                    }
                }

                //Определяем разницу просмотров и коментариев
                block.diffviews = block.views - block.pastviews;
                block.diffcomments = block.comments - block.pastcomments;
            }
        }

        //Определение последней новости
        void MainNews()
        {
            //var newspage = BrowsingContext.New(config).OpenAsync("https://" + site + "/news/").GetAwaiter().GetResult();
            StackPanel st = new StackPanel();
            var task = context.OpenAsync(settings[1] + "/news/").GetAwaiter().GetResult();
            string lastnews = task.QuerySelector("h2.title a").TextContent;

            st.Children.Add(new NormalTextLine(lastnews));
            mainstack.Children.Add(st);
        }

        //Визуализация блоков (классов)
        void OnDisplay()
        {
            Filling();

            foreach (SiteBlock block in listclass)
            {
                StackPanel st = new StackPanel() { /*Margin = new Thickness(0, 0, 0, 5)*/ };
                st.Children.Add(new Rectangle() { Fill = Brushes.Black, Margin = new Thickness (3), Height = 1 });
                st.Children.Add(new NormalTextLine(block.title));
                st.Children.Add(new ImageTextLine(block.autor, "user.png"));
                st.Children.Add(new ImageTextLine(block.postdate, "time.png"));
                st.Children.Add(new DeffTextLine(block.views, block.diffviews, "view.png"));
                st.Children.Add(new DeffTextLine(block.comments, block.diffcomments, "comment.png"));

                mainstack.Children.Add(st);

            }
        }

        //Подгонка размера
        void Resize()
        {
            Height = 18;
            foreach (StackPanel element in mainstack.Children)
            {
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                element.Arrange(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                Height = Height + element.ActualHeight;
            }
            Height = Height + 3;
        }

        //Обновление по таймеру
        void Run(object sender, EventArgs e)
        {
            settings = File.ReadAllLines(file_settings, Encoding.UTF8);
            dispatcherTimer.Interval = new TimeSpan(0, 0, Convert.ToInt32(settings[0]));

            mainstack.Children.Clear();
            MainNews();
            OnDisplay();
            Resize();
        }



        //Обновить принудительно
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            listclass.Clear();
            CreateClass();
            mainstack.Children.Clear();
            MainNews();
            OnDisplay();
        }

        //Закрыть
        private void Image_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        //Перемещение
        private void Image_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        //открыть окно настроек
        private void Image_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            Settings set = new Settings();
            set.Show();
        }
    }
}





//Класс для разниц
class DeffTextLine : StackPanel
{
    public DeffTextLine( int views, int deffviews, string img)
    {
        //Background = Brushes.Gray;
        Margin = new Thickness(3, 0, 3, 0);
        Orientation = Orientation.Horizontal;

        Children.Add(new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/" + img)), Margin = new Thickness(0, 0, 3, 0) });
        Children.Add(new TextBlock() { Text = views.ToString(), Foreground = Brushes.DimGray, FontSize = 10 });
        Children.Add(new TextBlock() { Text = " +" + deffviews.ToString(), Foreground = Brushes.Green, FontSize = 10 });
    }
}

//Класс для текстового поля с картинкой
class ImageTextLine : StackPanel
{
    public ImageTextLine( string str, string img)
    {
        //Background = Brushes.Gray;
        Margin = new Thickness(3, 0, 3, 0);
        Orientation = Orientation.Horizontal;

        Children.Add(new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/img/" + img)), Margin = new Thickness(0, 0, 3, 0) });
        Children.Add(new TextBlock() { Text = str, Foreground = Brushes.DimGray, FontSize = 10, TextTrimming = TextTrimming.CharacterEllipsis });
    }
}

//Класс для обычного текстового поля
class NormalTextLine : TextBlock
{
    public NormalTextLine(string str)
    {
        Margin = new Thickness(3, 0, 3, 0);
        TextTrimming = TextTrimming.CharacterEllipsis;
        FontSize = 10;
        Foreground = Brushes.DimGray;
        Text = str;
    }
}

//Класс содания блока парсинга сайта
class SiteBlock
{
    //Глобавльные переменные
    public string link;
    public string backpage;
    public string mainsite;

    public string title;
    public string autor;
    public string postdate;

    public int views = 0;
    public int pastviews;
    public int diffviews;

    public int comments = 0;
    public int pastcomments;
    public int diffcomments;

    public SiteBlock(string l)
    {
        link = l;
    }
}

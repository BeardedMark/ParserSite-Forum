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
using System.Windows.Shapes;
using System.Xml;

namespace PoEParser
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        string urlfile = "urls.txt";
        string settingslfile = "settings.txt";

        List<string> str = new List<string>();

        //Инициализация
        public Settings()
        {
            InitializeComponent();

            CB_interval.Items.Add(new Element("10 sec", "10"));
            CB_interval.Items.Add(new Element("30 sec", "30"));
            CB_interval.Items.Add(new Element("1 min", "60"));
            CB_interval.Items.Add(new Element("10 min", "600"));
            CB_interval.Items.Add(new Element("30 min", "1800"));
            CB_interval.Items.Add(new Element("1 hour", "3600"));

            CB_language.Items.Add(new Element("English", "https://www.pathofexile.com"));
            CB_language.Items.Add(new Element("Brazil", "https://br.pathofexile.com"));
            CB_language.Items.Add(new Element("Русский", "https://ru.pathofexile.com"));
            CB_language.Items.Add(new Element("Thailand", "https://th.pathofexile.com"));
            CB_language.Items.Add(new Element("Germany", "https://de.pathofexile.com"));
            CB_language.Items.Add(new Element("France", "https://fr.pathofexile.com"));
            CB_language.Items.Add(new Element("Spain", "https://es.pathofexile.com"));

            //Загрузка ссылок
            if (File.Exists(urlfile))
            {
                string[] urlsfile = File.ReadAllLines(urlfile);
                foreach (string url in urlsfile)
                {
                    listurl.Items.Add(url);
                }
            }

            if (File.Exists(settingslfile))
            {
                string[] settinglfile = File.ReadAllLines(settingslfile);

                CB_language.SelectedIndex = Convert.ToInt32(settinglfile[2]);
                CB_interval.SelectedIndex = Convert.ToInt32(settinglfile[3]);

                Title.IsChecked = Convert.ToBoolean(settinglfile[4]);
                Title.IsChecked = Convert.ToBoolean(settinglfile[5]);
                Title.IsChecked = Convert.ToBoolean(settinglfile[6]);
                Title.IsChecked = Convert.ToBoolean(settinglfile[7]);
            }


        }

        //Добавление в листбокс ссылки
        private void texturl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (texturl.Text.Contains("/view-thread/"))
                {
                    var config = Configuration.Default.WithDefaultLoader();
                    var docurl = BrowsingContext.New(config).OpenAsync(texturl.Text).GetAwaiter().GetResult();
                    if (docurl.QuerySelector("Title").TextContent.Contains("Форум - Новости -"))
                    {
                        texturl.Text = "";
                        MessageBox.Show("Новостные страницы не допускаются");
                    }
                    else
                    {
                        listurl.Items.Add(texturl.Text);
                        texturl.Text = "";
                    }
                }
                else
                {
                    texturl.Text = "";
                    MessageBox.Show("Допускаются только ссылки на темы форума");
                }
            }
        }

        // Удаление из листа
        private void listurl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                listurl.Items.Remove(listurl.SelectedItem);
            }
        }

        // Сохранение
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<string> urls = new List<string>();
            foreach (string str in listurl.Items)
            {
                urls.Add(str);
            }
            File.Delete(urlfile);
            File.AppendAllLines(urlfile, urls);

            Element saveinterval = CB_interval.SelectedItem as Element;
            Element savelanguage = CB_language.SelectedItem as Element;

            str.Add(saveinterval.more);
            str.Add(savelanguage.more);

            str.Add(CB_language.SelectedIndex.ToString());
            str.Add(CB_interval.SelectedIndex.ToString());

            str.Add(Title.IsChecked.ToString());
            str.Add(Autor.IsChecked.ToString());
            str.Add(Views.IsChecked.ToString());
            str.Add(Comments.IsChecked.ToString());

            File.Delete(settingslfile);
            File.WriteAllLines(settingslfile, str);
            Close();
        }
    }
}

public class Element
{
    public string more;
    public string maintext;
    public Element(string text, string info)
    {
        maintext = text;
        more = info;
    }
    public override string ToString()
    {
        return maintext;
    }
}

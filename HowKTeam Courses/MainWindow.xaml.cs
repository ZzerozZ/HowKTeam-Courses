using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HowKTeam_Courses
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window/*, INotifyPropertyChanged*/
    {
        ObservableCollection<MenuTreeItems> TreeItems;/*Item của Treeview chính*/

        private string homePage = "https://howkteam.com/";/*Địa chỉ trang chủ*/
        HttpClient httpClient;/*HttpClient để get code html*/

        //Handle và cookie:
        HttpClientHandler handle;
        CookieContainer cookie = new CookieContainer();

        public MainWindow()
        {
            InitializeComponent();//Khởi tạo MainForm
            this.DataContext = this;
            InitHttpClient();//Khởi tạo HttpClient

            TreeItems = new ObservableCollection<MenuTreeItems>();

            trvMain.ItemsSource = TreeItems;//Update ItemsSource
        }
          

        /// <summary>
        /// Create HttpClient
        /// </summary>
        private void InitHttpClient()
        {
            //Set handle:
            handle = new HttpClientHandler()
            {
                CookieContainer = cookie,
                ClientCertificateOptions = ClientCertificateOption.Automatic,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                AllowAutoRedirect = true,
                UseDefaultCredentials = false
            };

            httpClient = new HttpClient(handle);
            //Set base address(home page):
            httpClient.BaseAddress = new Uri("https://howkteam.com/");
        }

        /// <summary>
        /// Crawl data từ địa chỉ tham số
        /// </summary>
        /// <param name="url"> Địa chỉ crawl</param>
        /// <returns></returns>
        public string CrawlDataFromUrl(string url)
        {
            string html = "";
            //Get toàn bộ code html từ url:
            html = WebUtility.HtmlDecode(httpClient.GetStringAsync(url).Result);///WebUtility.HtmlDecode() <-- Decode để tránh lỗi font

            return html;
        }


        /// <summary>
        /// Get data các khóa học
        /// </summary>
        /// <param name="url">Thẻ chính(VD: Home, Learn,...)</param>
        public void Crawl(string url)
        {
            string html = CrawlDataFromUrl(url);/*Biến lưu html code*/
            //Get html code của các khóa học:
            var courses = Regex.Matches(html, @"<div class=""info-course(.*?)</div>", RegexOptions.Singleline);
            //Get thông tin về mỗi khóa:
            foreach (var course in courses)
            {
                string courseName = Regex.Match(course.ToString(), @"(?=<h2>).*?(?=</h2>)", RegexOptions.Singleline).Value.Replace("<h2>", "");/*Tên khóa học*/
                string link = Regex.Match(course.ToString(), @"'(.*?)'", RegexOptions.Singleline).Value.Replace("'", "");/*Link khóa học*/

                //Thêm khóa học vào danh sách:
                MenuTreeItems item = new MenuTreeItems();
                item.Name = courseName;
                item.Url = link;
                AddItemToTreeViewItem(TreeItems, item);


                string htmlCourse = CrawlDataFromUrl(link);/*Biến lưu html code về danh sách bài học*/
                //Get thông tin về các bài học:
                string sidebar = Regex.Match(htmlCourse, @"<div class=""list-group-level1 collapse in""(.*?)</div>", RegexOptions.Singleline).Value;/*Link bài học*/
                //Get html code của các bài học:
                var lesson = Regex.Matches(sidebar, @"<a href(.*?)</a>", RegexOptions.Singleline);

                //Get thông tin về các bài học mỗi khóa:
                foreach (var page in lesson)
                {
                    string lessonName = Regex.Match(page.ToString(), @"#sub-menu"">(.*?)</a>", RegexOptions.Singleline).Value.Replace("#sub-menu\"> ", "").Replace("</a>", "");/*Tên bài học*/
                    string linkLesson = Regex.Match(page.ToString(), @"<a href=""(.*?)""", RegexOptions.Singleline).Value.Replace("<a href=\"", "").Replace("\"", "");/*Link bài học*/

                    //Thêm bài học vào danh sách bài học của khóa:
                    MenuTreeItems sub = new MenuTreeItems();
                    sub.Name = lessonName;
                    sub.Url = linkLesson;
                    AddItemToTreeViewItem(item.Items, sub);
                }
            }
        }


        private void btnLoadData_Click(object sender, RoutedEventArgs e)
        {
            
            //Chia Task cho việc Crawl data:
            Task t = new Task(()=> { Crawl("Learn");});
            t.Start();
        }


        /// <summary>
        /// Thêm phần tử vào danh sách cây
        /// </summary>
        /// <param name="root">Cây hiện tại</param>
        /// <param name="node">Node thêm vào</param>
        void AddItemToTreeViewItem(ObservableCollection<MenuTreeItems> root, MenuTreeItems node)
        {
            App.Current.Dispatcher.Invoke((Action)delegate //Invoke tránh lỗi "This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread"
            {
                root.Add(node);//Thêm node vào root
            });

        }
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            webMain.Navigate(homePage + (sender as Button).Tag.ToString());
            webMain.Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Chia Task cho việc Crawl data:
            Task t = new Task(() => { Crawl("Learn"); });
            t.Start();
        }


        #region Diable Script Warning(Copy from Internet)
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        #endregion

        private void webMain_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SetSilent(webMain, true);
        }
    }
}

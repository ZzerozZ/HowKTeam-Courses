using System.Collections.ObjectModel;

namespace HowKTeam_Courses
{
    /// <summary>
    /// Phần tử của TreeView
    /// </summary>
    public class MenuTreeItems
    {
        private string name;
        private string url;

        public string Name { get => name; set => name = value; }
        public string Url { get => url; set => url = value; }
        public ObservableCollection<MenuTreeItems> Items { get; set; }

        public MenuTreeItems()
        {
            this.Items = new ObservableCollection<MenuTreeItems>();
        }

    }
}

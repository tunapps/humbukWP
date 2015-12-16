using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.ServiceModel;

using System.Xml;
using Windows.Web.Syndication;
using Windows.Web.Http;


// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace humbuk.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem
    {
        public SampleDataItem(String uniqueId, String title, String link, String imagePath, String description, String pubdate)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Link = link;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Pubdate = pubdate;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Link { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Pubdate { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup
    {
        public SampleDataGroup(String uniqueId, String title, String link, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Link = link;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Link { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();

        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<SampleDataGroup>> GetGroupsAsync()
        {
            
            await _sampleDataSource.GetClankyDataAsync();
            await _sampleDataSource.GetKomentareDataAsync();
            await _sampleDataSource.GetKecalroomDataAsync();

            //await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups;
        }

        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)
        {
            
            await _sampleDataSource.GetClankyDataAsync();
            await _sampleDataSource.GetKomentareDataAsync();
            await _sampleDataSource.GetKecalroomDataAsync();

            //await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetClankyDataAsync();
            await _sampleDataSource.GetKomentareDataAsync();
            await _sampleDataSource.GetKecalroomDataAsync();

            //await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private String upravText(String s)
        {
            string strWithTabs = s;

            string line = strWithTabs.Replace("\t", "").Replace("\n","");
            //while (line.IndexOf("  ") > 0)
            //{
            //    line = line.Replace("  ", " ");
            //}
            return line;
        }

        private async Task GetKomentareDataAsync()
        {
            if (this._groups.Count > 2)
                return;
            // this example pulls coca-cola's posts
            var _Uri = new Uri("http://kareljeproste.borec.cz/rsskomentare.php");

            // including user agent, otherwise FB rejects the request
            var _Client = new HttpClient();
            _Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            _Client.DefaultRequestHeaders.Add("Accept-Charset", "ISO-8859-2,utf-8;q=0.7,*;q=0.7");
            // fetch as string to avoid error
            var _Response = await _Client.GetAsync(_Uri);
            var _String = await _Response.Content.ReadAsStringAsync();

            // convert to xml (will validate, too)
            var _XmlDocument = new Windows.Data.Xml.Dom.XmlDocument();
            _XmlDocument.LoadXml(_String);

            // manually fill feed from xml
            var _Feed = new Windows.Web.Syndication.SyndicationFeed();
            _Feed.LoadFromXml(_XmlDocument);
            
            // continue as usual...
            SampleDataGroup group = new SampleDataGroup("Group-2",
                                                            "Komentáře",
                                                            "",
                                                            "http://kareljeproste.borec.cz/images/slunce.gif",
                                                            "");
            int i = 0;
            foreach (var item in _Feed.Items)
            {
                i++;
                SampleDataItem content = new SampleDataItem("Group-2-Item-" + i,
                                                            item.Title.Text,
                                                            item.Links[0].NodeValue,
                                                            "http://kareljeproste.borec.cz/images/slunce.gif", 
                                                            upravText(item.Summary.Text),
                                                            item.PublishedDate.DateTime.ToLocalTime().ToString()
                                                            );
                // do something
                group.Items.Add(content);
            }
            this.Groups.Add(group);
            
        }

        private async Task GetClankyDataAsync()
        {
            if (this._groups.Count > 2)
                return;
            // this example pulls coca-cola's posts
            var _Uri = new Uri("http://kareljeproste.borec.cz/rssclanky.php");

            // including user agent, otherwise FB rejects the request
            var _Client = new HttpClient();
            _Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

            // fetch as string to avoid error
            var _Response = await _Client.GetAsync(_Uri);
            var _String = await _Response.Content.ReadAsStringAsync();

            // convert to xml (will validate, too)
            var _XmlDocument = new Windows.Data.Xml.Dom.XmlDocument();
            _XmlDocument.LoadXml(_String);

            // manually fill feed from xml
            var _Feed = new Windows.Web.Syndication.SyndicationFeed();
            _Feed.LoadFromXml(_XmlDocument);

            // continue as usual...
            SampleDataGroup group = new SampleDataGroup("Group-1",
                                                            "Články",
                                                            "články",
                                                            "http://kareljeproste.borec.cz/images/slunce.gif",
                                                            "");
            int i = 0;
            foreach (var item in _Feed.Items)
            {
                i++;
                SampleDataItem content = new SampleDataItem("Group-1-Item-" + i,
                                                            item.Title.Text,
                                                            item.Links[0].NodeValue,
                                                            "http://kareljeproste.borec.cz/images/slunce.gif",
                                                            upravText(item.Summary.Text),
                                                            item.PublishedDate.DateTime.ToLocalTime().ToString()
                                                            );
                // do something
                group.Items.Add(content);
            }
            this.Groups.Add(group);

        }

        private async Task GetKecalroomDataAsync()
        {
            if (this._groups.Count > 2)
                return;
            // this example pulls coca-cola's posts
            var _Uri = new Uri("http://kareljeproste.borec.cz/rsskecalroom.php");

            // including user agent, otherwise FB rejects the request
            var _Client = new HttpClient();
            _Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

            // fetch as string to avoid error
            var _Response = await _Client.GetAsync(_Uri);
            var _String = await _Response.Content.ReadAsStringAsync();

            // convert to xml (will validate, too)
            var _XmlDocument = new Windows.Data.Xml.Dom.XmlDocument();
            _XmlDocument.LoadXml(_String);

            // manually fill feed from xml
            var _Feed = new Windows.Web.Syndication.SyndicationFeed();
            _Feed.LoadFromXml(_XmlDocument);

            // continue as usual...
            SampleDataGroup group = new SampleDataGroup("Group-3",
                                                            "Kecalroom",
                                                            "kecalroom",
                                                            "http://kareljeproste.borec.cz/images/slunce.gif",
                                                            "");
            int i=0;
            foreach (var item in _Feed.Items)
            {
                i++;
                SampleDataItem content = new SampleDataItem("Group-3-Item-"+i,
                                                            item.Title.Text,
                                                            item.Links[0].NodeValue,
                                                            "http://kareljeproste.borec.cz/images/slunce.gif",
                                                            upravText(item.Summary.Text),
                                                            item.PublishedDate.DateTime.ToLocalTime().ToString()
                                                            );
                // do something
                group.Items.Add(content);
            }
            this.Groups.Add(group);

        }


        private async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0)
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/SampleData.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["Subtitle"].GetString(),
                                                            groupObject["ImagePath"].GetString(),
                                                            groupObject["Description"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.Items.Add(new SampleDataItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["Title"].GetString(),
                                                       itemObject["Subtitle"].GetString(),
                                                       itemObject["ImagePath"].GetString(),
                                                       itemObject["Description"].GetString(),
                                                       itemObject["Content"].GetString()));
                }
                this.Groups.Add(group);
            }
        }
    }
}
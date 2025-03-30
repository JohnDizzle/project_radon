﻿using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Project_Radon.Contracts.Services;
using Newtonsoft.Json;
using Project_Radon.Models;
using Project_Radon.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;



namespace Project_Radon.Services
{

    interface IAppSettings
    {

        Uri HomeUrl { get; set; }
        bool SideKick { get; set; }
        int DefaultSearchProvider { get; set; }
        bool IsLocationOnOff { get; set; }
        string UserAgentWebCore { get; set; }
        ElementTheme ThemeDefault { get; set; }
        Visibility VisibilitySideToolBar { get; set; }
        string HomeUrlString { get; set; }
        bool TitleBarPinned { get; set; }

    }

    public partial class RadonAppSettings : ObservableObject, IAppSettings 
    {
        private object settings;

        public RadonAppSettings()
        {
        }

        public RadonAppSettings(object settings)
        {
            this.settings = settings;
        }

        public Uri HomeUrl { get; set; }
        public bool SideKick { get; set; }
        public int DefaultSearchProvider { get; set; }
        public bool IsLocationOnOff { get; set; }
        public string UserAgentWebCore { get; set; }
        public ElementTheme ThemeDefault { get; set; }
        public Visibility VisibilitySideToolBar { get; set; }
        public bool TitleBarPinned { get; set; }
        public string HomeUrlString { get; set; }
        
        public string AppColorTheme { get; set;  }

        public bool InlineMode { get; set;  }

        public bool SystemTitleBar { get; set; }

        public string Username { get; set; }

        public string QRCodeUrl { get; set; }
        public string ProfilePicture { get; set; }  
        public string OpenUrl { get; set; }
        public int AppThemeSetting { get; set; }
    }

    class SettingsService : ISettingsService
    {
        private static readonly SemaphoreSlim __Locker_acids_events = new SemaphoreSlim(1, 1);

        public static SettingsService Instance { get; set; }
        public RadonAppSettings AppSettings { get; set; }
        public event EventHandler<NotifyCollectionChangedEventArgs> FavoritesCollectionChanged;
        public event EventHandler<NotifyCollectionChangedEventArgs> HistoryCollectionChanged;
        internal string CorePathFavorites { get; set; } = Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER") + @"Favorites\";
        internal string CorePathSettings { get; set; } = Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER") + @"Settings\";
        internal string CorePathHistory { get; set; } = Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER") + @"History\";

        public SettingsService()
        {
            InitializeAsync();
            
            var path = Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER");

            if (File.Exists(Path.Combine(path, "Settings", "settings.json")))
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(path, "settings", "settings.json"));
                if (fileInfo.Length <= 0)
                {
                    AppSettings = new() { DefaultSearchProvider = 0, HomeUrl = new Uri("https://google.com"), IsLocationOnOff = false, SideKick = false };
                    File.WriteAllText(Path.Combine(path, "settings", "settings.json"), JsonConvert.SerializeObject(AppSettings));
                }
            }
            else
            {

                using (var fs = File.Create(Path.Combine(path, "Settings", "settings.json"))) { } ;

            }
            // load from file. 
            
            Instance = this;
            FavoritesStore.CollectionChanged += FavoritesStore_CollectionChanged;
            HistoryStore.CollectionChanged += HistoryStore_CollectionChanged;

        }

        public async void InitializeAsync()
        {
            var settings = await ReadSettings<RadonAppSettings>();
            if (settings is not null)
            {
                AppSettings = settings;
            }
            else {
                AppSettings = new RadonAppSettings();
            }
             
            historyModels =  await ReadHistory<ObservableCollection<HistoryModel>>();
        }

        private void HistoryStore_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            lock (historyModels) {
                WriteHistory(historyModels.Distinct());
                // this is bubbling up to historyviewmodel and mainpageviewmodel for myhistory collection. need to add to other pages sortly 
                HistoryCollectionChanged?.Invoke(this, e);
            }

        }

        private void FavoritesStore_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
            __Locker_acids_events.WaitAsync();

            FavoritesCollectionChanged?.Invoke(this, e);

            __Locker_acids_events.Release();
        }

        public void AddToHistory(string key, string value)
        {
            //   throw new NotImplementedException();
        }

        public void RemoveFromHistory(string key, string value)
        {
            //   throw new NotImplementedException();
        }

        public void AddToHistory(HistoryModel historyItem)
        {
            lock (historyModels)
            {
                historyModels.Add(historyItem);
                WriteHistory(historyModels.Distinct());
                HistoryCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, historyItem));
            }
        }

        public void RemoveFromHistory(HistoryModel historyItem)
        {
            lock (historyModels)
            {
                historyModels.Remove(historyItem);
                WriteHistory(historyModels.Distinct());
                HistoryCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, historyItem));
            }
        }
        public void Remove(string key)
        {
            throw new NotImplementedException();
        }


        public async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder; 
                var file = await storageFolder.GetFileAsync(fileName);
                return file != null;
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }
        }
        #region FileReadWriteAccessIO
        async Task<T> ReadFavorites<T>()
        {
            string json = "{}";
            string path = Path.Combine(Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER"), CorePathFavorites);
            string fileFavs = Path.Combine(path, "favorites.json");

            try
            {

                if (File.Exists(fileFavs))
                {
                    json = File.ReadAllText(fileFavs);
                    var settings = JsonConvert.DeserializeObject<T>(json);
                    return await Task.FromResult<T>(settings);

                }
                else
                {
                    var dir = Directory.CreateDirectory(path);
                    using (var fs = File.Create(fileFavs)) ;
                    return await Task.FromResult(JsonConvert.DeserializeObject<T>("[]"));

                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        async Task<T> ReadSettings<T>()
        {
            string json = "{}";
            string path = Path.Combine(Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER"), CorePathSettings);
            string fileFavs = Path.Combine(path, "settings.json");

            try
            {

                if (File.Exists(fileFavs))
                {
                    json = File.ReadAllText(fileFavs);
                    var settings = JsonConvert.DeserializeObject<T>(json);
                    return await Task.FromResult<T>(settings);

                }
                else
                {
                    var dir = Directory.CreateDirectory(path);
                    using (var fs = File.Create(fileFavs)) ;
                    return await Task.FromResult(JsonConvert.DeserializeObject<T>("{}"));

                }

            }
            catch (Exception)
            {

                throw;
            }



        }
        async Task<T> ReadHistory<T>()
        {
            string json = "[]";
            string path = Path.Combine(Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER"), CorePathHistory);
            string fileHistory = Path.Combine(path, "history.json");

            try
            {
                if (File.Exists(fileHistory))
                {
                    json = File.ReadAllText(fileHistory);
                    var history = JsonConvert.DeserializeObject<List<HistoryModel>>(json);

                    if (history is null)
                    {
                        return await Task.FromResult(JsonConvert.DeserializeObject<T>("[]"));
                    }   

                    // Load BitmapImage for each HistoryModel

                    foreach (var item in history.ToList())
                    {
                        var bitmap = new BitmapImage(new Uri($"https://www.google.com/s2/favicons?domain_url={item.TheUrl.AbsoluteUri}"));
                        item.TheContents = bitmap ?? new BitmapImage(new("ms-appx://Assets/StoreLogo.png"));

                    }
                    return await Task.FromResult(JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(history)));
                }
                else
                {
                    var dir = Directory.CreateDirectory(path);
                    using (var fs = File.Create(fileHistory)) ;
                    return await Task.FromResult(JsonConvert.DeserializeObject<T>("[]"));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        async void WriteHistory<T>(T history)
        {
            try
            {
                var file = CorePathHistory + "history.json";

                if (File.Exists(file))
                {
                    var json = JsonConvert.SerializeObject(history);
                    await File.WriteAllTextAsync(file, json);
                }
                else
                {
                    Directory.CreateDirectory(CorePathSettings);

                    if (Directory.Exists(CorePathSettings))
                    {
                        using (var fs = File.Create(file)) { }
                        var json = JsonConvert.SerializeObject(history);
                        await File.WriteAllTextAsync(file, json);
                    }
                }
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async void WriteFavorite<T>(T favorites)
        {
            try
            {
                var file = CorePathFavorites + "favorites.json";

                if (File.Exists(file))
                {
                    var json = JsonConvert.SerializeObject(favorites);
                    await File.WriteAllTextAsync(file, json);
                }
                else
                {
                    Directory.CreateDirectory(CorePathSettings);

                    if (Directory.Exists(CorePathSettings))
                    {
                        using (var fs = File.Create(file)) { };
                        var json = JsonConvert.SerializeObject(favorites);
                        await File.WriteAllTextAsync(file, json);
                    }
                }
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }
        async Task WriteSettings<T>(T settings)
        {


            try
            {
                var file = CorePathSettings + "settings.json";

                if (File.Exists(file))
                {
                    var json = JsonConvert.SerializeObject(settings);
                    await File.WriteAllTextAsync(file, json);
                }
                else
                {
                    Directory.CreateDirectory(CorePathSettings);

                    if (Directory.Exists(CorePathSettings))
                    {
                        using (var fs = File.Create(file)) { };
                        var json = JsonConvert.SerializeObject(settings);
                        await File.WriteAllTextAsync(file, json);
                    }
                }
                return;
            }
            catch (Exception)
            {
                throw;
            }


        }

        
        #endregion

        private ObservableCollection<HistoryModel> historyModels = new ObservableCollection<HistoryModel>();
        public ObservableCollection<HistoryModel> HistoryStore
        {
            get
            {
                return historyModels?.OrderByDescending(t=> t.DateTime).ToObservableCollection() ?? new();   

                //var result = ReadHistory<ObservableCollection<HistoryModel>>().GetAwaiter().GetResult();
                //return result ?? new ObservableCollection<HistoryModel>();
            }
            set
            {
                lock (historyModels)
                {
                    historyModels.Clear(); 
                    foreach(var item in value)
                    {
                        historyModels.Add(item); 
                    }
                    WriteHistory(historyModels.Distinct());
                    HistoryCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        public string OpenUrl
        {
            get
            {
                return AppSettings.OpenUrl;
            }
            set
            {
                AppSettings.OpenUrl = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }


        public string ProfilePicture
        {
            get
            {
                return AppSettings.ProfilePicture;
            }
            set
            {
                AppSettings.ProfilePicture = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }


        public string AppColorTheme
        {
            get
            {
                return AppSettings.AppColorTheme;
            }
            set
            {
                AppSettings.AppColorTheme = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }

        public bool InlineMode
        {
            get
            {
                return AppSettings.InlineMode;
            }
            set
            {
                AppSettings.InlineMode = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }

        public bool SystemTileBar
        {
            get
            {
                return AppSettings.SystemTitleBar;
            }
            set
            {
                AppSettings.SystemTitleBar = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }

        public string Username
        {
            get
            {
                return AppSettings.Username;
            }
            set
            {
                AppSettings.Username = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }

        public string QRCodeUrl
        {
            get
            {
                return AppSettings.QRCodeUrl;
            }
            set
            {
                AppSettings.QRCodeUrl = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }

        public string HomeUrlString
        {
            get
            {
                return AppSettings.HomeUrlString;

            }
            set
            {
                AppSettings.HomeUrlString = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }
        public Visibility VisibilitySideToolBar
        {

            get
            {
                return AppSettings.VisibilitySideToolBar;

            }
            set
            {
                AppSettings.VisibilitySideToolBar = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }

        }
        public ElementTheme ElementTheme
        {

            get
            {
                return AppSettings.ThemeDefault;

            }
            set
            {
                AppSettings.ThemeDefault = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }


        }

        public bool TitleBarPinned
        {
            get
            {
                return AppSettings.TitleBarPinned;
            }
            set
            {
                AppSettings.TitleBarPinned = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);

            }
        }
        public int DefaultSearchProvider
        {
            get
            {
                return AppSettings.DefaultSearchProvider;

            }
            set
            {
                AppSettings.DefaultSearchProvider = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }

        public bool SideKick
        {
            get
            {
                return AppSettings.SideKick;
            }
            set
            {
                AppSettings.SideKick = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }
        public string UserAgentWebCore
        {
            get
            {
                return AppSettings.UserAgentWebCore;
            }
            set
            {
                AppSettings.UserAgentWebCore = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }
        public bool IsLocationOnOff
        {
            get
            {
                return AppSettings.IsLocationOnOff;
            }
            set
            {
                AppSettings.IsLocationOnOff = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }
        public Uri HomeUrl
        {

            get
            {
                return AppSettings.HomeUrl;
            }
            set
            {
                AppSettings.HomeUrl = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }

        public int AppThemeSetting
        {

            get
            {
                return AppSettings.AppThemeSetting;
            }
            set
            {
                AppSettings.AppThemeSetting = value;
                WriteSettings<RadonAppSettings>(AppSettings).ConfigureAwait(false);
            }
        }




        public ObservableCollection<Bookmarks> FavoritesStore
        {
            get
            {
                var result = ReadFavorites<ObservableCollection<Bookmarks>>().GetAwaiter().GetResult();
                return result ?? new ObservableCollection<Bookmarks>();
            }
            set
            {
                var obj = new object();
                lock (obj)
                {
                    WriteFavorite(value);

                    //Write(nameof(FavoritesStore), value);
                    FavoritesCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }

            }
        }
    }
}

﻿using Microsoft.Extensions.DependencyInjection;
using Project_Radon.Contracts.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Yttrium_browser;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Project_Radon.Views
{
    public sealed partial class oobe3pfpdialog : ContentDialog
    {
        private readonly ISettingsService _settingsService;
        public oobe3pfpdialog()
        {
            _settingsService = App.Current.Services.GetService<ISettingsService>();
            this.InitializeComponent();
        }

        private void pfpPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pfpDisplayText.Text = (pfpPicker.SelectedItem as Image).Tag.ToString();
            _settingsService.AppSettings.ProfilePicture = (pfpPicker.SelectedItem as Image).Tag.ToString(); 
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}

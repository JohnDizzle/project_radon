﻿using Project_Radon.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Yttrium_browser;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Project_Radon.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class oobe5 : Page
    {
        public oobe5()
        {
            this.InitializeComponent();

            // Title bar code-behind
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            // Set XAML element as a drag region.
            Window.Current.SetTitleBar(titleBar);
            var ititleBar = ApplicationView.GetForCurrentView().TitleBar;
            ititleBar.ButtonBackgroundColor = Colors.Transparent;
            ititleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // load Settings if exists
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            String username = localSettings.Values["username"] as string;
            if ( username != null) { username_textbox.Text = username; username_Header.Text = username; nextButton.IsEnabled = true; }
            else { nextButton.IsEnabled = false; }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["username"] = username_textbox.Text;
            localSettings.Values["email"] = username_textbox.Text;
            username_Header.Text = username_textbox.Text;
            this.Frame.Navigate(typeof(oobe6), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void usernameSave_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Accidental event creation, ignore this function
        }

        private void username_textbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (username_textbox.Text == string.Empty) { nextButton.IsEnabled = false; }
            else { nextButton.IsEnabled = true; }
        }

        private void email_textbox_KeyDown(object sender, KeyRoutedEventArgs e)
        {

        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Project_Radon.Controls
{
    public sealed partial class RadonContentDialog : ContentDialog
    {
        public RadonContentDialog()
        {
            this.InitializeComponent();
        }

        // 👇 Add these properties
        public string TitleText
        {
            get => Title.Text; set => Title.Text = value;
        }

        public string SubtitleText
        {
            get => Subtitle.Text; set => Subtitle.Text = value;
        }
        public string DialogIconGlyph
        {
            get => DialogIcon.Glyph; set => DialogIcon.Glyph = value;
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}

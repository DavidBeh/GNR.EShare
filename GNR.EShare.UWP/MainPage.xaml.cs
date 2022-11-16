using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace GNR.EShare.UWP
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        public MainPage()
        {
            
            this.InitializeComponent();
            StorageApplicationPermissions.FutureAccessList.GetFileAsync("§", )
        }

        private void btn_main_Click(object sender, RoutedEventArgs e)
        { 
            var eng =  OcrEngine.TryCreateFromLanguage(new Language("de-DE"));
            eng.RecognizeAsync(null!).GetResults().Lines.First().Words.First().BoundingRect
        }
    }
}

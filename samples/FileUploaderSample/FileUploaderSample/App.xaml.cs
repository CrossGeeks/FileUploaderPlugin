using Plugin.FileUploader.Abstractions;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace FileUploaderSample
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            CrossMedia.Current.Initialize();

            MainPage = new NavigationPage(new FileUploaderSample.MainPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

﻿using Plugin.FileUploader;
using Plugin.FileUploader.Abstractions;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace FileUploaderSample
{
    public partial class MainPage : ContentPage
    {
        string filePath = string.Empty;
        bool isBusy = false;
        public MainPage()
        {
            
            InitializeComponent();
            takePhoto.Clicked += async (sender, args) =>
              {

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                  await DisplayAlert("No Camera", ":( No camera available.", "OK");
                  return;
                }

                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                  PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                  Directory = "Sample",
                  Name = "test.jpg"
                });

                if (file == null)
                  return;
                  filePath = file.Path;

                image.Source = ImageSource.FromStream(() =>
                {
                  var stream = file.GetStream();
                   
                  file.Dispose();
                  return stream;
                });
              };

              pickPhoto.Clicked += async (sender, args) =>
              {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                 await  DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                  return;
                }
                 var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                              {
                                  PhotoSize =  Plugin.Media.Abstractions.PhotoSize.Medium
                              });


                if (file == null)
                  return;

                  filePath = file.Path;
                image.Source = ImageSource.FromStream(() =>
                {
                  var stream = file.GetStream();
                //  file.Dispose();
                  return stream;
                });
              };

              takeVideo.Clicked += async (sender, args) =>
              {
                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakeVideoSupported)
                {
                  await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                  return;
                }

                var file = await CrossMedia.Current.TakeVideoAsync(new Plugin.Media.Abstractions.StoreVideoOptions
                {
                  Name = "video.mp4",
                  Directory = "DefaultVideos",
                });

                if (file == null)
                  return;
                  filePath = file.Path;
                await  DisplayAlert("Video Recorded", "Location: " + file.Path, "OK");

                file.Dispose();
              };

              pickVideo.Clicked += async (sender, args) =>
              {
                if (!CrossMedia.Current.IsPickVideoSupported)
                {
                  await DisplayAlert("Videos Not Supported", ":( Permission not granted to videos.", "OK");
                  return;
                }
                var file = await CrossMedia.Current.PickVideoAsync();

                if (file == null)
                  return;
                  filePath = file.Path;
                  await DisplayAlert("Video Selected", "Location: " + file.Path, "OK");
                file.Dispose();
              };

            CrossFileUploader.Current.FileUploadCompleted += Current_FileUploadCompleted;
            CrossFileUploader.Current.FileUploadError += Current_FileUploadError;
        }

        private void Current_FileUploadError(object sender, FileUploadResponse e)
        {
            isBusy = false;
            System.Diagnostics.Debug.WriteLine($"{e.StatusCode} - {e.Message}");
            Device.BeginInvokeOnMainThread(async() =>
            {
               await DisplayAlert("File Upload", "Upload Failed", "Ok");
            });
        }

        private void Current_FileUploadCompleted(object sender, FileUploadResponse e)
        {
            isBusy = false;
            System.Diagnostics.Debug.WriteLine($"{e.StatusCode} - {e.Message}");
            Device.BeginInvokeOnMainThread(async() =>
            {
                await DisplayAlert("File Upload", "Upload Completed", "Ok");
            });
        }

        void OnUpload(object sender,EventArgs args)
        {
            if (isBusy)
                return;
            isBusy = true;
            CrossFileUploader.Current.UploadFileAsync("<URL HERE>", new FilePathItem("<FIELD NAME HERE>",filePath), new Dictionary<string, string>()
            {
               /*<HEADERS HERE>*/
            }
            );
        }

    }
}

using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using System.IO;
using Windows.ApplicationModel;

namespace JetImageLoader
{
    public abstract class BaseJetImageLoaderConverter : IValueConverter
    {

        protected virtual JetImageLoader JetImageLoader { get; set; }

        protected BaseJetImageLoaderConverter()
        {
            var config = GetJetImageLoaderConfig();
            if (config == null)
            {
                throw new ArgumentException("JetImageLoaderConfig can not be null");
            }
            JetImageLoader = JetImageLoader.Instance;
            JetImageLoader.Initialize(config);
        }

        public virtual object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (DesignMode.DesignModeEnabled)
            {
                // hack to hide warning "Unable to determine application identity of the caller" in XAML editor
                // no sideeffects in runtime on WP
                return null;
            }
            Uri imageUri;
            var bitmapImage = new BitmapImage();
            bitmapImage.UriSource = new Uri(new Uri("ms-appx://"), "/Assets/placeholder.jpg");
            if (value is string && (value as string).Length > 0 && Uri.IsWellFormedUriString(value as string, UriKind.RelativeOrAbsolute))
            {
                try
                {
                    imageUri = new Uri((string)value);
                }
                catch
                {
                    //bitmapImage.SetSource(Application.GetResourceStream(new Uri(@"Assets/placeholder.jpg", UriKind.Relative)).Stream);
                    return bitmapImage;
                }
            }
            else if (value is Uri)
            {
                imageUri = (Uri)value;
            }
            else
            {
                //bitmapImage.SetSource(Application.GetResourceStream(new Uri(@"Assets/placeholder.jpg", UriKind.Relative)).Stream);
                return bitmapImage;
            }
            if (imageUri.Scheme == "http" || imageUri.Scheme == "https")
            {
                bitmapImage.UriSource = new Uri(new Uri("ms-appx://"), "/Assets/placeholder.jpg");
                var LoadImageStreamTask = Task.Run(() => JetImageLoader.LoadImageStream(imageUri));
                var uiTask = LoadImageStreamTask.ContinueWith(async getImageStreamTask =>
                {
                    if (getImageStreamTask.Result != null)
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                getImageStreamTask.Result.Seek(0, SeekOrigin.Begin);
                                bitmapImage.SetSource(getImageStreamTask.Result.AsRandomAccessStream());
                            }
                            catch
                            {
                                //bitmapImage.SetSource(Application.GetResourceStream(new Uri(@"Assets/placeholder.jpg", UriKind.Relative)).Stream);
                                //return bitmapImage;
                            }
                        });
                    }
                });
                return bitmapImage;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }

        protected abstract JetImageLoaderConfig GetJetImageLoaderConfig();

    }

}
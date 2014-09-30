using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

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

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                // hack to hide warning "Unable to determine application identity of the caller" in XAML editor
                // no sideeffects in runtime on WP
                return null;
            }

            Uri imageUri;
            var bitmapImage = new BitmapImage();
            bitmapImage.UriSource = new Uri("/Assets/placeholder.jpg", UriKind.RelativeOrAbsolute);

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
                bitmapImage.UriSource = new Uri("/Assets/placeholder.jpg", UriKind.RelativeOrAbsolute);
                Task.Factory.StartNew(() => JetImageLoader.LoadImageStream(imageUri).ContinueWith(getImageStreamTask =>
                {
                    if (getImageStreamTask.Result != null)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            try
                            {
                                //bitmapImage.DecodePixelType = DecodePixelType.Physical;
                                //bitmapImage.DecodePixelHeight = 100;
                                //bitmapImage.DecodePixelWidth = 100;
                                //bitmapImage.DecodePixelType = DecodePixelType.Logical;
                                getImageStreamTask.Result.Seek(0, System.IO.SeekOrigin.Begin);
                                bitmapImage.SetSource(getImageStreamTask.Result);
                            }
                            catch
                            {
                                //bitmapImage.SetSource(Application.GetResourceStream(new Uri(@"Assets/placeholder.jpg", UriKind.Relative)).Stream);
                                //return bitmapImage;
                            }
                        });
                    }
                }));

                return bitmapImage;
            }

            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected abstract JetImageLoaderConfig GetJetImageLoaderConfig();
    }
}

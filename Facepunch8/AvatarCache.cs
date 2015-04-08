using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CaledosLab.Portable.Logging;

namespace Facepunch8
{
    public class AvatarCache
    {
        public delegate void AvatarCallback(BitmapImage avatar);

        private static AvatarCache _instance = new AvatarCache();

        public static AvatarCache Instance { get { return _instance; } }

        private Dictionary<int, BitmapImage> images = new Dictionary<int, BitmapImage>();
        private object _lock = new object();

        private AvatarCache()
        {
            //...
        }

        public void RetrieveAvatar(int userId, AvatarCallback acb)
        {
            lock (_lock)
            {
                if (images.ContainsKey(userId))
                {
                    acb(images[userId]);
                    return;
                }
            }

            //DL the img
            try
            {
                var request = WebRequest.CreateHttp("http://facepunch.com/image.php?u=" + userId);
                request.Method = "GET";
                
                request.BeginGetResponse(
                        a =>
                        {
                            try
                            {
                                var response = (HttpWebResponse)request.EndGetResponse(a);

                                var responseStream = response.GetResponseStream();

                                //This ungodly code is from stackoverflow...
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    responseStream.CopyTo(memoryStream);
                                    memoryStream.Position = 0;
                                    byte[] buffer = null;
                                    if (memoryStream != null && memoryStream.Length > 0)
                                    {
                                        BinaryReader binaryReader = new BinaryReader(memoryStream);
                                        buffer = binaryReader.ReadBytes((int)memoryStream.Length);
                                        Stream stream = new MemoryStream();
                                        stream.Write(buffer, 0, buffer.Length);
                                        stream.Seek(0, SeekOrigin.Begin);
                                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            BitmapImage bitmapImage = new BitmapImage { CreateOptions = BitmapCreateOptions.None };
                                            bitmapImage.SetSource(stream);

                                            lock (_lock)
                                            {
                                                if (images.ContainsKey(userId)) //In case there were more than one call at the same time
                                                {
                                                    acb(images[userId]);
                                                }
                                                else
                                                {
                                                    images.Add(userId, bitmapImage);
                                                    acb(bitmapImage);
                                                }
                                            }
                                        });
                                    }

                                }
                            }
                            catch (WebException ex)
                            {
                                Logger.WriteLine(ex);
                            }
                        }, null);
            }
            catch (WebException ex)
            {
                Logger.WriteLine(ex);
            }
        }
    }
}

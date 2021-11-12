using System;
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
using DJI.WindowsSDK;
using Windows.UI.Xaml.Media.Imaging;
using DJIVideoParser;
using System.Threading.Tasks;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace DJIWSDKDemo
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //use videoParser to decode raw data.
        private DJIVideoParser.Parser videoParser;
        public MainPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;

            //Replace with your registered App Key. Make sure your App Key matched your application's package name on DJI developer center.
            DJISDKManager.Instance.RegisterApp("75eaa701e077413fdcbeef2c");
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {
                System.Diagnostics.Debug.WriteLine("Register app successfully.");

                //The product connection state will be updated when it changes here.
                DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged += async delegate (object sender, ProductTypeMsg? value)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        if (value != null && value?.value != ProductType.UNRECOGNIZED)
                        {
                            System.Diagnostics.Debug.WriteLine("The Aircraft is connected now.");
                            //You can load/display your pages according to the aircraft connection state here.
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("The Aircraft is disconnected now.");
                            //You can hide your pages according to the aircraft connection state here, or show the connection tips to the users.
                        }
                    });
                };

                //If you want to get the latest product connection state manually, you can use the following code
                var productType = (await DJISDKManager.Instance.ComponentManager.GetProductHandler(0).GetProductTypeAsync()).value;
                if (productType != null && productType?.value != ProductType.UNRECOGNIZED)
                {
                    System.Diagnostics.Debug.WriteLine("The Aircraft is connected now.");
                    //You can load/display your pages according to the aircraft connection state here.
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Register SDK failed, the error is: ");
                System.Diagnostics.Debug.WriteLine(resultCode.ToString());
            }
        }

        //raw data
        void OnVideoPush(VideoFeed sender, byte[] bytes)
        {
            videoParser.PushVideoData(0, 0, bytes, bytes.Length);
        }

        //Decode data. Do nothing here. This function would return a bytes array with image data in RGBA format.
        async void ReceiveDecodedData(byte[] data, int width, int height)
        {
        }

        //We need to set the camera type of the aircraft to the DJIVideoParser. After setting camera type, DJIVideoParser would correct the distortion of the video automatically.
        private void OnCameraTypeChanged(object sender, CameraTypeMsg? value)
        {
            if (value != null)
            {
                switch (value.Value.value)
                {
                    case CameraType.MAVIC_2_ZOOM:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Mavic2Zoom);
                        break;
                    case CameraType.MAVIC_2_PRO:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Mavic2Pro);
                        break;
                    default:
                        this.videoParser.SetCameraSensor(AircraftCameraType.Others);
                        break;
                }

            }
        }
    }
}

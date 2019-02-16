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
using GoogleTasksUWPAPI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GoogleTasksAPITesting
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly string _bundleCallbackString = $"{ClientSecrets.BundleId}:/oauth2redirect";
        private readonly GTasksOAuth _oAuthClient;
        public MainPage()
        {
            this.InitializeComponent();
            _oAuthClient = new GTasksOAuth(ClientSecrets.ClientId, _bundleCallbackString);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _oAuthClient.StartAuthorisationRequest();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _oAuthClient.HandleUriCallback(e);
        }
    }
}

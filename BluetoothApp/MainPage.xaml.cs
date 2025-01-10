using Android.Bluetooth;
using Java.Util;

namespace BluetoothApp
{
    public partial class MainPage : ContentPage
    {
        BluetoothAdapter? _bluetoothAdapter;
        BluetoothSocket? _bluetoothSocket;
        BluetoothDevice? _bluetoothDevice;

        public MainPage()
        {
            InitializeComponent();
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RequestBluetoothPermissions();
        }

        // Požádání o potřebná Bluetooth oprávnění
        private async void RequestBluetoothPermissions()
        {
            var bluetoothPermission = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
            if (bluetoothPermission != PermissionStatus.Granted)
            {
                bluetoothPermission = await Permissions.RequestAsync<Permissions.Bluetooth>();
            }

            if (bluetoothPermission != PermissionStatus.Granted)
            {
                await DisplayAlert("Chyba", "Bez oprávnění pro Bluetooth se nemůžete připojit k zařízení.", "OK");
            }
        }

        private void OnConnectButtonClicked(object sender, EventArgs e)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            ConnectToHC05();     
        }

        private void ConnectToHC05()
        {
            if (_bluetoothAdapter == null || !_bluetoothAdapter.IsEnabled)
            {
                DisplayAlert("Chyba", "Bluetooth není zapnutý.", "OK");
                return;
            }

            // Získání spárovaných zařízení
            var pairedDevices = _bluetoothAdapter.BondedDevices;
            var hc05Device = pairedDevices.FirstOrDefault(device => device.Name.Contains("HC-05"));

            if (hc05Device == null)
            {
                DisplayAlert("Chyba", "Zařízení HC-05 není spárováno.", "OK");
                return;
            }

            _bluetoothDevice = hc05Device;
            ConnectToDevice();
        }

        private async void ConnectToDevice()
        {
            try
            {
                var uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"); // Serial Port Profile (SPP)
                _bluetoothSocket = _bluetoothDevice.CreateRfcommSocketToServiceRecord(uuid);
                await _bluetoothSocket.ConnectAsync();

                if (_bluetoothSocket.IsConnected)
                {
                    // Skrytí overlay a zobrazení ovládání
                    // Skrytí kolečka po připojení nebo chybě
                    LoadingIndicator.IsVisible = false;
                    LoadingIndicator.IsRunning = false;
                    OverlayLayer.IsVisible = false;
                    ControlLayer.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Chyba při připojování: {ex.Message}", "OK");
            }
        }

        private async void OnDisconnectButtonClicked(object sender, EventArgs e)
        {
            try
            {
                _bluetoothSocket.Close();
                OverlayLayer.IsVisible = true;
                ControlLayer.IsVisible = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Chyba při odpojování: {ex}", "OK");
            }
        }

        private async Task SendDataToHC05(string message)
        {
            if (_bluetoothSocket == null || !_bluetoothSocket.IsConnected)
            {
                await DisplayAlert("Chyba", "Bluetooth není připojeno.", "OK");
                return;
            }

            try
            {
                var outputStream = _bluetoothSocket.OutputStream;
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message);
                await outputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Chyba při odesílání dat: {ex.Message}", "OK");
            }
        }
        private async void OnButtonPressed(object sender, EventArgs e, string command)
        {
            try
            {
                // Odeslat příkaz podle parametru při stisknutí tlačítka
                await SendDataToHC05(command);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Chyba při odesílání: {ex.Message}", "OK");
            }
        }
        private void OnForwardButtonPressed(object sender, EventArgs e)
        {
            OnButtonPressed(sender, e, "1");
        }

        private void OnBackwardButtonPressed(object sender, EventArgs e)
        {
            OnButtonPressed(sender, e, "2");
        }

        private void OnLeftButtonPressed(object sender, EventArgs e)
        {
            OnButtonPressed(sender, e, "3");
        }

        private void OnRightButtonPressed(object sender, EventArgs e)
        {
            OnButtonPressed(sender, e, "4");
        }

        private void OnButtonReleased(object sender, EventArgs e)
        {
            OnButtonPressed(sender, e, "0");
        }
    }
}

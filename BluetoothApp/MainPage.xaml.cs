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

        // Event handler pro připojení k HC-05
        private void OnConnectButtonClicked(object sender, EventArgs e)
        {
            ConnectToHC05();
        }

        // Připojení k HC-05
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

        // Připojení k Bluetooth zařízení
        private async void ConnectToDevice()
        {
            try
            {
                // UUID pro SPP profil
                var uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"); // Serial Port Profile (SPP)
                _bluetoothSocket = _bluetoothDevice.CreateRfcommSocketToServiceRecord(uuid);

                // Pokus o připojení
                await _bluetoothSocket.ConnectAsync();

                if (_bluetoothSocket.IsConnected)
                {
                    await DisplayAlert("Úspěch", "Připojeno k zařízení HC-05!", "OK");
                    StatusLabel.Text = "Status: Připojeno";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Chyba při připojování: {ex.Message}", "OK");
            }
        }

		private void OnTurnOnLedClicked(object sender, EventArgs e)
		{
			SendDataToHC05("1");
		}

		private void OnTurnOffLedClicked(object sender, EventArgs e)
		{
			SendDataToHC05("0");
		}

        // Příklad odesílání dat na HC-05
        private async Task SendDataToHC05(string message)
        {
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

        // Příklad čtení dat z HC-05
        private async Task ReadDataFromHC05()
        {
            try
            {
                var inputStream = _bluetoothSocket.InputStream;
                byte[] buffer = new byte[1024];
                int bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length);
                string message = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                // Zpracování přijatých dat
            }
            catch (Exception ex)
            {
                await DisplayAlert("Chyba", $"Chyba při čtení dat: {ex.Message}", "OK");
            }
        }
    }
}

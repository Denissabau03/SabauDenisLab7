using Microsoft.Maui.Devices.Sensors;
using Plugin.LocalNotification;
using SabauDenisLab7.Models;
using SabauDenisLab7.Data;
using SQLiteNetExtensions.Attributes;

namespace SabauDenisLab7
{
    public partial class ShopPage : ContentPage
    {
        public ShopPage()
        {
            InitializeComponent();
        }

        // Save the shop to the database
        async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            var shop = (Shop)BindingContext;

            // Check if the shop has a name and address before saving
            if (string.IsNullOrEmpty(shop.ShopName) || string.IsNullOrEmpty(shop.Adress))
            {
                await DisplayAlert("Error", "Please enter a valid shop name and address.", "OK");
                return;
            }

            await App.Database.SaveShopAsync(shop);
            await Navigation.PopAsync();
        }

        // Show the shop location on the map
        async void OnShowMapButtonClicked(object sender, EventArgs e)
        {
            var shop = (Shop)BindingContext;
            var address = shop.Adress;

            // Check if the address is provided
            if (string.IsNullOrEmpty(address))
            {
                await DisplayAlert("Error", "Please enter a valid address.", "OK");
                return;
            }

            // Get the location from the address
            var locations = await Geocoding.GetLocationsAsync(address);
            var shopLocation = locations?.FirstOrDefault();

            if (shopLocation == null)
            {
                await DisplayAlert("Error", "Could not find the shop location.", "OK");
                return;
            }

            var options = new MapLaunchOptions
            {
                Name = shop.ShopName
            };

            var myLocation = await Geolocation.GetLocationAsync();
            if (myLocation != null)
            {
                var distance = myLocation.CalculateDistance(shopLocation, DistanceUnits.Kilometers);

                // If the shop is within 5 kilometers, show a notification
                if (distance < 5)
                {
                    var request = new NotificationRequest
                    {
                        Title = "You have shopping to do nearby!",
                        Description = address,
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = DateTime.Now.AddSeconds(1)
                        }
                    };
                    LocalNotificationCenter.Current.Show(request);
                }
            }

            // Open the map with the shop's location
            await Map.OpenAsync(shopLocation, options);
        }

        // Delete the shop from the database
        async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            var shop = (Shop)BindingContext;

            // Confirm deletion
            var confirm = await DisplayAlert("Confirm Delete",
                                             "Are you sure you want to delete this shop?",
                                             "Yes",
                                             "No");

            if (confirm)
            {
                // Delete the shop from the database
                await App.Database.DeleteShopAsync(shop);

                // Navigate back after deletion
                await Navigation.PopAsync();
            }
        }
    }
}

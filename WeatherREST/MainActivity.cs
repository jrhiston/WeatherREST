using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System;
using System.Json;

namespace WeatherREST
{
    [Activity(Label = "WeatherREST", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // Get the latitude/longitude EditBox and button resources:
            EditText city = FindViewById<EditText>(Resource.Id.cityText);
            EditText code = FindViewById<EditText>(Resource.Id.countryCodeText);
            Button button = FindViewById<Button>(Resource.Id.getWeatherButton);

            // When the user clicks the button ...
            button.Click += async (sender, e) => {

                // http://api.openweathermap.org/data/2.5/weather?lat=35&lon=139&appid=3d02475102ad30c9e35b4c65964f625c

                // Get the latitude and longitude entered by the user and create a query.
                string url = "http://api.openweathermap.org/data/2.5/weather?q=" +
                             city.Text +
                             "," +
                             code.Text +
                             "&units=metric" +
                             "&appid=3d02475102ad30c9e35b4c65964f625c";

                // Fetch the weather information asynchronously, 
                // parse the results, then update the screen:
                JsonValue json = await FetchWeatherAsync(url);
                ParseAndDisplay(json);
            };
        }

        private async Task<JsonValue> FetchWeatherAsync(string url)
        {
            // Create an HTTP web request using the URL:
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            // Send the request to the server and wait for the response:
            using (WebResponse response = await request.GetResponseAsync())
            {
                // Get a stream representation of the HTTP web response:
                using (Stream stream = response.GetResponseStream())
                {
                    // Use this stream to build a JSON document object:
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    //Console.Out.WriteLine("Response: {0}", jsonDoc.ToString());

                    // Return the JSON document:
                    return jsonDoc;
                }
            }
        }

        // Parse the weather data, then write temperature, humidity, 
        // conditions, and location to the screen.
        private void ParseAndDisplay(JsonValue json)
        {
            try
            {
                // Get the weather reporting fields from the layout resource:
                TextView location = FindViewById<TextView>(Resource.Id.locationText);
                TextView temperature = FindViewById<TextView>(Resource.Id.tempText);
                TextView humidity = FindViewById<TextView>(Resource.Id.humidText);
                TextView conditions = FindViewById<TextView>(Resource.Id.condText);
                TextView sunrise = FindViewById<TextView>(Resource.Id.sunriseText);
                TextView sunset = FindViewById<TextView>(Resource.Id.sunsetText);

                // Extract the array of name/value results for the field name "weatherObservation". 
                var main = json["main"];
                var sys = json["sys"];

                sunrise.Text = FromUnixTime(sys["sunrise"]).ToShortTimeString();
                sunset.Text = FromUnixTime(sys["sunset"]).ToShortTimeString();

                // Extract the "stationName" (location string) and write it to the location TextBox:
                location.Text = json["name"] + ", " + sys["country"];

                // The temperature is expressed in Celsius:
                double temp = main["temp"];
                // Write the temperature (one decimal place) to the temperature TextBox:
                temperature.Text = String.Format("{0:F1}", temp) + "° C";

                // Get the percent humidity and write it to the humidity TextBox:
                double humidPercent = main["humidity"];
                humidity.Text = humidPercent.ToString() + "%";

                conditions.Text = "";
                // Get the "clouds" and "weatherConditions" strings and 
                // combine them. Ignore strings that are reported as "n/a":
                var weather = (JsonArray)json["weather"];
                foreach (var item in weather)
                {
                    string mainWeather = item["main"];
                    if (mainWeather.Equals("n/a"))
                        mainWeather = "";
                    string mainDescription = item["description"];
                    if (mainDescription.Equals("n/a"))
                        mainDescription = "";

                    // Write the result to the conditions TextBox:
                    conditions.Text += $"{mainWeather} - {mainDescription}. ";
                }
            }
            catch (Exception exception)
            {
                Toast.MakeText(ApplicationContext, "Failed to find city and/or country code.", ToastLength.Long).Show();
            }
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}


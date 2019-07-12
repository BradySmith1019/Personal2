using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Device.Location;
using Microsoft.Win32;

namespace WelcomeApp
{
    class Program
    {

        private const string API_KEY = "1cc64de6467eb3d614423cc6341472f1";

        private const string currentURL = "http://api.openweathermap.org/data/2.5/weather?" + "lat=@LAT@&lon=@LON@&mode=xml&units=imperial&APPID=" + API_KEY;

        private const string ForecastURL = "http://api.openweathermap.org/data/2.5/forecast?" + "lat=@LAT@&lon=@LON@&mode=xml&units=imperial&APPID=" + API_KEY;

        static GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome Brady");
            Console.WriteLine();
            Console.Write("Retrieving Location... ");
            watcher.StatusChanged += WatcherStatusChanged;
            watcher.Start();
            SetStartup();
            Console.Read();

        }

        private static void WatcherStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                Console.Write("Location Found\n\n");

                GeoCoordinate coord = watcher.Position.Location;

                string curUrl = currentURL.Replace("@LAT@", coord.Latitude.ToString());
                curUrl = curUrl.Replace("@LON@", coord.Longitude.ToString());
                string forUrl = ForecastURL.Replace("@LAT@", coord.Latitude.ToString());
                forUrl = forUrl.Replace("@LON@", coord.Longitude.ToString());


                using (WebClient client = new WebClient())
                {
                    try
                    {
                        DisplayCurrentTemp(client.DownloadString(curUrl));
                    }

                    catch (Exception)
                    {
                        Console.WriteLine("Error retrieving current temperature");
                    }
                    try
                    {
                        Console.WriteLine("The weather forecast for " + DateTime.Today.DayOfWeek + ", " + DateTime.Now.ToString("MMMM") + " " + DateTime.Today.Day + " is:");
                        Console.WriteLine();
                        DisplayForecast(client.DownloadString(forUrl));
                    }

                    catch (Exception)
                    {
                        Console.WriteLine("Error retrieving weather");
                    }
                }
            }
        }

        private static void DisplayCurrentTemp(string xml)
        {
            XmlDocument currentDoc = new XmlDocument();
            currentDoc.LoadXml(xml);

            char degrees = (char)176;

            string currentCity = "";
            string currentTemp = "";
            string currentClouds = "";
            string currentWind = "";
            DateTime sunset = new DateTime();

            foreach (XmlNode other in currentDoc.SelectNodes("//current"))
            {
                XmlNode tempNode = other.SelectSingleNode("temperature");
                currentTemp = tempNode.Attributes["value"].Value;

                XmlNode cityNode = other.SelectSingleNode("city");
                XmlNode sunsetNode = cityNode.SelectSingleNode("sun");
                currentCity = cityNode.Attributes["name"].Value;
                sunset = DateTime.Parse(sunsetNode.Attributes["set"].Value, null, System.Globalization.DateTimeStyles.AssumeUniversal);

                XmlNode cloudNode = other.SelectSingleNode("clouds");
                currentClouds = cloudNode.Attributes["name"].Value;

                XmlNode windNode = other.SelectSingleNode("wind");
                XmlNode windNameNode = windNode.SelectSingleNode("speed");
                currentWind = windNameNode.Attributes["name"].Value;

            }

            Console.WriteLine("The current temperature in " + currentCity + " is " + currentTemp + degrees);
            Console.WriteLine("Sky: " + currentClouds + "\n" + "Wind: " + currentWind);
            Console.WriteLine("The sun will set at " + sunset.ToShortTimeString());
            Console.WriteLine();
        }

        private static void DisplayForecast(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            char degrees = (char)176;
            bool isRain = false;
            bool isSnow = false;

            foreach (XmlNode timeNode in doc.SelectNodes("//time"))
            {
                DateTime time = DateTime.Parse(timeNode.Attributes["from"].Value, null, System.Globalization.DateTimeStyles.AssumeUniversal);

                XmlNode tempNode = timeNode.SelectSingleNode("temperature");
                string temp = tempNode.Attributes["value"].Value;

                XmlNode precNode = timeNode.SelectSingleNode("precipitation");

                if (precNode.Attributes.Count != 0)
                {
                    string precipitation = precNode.Attributes["type"].Value;

                    if (precipitation.Equals("rain"))
                    {
                        isRain = true;
                    }

                    if (precipitation.Equals("snow"))
                    {
                        isSnow = true;
                    }
                }

                if (time.ToShortTimeString().Equals("12:00 AM") || time.ToShortTimeString().Equals("12:00 PM"))
                {
                    Console.Write(time.ToShortTimeString() + "\t" + temp + degrees);
                    if (isRain)
                    {
                        Console.Write(" Possible Rain");
                    }
                    if (isSnow)
                    {
                        Console.Write(" Possible Snow");
                    }
                    else
                    {
                        Console.Write("\n");
                    }
                    Console.WriteLine();

                    if (time.ToShortTimeString().Equals("12:00 AM"))
                    {
                        break;
                    }
                }
                else
                {
                    Console.Write(time.ToShortTimeString() + "\t\t" + temp + degrees);
                    if (isRain)
                    {
                        Console.Write(" Possible Rain");
                    }
                    if (isSnow)
                    {
                        Console.Write(" Possible Snow");
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }
        }

        private static void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string dir = AppDomain.CurrentDomain.BaseDirectory + "WelcomeApp";
            rk.SetValue("WelcomeApp", @dir);
        }
    }
}

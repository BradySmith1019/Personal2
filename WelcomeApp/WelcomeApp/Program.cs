﻿/* Brady Smith
*  1/14/2020
*  Welcome App
*/

using System;
using System.Linq;
using System.Net;
using System.Device.Location;
using Microsoft.Win32;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Net.Mail;
using MySql.Data;
using MySql.Data.MySqlClient;

/* Console application that displays various weather information obtained 
 * via the Dark Sky API and the flavor of the day for my local Culver's.
 */
namespace WelcomeApp
{
    class Program
    {
        // Placeholder for the required API key for accessing the Dark Sky API
        private const string API_KEY = "74daa3f39f330feb4ead7e52c3cb4352";

        // URL used to obtain the forecast. Latitude and Longitude are determined using a GeoCoordinateWatcher
        private const string ForecastURL = "https://api.darksky.net/forecast/" + API_KEY + "/@LAT@,@LON@?exclude=alerts,flags,minutely?units=auto";

        // Used to obtain the machine's current latitude and longitude
        static GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();

        // Holds the current flavor of the day
        public static string flavor = "";

        public static string high = "";

        public static string low = "";

        public static string temp = "";

        public static string description = "";

        public static string connectionString = "server=localhost;database=welcomeapp;uid=root;password=BSmith47";


        /* Entry point for the application
         */
        static void Main(string[] args)
        {
            // Attempts to make the console large enough to display all the information
            // without having to scroll]
            try
            {
                Console.WindowHeight = 56;
            }
            catch (ArgumentOutOfRangeException)
            {
                try
                {
                    Console.WindowHeight = 46;
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WindowHeight = 35;
                }
            }

            Console.Title = "Welcome";
            Console.WriteLine("\n" + "  Welcome Brady");
            Console.WriteLine();

            // Calls the method that gets today's flavor of the day
            getFlavor();

            // Calls the method that sends a text to a given phone number containing
            // the flavor of the day
            sendText();

            

            Console.Write("  Retrieving Location... ");

            // Retrieves the current location
            //watcher.StatusChanged += WatcherStatusChanged;
            watcher.TryStart(true, TimeSpan.FromMilliseconds(1000));
            GeoCoordinate coord = watcher.Position.Location;
            WatcherStatusChanged(coord.Latitude, coord.Longitude);

            AddToDB();

            ShowOptional();

            // Calls the method that makes this program run on computer startup
            SetStartup();
        }

        /* Used to determine the current location of the computer.
         * Gets added to the watcher object in the event that the
         * watcher's status has changed
         */
        private static void WatcherStatusChanged(double lat, double lon)
        {
            //if (e.Status == GeoPositionStatus.Ready)
            //{
                Console.Write("Location Found\n\n");

                // Used to hold the coordinates of the current location
                //GeoCoordinate coord = watcher.Position.Location;

                // Replaces the latitude and longitude in the forecast URL so the proper URL
                // can be accessed
                string forUrl = ForecastURL.Replace("@LAT@", lat.ToString());
                forUrl = forUrl.Replace("@LON@", lon.ToString());

                using (WebClient client = new WebClient())
                {
                    // Attempts to call the DisplayCurrentTemp method using the forecast URL
                    try
                    {
                        DisplayCurrentTemp(client.DownloadString(forUrl));
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                   
                }
            //}
        }

        /* Used to retrieve the current flavor of the day from
         * the Culver's website
         */
        private static void getFlavor()
        {
            HtmlWeb web = new HtmlWeb();

            HtmlDocument document = web.Load("https://www.culvers.com/restaurants/midvale");
            HtmlNode[] node = document.DocumentNode.SelectNodes("//a").ToArray();

            // Used to determine if the line with the flavor of the day has been reached
            bool theOne = false;

            // Used to count the number of lines that have been read in the HTML document
            int count = 0;

            if (node != null)
            {

                foreach (HtmlNode link in node)
                {
                    if (theOne)
                    {
                        // It was found from reading the Html document through that the count equals 1
                        // when the part of the line with the flavor of the day has been reached
                        if (count == 1)
                        {
                            flavor = link.InnerText;
                            Console.WriteLine("  The flavor of the day is " + link.InnerText + "\n");
                            break;
                        }
                        else
                        {
                            count++;
                        }
                    }

                    // It was found that the line after the line that reads "View Next Month"
                    // is the line with the flavor of the day
                    if (link.InnerText.Equals("View Next Month") || link.InnerText.Equals("View Entire Month"))
                    {
                        theOne = true;
                    }
                }
            }
        }

        /* Helper method used in the sendText method to send a text to a person through
         * an email service's email-to-text service
         */
        private static void SendEmail(string to_name, string to_email,
            string from_name, string from_email,
            string host, int port, bool enable_ssl, string password,
            string subject, string body)
        {
            // Make the mail message
            MailAddress from_address =
                new MailAddress(from_email, from_name);
            MailAddress to_address =
                new MailAddress(to_email, to_name);
            MailMessage message =
                new MailMessage(from_address, to_address);
            message.Subject = subject;
            message.Body = body;

            // Get the SMTP client
            SmtpClient client = new SmtpClient()
            {
                Host = host,
                Port = port,
                EnableSsl = enable_ssl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    from_address.Address, password),
            };

            // Send the message
            client.Send(message);
        }

        /*
         * Used to send a text to a recipient with the help of the sendEmail method
         */
        private static void sendText()
        {
            try
            {
                // The recipient's service provider goes here eg. ATT
                string carrier_email = "@txt.att.net";

                // Recipient's phone number goes here as a string
                string phone = "9202856365";

                // Constructs an email using the recipient's phone number and service provider
                string to_email = phone + carrier_email;

                // Calls the send email method with the recipient's name, the sender's email address, and the sender's
                // email address password in the blank strings. Sends the email, which sends the text to the recipient
                // with the flavor of the day
                SendEmail("Ashley", to_email,
                    "Culvers", "bradyboy95@hotmail.com",
                    "smtp.live.com", 587,
                    true, "BSmith47",
                    "Flavor of the Day", "The flavor of the day is " + flavor);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /* Displays the current temperature and forecast
         * 
         * Parameters:
         *      xml: the response from the Dark Sky API in xml format
         */
        private static void DisplayCurrentTemp(string xml)
        {
            // Converts the xml object to Json because I found it easier to parse
            dynamic response = JsonConvert.DeserializeObject(xml);

            // Gets the information regarding the current conditions
            dynamic currently = response["currently"];

            // Holds the degree symbol to be used in displaying the temperature
            char degree = (char)176;

            temp = currently["temperature"] + degree.ToString();
            description = currently["summary"];

            Console.WriteLine("  Summary: " + description + "\n");
            Console.WriteLine("  Current temperature is " + temp + "\n");

            // Gets the information regarding the hourly conditions
            dynamic hourly = response["hourly"];

            // Gets the information regarding the daily conditions
            dynamic daily = response["daily"];

            foreach (dynamic d in daily["data"])
            {
                high = d["temperatureHigh"] + degree.ToString();
                low = d["temperatureLow"] + degree.ToString();
                Console.WriteLine("  High for today:   " + high + "\n");
                Console.WriteLine("  Low for today:   " + low + "\n");

                // Converts the sunset time into a readable format
                DateTimeOffset sunsetTime = DateTimeOffset.FromUnixTimeSeconds((long)d["sunsetTime"]);
                string sunsetTimes = sunsetTime.ToLocalTime().ToString();
                string[] split = sunsetTimes.Split(' ');
                string[] noColon = split[1].Split(':');
                Console.WriteLine("  Sunset:  " + noColon[0] + ":" + noColon[1] + " " + split[2] + "\n");

                // Holds the phase of the moon, which is represented as a double in the range of 0 - 1
                double moon = (double)d["moonPhase"];

                if (moon == 0)
                {
                    Console.WriteLine("  Moon phase:   New Moon\n");
                }

                if (moon < 0.25 && moon > 0)
                {
                    Console.WriteLine("  Moon phase:   Waxing Crescent\n");
                }

                if (moon == 0.25)
                {
                    Console.WriteLine("  Moon phase:   First Quarter\n");
                }

                if (moon > 0.25 && moon < 0.5)
                {
                    Console.WriteLine("  Moon phase:   Waxing Gibbous\n");
                }

                if (moon == 0.5)
                {
                    Console.WriteLine("  Moon phase:   Full Moon\n");
                }

                if (moon > 0.5 && moon < 0.75)
                {
                    Console.WriteLine("  Moon phase:   Waning Gibbous\n");
                }

                if (moon == 0.75)
                {
                    Console.WriteLine("  Moon phase:   Last Quarter\n");
                }

                if (moon > 0.75)
                {
                    Console.WriteLine("  Moon phase:   Waning Crescent\n");
                }
                break;
            }

            Console.WriteLine("  Hourly Summary: " + hourly["summary"] + "\n");

            foreach (dynamic data in hourly["data"])
            {
                // Converts the hour into a readable format
                DateTimeOffset hourTime = DateTimeOffset.FromUnixTimeSeconds((long)data["time"]);
                string hourTimes = hourTime.ToLocalTime().ToString();
                string[] split = hourTimes.Split(' ');
                string[] noColon = split[1].Split(':');

                // Formats the double digit hours to make the output to the console look better
                if (noColon[0].Equals("10") || noColon[0].Equals("11") || (noColon[0].Equals("12") && split[2].Equals("PM")))
                {
                    Console.WriteLine("  " + noColon[0] + " " + split[2] + ":     " + data["temperature"] + degree.ToString() + "\t" + data["summary"] + "\n");
                    continue;
                }

                // Stops reading hourly data when the end of the day has been reached
                if (noColon[0].Equals("12") && split[2].Equals("AM"))
                {
                    Console.WriteLine("  " + noColon[0] + " " + split[2] + ":     " + data["temperature"] + degree.ToString() + "\t" + data["summary"] + "\n");
                    break;
                }

                Console.WriteLine("  " + noColon[0] + " " + split[2] + ":\t     " + data["temperature"] + degree.ToString() + "\t" + data["summary"] + "\n");
            }
        }

        private static void AddToDB()
        {
            if (temp.Equals("") || high.Equals("") || low.Equals("") || description.Equals("") || flavor.Equals(""))
            {
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "insert ignore into weather (Date, Temperature, High, Low, Description) values (@date, @temp, @high, @low, @description); insert ignore into flavors (Date, Flavor) values (@date, @flavor);";
                    command.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy.MM.dd"));
                    command.Parameters.AddWithValue("@temp", temp);
                    command.Parameters.AddWithValue("@high", high);
                    command.Parameters.AddWithValue("@low", low);
                    command.Parameters.AddWithValue("@description", description);
                    command.Parameters.AddWithValue("@flavor", flavor);

                    command.ExecuteNonQuery();
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void ShowOptional()
        {
            string next;
            while ((next = Console.ReadLine()) != null)
            {
                if (next == "show weather")
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            conn.Open();

                            MySqlCommand command = conn.CreateCommand();
                            command.CommandText = "select * from weather;";

                            MySqlDataReader myReader;
                            myReader = command.ExecuteReader();
                            Console.WriteLine("\r\n   Date     | Temperature |  High  |   Low   |  Description");
                            Console.WriteLine("------------+-------------+--------+---------+-----------------");
                            try
                            {
                                while (myReader.Read())
                                {
                                    if (myReader["Date"].ToString().Split(' ')[0].Length == 8)
                                    {
                                        if (myReader["Low"].ToString().Length == 3)
                                        {
                                            Console.WriteLine(myReader["Date"].ToString().Split(' ')[0] + "    |    " + myReader["Temperature"] + "     |  " + myReader["High"] + "  |   " + myReader["Low"] + "   |  " + myReader["Description"]);

                                        }
                                        else
                                        {
                                            Console.WriteLine(myReader["Date"].ToString().Split(' ')[0] + "    |    " + myReader["Temperature"] + "     |  " + myReader["High"] + "  |   " + myReader["Low"] + "  |  " + myReader["Description"]);

                                        }
                                    }
                                    else
                                    {
                                        if (myReader["Low"].ToString().Length == 3)
                                        {
                                            Console.WriteLine(myReader["Date"].ToString().Split(' ')[0] + "   |    " + myReader["Temperature"] + "     |  " + myReader["High"] + "  |   " + myReader["Low"] + "   |  " + myReader["Description"]);

                                        }
                                        else
                                        {
                                            Console.WriteLine(myReader["Date"].ToString().Split(' ')[0] + "   |    " + myReader["Temperature"] + "     |  " + myReader["High"] + "  |   " + myReader["Low"] + "  |  " + myReader["Description"]);
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                myReader.Close();
                            }
                        }

                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }

            }
        }

        /* Uses a registry key to set this program to run automatically whenever the computer
         * is booted up
         */
        private static void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string dir = AppDomain.CurrentDomain.BaseDirectory + "WelcomeApp";
            rk.SetValue("WelcomeApp", @dir);
        }
    }
}

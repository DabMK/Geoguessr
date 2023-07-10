using System;
using Geoguessr.Modules;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

#pragma warning disable CS8602
namespace Geoguessr.Actions
{
    internal class Street
    {
        readonly private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string loc = string.Empty;

        private static double mLat = 0, mxLat = 0, mLng = 0, mxLng = 0, lat = 0, lng = 0;


        public static void Play(ref bool oob)
        {
            Location.Load(ref mLat, ref mxLat, ref mLng, ref mxLng, ref loc);
            Console.Clear();

            while (true)
            {
                Random rnd = new();
                lat = mLat + (mxLat - mLat) * rnd.NextDouble();
                lng = mLng + (mxLng - mLng) * rnd.NextDouble();

                if (string.IsNullOrWhiteSpace(loc))
                {
                    Utilities.Log("Generated new Location!", ConsoleColor.Blue);
                }
                else
                {
                    Utilities.Log($"Generated new Location! [{loc}]", ConsoleColor.Blue);
                }

                string streetURL = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={lng}";
                string response = Utilities.Get(streetURL, Utilities.FakeAgent());

                JObject data = JObject.Parse(response);
                if (data["error"] != null && data["error"].ToString() == "Unable to geocode") { Utilities.Log("Location found doesn't exist. Searching for a new location...\n", ConsoleColor.Red); continue; }
                string[] boundingbox = data["boundingbox"].ToString().Replace("\n", "").Replace("[", "").Replace("]", "").Replace("\"", "").Split(',');

                if (!((lat >= double.Parse(boundingbox[0]) || lat <= double.Parse(boundingbox[1])) && (lng >= double.Parse(boundingbox[2]) || lng <= double.Parse(boundingbox[3]))))
                {
                    Utilities.Log($"BoundingBox wasn't respected. Generating a location inside of the closest BoundingBox...", ConsoleColor.Yellow);

                    double oldLat = lat;
                    double oldLng = lng;

                    lat = Utilities.ClosestSureArea(oldLat, double.Parse(boundingbox[0]), double.Parse(boundingbox[1]), oldLng, double.Parse(boundingbox[2]), double.Parse(boundingbox[3]))[0];
                    lng = Utilities.ClosestSureArea(oldLat, double.Parse(boundingbox[0]), double.Parse(boundingbox[1]), oldLng, double.Parse(boundingbox[2]), double.Parse(boundingbox[3]))[1];

                    if (lat < mLat || lat > mxLat || lng < mLng || lng > mxLng)
                    {
                        if (oob)
                        {
                            Utilities.Log($"Spot is outside of bounds.", ConsoleColor.Yellow);
                        }
                        else
                        {
                            Utilities.Log($"New spot is outside of bounds. Searching for a new location...\n", ConsoleColor.Red);
                            continue;
                        }
                    }
                }

                if (data["address"] != null)
                {
                    if (data["address"]["road"] != null && !string.IsNullOrEmpty(data["address"]["road"].ToString()))
                    {
                        Console.Clear();
                        Utilities.Log($"Location Loaded Succesfully!  - (Press '0' to receive a hint, '1' to guess and '2' to give up)\n");
                        Utilities.Log($"ROAD NAME: \"{data["address"]["road"]}\"\n");

                        int level = 0;
                        while (true)
                        {
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (int.TryParse(key.KeyChar.ToString(), out int choice))
                            {
                                switch (choice)
                                {
                                    case 0:
                                        level++;
                                        LoadSuggestion(level, data);
                                        continue;
                                    case 1:
                                        Console.Write(" - PUT ROAD COORDINATES (EVERY POINT THAT ROADS IS IN COUNTS)\n");
                                        Console.Write("Road Latitude Coordinate (type \"back\" to go back): ");
                                        string? cc = Console.ReadLine();
                                        if (string.IsNullOrWhiteSpace(cc) || string.Equals(cc, "back", StringComparison.OrdinalIgnoreCase)) { continue; }
                                        Console.Write("Road Longitude Coordinate (type \"back\" to go back): ");
                                        string? c2 = Console.ReadLine();
                                        if (string.IsNullOrWhiteSpace(c2) || string.Equals(c2, "back", StringComparison.OrdinalIgnoreCase)) { continue; }

                                        if (double.TryParse(cc, out double lat2) && double.TryParse(c2, out double lng2))
                                        {
                                            string streetURL2 = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat2}&lon={lng2}";
                                            string response2 = Utilities.Get(streetURL2, Utilities.FakeAgent());
                                            JObject data2 = JObject.Parse(response2);

                                            if (data2["address"] != null && data2["address"]["road"] != null && string.Equals(data2["address"]["road"].ToString(), data["address"]["road"].ToString(), StringComparison.OrdinalIgnoreCase))
                                            {
                                                Utilities.Log("\n\nRIGHT!", ConsoleColor.Green, false);
                                                Console.ReadKey();
                                                return;
                                            }
                                            else
                                            {
                                                Utilities.Log($"\n\nWRONG! (You guessed (Press '1' to view road)", ConsoleColor.Red, false);
                                                ConsoleKeyInfo fKey = Console.ReadKey();
                                                if (int.TryParse(fKey.KeyChar.ToString(), out int fChoice))
                                                {
                                                    if (fChoice == 1)
                                                    {
                                                        ProcessStartInfo startInfo = new()
                                                        {
                                                            WindowStyle = ProcessWindowStyle.Hidden,
                                                            CreateNoWindow = true,
                                                            FileName = "cmd.exe",
                                                            Arguments = $"/c start \"\" \"https://www.google.com/maps/@?api=1&map_action=pano&viewpoint={lat},{lng}\"",
                                                            UseShellExecute = true
                                                        };
                                                        Process.Start(startInfo);
                                                    }
                                                }
                                            }
                                            return;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    case 2:
                                        ProcessStartInfo startInfoGivenUp = new()
                                        {
                                            WindowStyle = ProcessWindowStyle.Hidden,
                                            CreateNoWindow = true,
                                            FileName = "cmd.exe",
                                            Arguments = $"/c start \"\" \"https://www.google.com/maps/@?api=1&map_action=pano&viewpoint={lat},{lng}\"",
                                            UseShellExecute = true
                                        };
                                        Process.Start(startInfoGivenUp);
                                        return;
                                    default:
                                        continue;
                                }
                            }
                            else
                            {
                                Utilities.Log($"Invalid button! Try again!\n", ConsoleColor.Yellow);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        Utilities.Log($"Road has no name. Generating a new road...\n", ConsoleColor.Red);
                        continue;
                    }
                }
                else
                {
                    Utilities.Log($"Feature found isn't a road. Generating a new position...\n", ConsoleColor.Red);
                    continue;
                }
            }
        }

        private static void LoadSuggestion(int level, JToken data)
        {
            switch (level)
            {
                case 1:
                    string fCity;
                    if (data["address"]["city"] != null) { fCity = data["address"]["city"].ToString(); }
                    else if (data["address"]["town"] != null) { fCity = data["address"]["town"].ToString(); }
                    else if (data["address"]["village"] != null) { fCity = data["address"]["village"].ToString(); }
                    else if (data["address"]["hamlet"] != null) { fCity = data["address"]["hamlet"].ToString(); }
                    else { fCity = "No City"; }

                    Utilities.Log($" - CITY: \"{fCity}\"", ConsoleColor.DarkGreen, false);
                    break;
                case 2:
                    string pc;
                    if (data["address"]["postcode"] != null) { pc = data["address"]["postcode"].ToString(); } else { pc = "No Postcode"; }

                    Utilities.Log($" - POSTCODE: \"{pc}\"", ConsoleColor.DarkGreen, false);
                    break;
                case 3:
                    string suburb;
                    if (data["address"]["quarter"] != null) { suburb = data["address"]["quarter"].ToString(); }
                    else if (data["address"]["suburb"] != null) { suburb = data["address"]["suburb"].ToString(); }
                    else if (data["address"]["neighbourhood"] != null) { suburb = data["address"]["neighbourhood"].ToString(); }
                    else { suburb = "No Suburb"; }

                    Utilities.Log($" - SUBURB/CLOSEST SUBURB: \"{suburb}\"", ConsoleColor.DarkGreen, false);
                    break;
                default:
                    Utilities.Log(" - NO MORE HINTS!", ConsoleColor.Yellow, false);
                    break;
            }
        }
    }
}
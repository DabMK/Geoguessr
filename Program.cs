using System;
using Geoguessr.Modules;
using Geoguessr.Actions;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

#pragma warning disable CS8600
#pragma warning disable CS8602
namespace Geoguessr
{
    internal class Program
    {
        private static bool locationsInfo = false, offroad = true, country = false, oob = false, closest = true;

        private static double mLat = 0, mxLat = 0, mLng = 0, mxLng = 0, lat = 0, lng = 0;

        private static string URL = string.Empty, state = string.Empty, cc = string.Empty, loc = string.Empty;
        readonly private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);


        private static void Main(string[] args)
        {
            if (args is null) { throw new ArgumentNullException(nameof(args)); }
            Initialize();

        Start:
            country = false;
            Console.WriteLine("\nGEOGUESSR:");
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("1) Add Location");
            Console.WriteLine("2) Load Location");
            Console.WriteLine("3) Load Country");
            Console.WriteLine("4) Give Specific Area");
            Console.WriteLine("5) Entire World");
            Console.WriteLine("6) Settings");
            Console.WriteLine("7) Locations List");
            Console.WriteLine("8) Delete Locations");
            Console.WriteLine("9) Quit");
            Console.WriteLine("-------------------------------------------");
            Console.Write("\n\nYour choice: ");
            string? action = Console.ReadLine();
            Console.Write("\n");

            switch (action.ToLower().Replace(" ", ""))
            {
                case "addlocation":
                case "1":
                    Console.Write("- ADD LOCATION\n");
                    Location.Add();
                    goto Start;

                case "loadlocation":
                case "2":
                    Console.Write("- LOAD LOCATION\n");
                    Location.Load(ref mLat, ref mxLat, ref mLng, ref mxLng, ref loc);
                    break;

                case "country":
                case "3":
                    Console.Write("- CHOOSE COUNTRY\n");
                    Country.Load(ref state, ref cc, ref mLat, ref mxLat, ref mLng, ref mxLng, ref country, ref loc);
                    break;

                case "give":
                case "4":
                    Console.Write("- GIVE LOCATION\n");
                    Location.Give(ref mLat, ref mxLat, ref mLng, ref mxLng);
                    break;

                case "world":
                case "5":
                    mLat = -90;
                    mxLat = 90;
                    mLng = -180;
                    mxLng = 180;
                    loc = "World";
                    break;

                case "settings":
                case "6":
                    Console.Write("- SETTINGS\n");
                    Settings.Load(ref locationsInfo, ref offroad, ref oob);
                    Console.Clear();
                    goto Start;

                case "list":
                case "7":
                    Location.List();
                    Console.Clear();
                    goto Start;

                case "delete":
                case "8":
                    Location.List(true);
                    Console.Clear();
                    goto Start;

                case "quit":
                case "9":
                    Environment.Exit(0);
                    return;

                default:
                    Utilities.Log("\nInvalid choice! Try again!", ConsoleColor.Red, false);
                    Console.ReadKey();
                    Console.Clear();
                    goto Start;
            }
            Console.Clear();

            if (mLat == 0 && mxLat == 0 && mLng == 0 && mxLng == 0) { goto Start; }
            Retry:
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
                string response = Utilities.Get(streetURL, FakeAgent());

                JObject data = JObject.Parse(response);
                if (data["error"] != null && data["error"].ToString() == "Unable to geocode") { Utilities.Log("Location found doesn't exist. Searching for a new location...\n", ConsoleColor.Red); continue; }
                string[] boundingbox = data["boundingbox"].ToString().Replace("\n", "").Replace("[", "").Replace("]", "").Replace("\"", "").Split(',');
                if (locationsInfo) { LoadLocation(data); }

                if (country)
                {
                    if (data["address"]["country_code"] != null)
                    {
                        if (data["address"]["country_code"].ToString() != cc)
                        {
                            Utilities.Log($"Country codes doesn't match. Searching for a new location...\n", ConsoleColor.Red);
                            continue;
                        }
                    }
                    else
                    {
                        Utilities.Log($"Can't decode country. Searching for a new location...\n", ConsoleColor.Red);
                        continue;
                    }
                }
                if (data["address"]["road"] == null)
                {
                    Utilities.Log($"Location found isn't a road. Searching for a new location...\n", ConsoleColor.Red);
                    continue;
                }

                // BOUNDINGBOX CHECK
                if ((lat >= double.Parse(boundingbox[0]) || lat <= double.Parse(boundingbox[1])) && (lng >= double.Parse(boundingbox[2]) || lng <= double.Parse(boundingbox[3])))
                {
                    if (locationsInfo) { Utilities.Log($"Boundingbox respected"); }
                }
                else
                {
                    Utilities.Log($"BoundingBox wasn't respected. Generating a location inside of the closest BoundingBox...", ConsoleColor.Yellow);

                    double oldLat = lat;
                    double oldLng = lng;

                    if (closest)
                    {
                        lat = FixBounds(oldLat, double.Parse(boundingbox[0]), double.Parse(boundingbox[1]));
                        lng = FixBounds(oldLng, double.Parse(boundingbox[2]), double.Parse(boundingbox[3]));
                    }
                    else
                    {
                        lat = ClosestSureArea(oldLat, double.Parse(boundingbox[0]), double.Parse(boundingbox[1]), oldLng, double.Parse(boundingbox[2]), double.Parse(boundingbox[3]))[0];
                        lng = ClosestSureArea(oldLat, double.Parse(boundingbox[0]), double.Parse(boundingbox[1]), oldLng, double.Parse(boundingbox[2]), double.Parse(boundingbox[3]))[1];
                    }

                    if (lat < mLat || lat > mxLat || lng < mLng || lng > mxLng)
                    {
                        if (locationsInfo) { Utilities.Log($"OOB STATUS:\n{mLat} < {lat} < {mxLat}\n{mLng} < {lng} < {mxLng}"); }
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
                URL = $"https://www.google.com/maps/@?api=1&map_action=pano&viewpoint={lat},{lng}";
                
                int radius = 30;
                string endpoint = "https://overpass-api.de/api/interpreter";
                string query = $"[out:json][timeout:25];(way[name](around:{radius},{lat},{lng});node[name](around:{radius},{lat},{lng}););out body;>;out skel qt;";
                string map = Utilities.Get($"{endpoint}?data={query}", FakeAgent());
                JObject mapData;
                try { mapData = JObject.Parse(map); } catch { Utilities.Log("Server Crashed. Searching for a new location...\n", ConsoleColor.Red); continue; }

                // CHECK IF ROAD IS IN A CONDITION THAT HAS ALLOWED GOOGLE STREET VIEW CAR TO GO THERE
                if (mapData["elements"] == null || string.IsNullOrWhiteSpace(mapData["elements"].ToString()) || mapData["elements"].ToString() == "[]")
                {
                    Utilities.Log("Street has not been explored by Google Street View. Searching for a new location...\n", ConsoleColor.Red);
                    continue;
                }
                int count = 0;
                bool ok = false;
                while (true)
                {
                    JArray? mapArray = (JArray)mapData["elements"];
                    if (count > (mapArray.Count - 1)) { break; }
                    if (mapData["elements"][count] == null) { break; }

                    JToken? item = mapData["elements"][count];
                    if (item["tags"] == null) { count++; continue; }

                    if (!offroad)
                    {
                        if ((item["tags"]["highway"] != null && item["tags"]["highway"].ToString() == "tertiary") || (item["tags"]["surface"] != null && item["tags"]["surface"].ToString() == "unpaved"))
                        {
                            Utilities.Log("Off-Road Location Declined.", ConsoleColor.Yellow);
                            ok = false;
                            break;
                        }
                    }

                    if (!mapData.ToString().Contains("addr:street"))
                    {
                        string[] split = item["tags"].ToString().Replace("{", "").Replace("}", "").Replace("\n", "").Replace(" ", "").Replace("\"", "").Split(',');
                        string splitter = string.Empty;
                        foreach (string s in split)
                        {
                            splitter += s.Split(':')[0] + ",";
                        }
                        if (splitter.Split(',').Length == 2 || splitter.Split(',').Length == 3 && (splitter.Contains("highway") && splitter.Contains("name")))
                        {
                            count++;
                            continue;
                        }

                        // FILTERS
                        if (item["tags"]["sac_scale"] != null || item["tags"]["trail_visibility"] != null)
                        {
                            count++;
                            continue;
                        }

                        if (item["tags"] != null && item["tags"]["highway"] != null)
                        {
                            ok = true;
                            break;
                        }
                    }
                    else
                    {
                        ok = true;
                        break;
                    }
                    count++;
                }

                if (!ok) { Utilities.Log("Elements of Street are corrupted. Searching for a new location...\n", ConsoleColor.Red); continue; }
                break;
            }

            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe",
                Arguments = $"/c start \"\" \"{URL}\"",
                UseShellExecute = true
            };
            Process.Start(startInfo);
            Utilities.Log($"Location Loaded Succesfully!");

            ConsoleKeyInfo key = Console.ReadKey();
            Console.Clear();
            if (char.TryParse(key.KeyChar.ToString(), out char but))
            {
                if (but == 'r' || but == 'R')
                {
                    goto Retry;
                }
            }
            goto Start;
        }


        // FUNCTION TO GET THE START OR THE END OF THE ROAD (DEPENDING ON WHICH IS THE CLOSEST FROM THE WRONG LOCATION)
        private static double[] ClosestSureArea(double num, double lower, double upper, double num2, double lower2, double upper2)
        {
            double[] closest = new double[2];
            double latitude = Math.Min(Math.Abs(lower - num), Math.Abs(upper - num));
            double longitude = Math.Min(Math.Abs(lower2 - num2), Math.Abs(upper2 - num2));

            if (longitude < latitude)
            {
                if (Math.Abs(lower2 - num2) > Math.Abs(upper2 - num2))
                {
                    closest[0] = lower;
                    closest[1] = lower2;
                }
                else
                {
                    closest[0] = upper;
                    closest[1] = upper2;
                }
            }
            else
            {
                if (Math.Abs(lower - num) > Math.Abs(upper - num))
                {
                    closest[0] = lower;
                    closest[1] = lower2;
                }
                else
                {
                    closest[0] = upper;
                    closest[1] = upper2;
                }
            }
            return closest;
        }

        // FUNCTION TO FIND THE CLOSEST AREA FROM THE WRONG ONE INSIDE OF THE BOUNDINGBOX (RANGE MAY BE BROKEN)
        private static double FixBounds(double num, double lower, double upper)
        {
            if (num >= lower && num <= upper)
            {
                return num;
            }
            else
            {
                double distanceToA = num - lower;
                double distanceToB = upper - num;

                if (distanceToA < distanceToB)
                {
                    return lower;
                }
                else
                {
                    return upper;
                }
            }
        }

        // VIEW LOCATION DEBUG INFO
        private static void LoadLocation(JToken data)
        {
            if (data["address"]["country"] != null)
            {
                Utilities.Log($"Country:          {data["address"]["country"]}");
            }
            if (data["address"]["state"] != null)
            {
                Utilities.Log($"State:            {data["address"]["state"]}");
            }
            if (data["address"]["county"] != null)
            {
                Utilities.Log($"County:           {data["address"]["county"]}");
            }
            if (data["address"]["city"] != null)
            {
                Utilities.Log($"City:             {data["address"]["city"]}");
            }
            if (data["address"]["municipality"] != null)
            {
                Utilities.Log($"Municipality:     {data["address"]["municipality"]}");
            }
            if (data["address"]["village"] != null)
            {
                Utilities.Log($"Village:          {data["address"]["village"]}");
            }
            if (data["address"]["hamlet"] != null)
            {
                Utilities.Log($"Hamlet:          {data["address"]["hamlet"]}");
            }
            if (data["address"]["suburb"] != null)
            {
                Utilities.Log($"Suburb:           {data["address"]["suburb"]}");
            }
            if (data["address"]["quarter"] != null)
            {
                Utilities.Log($"Quarter:          {data["address"]["quarter"]}");
            }
            if (data["address"]["isolated_dwelling"] != null)
            {
                Utilities.Log($"Dwelling:         {data["address"]["isolated_dwelling"]}");
            }
        }

        private static void Initialize()
        {
            if (!Directory.Exists(@$"{appData}\Geoguessr\")) { Directory.CreateDirectory(@$"{appData}\Geoguessr\"); }
            if (!Directory.Exists(@$"{appData}\Geoguessr\Settings\")) { Directory.CreateDirectory(@$"{appData}\Geoguessr\Settings\"); }

            Utilities.CheckSetting(ref locationsInfo, "debug");
            Utilities.CheckSetting(ref offroad, "offroad", true);
            Utilities.CheckSetting(ref oob, "oob");
            Utilities.CheckSetting(ref closest, "closest", true);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
        }

        // GENERATE A FAKE AGENT TO BYPASS A POSSIBLE AGENT CHECKER IN THE APIs
        private static Dictionary<string, string> FakeAgent()
        {
            Random random = new();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string versionChars = "0123456789";
            string? rnd = new(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
            string? rndVersion = new(Enumerable.Repeat(versionChars, 2).Select(s => s[random.Next(s.Length)]).ToArray());
            string? rndSubVersion = new(Enumerable.Repeat(versionChars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
            string? rndX = new(Enumerable.Repeat(versionChars, 2).Select(s => s[random.Next(s.Length)]).ToArray());

            return new Dictionary<string, string>
            {
                { "User-Agent", $"{rnd}/{rndVersion}.{rndSubVersion} (X{rndX}; Linux x86_64)" }
            };
        }
    }
}
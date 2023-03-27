using System;
using System.IO;
using Geoguessr.Modules;

#pragma warning disable CS8602
namespace Geoguessr.Actions
{
    internal class Location
    {
        readonly private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);


        public static void List(bool delete = false)
        {
            Console.Clear();
            if (Directory.EnumerateFileSystemEntries(@$"{appData}\Geoguessr\").Any())
            {
                string[] allLocations = Directory.GetDirectories(@$"{appData}\Geoguessr\");
                int locations = allLocations.Length;
                
                Console.WriteLine($"\nGEOGUESSR - SAVED LOCATIONS ({locations - 1}):");
                Console.WriteLine("-------------------------------------------");
                
                Console.ForegroundColor = ConsoleColor.Blue;
                int count = 0;
                for (int i = 0; i < locations; i++)
                {
                    string location = Path.GetFileName(allLocations[i]);

                    if (location != "Settings")
                    {
                        count++;
                        Console.WriteLine($"{count}) {location}");
                    }
                }
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("-------------------------------------------");

                string? del;
                bool ok;
                string chosen = string.Empty;

                if (delete)
                {
                    do
                    {
                        Console.Write("\n\nWhich one to delete (type \"back\" to go back)? ");
                        del = Console.ReadLine();
                        if (del == null) { Console.Write("\n"); } else if (del == "back") { return; }
                        ok = false;
                        foreach (string location in allLocations)
                        {
                            if (location == "Settings") { continue; }
                            if (del == Path.GetFileName(location))
                            {
                                ok = true;
                                chosen = location;
                                break;
                            }
                        }
                    }
                    while (del == null || !ok);
                    Console.Write($"\nAre you sure you want to delete \"{Path.GetFileName(chosen)}\" (Yes/No)?");
                    string? confirm = Console.ReadLine();
                    if (confirm != null && confirm == "Yes")
                    {
                        Directory.Delete(chosen);
                        Console.WriteLine($"\"{Path.GetFileName(chosen)}\" Deleted Succesfully!");
                    }
                    else
                    {
                        Console.WriteLine($"\"{Path.GetFileName(chosen)}\" wasn't Deleted!");
                    }
                }
            }
            else
            {
                Utilities.Log("\nThere are no locations saved!", ConsoleColor.Red);
            }
            Console.ReadKey();
        }

        public static void Give(ref double mLat, ref double mxLat, ref double mLng, ref double mxLng)
        {
            Console.WriteLine("\nYou can get coordinates in the website \"https://boundingbox.klokantech.com/\"\nTo copy the coordinates, select \"CSV RAW\" in the bottom\nType \"back\" to go back");
            
        Give:
            Console.Write("\nMin Latitude: ");
            string? minLats2 = Console.ReadLine();
            mLat = 0; mxLat = 0; mLng = 0; mxLng = 0;
            if (minLats2 == "back") { return; }

            if (double.TryParse(minLats2, out double minLat2))
            {
                mLat = minLat2;
            }
            else
            {
                Utilities.Log("There was an error while trying to parse the coordinates", ConsoleColor.Red);
                goto Give;
            }

            Console.Write("Max Latitude: ");
            string? maxLats2 = Console.ReadLine();
            if (maxLats2 == "back") { mLat = 0; mxLat = 0; mLng = 0; mxLng = 0; return; }

            if (double.TryParse(maxLats2, out double maxLat2))
            {
                mxLat = maxLat2;
            }
            else
            {
                Utilities.Log("There was an error while trying to parse the coordinates", ConsoleColor.Red);
                goto Give;
            }

            Console.Write("Min Longitude: ");
            string? minLngs2 = Console.ReadLine();
            if (minLngs2 == "back") { mLat = 0; mxLat = 0; mLng = 0; mxLng = 0; return; }

            if (double.TryParse(minLngs2, out double minLng2))
            {
                mLng = minLng2;
            }
            else
            {
                Utilities.Log("There was an error while trying to parse the coordinates", ConsoleColor.Red);
                goto Give;
            }

            Console.Write("Max Longitude: ");
            string? maxLngs2 = Console.ReadLine();
            if (maxLngs2 == "back") { mLat = 0; mxLat = 0; mLng = 0; mxLng = 0; return; }

            if (double.TryParse(maxLngs2, out double maxLng2))
            {
                mxLng = maxLng2;
            }
            else
            {
                Utilities.Log("There was an error while trying to parse the coordinates", ConsoleColor.Red);
                goto Give;
            }
        }

        public static void Load(ref double mLat, ref double mxLat, ref double mLng, ref double mxLng, ref string loc)
        {
        Load:
            Console.Write("Location Name (type \"back\" to go back): ");
            string? action = Console.ReadLine();
            string plc = string.Empty;
            mLat = 0; mxLat = 0; mLng = 0; mxLng = 0;

            if (action == "back") { return; }
            if (Directory.EnumerateFileSystemEntries(@$"{appData}\Geoguessr\").Any())
            {
                if (!action.ToLower().Replace(" ", "").Any(x => !char.IsLetter(x)))
                {
                    bool check = false;
                    foreach (string place in Directory.GetDirectories(@$"{appData}\Geoguessr\"))
                    {
                        if (action.ToLower().Replace(" ", "") == Path.GetFileName(place).ToLower().Replace(" ", ""))
                        {
                            if (!File.Exists(@$"{place}\MinLat.geoguessr") || !File.Exists(@$"{place}\MaxLat.geoguessr") || !File.Exists(@$"{place}\MinLng.geoguessr") || !File.Exists(@$"{place}\MaxLng.geoguessr"))
                            {
                                Utilities.Log($"There was an error while trying to find the coordinates!\n", ConsoleColor.Red);
                                goto Load;
                            }
                            if (double.TryParse(File.ReadAllText(@$"{place}\MinLat.geoguessr"), out double minLat3))
                            {
                                mLat = minLat3;
                            }
                            else
                            {
                                Utilities.Log($"There was an error while trying to parse the coordinates from \"{Path.GetFileName(place)}\"!\n", ConsoleColor.Red);
                                goto Load;
                            }
                            if (double.TryParse(File.ReadAllText(@$"{place}\MaxLat.geoguessr"), out double maxLat3))
                            {
                                mxLat = maxLat3;
                            }
                            else
                            {
                                Utilities.Log($"There was an error while trying to parse the coordinates from \"{Path.GetFileName(place)}\"!\n", ConsoleColor.Red);
                                goto Load;
                            }
                            if (double.TryParse(File.ReadAllText(@$"{place}\MinLng.geoguessr"), out double minLng3))
                            {
                                mLng = minLng3;
                            }
                            else
                            {
                                Utilities.Log($"There was an error while trying to parse the coordinates from \"{Path.GetFileName(place)}\"!\n", ConsoleColor.Red);
                                goto Load;
                            }
                            if (double.TryParse(File.ReadAllText(@$"{place}\MaxLng.geoguessr"), out double maxLng3))
                            {
                                mxLng = maxLng3;
                            }
                            else
                            {
                                Utilities.Log($"There was an error while trying to parse the coordinates from \"{Path.GetFileName(place)}\"!\n", ConsoleColor.Red);
                                goto Load;
                            }
                            check = true;
                            loc = Path.GetFileName(place);
                            break;
                        }
                    }
                    if (!check)
                    {
                        Utilities.Log($"The place \"{action}\" isn't saved in the program!\n", ConsoleColor.Red);
                        goto Load;
                    }
                }
                else
                {
                    Utilities.Log($"Invalid characters used in the place name!\n", ConsoleColor.Red);
                    goto Load;
                }
            }
            else
            {
                Utilities.Log($"There are no locations saved!\n", ConsoleColor.Red);
                goto Load;
            }
        }

        public static void Add()
        {
            Add:
            string? newPlace;
            while (true)
            {
                bool ok = true;
                Console.Write("Place Name (type \"back\" to go back): ");
                newPlace = Console.ReadLine();
                if (newPlace == "back" || newPlace == "settings" || string.IsNullOrWhiteSpace(newPlace)) { Console.Clear(); return; }

                foreach (string dir in Directory.GetDirectories(@$"{appData}\Geoguessr\"))
                {
                    if (newPlace.Replace(" ", "").ToLower() == Path.GetFileName(dir).Replace(" ", "").ToLower())
                    {
                        Utilities.Log("The place alredy exists!\n", ConsoleColor.Red);
                        ok = false;
                        break;
                    }
                }
                if (ok) { break; } else { continue; }
            }
            
            Console.WriteLine("\nYou can get coordinates in the website \"https://boundingbox.klokantech.com/\"\nTo copy the coordinates, select \"CSV RAW\" in the bottom");
            Console.Write("\nMin Latitude: ");
            string? minLats = Console.ReadLine();
            if (!double.TryParse(minLats, out double minLat))
            {
                Utilities.Log("There was an error while trying to parse the coordinates\n", ConsoleColor.Red);
                goto Add;
            }

            Console.Write("Max Latitude: ");
            string? maxLats = Console.ReadLine();
            if (!double.TryParse(maxLats, out double maxLat))
            {
                Utilities.Log("There was an error while trying to parse the coordinates\n", ConsoleColor.Red);
                goto Add;
            }

            Console.Write("Min Longitude: ");
            string? minLngs = Console.ReadLine();
            if (!double.TryParse(minLngs, out double minLng))
            {
                Utilities.Log("There was an error while trying to parse the coordinates\n", ConsoleColor.Red);
                goto Add;
            }

            Console.Write("Max Longitude: ");
            string? maxLngs = Console.ReadLine();
            if (!double.TryParse(maxLngs, out double maxLng))
            {
                Utilities.Log("There was an error while trying to parse the coordinates\n", ConsoleColor.Red);
                goto Add;
            }

            Directory.CreateDirectory(@$"{appData}\Geoguessr\{newPlace}\");
            File.WriteAllText(@$"{appData}\Geoguessr\{newPlace}\MinLat.geoguessr", minLat.ToString());
            File.WriteAllText(@$"{appData}\Geoguessr\{newPlace}\MaxLat.geoguessr", maxLat.ToString());
            File.WriteAllText(@$"{appData}\Geoguessr\{newPlace}\MinLng.geoguessr", minLng.ToString());
            File.WriteAllText(@$"{appData}\Geoguessr\{newPlace}\MaxLng.geoguessr", maxLng.ToString());

            Console.Clear();
        }
    }
}
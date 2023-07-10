using System;
using Geoguessr.Modules;

namespace Geoguessr.Actions
{
    internal class Settings
    {
        private static bool debug = false, road = false, bound = false, closest = false;

        readonly private static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);


        public static void Load(ref bool locationsInfo, ref bool offroad, ref bool oob)
        {
            while (true)
            {
                Initialize();
                Console.WriteLine("\nGEOGUESSR - SETTINGS:");
                Console.WriteLine("-------------------------------------------\n");
                Console.WriteLine("1) Show debug info              - " + (debug ? "ON" : "OFF"));
                Console.WriteLine("2) Accept Off-Road Streets      - " + (road ? "ON" : "OFF"));
                Console.WriteLine("3) Accept Out-Bounded Locations - " + (bound ? "ON" : "OFF"));
                Console.WriteLine("4) Accept Only Intersections    - " + (closest ? "OFF" : "ON"));
                Console.WriteLine("5) Main Menu");
                Console.WriteLine("-------------------------------------------\n\n");

                while (true)
                {
                    Console.Write("> ");
                    ConsoleKeyInfo key = Console.ReadKey();

                    if (int.TryParse(key.KeyChar.ToString(), out int num))
                    {
                        switch (num)
                        {
                            case 1:
                                File.WriteAllText(@$"{appData}\Geoguessr\Settings\debug.geoguessr", (!debug).ToString());
                                locationsInfo = !debug;
                                goto Break;
                            case 2:
                                File.WriteAllText(@$"{appData}\Geoguessr\Settings\offroad.geoguessr", (!road).ToString());
                                offroad = !offroad;
                                goto Break;
                            case 3:
                                File.WriteAllText(@$"{appData}\Geoguessr\Settings\oob.geoguessr", (!bound).ToString());
                                oob = !bound;
                                goto Break;
                            case 4:
                                File.WriteAllText(@$"{appData}\Geoguessr\Settings\closest.geoguessr", (!closest).ToString());
                                oob = !closest;
                                goto Break;
                            case 5:
                                goto BreakAll;
                            default:
                                break;
                        }
                    }
                    continue;
                    Break:
                    break;
                }
                continue;
                BreakAll:
                break;
            }
        }


        private static void Initialize()
        {
            Console.Clear();
            Utilities.CheckSetting(ref debug, "debug");
            Utilities.CheckSetting(ref road, "offroad", true);
            Utilities.CheckSetting(ref bound, "oob");
            Utilities.CheckSetting(ref closest, "closest", true);
        }
    }
}
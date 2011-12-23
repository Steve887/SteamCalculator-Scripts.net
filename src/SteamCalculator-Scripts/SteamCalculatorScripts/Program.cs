using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamCalculatorScripts.DAL;
using SteamCalculatorScripts.DAL.Entities;
using NHibernate;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;

namespace SteamCalculatorScripts
{
    class Program
    {
        static void Main()
        {
            List<string> countries = new List<string>() { "at", "au", "de", "no", "pl", "uk", "us" };
            List<SteamGame> games = new List<SteamGame>();

            DateTime startDate = DateTime.Now;

            foreach (var country in countries)
            {
                Console.WriteLine(String.Format("Connecting to {0} Steam Store", country.ToUpper()));

                string result = GetStoreContent(country, 1);

                // get the text "showing x - y of z" so we know how many records are on the current page, and how many total records there are
                var records = Regex.Split(result, @"showing\s\d+\s-\s(\d+)\sof\s(\d+)");

                double gamesPerPage = Convert.ToDouble(records[1]);
                double totalEntries = Convert.ToDouble(records[2]);
                var totalPages = Math.Ceiling(totalEntries / gamesPerPage);

                Console.WriteLine("Found {0} entries on {1} total pages", totalEntries, totalPages);

                // makes the console output look pretty
                string topLeftCornerCharacter = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x95, 0x94 });
                string bottomLeftCornerCharacter = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x95, 0x9A });
                string topRightCornerCharacter = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x95, 0x97 });
                string bottomRightCornerCharacter = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x95, 0x9D });
                string doubleHorizontalLine = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x95, 0x90 });
                string doubleVerticalLine = Encoding.UTF8.GetString(new byte[] { 0xE2, 0x95, 0x91 });

                for (int page = 1; page < totalPages + 1; page++)
                {
                    int gameCount = Convert.ToInt32((page - 1) * gamesPerPage + 1);

                    Console.WriteLine(topLeftCornerCharacter + String.Concat(Enumerable.Repeat(doubleHorizontalLine, 78)) + topRightCornerCharacter);
                    Console.WriteLine(doubleVerticalLine + "                                                                              " + doubleVerticalLine);
                    Console.WriteLine(doubleVerticalLine + String.Format("   Loading '{0}', page {1} of {2}                                                ", country.ToUpper(), page, totalPages) + doubleVerticalLine);
                    Console.WriteLine(doubleVerticalLine + String.Format("   Entries {0} - {1} of {2}                                                ", gameCount, page * gamesPerPage, totalEntries) + doubleVerticalLine);
                    Console.WriteLine(doubleVerticalLine + "                                                                              " + doubleVerticalLine);
                    Console.WriteLine(bottomLeftCornerCharacter + String.Concat(Enumerable.Repeat(doubleHorizontalLine, 78)) + bottomRightCornerCharacter);

                    int tempCursorPos = 0;

                    // Extracts the game data on the page between the List Items comment tags
                    string tempContent = Regex.Split(GetStoreContent(country, page), "<!-- List Items -->(.+?)<!-- End List Items -->", RegexOptions.Singleline)[1];

                    for (int i = gameCount; i < (gameCount + gamesPerPage); i++)
                    {
                        tempContent = tempContent.Substring(tempCursorPos);

                        SteamGame game = new SteamGame();

                        // Extracts the App Id from the Steam store link
                        var appId = Convert.ToInt32(Regex.Match(tempContent, @"/store\.steampowered\.com\/app\/(\d+)/").Groups[1].Value);

                        // Extracts the price
                        var price = Regex.Split(tempContent, "<div class=\"col search_price\">(.+?)</div>", RegexOptions.Singleline)[1];
                        decimal formattedPrice = 0;
                        try
                        {
                            if (price.ToLower().Contains("free"))
                            {
                            }
                            else if (price.Contains("strike"))
                            {
                                // is a discounted item, so get the second price
                                formattedPrice = Convert.ToDecimal(Regex.Split(price, @"(\d+[\.|\,]\d+)")[3].Replace(',', '.'));
                            }
                            else
                            {
                                // otherwise, get the price which will be numbers separated by a full stop or comma
                                formattedPrice = Convert.ToDecimal(Regex.Split(price, @"(\d+[\.|\,]\d+)")[1].Replace(',', '.'));
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Can happen if there isn't a price for a game, since we've already assigned 0 to formattedPrice, we'll just continue
                        }

                        if (games.Exists(x => x.AppId == appId))
                        {
                            game = games.Single(x => x.AppId == appId);
                            game.AddPrice(new SteamGamePrice(formattedPrice, country));
                        }
                        else
                        {
                            game.AppId = appId;

                            // Extract the Release Date and make sure it's real
                            var releaseDate = Regex.Match(tempContent, "<div class=\"col search_released\">(.+?)</div>", RegexOptions.Singleline).Groups[1].Value;

                            DateTime formattedReleaseDate;
                            if (!DateTime.TryParse(releaseDate, out formattedReleaseDate))
                            {
                                game.ReleaseDate = null;
                            }
                            else
                            {
                                game.ReleaseDate = formattedReleaseDate;
                            }

                            game.Title = Regex.Match(tempContent, "<h4>(.+?)</h4>", RegexOptions.Singleline).Groups[1].Value;
                            game.LastUpdate = DateTime.Now; // We'll use this to check for old games that aren't in the store any more

                            game.AddPrice(new SteamGamePrice(formattedPrice, country));

                            games.Add(game);
                        }

                        // Move the cursor to the next game, which comes after the <div style="clear: both;"></div> tags
                        tempCursorPos = tempContent.IndexOf("<div style=\"clear: both;\"></div>") + "<div style=\"clear: both;\"></div>".Length;

                        Console.WriteLine(String.Format("Title: {0}", game.Title));
                        Console.WriteLine(String.Format("App Id: {0}", game.AppId));
                        Console.WriteLine(String.Format("Price: {0}", game.Price.Single(x => x.RegionCode == country).Price));
                        Console.WriteLine(String.Format("Release Date: {0}", game.ReleaseDate));
                        Console.WriteLine();

                        if (i == Convert.ToInt32(totalEntries))
                        {
                            // We've reached the end, so let's get out
                            break;
                        }
                    }
                    Console.WriteLine("+---------+---------+-------------+--------------------------------------------+");
                }
            }

            ISessionFactory sessionFactory = SessionFactory.CreateSessionFactory();

            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    foreach (var game in games)
                    {
                        // Save each game to the database (takes a long time)
                        session.SaveOrUpdate(game);
                    }

                    transaction.Commit();
                }

                // If the game wasn't updated in this run, set Flags to 0 so we know to hide it on the front end, or remove it later
                session.CreateSQLQuery(String.Format("UPDATE SteamGame SET Flags = 0 WHERE LastUpdate is null or LastUpdate < {0}", startDate));

            }
        }

        private static string GetStoreContent(string country, int pageNumber)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["StoreURL"].Replace("$country", country).Replace("$page", pageNumber.ToString()));
            myRequest.Method = "GET";

            string result = string.Empty;
            using (WebResponse myResponse = myRequest.GetResponse())
            {
                using (StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }
}

using System.Collections.Concurrent;
using System.Data;
using CSharp.oDesk.Analyze.Helpers;
using CSharp.oDesk.Analyze.Properties;
using CSharp.oDesk.Api;
using CSharp.oDesk.Api.Interfaces;
using CSharp.oDesk.Connect;
using Spring.Json;
using Spring.Social.OAuth1;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CSharp.oDesk.Analyze
{
    class Program
    {
        // Please, don't forget to set connection string and your consumer key & secret in setting file

        private static readonly ConcurrentBag<string> Contractors = new ConcurrentBag<string>(); 

        private static void Main()
        {
            var startDate = DateTime.Now;

            try
            {

                var oDeskServiceProvider = new oDeskServiceProvider(Settings.Default.ODeskApiKey, Settings.Default.ODeskApiSecret);
                
                //Settings.Default.Reset();
               
                if (string.IsNullOrEmpty(Settings.Default.ODeskAccessTokenValue) ||
                    string.IsNullOrEmpty(Settings.Default.ODeskAccessTokenSecret))
                {
                    /* OAuth 'dance' */

                    // Authentication using Out-of-band/PIN Code Authentication
                    Console.Write("Getting request token...");
                    var oauthToken = oDeskServiceProvider.OAuthOperations.FetchRequestTokenAsync("oob", null).Result;
                    Console.WriteLine("Done");

                    var authenticateUrl = oDeskServiceProvider.OAuthOperations.BuildAuthorizeUrl(oauthToken.Value, null);
                    Console.WriteLine("Redirect user for authentication: " + authenticateUrl);
                    Process.Start(authenticateUrl);
                    Console.WriteLine("Enter PIN Code from oDesk authorization page:");
                    var pinCode = Console.ReadLine();

                    Console.Write("Getting access token...");
                    var requestToken = new AuthorizedRequestToken(oauthToken, pinCode);
                    var oauthAccessToken = oDeskServiceProvider.OAuthOperations.ExchangeForAccessTokenAsync(requestToken, null).Result;
                    Console.WriteLine("Done");

                    Settings.Default.ODeskAccessTokenValue = oauthAccessToken.Value;
                    Settings.Default.ODeskAccessTokenSecret = oauthAccessToken.Secret;
                    Settings.Default.Save();
                }
                else
                {
                    Console.Write("Getting access token from settings... ");
                    Console.WriteLine("Done");
                }

                var errors = new ConcurrentBag<string>();

                /* API */

                IoDesk oDesk = oDeskServiceProvider.GetApi(Settings.Default.ODeskAccessTokenValue, Settings.Default.ODeskAccessTokenSecret);
                
                /* Get List skills */

                var skills = GetSkills(oDesk).ToList();
                
                /* Get Jobs */

                Parallel.ForEach(skills, skill => GetByMultipleItems(oDesk, "jobs", "dbo.Jobs", skill,
                    "/api/profiles/v2/search/jobs.json?skills=<skills>&paging={0};{1}".Replace("<skills>", HttpUtility.UrlEncode(skill)), errors, 
                    job => new Job
                    {
                        Id = Guid.NewGuid(),
                        OdeskId = job.GetValue("id").ToStringWithoutQuotes(),
                        Title = job.GetValue("title").ToStringWithoutQuotes().Truncate(500),
                        OdeskCategory = job.GetValue("category").ToStringWithoutQuotes(),
                        OdeskSubcategory = job.GetValue("subcategory").ToStringWithoutQuotes(),
                        DateCreated = job.GetValue("date_created").ToDateTime(),
                        Budjet = job.GetValue("budget").ToInt32(),
                        ClientCountry = job.GetValue("client").GetValue("country").ToStringWithoutQuotes(),
                        Skill = skill
                    }));

                /* Get Frelancers (Contractors) Skills */

                Parallel.ForEach(skills, skill => GetByMultipleItems(oDesk, "providers", "dbo.Contractors_Skills", skill,
                    "/api/profiles/v2/search/providers.json?skills=<skills>&is_odesk_ready=1&include_entities=1&paging={0};{1}".Replace("<skills>", HttpUtility.UrlEncode(skill)), errors,
                    contractor => new ContractorSkill
                    {
                        Id = Guid.NewGuid(),
                        OdeskId = contractor.ToStringWithoutQuotes(),
                        Skill = skill
                    }));

                /* Get Frelancers (Contractors) Details */

                Parallel.ForEach(Contractors, contractorId => GetSingleItem(oDesk, "profile", "dbo.Contractors", contractorId,
                    string.Format("/api/profiles/v1/providers/{0}/brief.json", contractorId), errors, 
                    profile => new Contractor
                    {
                        Id = Guid.NewGuid(),
                        OdeskId = contractorId,
                        TotalHours = profile.GetValue("dev_total_hours").ToDouble(),
                        EngSkill = profile.GetValue("dev_eng_skill").ToInt32(),
                        Country = profile.GetValue("dev_country").ToStringWithoutQuotes(),
                        TotalFeedback = profile.GetValue("dev_tot_feedback").ToDouble(),
                        IsAffiliated = profile.GetValue("dev_is_affiliated").ToInt32(),
                        AdjScore = profile.GetValue("dev_adj_score").ToDouble(),
                        AdjScoreRecent = profile.GetValue("dev_adj_score_recent").ToDouble(),
                        LastWorkedTs = profile.GetValue("dev_last_worked_ts").ToInt64(),
                        LastWorked = profile.GetValue("dev_last_worked").ToStringWithoutQuotes(),
                        PortfolioItemsCount = profile.GetValue("dev_portfolio_items_count").ToInt32(),
                        UiProfileAccess = profile.GetValue("dev_ui_profile_access").ToStringWithoutQuotes(),
                        BilledAssignments = profile.GetValue("dev_billed_assignments").ToInt32(),
                        BillRate = profile.GetValue("dev_bill_rate").ToDouble(),
                        RecNo = profile.GetValue("dev_recno").ToInt32(),
                        City = profile.GetValue("dev_city").ToStringWithoutQuotes(),
                        LastActivity = profile.GetValue("dev_last_activity").ToStringWithoutQuotes(),
                        ShortName = profile.GetValue("dev_short_name").ToStringWithoutQuotes()
                    }));
                

                /* Show all previous errors */

                foreach (var error in errors)
                {
                    var tmp = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(error);
                    Console.ForegroundColor = tmp;
                }

                Console.WriteLine("Done!");
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (ex is oDeskApiException)
                    {
                        Console.WriteLine(ex.Message);
                        return true;
                    }
                    return false;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                var interval = DateTime.Now - startDate;

                Console.WriteLine("Total elapsed time: {0} h {1} min {2} sec", interval.Days * 24 + interval.Hours,
                    interval.Minutes, interval.Seconds);
                Console.ReadLine();
            }
        }

        private static IEnumerable<string> GetSkills(IoDesk oDesk)
        {
            Task<JsonValue> skillsSearchcall = oDesk.RestOperations.GetForObjectAsync<JsonValue>("/api/profiles/v1/metadata/skills.json");

            return skillsSearchcall.Result.GetValues("skills").Select(x => x.ToStringWithoutQuotes());

            //yield return "ColdFusion";
        }


        private static void GetSingleItem<T>(IoDesk oDesk, string apiItemName, string tableName, string key, string url, ConcurrentBag<string> errors, Func<JsonValue, T> convert)
        {
            try
            {
                T item = default(T);

                do
                {
                    try
                    {
                        var apiCall = oDesk.RestOperations.GetForObjectAsync<JsonValue>(url);

                        JsonValue json = apiCall.Result.GetValue(apiItemName);
                        
                        item = convert(json);

                        Console.WriteLine("Key '{0}' ok.", key);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(key + " " + ex.Message);
                    }
                } while (Equals(item,default(T)));


                SaveDataTableToDatabase(new List<T> {item}.ConvertToDatatable(), tableName);
            }
            catch (Exception ex)
            {
                var error = string.Format("Key '{0}' failed. {1}", key, ex.Message);
                Console.WriteLine(error);
                errors.Add(error);
            }
        }

        private static void GetByMultipleItems<T>(IoDesk oDesk, string apiItemsName, string tableName, string key, string url, ConcurrentBag<string> errors, Func<JsonValue, T> convert)
        {
            try
            {
                const int itemsPerPage = 100;

                int numberOfReturnedJobs = itemsPerPage;
                var offset = 0;

                var list = new List<T>();

                do
                {
                    try
                    {
                        var apiCall = oDesk.RestOperations.GetForObjectAsync<JsonValue>(string.Format(url, offset, itemsPerPage));

                        ICollection<JsonValue> collection = apiCall.Result.GetValues(apiItemsName);

                        numberOfReturnedJobs = collection.Count;

                        list.AddRange(collection.Select(convert));

                        var pagination = apiCall.Result.GetValue("paging");
                        Debug.Assert(numberOfReturnedJobs == Convert.ToInt32(pagination.GetValue("count").ToString()));
                        Debug.Assert(offset == Convert.ToInt32(pagination.GetValue("offset").ToString()));

                        Console.WriteLine("{0,5} +100 / {1,5}: {2}", offset, pagination.GetValue("total"),key);

                        offset += itemsPerPage;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0,5} +100 / {1,5}: {2} ({3})", offset, "fail", key, ex.Message);
                    }

                } while (numberOfReturnedJobs == itemsPerPage);

                var dataTable = list.ConvertToDatatable();

                SaveDataTableToDatabase(dataTable, tableName);

                if (apiItemsName == "providers")
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (!Contractors.Contains(row["OdeskId"].ToString()))
                        {
                            Contractors.Add(row["OdeskId"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var error = string.Format("Key '{0}' failed. {1}", key, ex.Message);
                Console.WriteLine(error);
                errors.Add(error);
            }
        }

        private static void SaveDataTableToDatabase(DataTable sourceDataTable, string destinationTableName)
        {
            if (sourceDataTable.Rows.Count > 0)
            {
                using (var dbConnection = new SqlConnection(Settings.Default.ConnectionString))
                {
                    dbConnection.Open();

                    using (
                        var s = new SqlBulkCopy(dbConnection)
                        {
                            DestinationTableName = destinationTableName
                        })
                    {
                        s.WriteToServer(sourceDataTable);
                    }
                }
            }
        }

        
    }

}
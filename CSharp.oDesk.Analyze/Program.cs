using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CSharp.oDesk.Analyze.Helpers;
using CSharp.oDesk.Analyze.Properties;
using CSharp.oDesk.Api;
using CSharp.oDesk.Api.Interfaces;
using CSharp.oDesk.Connect;
using Spring.Json;
using Spring.Social.OAuth1;

namespace CSharp.oDesk.Analyze
{
    class Program
    {
        // Please, don't forget to set connection string and your consumer key & secret in setting file
        

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

                var errors = new List<string>();

                /* API */

                IoDesk oDesk = oDeskServiceProvider.GetApi(Settings.Default.ODeskAccessTokenValue, Settings.Default.ODeskAccessTokenSecret);
                
                /* Get List skills */

                var skills = GetSkills(oDesk).ToList();

                var maxSkillLen = skills.Max(x => x.Length);
                
                /* Get Jobs */

                Parallel.ForEach(skills, skill => GetBySkill(oDesk, "jobs", "dbo.Jobs", skill, 
                    "https://www.odesk.com/api/profiles/v2/search/jobs.json?skills=<skills>&paging={0};{1}".Replace("<skills>", HttpUtility.UrlEncode(skill)), errors,
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

                /* Get Frelancers (Contractors) */

                Parallel.ForEach(skills, skill => GetBySkill(oDesk, "providers", "dbo.Contractors", skill,
                    "https://www.odesk.com/api/profiles/v2/search/providers.json?skills=<skills>&is_odesk_ready=1&paging={0};{1}".Replace("<skills>", HttpUtility.UrlEncode(skill)), errors,
                    contractor => new Contractor
                    {
                        Id = Guid.NewGuid(),
                        OdeskId = contractor.GetValue("id").ToStringWithoutQuotes(),
                        Rate = contractor.GetValue("rate").ToDouble(),
                        Feedback = contractor.GetValue("feedback").ToDouble(),
                        Country = contractor.GetValue("country").ToStringWithoutQuotes(),
                        LastActivity = contractor.GetValue("last_activity").ToDateTime(),
                        MemberSince = contractor.GetValue("member_since").ToDateTime(),
                        PortfolioItemsCount = contractor.GetValue("portfolio_items_count").ToInt32(),
                        TestPassedCount = contractor.GetValue("test_passed_count").ToInt32(),
                        ProfileType = contractor.GetValue("profile_type").ToStringWithoutQuotes(),
                        Skill = skill
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
                var endDate = DateTime.Now;
                Console.WriteLine("Total elapsed time: {0} h {1} min {2} sec", (endDate - startDate).TotalHours,
                    (endDate - startDate).Minutes, (endDate - startDate).Seconds);
                Console.ReadLine();
            }
        }

        private static IEnumerable<string> GetSkills(IoDesk oDesk)
        {
            var skillsSearchcall = oDesk.RestOperations.GetForObjectAsync<JsonValue>("/api/profiles/v1/metadata/skills.json");

            return skillsSearchcall.Result.GetValues("skills").Select(x => x.ToStringWithoutQuotes());
        }

        private static void GetBySkill<T>(IoDesk oDesk, string apiItemsName, string tableName, string skill, string url, List<string> errors, Func<JsonValue, T> convert)
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

                        Console.WriteLine("{0,5} +100 / {1,5}: {2}", offset, pagination.GetValue("total"),skill);

                        offset += itemsPerPage;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{0,5} +100 / {1,5}: {2} ({3})", offset, "fail", skill, ex.Message);
                    }

                } while (numberOfReturnedJobs == itemsPerPage);

                var jobsDataTable = list.ConvertToDatatable();

                if (jobsDataTable.Rows.Count > 0)
                {
                    using (var dbConnection = new SqlConnection(Settings.Default.ConnectionString))
                    {
                        dbConnection.Open();

                        using (
                            var s = new SqlBulkCopy(dbConnection)
                            {
                                DestinationTableName = tableName
                            })
                        {
                            s.WriteToServer(jobsDataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var error = string.Format("Keyword '{0}' failed. {1}", skill, ex.Message);
                Console.WriteLine(error);
                errors.Add(error);
            }
        }

        
    }

}
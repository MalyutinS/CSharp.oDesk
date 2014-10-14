using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CSharp.oDesk.Api;
using CSharp.oDesk.Connect;
using CSharp.oDesk.JobsSearch.Helpers;
using CSharp.oDesk.JobsSearch.Properties;
using Spring.Json;
using Spring.Social.OAuth1;

namespace CSharp.oDesk.JobsSearch
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

                var oDesk = oDeskServiceProvider.GetApi(Settings.Default.ODeskAccessTokenValue, Settings.Default.ODeskAccessTokenSecret);

                /* Get Jobs */

                Parallel.ForEach(GetSkills(), skill =>
                {
                    try
                    {
                        int numberOfReturnedJobs;
                        var offset = 0;
                        const int jobsPerPage = 100;

                        do
                        {
                            var jobsSearchcall =
                                oDesk.RestOperations.GetForObjectAsync<JsonValue>(
                                    string.Format(
                                        "https://www.odesk.com/api/profiles/v2/search/jobs.json?skills={0}&paging={1};{2}",
                                        skill.Keyword, offset, jobsPerPage));

                            offset += jobsPerPage;

                            ICollection<JsonValue> jobs = jobsSearchcall.Result.GetValues("jobs");

                            numberOfReturnedJobs = jobs.Count;
                            
                            var jobsDataTable = jobs.Select(job => new Job
                            {
                                Id = Guid.NewGuid(),
                                OdeskId = job.GetValue("id").ToStringWithoutQuotes(),
                                Title = job.GetValue("title").ToStringWithoutQuotes().Truncate(500),
                                OdeskCategory = job.GetValue("category").ToStringWithoutQuotes(),
                                OdeskSubcategory = job.GetValue("subcategory").ToStringWithoutQuotes(),
                                DateCreated = DateTime.Parse(job.GetValue("date_created").ToStringWithoutQuotes()),
                                Budjet =
                                    !job.GetValue("budget").IsNull ? int.Parse(job.GetValue("budget").ToString()) : 0,
                                ClientCountry =
                                    !job.GetValue("client").GetValue("country").IsNull
                                        ? job.GetValue("client").GetValue("country").ToStringWithoutQuotes()
                                        : string.Empty,
                                SearchCategory = string.Empty,
                                SearchSubCategory = string.Empty,
                                SearchName = skill.Title,
                                SearchKeyword = skill.Keyword
                            }).ToList().ConvertToDatatable();

                            if (jobsDataTable.Rows.Count > 0)
                            {


                                using (var dbConnection = new SqlConnection(Settings.Default.ConnectionString))
                                {
                                    dbConnection.Open();

                                    using (
                                        var s = new SqlBulkCopy(dbConnection)
                                        {
                                            DestinationTableName = "dbo.Jobs"
                                        })
                                    {
                                        s.WriteToServer(jobsDataTable);
                                    }
                                }

                            }

                            var pagination = jobsSearchcall.Result.GetValue("paging");
                            Debug.Assert(numberOfReturnedJobs == int.Parse(pagination.GetValue("count").ToString()));

                            Console.WriteLine("{0,5} +100 / {1,5}: {2}", pagination.GetValue("offset"),
                                pagination.GetValue("total"), skill.Keyword);

                        } while (numberOfReturnedJobs == jobsPerPage);
                    }
                    catch (Exception ex)
                    {
                        var error = string.Format("Keyword '{0}' failed for Jobs. {1}", skill.Keyword, ex.Message);
                        Console.WriteLine(error);
                        errors.Add(error);
                    }
                });

                /* Get Frelancers */

                Parallel.ForEach(GetSkills(), skill =>
                {
                    try
                    {
                        int numberOfReturnedContractors;
                        var offset = 0;
                        const int contractorsPerPage = 100;

                        do
                        {
                            var jobsSearchcall =
                                oDesk.RestOperations.GetForObjectAsync<JsonValue>(
                                    string.Format(
                                        "https://www.odesk.com/api/profiles/v2/search/providers.json?skills={0}&is_odesk_ready=1&paging={1};{2}",
                                        skill.Keyword, offset, contractorsPerPage));

                            offset += contractorsPerPage;

                            var contractors = jobsSearchcall.Result.GetValues("providers");

                            numberOfReturnedContractors = contractors.Count;

                            var jobsDataTable = contractors.Select(contractor => new Contractor
                            {
                                Id = Guid.NewGuid(),
                                OdeskId = contractor.GetValue("id").ToStringWithoutQuotes(),
                                Rate = double.Parse(contractor.GetValue("rate").ToStringWithoutQuotes()),
                                Feedback = double.Parse(contractor.GetValue("feedback").ToStringWithoutQuotes()),
                                Country = contractor.GetValue("country").ToStringWithoutQuotes(),
                                LastActivity = !contractor.GetValue("last_activity").IsNullOrEmpty()
                                    ? DateTime.Parse(contractor.GetValue("last_activity").ToStringWithoutQuotes())
                                    : new DateTime(2000, 1, 1),
                                MemberSince = !contractor.GetValue("member_since").IsNullOrEmpty()
                                    ? DateTime.Parse(contractor.GetValue("member_since").ToStringWithoutQuotes())
                                    : new DateTime(2000, 1, 1),
                                PortfolioItemsCount =
                                    int.Parse(contractor.GetValue("portfolio_items_count").ToStringWithoutQuotes()),
                                TestPassedCount =
                                    int.Parse(contractor.GetValue("test_passed_count").ToStringWithoutQuotes()),
                                ProfileType = contractor.GetValue("profile_type").ToStringWithoutQuotes(),
                                SearchName = skill.Title,
                                SearchKeyword = skill.Keyword
                            }).ToList().ConvertToDatatable();

                            if (jobsDataTable.Rows.Count > 0)
                            {
                                using (var dbConnection = new SqlConnection(Settings.Default.ConnectionString))
                                {
                                    dbConnection.Open();

                                    using (
                                        var s = new SqlBulkCopy(dbConnection)
                                        {
                                            DestinationTableName = "dbo.Contractors"
                                        })
                                    {
                                        s.WriteToServer(jobsDataTable);
                                    }
                                }
                            }

                            var pagination = jobsSearchcall.Result.GetValue("paging");
                            Debug.Assert(numberOfReturnedContractors ==
                                         int.Parse(pagination.GetValue("count").ToString()));

                            Console.WriteLine("{0,5} +100 / {1,5}: {2}", pagination.GetValue("offset"),
                                pagination.GetValue("total"), skill.Keyword);

                        } while (numberOfReturnedContractors == contractorsPerPage);
                    }
                    catch (Exception ex)
                    {
                        var error = string.Format("Keyword '{0}' failed for Contractors. {1}", skill.Keyword, ex.Message);
                        Console.WriteLine(error);
                        errors.Add(error);
                    }
                });


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
                Console.WriteLine("Total elapsed time: {0} h {1} min {2} sec", (endDate - startDate).Hours,
                    (endDate - startDate).Minutes, (endDate - startDate).Seconds);
                Console.ReadLine();
            }
        }

        private static IEnumerable<Skill> GetSkills()
        {
            using (var connection = new SqlConnection(Settings.Default.ConnectionString))
            {
                var cmd = new SqlCommand("SELECT Title,Keyword FROM dbo.Skills WHERE Active = 1 ", connection);

                connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    yield return new Skill
                    {
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Keyword = reader.GetString(reader.GetOrdinal("Keyword")),
                    };
                }
            }
        }
    }

}
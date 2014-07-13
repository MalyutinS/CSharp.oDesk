using System;
using System.Collections.Generic;
using System.Data;
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

                

                /* API */

                var oDesk = oDeskServiceProvider.GetApi(Settings.Default.ODeskAccessTokenValue, Settings.Default.ODeskAccessTokenSecret);


                Parallel.ForEach(GetSearchDetails(), searchDetail =>
                {
                    foreach (var keyword in searchDetail.Keywords)
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
                                            "https://www.odesk.com/api/profiles/v2/search/jobs.json?q={0}&paging={1};{2}",
                                            keyword, offset, jobsPerPage));

                                offset += jobsPerPage;

                                var jobs = jobsSearchcall.Result.GetValues("jobs");

                                numberOfReturnedJobs = jobs.Count;

                                DataTable jobsDataTable = jobs.Select(job => new Job
                                {
                                    Id = Guid.NewGuid(),
                                    OdeskId = job.GetValue("id").ToStringWithoutQuotes(),
                                    Title = job.GetValue("title").ToStringWithoutQuotes(),
                                    OdeskCategory = job.GetValue("category").ToStringWithoutQuotes(),
                                    OdeskSubcategory = job.GetValue("subcategory").ToStringWithoutQuotes(),
                                    DateCreated = DateTime.Parse(job.GetValue("date_created").ToStringWithoutQuotes()),
                                    Budjet = !job.GetValue("budget").IsNull
                                        ? int.Parse(job.GetValue("budget").ToString())
                                        : 0,
                                    ClientCountry = !job.GetValue("client").GetValue("country").IsNull
                                        ? job.GetValue("client").GetValue("country").ToStringWithoutQuotes()
                                        : string.Empty,
                                    SearchCategory = searchDetail.Category,
                                    SearchSubCategory = searchDetail.SubCategory,
                                    SearchName = searchDetail.Name,
                                    SearchKeyword = keyword
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

                                Console.WriteLine("{0}: {1} +100 / {2}", keyword, pagination.GetValue("offset"),
                                    pagination.GetValue("total"));

                            } while (numberOfReturnedJobs == jobsPerPage);
                        }
                        catch (Exception ex)
                        {
                            var tmp = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Keyword '{0}' failed. {1}", keyword, ex.Message);
                            Console.ForegroundColor = tmp;
                        }
                    }
                });

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

        private static IEnumerable<SearchDetail> GetSearchDetails()
        {
            using (var connection = new SqlConnection(Settings.Default.ConnectionString))
            {
                var cmd = new SqlCommand("SELECT Category,SubCategory,Name,Keywords FROM dbo.SearchDetails",connection);

                connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    yield return new SearchDetail
                    {
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        SubCategory = reader.GetString(reader.GetOrdinal("SubCategory")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Keywords = reader.GetString(reader.GetOrdinal("Keywords")).Split(new[] {','}),
                    };
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using HtmlAgilityPack;
using MattchedIT.Common.Data;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // clear database
            //DbInterface.ExecuteQueryNoReturn("votes", "TRUNCATE TABLE dbo.VotingHistory");
            //DbInterface.ExecuteQueryNoReturn("votes", "TRUNCATE TABLE dbo.Vote");
            //DbInterface.ExecuteQueryNoReturn("votes", "TRUNCATE TABLE dbo.Member");
            //DbInterface.ExecuteQueryNoReturn("votes", "TRUNCATE TABLE dbo.VoteState");

            /*
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2004");
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2005");
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2006");
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2007");
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2008");
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2009");
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2010");
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2011");
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2012");
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2013");
             */
            LoadVote("http://www.statesassembly.gov.je/Pages/Votes.aspx?showAll=1&Year=2014");

            Console.Out.WriteLine("Press any key to exit");
            // wait for a keypress
            Console.ReadKey();
        }

        public static void LoadVote(string url)
        {
            // now start loading

            // download page
            Console.Out.WriteLine("Reading: " + url);
            string data = new System.Net.WebClient().DownloadString(url);
            //Console.Out.WriteLine(data);

            // parse it
            Console.Out.WriteLine("Parsing...");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            // now find each node which contains a vote link
            Console.Out.WriteLine("Extracting items...");
            var root = doc.DocumentNode;
            var nodes = root.SelectNodes("//div[contains(@class, 'item')]");
            
            //XPathNavigator nav = doc.CreateNavigator();
            //XPathNodeIterator iterate = nav.Select("//div.item");

            Console.Out.WriteLine("Iterating items...");
            //while (iterate.MoveNext())
            foreach(var node in nodes)
            {
                // get details
                var details = node.SelectNodes(".//div[contains(@class, 'details')]");

                if (details.Count >= 3)
                {
                    // get link and title
                    var link = details[0].SelectSingleNode(".//a");
                    string voteUrl = link.GetAttributeValue("href", string.Empty).Trim();
                    string title = link.InnerText.Trim();

                    // date and reference
                    string date = details[1].InnerText.Trim();
                    string reference = details[2].InnerText.Trim();

                    // status
                    string status = details.Count == 4 ? details[3].InnerText.Trim() : string.Empty;

                    Console.Out.WriteLine(title);

                    // don't insert duplicates
                    //bool exists = DbInterface.ExecuteQueryScalar<bool>("votes", string.Format("SELECT COUNT(*) FROM dbo.Vote WHERE Reference = '{0}' and name='{1}'", reference));

                    //if (exists)
                    //{
                     //   Console.Out.WriteLine("ALREADY EXISTS, SKIPPING...");
                     //   continue;
                    //}

                    // Create header in db
                    DbParameter[] parameters = new DbParameter[]
                    {
                        new DbParameter("@Title", DbType.String, 500, title),
                        new DbParameter("@Date", DbType.DateTime, DateTime.Parse(date)),
                        new DbParameter("@Reference", DbType.String, 50, reference),
                        new DbParameter("@Status", DbType.String, 150, status),
                        new DbParameter("@VoteId", DbType.Int32, ParameterDirection.Output, 0)
                    };

                    DbInterface.ExecuteProcedureNoReturn("votes", "dbo.Vote_Insert", parameters);

                    int voteId = Convert.ToInt32(parameters[parameters.Length-1].Value);

                    // get the actual details and create detail rows
                    GetVoteDetails(voteId, voteUrl);
                }
                /*
                 * <div class="item">
                        <div class="row">
                            <div class="title">
                                Title</div>
                            <div class="details">
                                <a href="/Pages/Votes.aspx?VotingId=3127">
                                    Composition and Election of the States Assembly: reform - proposal 1 (P.93/2013) - third amendment
                                </a>
                            </div>
                        </div>
                        <div class="row">
                            <div class="header">
                                Vote date:</div>
                            <div class="details">
                                21/01/2014</div>
                        </div>
                        <div class="row">
                            <div class="header">
                                Reference:</div>
                            <div class="details">
                                P.93/2013(Amd)(3)
                            </div>
                        </div>
                        
                        
                    </div>
                 */
            }
        }

        private static void GetVoteDetails(int voteId, string url)
        {
            // fix urls
            if (url.StartsWith("/"))
            {
                url = "http://www.statesassembly.gov.je" + url;
            }

            // download page
            Console.Out.WriteLine("Reading from:" + url);
            string data = new System.Net.WebClient().DownloadString(url);

            // parse it
            Console.Out.WriteLine("Parsing...");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            // now find each node which contains a vote link
            Console.Out.WriteLine("Extracting rows...");
            var root = doc.DocumentNode;
            var nodes = root.SelectNodes("//div[contains(@class, 'row')]");

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node.ChildNodes.Count == 5)
                    {
                        var positionNode = node.SelectSingleNode(".//div[contains(@class, 'position')]");
                        string position = positionNode != null ? positionNode.InnerText.Trim() : string.Empty;

                        var nameNode = node.SelectSingleNode(".//a");
                        var votedNode = node.SelectSingleNode(".//div[contains(@class, 'right')]");
                        
                        if (nameNode == null || votedNode == null)
                        {
                            // this seemed to happen on a vote which had a duff id and redirected to a main list
                            Console.Out.WriteLine("BAD LINK - skipping");
                            continue;
                        }

                        string name = nameNode.InnerText.Trim();
                        string voted = votedNode.InnerText.Trim();

                        // insert data
                        DbParameter[] parameters = new DbParameter[]
                        {
                            new DbParameter("@VoteId", DbType.Int32, voteId),
                            new DbParameter("@Position", DbType.String, 250, position),
                            new DbParameter("@Name", DbType.String, 250, name),
                            new DbParameter("@Voted", DbType.String, 250, voted)
                        };

                        DbInterface.ExecuteProcedureNoReturn("votes", "dbo.VotingHistory_Insert", parameters);

                        //Console.Out.WriteLine(name);
                    }
                    else
                    {
                        Console.Out.WriteLine("CHILDNODES != 5");
                    }
                }
            }
        }

        /*
         * <div class="row">
                        <div class="left">
                            <div class="position">
                                Senator
                            </div>
                            <div class="member">
                                <a href="/Pages/Members.aspx?MemberId=66">
                                    Sarah Craig Ferguson
                                </a>
                            </div>
                        </div>
                        <div class="right">
                            Pour
                        </div>
                    </div>
         */
    }
}

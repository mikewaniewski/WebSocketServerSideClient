using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using Newtonsoft.Json.Linq;



namespace WebSocketServerSideClient
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var ws = new WebSocket("ws://websocket.externalServer.com"))
            {
                ws.OnMessage += async (sender, e) 
                        => await SaveDataAsync(e.Data);
                //Lines above delegate each Message received from ws
                //to be processed by SaveDataAsync method asynchronously
                //on a separate Thread, to allow finishing all operations, even
                //if before the finish, next Message arrives.

                ws.Connect();

                while (true)
                {
                    Thread.Sleep(15000);
                }

            }
        }

        private static async Task SaveDataAsync(object JSONdata)
        {


            await Task.Run(() =>
            {
                using (DataClasses1DataContext dbc = new DataClasses1DataContext())
                {
                    var jo = JObject.Parse(JSONdata.ToString());
                    var PublicationDate = jo["PublicationDate"].ToString();
                    DateTime _PublicationDate = Convert.ToDateTime(PublicationDate);

                  

                    int count = jo["Items"].Count();

                    for (int i = 0; i < count; i++)
                    {
                        string Code = jo["Items"][i]["Code"].ToString();
                        string Price = jo["Items"][i]["Price"].ToString();
                        double _Price = 0;


                        _Price = double.Parse(Price.Replace(",", "."), CultureInfo.InvariantCulture);

                        StockPriceHistory newHistItem = new StockPriceHistory
                        {

                            PublicationDate = _PublicationDate,
                            Price = _Price,
                            StockCode = Code

                        };

                        dbc.StockPriceHistories.InsertOnSubmit(newHistItem);

                    }
                    dbc.SubmitChanges();
                


                }
            });



        }
    }
}

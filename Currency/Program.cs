using System;
using System.Net;
using System.Text;
using System.Xml;

namespace Currency
{
    class Program
    {
        static private void Start()
        {
            string valute = "GBP";
            string today = GetDate();
            using (DataBase dataBase = new DataBase())
            {
                dataBase.Dispose();
                Parse parse = new Parse(GetData(GetDate()));
                Console.WriteLine(dataBase.Each(GetDate(), valute));

                while (today == GetDate())
                {
                    if (today != GetDate())
                    {
                        today = GetDate();
                        parse = new Parse(GetData(GetDate()));
                        Console.WriteLine(dataBase.Each(GetDate(), valute));
                    }
                }
            }
        }

        static void Main(string[] args)
        {

            Start();
        }
        private static string GetData(string date)
        {
            string url = $"http://www.cbr.ru/scripts/XML_daily.asp?date_req={date}";
            using (var webclient = new WebClient())
            {
                var response = webclient.DownloadString(url);
                return response;
            }
        }

        private static string GetDate()
        {
            string _date = DateTime.Today.ToString("dd.MM.yyyy");
            return _date;
        }
    }

    class Parse
    {
        private string xml;

        public Parse(string xml)
        {
            this.xml = xml;
            Handler();
        }

        private void Handler()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlElement date = doc.DocumentElement;
            using (DataBase dataBase = new DataBase())
            {
                foreach (XmlNode valute in date)
                {
                    dataBase.Create(
                        valute.SelectSingleNode("./Value").InnerText,
                        valute.SelectSingleNode("./CharCode").InnerText,
                        date.Attributes["Date"].Value
                        );
                }
            }

        }
    }
}

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
            DataBase dataBase = new DataBase();
            //dataBase.Insert("9.08", GetDate(), valute);
            dataBase.Each(GetDate(), valute);
            //Parse parse = new Parse(GetData(GetDate()));
            //Console.WriteLine(dataBase.Select(GetDate(), valute));
            /*
            while (today == GetDate())
            {
                if (today != GetDate())
                {
                    today = GetDate();
                    parse = new Parse(GetData(GetDate()));
                    Console.WriteLine(dataBase.Select(GetDate(), valute));
                }
            }
            */
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
            DateTime _date = DateTime.Today;
            StringBuilder date = new StringBuilder();
            if (_date.Day.ToString().Length == 1)
            {
                date.Append("0");
            }
            date.Append(_date.Day.ToString());
            date.Append(".");
            if (_date.Month.ToString().Length == 1)
            {
                date.Append("0");
            }
            date.Append(_date.Month.ToString());
            date.Append(".");
            date.Append(_date.Year.ToString());
            return date.ToString();
        }
    }

    class Parse
    {
        private string xml;

        public Parse(string xml)
        {
            this.xml = xml;
            //Handler();
        }

        private void Handler()
        {
            /*
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlElement date = doc.DocumentElement;
            DataBase dataBase = new DataBase();
            foreach (XmlNode valute in date)
            {
                dataBase.Insert(
                    valute.SelectSingleNode("./Value").InnerText,
                    valute.SelectSingleNode("./CharCode").InnerText,
                    date.Attributes["Date"].Value
                    );
            }
            */
        }
    }
}

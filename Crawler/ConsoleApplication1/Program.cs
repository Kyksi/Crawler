using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using HtmlAgilityPack;
using System.Web;

namespace ConsoleApplication1
{
    class Program
    {
        public List<String> lista;
        public List<String> lista2;
        static void Main(string[] args)
        {
            
            Program p = new Program();
            p.lista2 = new List<string>();
            p.step1_2013();
            p.getholidays_2013();
            p.SaveText2DataBase_2013();

        }

        #region  void step1_2013()
        private  void step1_2013()
        {
            string url = "http://www.qppstudio.net/publicholidays.htm";
            
            HtmlAgilityPack.HtmlDocument document = new HtmlDocument();
            StreamReader reader = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream(), Encoding.Default);            
            document.Load(reader);

            HtmlNode someNode = document.GetElementbyId("AutoNumber4");

            HtmlNodeCollection allLinks1 = someNode.SelectNodes("tr[2]/td[1]/div/span/font/font/a");
            HtmlNodeCollection allLinks2 = someNode.SelectNodes("tr[2]/td[2]/div/font/font/a");
            HtmlNodeCollection allLinks3 = someNode.SelectNodes("tr[2]/td[3]/div/font/font/a");
            HtmlNodeCollection allLinks4 = someNode.SelectNodes("tr[2]/td[4]/div/span/font/font/a");

            lista = new List<string>();
            foreach (HtmlNode n in allLinks1)
            {
                HtmlAttribute att = n.Attributes["href"];
                string country = n.InnerText;
                string newurl = "http://www.qppstudio.net/" + att.Value.Replace("publicholidays2012", "publicholidays2013");
                lista.Add(newurl);
            }
            
        }
        #endregion

        #region  void getholidays_2013()
        private  void getholidays_2013()
        {           
            int i = 0;
            foreach (string u in lista)
            {                
                string url = u;              
                SaveHtmlFiles(u, i.ToString());
                Console.WriteLine(u);
                i++;
            }
        }
        #endregion

        #region static void SaveHtmlFiles(string url, string country)
        public static void SaveHtmlFiles(string url, string country)
        {
            try
            {
                HtmlAgilityPack.HtmlDocument document = new HtmlDocument();
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; AOL 9.5; AOLBuild 4337.34; Windows NT 6.0; WOW64; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; .NET CLR 3.5.30729; .NET CLR 3.0.30618)";
                request.UserAgent = @"Mozilla/5.0 (iPad; U; CPU OS 3_2_1 like Mac OS X; en-us) AppleWebKit/531.21.10 (KHTML, like Gecko) Mobile/7B405";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);             
                string filename = @"c:\strony\" + country + ".html";
                File.WriteAllText(filename, reader.ReadToEnd(), Encoding.Default);
            }
            catch (Exception ee)
            {
                
                
            }
        }
        #endregion

        #region  void SaveText2DataBase_2013()
        public  void SaveText2DataBase_2013()
        {
            string path = @"c:\strony\";
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fi = di.GetFiles();
            int i=0;
            foreach (FileInfo f in fi)
            {              
                    processSingleFile(f.FullName, i.ToString(), i);
                    i++;
            }

        }
        #endregion

        #region  void processSingleFile(string fullpath, string countryName,int fk_country)
        private  void processSingleFile(string fullpath, string countryName, int fk_country)
        {                  
            HtmlAgilityPack.HtmlDocument document = new HtmlDocument();
            String startText = "<script type=\"text/javascript\"> document.write(obCkDate(";
            String endText = "</script>";
            int startIdx;
            int endIdx;

            FileStream fs = new FileStream(fullpath, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            String s = sr.ReadToEnd();
            while (s.IndexOf(startText) > 0)
            {
                startIdx = s.IndexOf(startText);
                endIdx = s.IndexOf(endText, startIdx);
                int l = endIdx + endText.Length - startIdx;
                s = s.Remove(startIdx, l);
                s = s.Insert(startIdx, "<tr>");

            }
            fs.Close();
            File.WriteAllText(fullpath, s, Encoding.Default);
            document.Load(fullpath);

            HtmlNodeCollection tables = document.DocumentNode.SelectNodes("//table");
            //HtmlNodeCollection tables = document.DocumentNode.SelectNodes("html/body/table[2]/tr/td[3]/table/tr[5]/td/table");

            int i = 0;

            foreach (HtmlNode table in tables)
            {

                if (table.InnerText.Contains("Date in 2013"))               
                {                  
                    i++;
                    if (i == 3)
                    {                       
                        int prevmonth = -1;
                        foreach (HtmlNode obj in table.ChildNodes)
                        {
                            if (obj.Name == "tr")
                            {
                                string value1 = "";
                                string value2 = "";
                                string value3 = "";
                                string value4 = "";

                                int day = -1;
                                int month = -1;
                                int year = 2012;
                                HtmlNode td1 = obj.SelectSingleNode("td[1]");
                                HtmlNode td2 = obj.SelectSingleNode("td[2]");
                                HtmlNode td3 = obj.SelectSingleNode("td[3]");
                                HtmlNode td4 = obj.SelectSingleNode("td[4]");


                                value1 = Clear(td1.InnerText);
                                value1 = value1.Trim().Replace("*", "");
                                value1 = System.Web.HttpUtility.UrlDecode(value1);
                                
                                int c1 = value1.LastIndexOf("<!--HPSTART-->");
                                int c2 = value1.LastIndexOf("<!--HPEND-->");

                                if (c1 > 0 && c2 > 0)
                                {
                                    value1 = value1.Substring(c1 + 14, c2 - c1 - 14);

                                    if (value1.ToLower().Contains("jan.")) month = 1;
                                    if (value1.ToLower().Contains("feb.")) month = 2;
                                    if (value1.ToLower().Contains("mar.")) month = 3;
                                    if (value1.ToLower().Contains("apr.")) month = 4;
                                    if (value1.ToLower().Contains("may")) month = 5;
                                    if (value1.ToLower().Contains("jun.")) month = 6;
                                    if (value1.ToLower().Contains("jul.")) month = 7;
                                    if (value1.ToLower().Contains("aug.")) month = 8;
                                    if (value1.ToLower().Contains("sep.")) month = 9;
                                    if (value1.ToLower().Contains("oct.")) month = 10;
                                    if (value1.ToLower().Contains("nov.")) month = 11;
                                    if (value1.ToLower().Contains("dec.")) month = 12;

                                    int aa = value1.LastIndexOf(';');
                                    if (aa > 0)
                                    {
                                        string tmp = value1.Substring(aa + 1, value1.Length - aa - 1);
                                        tmp = tmp.Replace("</font>", "");
                                        if (int.TryParse(tmp, out day))
                                        {

                                        }
                                    }
                                }
                                value2 = Clear(td2.InnerText);
                                value3 = Clear(td3.InnerText);//nazwa święta

                                if (td4 != null)
                                {
                                    value4 = Clear(td4.InnerText);
                                }
                                if (month > 0)
                                    prevmonth = month;
                                if (day > 0 && month < 0)
                                    month = prevmonth;
                                if (day > 0 && month > 0)
                                {
                                    
                                    //tb_holiday tbholiday = new tb_holiday();

                                    
                                    //tbholiday.countryName = countryName;
                                    //tbholiday.fk_country = fk_country;
                                    DateTime holiday = new DateTime(2013, month, day);
                                    string weekday = value2;
                                    string holidayname = value3;
                                    //tbholiday.dateTime = holiday;
                                    string observence = value4;
                                    //tbholiday.name = value3;
                                    //tbholiday.observence = observence;
                                    //db.tb_holidays.InsertOnSubmit(tbholiday);
                                    //db.SubmitChanges();
                                    lista2.Add(countryName + value3 + observence+holiday.ToString());
                                }


                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Clear
        private static string Clear(string a)
        {
            return a.Replace("\n", "").Trim();
        }
        #endregion
    }
}

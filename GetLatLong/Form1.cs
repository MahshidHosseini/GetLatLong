using GetLatLong.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using static GetLatLong.Models.GeoCodeResponse;

namespace GetLatLong
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        String _filePath = @"D:\Log\LatLongLog\LatLong.txt";
        private void btnGetLatLong_Click(object sender, EventArgs e)
        {
            
            int from = int.Parse(txtFrom.Text);
            int to = int.Parse(txtTo.Text);

            MessageBox.Show("Start");

            LatLongEntities1 context = new LatLongEntities1();

            foreach (var result1 in context.Accommodation2.Where(p => p.AccommodationlID >= from && p.AccommodationlID <= to))
            {
                using (LatLongEntities1 db = new LatLongEntities1())
                {
                    try
                    {


                        long hotelId = result1.AccommodationlID;

                        Accommodation2 hotel = db.Accommodation2.SingleOrDefault(p => p.AccommodationlID == hotelId);
                        WebClient web = new WebClient();
                        //if (hotel.Address.Trim() != string.Empty)
                        //{
                        //1)string address = hotel.Name+ IsNull(hotel.Address) + IsNull(hotel.Country) + IsNull(hotel.CityName);
                        //2)string address =hotel.Address+ IsNull(hotel.Country) + IsNull(hotel.CityName);
                        //3)
                        string temp = "";
                        if (hotel.Address != null)
                            temp = hotel.Address.Trim();
                        else
                            temp = hotel.Name;
                        string address = temp + IsNull(hotel.LocationNameLong);

                            string url = $"http://maps.google.com/maps/api/geocode/xml?address={address} & Sensore=true";
                            string res = web.DownloadString(url);

                            if (res.Contains("<status>ZERO_RESULTS</status>"))
                            {
                                string result = $"AccommodationlID: {hotel.AccommodationlID} With address: {url} = Zero Result";
                                File.AppendAllLines(_filePath, new string[] { result });
                                continue;
                            }
                            var geoCode = ConvertXMToObject<GeocodeResponse>(res);

                            hotel.Lat = geoCode.result.geometry.location.lat.ToString();
                            hotel.Lng = geoCode.result.geometry.location.lng.ToString();

                            db.SaveChanges();
                        }
               

                    //}
                    catch (Exception ex)
                    {
                        try
                        {


                            long hotelId = result1.AccommodationlID;
                            Accommodation2 accomodation = context.Accommodation2.SingleOrDefault(p => p.AccommodationlID == hotelId);
                            string lineError = ex.LineNumber().ToString();
                            string message = ex.InnerException == null ? ex.Message : ex.InnerException.ToString();
                            InsertLog(result1.AccommodationlID, "Error", lineError, message);
                            db.SaveChanges();


                        }
                        catch (Exception)
                        {
                            //test

                        }

                    }

                }
            }

            MessageBox.Show("end");
          

        }
        public void InsertLog(long HotelID, string Exception, string line, string ErrorText)
        {
            try
            {
                var context = new LatLongEntities1();
                logHotel log = new logHotel();
                log.HotelID = HotelID;
                log.exeption = Exception;
                log.line = line;
                log.line1 = ErrorText;
                context.logHotel.Add(log);
                context.SaveChanges();
            }
            catch (Exception ex)
            {

            }

        }
        private GeocodeResponse ConvertXMToObject<T>(string xml)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            using (StringReader rdr = new StringReader(xml))
            {
                var xmlResult = (GeocodeResponse)xsSubmit.Deserialize(rdr);
                return xmlResult;
            }
        }


        private string IsNull(string input)
        {
            string res;
            if (input == string.Empty)
                res = " ";
            else
                res = ","+input;
            return res;
            
        }
    }

}
public static class ExceptionHelper
{
    public static int LineNumber(this Exception e)
    {

        int linenum = 0;
        try
        {
            //linenum = Convert.ToInt32(e.StackTrace.Substring(e.StackTrace.LastIndexOf(":line") + 5));

            //For Localized Visual Studio ... In other languages stack trace  doesn't end with ":Line 12"
            linenum = Convert.ToInt32(e.StackTrace.Substring(e.StackTrace.LastIndexOf(' ')));

        }
        catch
        {
            //Stack trace is not available!
        }
        return linenum;
    }
}


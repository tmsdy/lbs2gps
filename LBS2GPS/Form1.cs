using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Utilities.Lib;

namespace LBS2GPS
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
 

        public class CellServiceEntity
        {
            // Methods
         //   public void  CellServiceEntity();

            // Properties
            public string address { get; set; }
            public string lac { get; set; }
            public string cell { get; set; }
            public int distance { get; set; }
            public string lat { get; set; }
            public string lng { get; set; }
            public string QQlat { get; set; }
            public string QQlng { get; set; }
            public int mcc { get; set; }
            public int mnc { get; set; }
        }
 
        public class Coords
        {
            // Methods
            //public Coords();

            // Properties
            public decimal lng { get; set; }
            public decimal lat { get; set; }
        }

        public class MGCoords
        {
            public int error { get; set; }
            public CellServiceEntity result { get; set; }

            public string reason { get; set; }
        }

        public class QQCoords
        {
            // Methods
            //public QQCoords();

            // Properties
            public List<Coords> locations { get; set; }
            public int status { get; set; }
            public string message { get; set; }
        }

 

        public string sQQkey = "D5CBZ-42MRW-GKERA-RZI4B-VX2W2-3DBOE";
        private delegate void SetTextHandler(string text);

        private delegate void SetURLHandler(string url);

        private void SetURL(string url)
        { 
            if (webBrowser1.InvokeRequired == true)
            {
                SetURLHandler set = new SetURLHandler(SetText);//委托的方法参数应和SetText一致
                webBrowser1.Invoke(set, new object[] { url }); //此方法第二参数用于传入方法,代替形参text
            }
            else
            {
                webBrowser1.Navigate(url);
            }
        }
        private void SetText(string text)
        {

            if (richTextBox1.InvokeRequired == true)
            {
                SetTextHandler set = new SetTextHandler(SetText);//委托的方法参数应和SetText一致
                richTextBox1.Invoke(set, new object[] { text }); //此方法第二参数用于传入方法,代替形参text
            }
            else
            { 
                richTextBox1.AppendText(text);
            }
        }



        public  string[] GetQQCoordEx(string lng, string lat, int from)
        {

            string requestUriString = string.Format("http://apis.map.qq.com/ws/coord/v1/translate?locations={1},{0}&type={2}&key={3}", new object[] { lng, lat, from, this.sQQkey });
            string[] strArray = new string[2];
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
                request.Timeout = 0x5dc;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string str2 = string.Empty;
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    str2 = reader.ReadToEnd();
                    reader.Close();
                }
                QQCoords coords = JsonConvert.DeserializeObject<QQCoords>(str2);
                if (coords.status == 0)
                {
                    strArray[0] = coords.locations[0].lng.ToString();
                    strArray[1] = coords.locations[0].lat.ToString();
                }
            }
            catch
            {
                strArray[0] = lng;
                strArray[1] = lat;
            }
            return strArray;
        }

 
        private void httpget_lbsMG()
        {
            string sRequestUrl = "http://open.u12580.com/api/v1/Cell?mcc=0460&mnc=0&key=1&type=0&lac=" + textBox4.Text + "&cid=" + textBox5.Text ;

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(sRequestUrl);
            httpRequest.Timeout = 10000;
            httpRequest.Method = "GET";
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            StreamReader sr = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("UTF-8"));
            string http_result = sr.ReadToEnd();
            http_result = http_result.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            //SetText(sRequestUrl + "\n");
            int status = (int)httpResponse.StatusCode;
            sr.Close();
  //MG接口 {"error":0,"reason":"OK","result":{"lac":"9346","cell":"4713","lng":"113.860915","lat":"22.571788","distance":0,"address":"","mcc":460,"mnc":0}}
            MGCoords coords = JsonConvert.DeserializeObject<MGCoords>(http_result);
            if (coords.error == 0)
            {
                //转换格式
                CellServiceEntity entity = new CellServiceEntity();

                string[] strArray = GetQQCoordEx(coords.result.lng, coords.result.lat, 1);
                coords.result.QQlat = strArray[1];
                coords.result.QQlng = strArray[0];
                entity = coords.result;


                SetText("MG接口：" + entity.lat + "," + entity.lng + "\n");

                textBox6.Text = entity.lat + "," + entity.lng;

                //string url = string.Format("http://apis.map.qq.com/ws/staticmap/v2/?center={0},{1}&zoom=15&size=400*300&maptype=roadmap&markers=label:{3}|{0},{1}&key={2}", new object[] { entity.lat, entity.lng, this.sQQkey,'0' });

            }
            
            else 
            {
                //	查不到就查联通的  9529 21405 是联通的
                 sRequestUrl = "http://open.u12580.com/api/v1/Cell?mcc=0460&mnc=1&key=1&type=0&lac=" + textBox4.Text + "&cid=" + textBox5.Text;

                 httpRequest = (HttpWebRequest)WebRequest.Create(sRequestUrl);
                 httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                 sr = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("UTF-8"));
                 http_result = sr.ReadToEnd();
                http_result = http_result.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            
                 status = (int)httpResponse.StatusCode;
                sr.Close();
               
                 coords = JsonConvert.DeserializeObject<MGCoords>(http_result);
                 if (coords.error == 0)
                 {
                     //转换格式
                     CellServiceEntity entity = new CellServiceEntity();

                     string[] strArray = GetQQCoordEx(coords.result.lng, coords.result.lat, 1);
                     coords.result.QQlat = strArray[1];
                     coords.result.QQlng = strArray[0];
                     entity = coords.result;


                     SetText("MG接口联通：" + entity.lat + "," + entity.lng + "\n");

                     textBox6.Text = entity.lat + "," + entity.lng;
                 }
                 else
                 {
                     textBox6.Text = "查询失败";
                 }
            }


        }
        private void httpget_lbsCellId()
        {
            //http://www.cellid.cn/cidInfo.php?hex=false&lac=34860&cell_id=62041
            string sRequestUrl = "http://www.cellid.cn/cidInfo.php?hex=false&lac=" + textBox4.Text + "&cell_id=" + textBox5.Text;
            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(sRequestUrl);
                httpRequest.Timeout = 50000;
                httpRequest.Method = "GET";
                //如果服务器返回错误，他还会继续再去请求，不会使用之前的错误数据，做返回数据
                httpRequest.ServicePoint.Expect100Continue = false;
                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("UTF-8"));
                string result = sr.ReadToEnd();
                result = result.Replace("\r", "").Replace("\n", "").Replace("\t", "");
                //SetText(sRequestUrl + "\n");
                //SetText("CellId接口：" + result + "\n");
                int status = (int)httpResponse.StatusCode;
                sr.Close();

//CellId接口：cidMap(23.03334187,113.34561136,'(9441,60595)23.03334187,113.34561136<br>广东省广州市番禺区沙溪大道')

                //转换格式
                CellServiceEntity entity = new CellServiceEntity();
                if (result.Contains("cidMap"))
                {
                    entity.cell = textBox5.Text;
                    entity.lac = textBox4.Text;
                    result = result.Substring(7, result.Length - 9);
                    entity.QQlat = result.Split(new char[] { ',' })[0];

                    entity.QQlng = result.Split(new char[] { ',' })[1];
                    string[] strArray = GetQQCoordEx(entity.QQlng, entity.QQlat, 1);

                    entity.lng = ((Checkout.GetDouble(entity.QQlng)) * 2.0 - (Checkout.GetDouble(strArray[0]))).ToString();
                    entity.lat = ((Checkout.GetDouble(entity.QQlat)) * 2.0 - (Checkout.GetDouble(strArray[1]))).ToString();
                    entity.address = result.Split(new char[] { '>' })[1];


                    SetText("CellId接口：" + entity.lat + "," + entity.lng + "\n");

                    textBox7.Text = entity.lat + "," + entity.lng; 
                }
                else
                {
                    textBox7.Text = "查询失败";
                }
            }
            catch (Exception er)
            {
                SetText("CellId接口：" + er.ToString() + "\n");
                textBox7.Text = "查询失败";
            }

        }


 
   
 
            //CellMap接口：23.0356072931741,113.342499106817,23.033004,113.347942,广东省广州市番禺区ym03,2000 

    
        private void httpget_lbsCellMap()
        {
            //http://www.cellmap.cn/cellmapapi/cellmap_gsm2gps_api.aspx?lac=9723&cell=3871

            string sRequestUrl = "http://www.cellmap.cn/cellmapapi/cellmap_gsm2gps_api.aspx?lac=" + textBox4.Text + "&cell=" + textBox5.Text;

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(sRequestUrl);
            httpRequest.Timeout = 10000;
            httpRequest.Method = "GET";
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            StreamReader sr = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("UTF-8"));
            string result = sr.ReadToEnd();
            result = result.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            //SetText(sRequestUrl + "\n");
            //SetText("CellMap接口：" + result + "\n");
            int status = (int)httpResponse.StatusCode;
            sr.Close();

            CellServiceEntity entity = new CellServiceEntity();


            if (result.Length > 0)
            {
                entity.cell = textBox5.Text;
                entity.lac = textBox4.Text;
                entity.lat = result.Split(new char[] { ',' })[0];
                entity.lng = result.Split(new char[] { ',' })[1];
                entity.QQlat = result.Split(new char[] { ',' })[2];
                entity.QQlng = result.Split(new char[] { ',' })[3];
                entity.address = result.Split(new char[] { ',' })[4];
                entity.distance =Convert.ToInt32(result.Split(new char[] { ',' })[5]);

                SetText("CellMap接口：" + entity.lat + "," + entity.lng + "\n");

                textBox8.Text = entity.lat + "," + entity.lng; 
            }
            else
            {
                textBox8.Text = "查询失败";
            }

        }


        private void httpget_lbsMapbar()
        {
            //[{"mcc":460,"mnc":0,"lac":32971,"cid":25632,"dbm":-66}]
            //http://app.qinmi.co/openapi/v1/Cell
            ASCIIEncoding encoding = new ASCIIEncoding();
            string postData = "[{\"mcc\":460,\"mnc\":0,\"lac\":" + textBox4.Text + ",\"cid\":" + textBox5.Text + ",\"dbm\":-66}]";

            byte[] data = Encoding.UTF8.GetBytes(postData);

            HttpWebRequest myRequest =
            (HttpWebRequest)WebRequest.Create("http://app.qinmi.co/openapi/v1/Cell");
            myRequest.Method = "POST";
            myRequest.ContentType = "application/json";
            myRequest.ContentLength = data.Length;
            Stream newStream = myRequest.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            result = result.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            int status = (int)myResponse.StatusCode;
            reader.Close(); 
            //SetText("Mapbar接口：" + result + "\n");

            //Mapbar接口：{"error":0,"reason":"","result":{"lac":null,"cell":null,"lng":"113.34180465529","lat":"23.034857295622","distance":0,"address":"中国广东省广州市番禺区洛浦街道员岗沙 (113.34726,23.03242)","mcc":0,"mnc":0}}

            MGCoords coords = JsonConvert.DeserializeObject<MGCoords>(result);
            if (coords.error == 0)
            {
                //转换格式
                CellServiceEntity entity = new CellServiceEntity();
                string address_qqlnglat = coords.result.address;
                string[] address_qq = address_qqlnglat.Split('(');
                coords.result.address = address_qq[0];
                coords.result.QQlng = address_qq[1].Split(new char[] { ',' })[0];

                coords.result.QQlat = address_qq[1].Split(new char[] { ',' })[1].Split(new char[] { ')' })[0];
                  
                entity = coords.result;
                textBox9.Text = entity.lat + "," + entity.lng;
                SetText("Mapbar接口：" + entity.lat + "," + entity.lng + "\n");
                string url = string.Format(" http://maps.gpspax.com/showMap.aspx?zoom=16&n={0},{1}", new object[] { entity.lat, entity.lng });
                webBrowser1.Navigate(url);

            }
            else
            {
                textBox9.Text = "查询失败";
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            string sLacCell = textBox1.Text;
            long lLac = 0;
            long lCell = 0;
            sLacCell = sLacCell.Trim();
            if ((sLacCell.Length != 8) && (textBox4.Text.Length == 0))
            {
                richTextBox1.AppendText("请输入正确的LBS信息\n");
                return;
            }
            else if (sLacCell.Length == 8)
            {
                textBox2.Text = sLacCell.Substring(0, 4);
                textBox3.Text = sLacCell.Substring(4, 4);
                lLac = Convert.ToInt64(sLacCell.Substring(0, 4), 16);
                lCell = Convert.ToInt64(sLacCell.Substring(4, 4), 16);

                textBox4.Text = lLac.ToString();
                textBox5.Text = lCell.ToString();
            }
            else
            {
                lLac = Convert.ToInt64(textBox4.Text);
                lCell = Convert.ToInt64(textBox5.Text);

                textBox2.Text = Convert.ToString(lLac,16);
                textBox3.Text = Convert.ToString(lCell, 16);
 
            }
            SetText("***************" + textBox4.Text +","+ textBox5.Text + "*************\n");
            textBox6.Text = "正在查询中……";
            textBox7.Text = "正在查询中……";
            textBox8.Text = "正在查询中……";
            textBox9.Text = "正在查询中……";
            SetText("");
            System.Threading.Thread threadMg = new Thread(new ThreadStart(httpget_lbsMG));
            threadMg.Start();
            System.Threading.Thread threadCellId = new Thread(new ThreadStart(httpget_lbsCellId));
            threadCellId.Start();
            System.Threading.Thread threadCellMap = new Thread(new ThreadStart(httpget_lbsCellMap));
            threadCellMap.Start(); 
            System.Threading.Thread threadMapbar = new Thread(new ThreadStart(httpget_lbsMapbar));
            threadMapbar.Start();

 


        }

        private void button2_Click(object sender, EventArgs e)
        { 
            webBrowser1.Navigate(" http://maps.gpspax.com/showMap.aspx?zoom=16&n=" + textBox6.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(" http://maps.gpspax.com/showMap.aspx?zoom=16&n=" + textBox7.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(" http://maps.gpspax.com/showMap.aspx?zoom=16&n=" + textBox8.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(" http://maps.gpspax.com/showMap.aspx?zoom=16&n=" + textBox9.Text);
        }
    }
}

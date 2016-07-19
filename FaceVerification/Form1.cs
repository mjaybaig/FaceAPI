using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using Newtonsoft.Json.Linq;

namespace FaceVerification
{
    public partial class Form1 : Form
    {
        IFaceServiceClient faceServiceClient;
        PleaseWaitForm pleaseWait;
        public Form1()
        {
            InitializeComponent();
            faceServiceClient = new FaceServiceClient("16306c6a4b53406da6eca3fb9e42dedb");
            pleaseWait = new PleaseWaitForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
           
            this.pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
           
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            
            this.pictureBox2.Image = Image.FromFile(openFileDialog2.FileName);
        }

        private async void btnCompare_Click(object sender, EventArgs e)
        {
            //            Microsoft.ProjectOxford.Face.FaceServiceClient f = new Microsoft.ProjectOxford.Face.FaceServiceClient("16306c6a4b53406da6eca3fb9e42dedb");\
           
            textBox1.Text = "";
            Face[] faces = new Face[2];


            this.UseWaitCursor = true;
            // Display form modelessly
           // pleaseWait.Show();

            //  ALlow main UI thread to properly display please wait form.
            //Application.DoEvents();

            // Show or load the main form.
            //mainForm.ShowDialog();

            faces[0] = await UploadAndDetectFaces(openFileDialog1.FileName);
            faces[1] = await UploadAndDetectFaces(openFileDialog2.FileName);

            //this.textBox1.Text = faces[0].FaceId.ToString();

            VerifyFace(faces);

            

        }

        private async Task<Face> UploadAndDetectFaces(string imageFilePath)
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    //faceServiceClient
                    Face[] faces = await faceServiceClient.DetectAsync(imageFileStream);
                    return faces[0];
                   // var faceRects = faces.Select(face => face.FaceRectangle);
                   // return faceRects.ToArray();
                }
            }
            catch (Exception)
            {
                return new Face();
            }
        }


        async void VerifyFace(Face[] faces)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "16306c6a4b53406da6eca3fb9e42dedb");
            
            var uri = "https://api.projectoxford.ai/face/v1.0/verify?" + queryString;

            HttpResponseMessage response;

            string body = "{\"faceId1\":\""+ faces[0].FaceId + "\",\"faceId2\":\""+ faces[1].FaceId + "\"}";

            //string body = "{\"faceId1\":\"aba4d170-475d-4a6b-83f2-989693d2f9c1\",\"faceId2\":\"aba4d170-475d-4a6b-83f2-989693d2f9c1\"}";


             /*// Request body
             byte[] byteData = Encoding.UTF8.GetBytes(body);


            //Newtonsoft.Json.Linq.JObject obj = new Newtonsoft.Json.Linq.JObject();

            
             using (var content = new ByteArrayContent(byteData))
             {
                 content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                 response = await client.PostAsync(uri, content);
             }*/
            HttpContent theContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
           
            //HttpContent cont = theContent.
            response = await client.PostAsync(uri, theContent);

            String jsonString= await response.Content.ReadAsStringAsync();


            JObject jasonObj = JObject.Parse(jsonString);
            if(jasonObj!=null && jasonObj["isIdentical"].ToString().Equals("True"))
            {
                textBox1.Text = "The images are a match with " + (Decimal.Parse(jasonObj["confidence"].ToString())*100) +"% probability";
            }
            else if (jasonObj != null && jasonObj["isIdentical"].ToString().Equals("False"))
            {
                textBox1.Text = "The images do not match. Probability is " + (Decimal.Parse(jasonObj["confidence"].ToString()) * 100) + "%.";
            }
            else
            {
                textBox1.Text = jsonString;
            }

            //pleaseWait.Close();
            this.UseWaitCursor = false;
            //textBox1.Text = await response.Content.ReadAsStringAsync();


            //response.Content.ToString();


        }


    }
}

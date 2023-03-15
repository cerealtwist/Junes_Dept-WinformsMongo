using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FormMSAccess
{
    public partial class Form2 : Form
    {

        public static IMongoClient client = new MongoClient();

        public static IMongoDatabase db = client.GetDatabase("inventaris_db");

        public Form2()
        {
            InitializeComponent();
        }


        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            // query Database MongoDB untuk mencari user dengan credentials yang valid.
            var usersCollection = db.GetCollection<BsonDocument>("users");
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("username", username),
                Builders<BsonDocument>.Filter.Eq("password", password)
            );
            var result = usersCollection.Find(filter).FirstOrDefault();

            if (result != null)
            {
                // Jika Autentikasi Berhasil
                Form1 mainForm = new Form1();
                mainForm.Show();
                this.Hide();
            }
            else
            {
                // Jika Autentikasi Gagal
                lblError.Text = "*Invalid Credentials.";
            }
        }

        private void txt_Username_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

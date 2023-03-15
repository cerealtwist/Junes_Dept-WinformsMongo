using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FormMSAccess
{
    public partial class Form1 : Form
    {
        public static IMongoClient client = new MongoClient();

        public static IMongoDatabase db = client.GetDatabase("inventaris_db");

        public static IMongoCollection<Product> collection = db.GetCollection<Product>("invcol");

        public Form1()
        {
            InitializeComponent();
            readData();

        }

        public class Product
        {
            [BsonId]
            public ObjectId id { get; set; }
            [BsonElement("Name")]
            public string name { get; set; }
            [BsonElement("Category")]
            public string category { get; set; }
            [BsonElement("Price")]
            public string price { get; set; }
            [BsonElement("Brand")]
            public string brand { get; set; }
            [BsonIgnore]
            public Image image { get; set; }
            [BsonElement("ImageData")]
            public byte[] imageData { get; set; }

            public Product(string name, string category, string price, string brand, Image image)
            {
                this.name = name;
                this.category = category;
                this.price = price;
                this.brand = brand;
                this.image = image;
                if (image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        this.imageData = ms.ToArray();
                    }
                }
            }
        

            public void SetImage()
            {
                if (this.imageData != null)
                {
                    using (MemoryStream ms = new MemoryStream(this.imageData))
                    {
                        this.image = Image.FromStream(ms);
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView.AutoGenerateColumns = true;
            dataGridView.Columns[0].Visible = false; // Hide the ID column
            dataGridView.Columns[5].Visible = false; // Hide the Image column


            // Add the Image column with a custom renderer
            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol.DataPropertyName = "Image";
            imgCol.ImageLayout = DataGridViewImageCellLayout.Stretch;
            imgCol.HeaderText = "Image";
            imgCol.Name = "imgColumn";
            imgCol.ReadOnly = true;
            imgCol.Width = 100;

            dataGridView.Columns.Add(imgCol);

            dataGridView.CellFormatting += dataGridView_CellFormatting;
            dataGridView.CellPainting += dataGridView_CellPainting;

            readData();
        }
        private void AddImageColumn()
        {
            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol.HeaderText = "Image";
            imgCol.ImageLayout = DataGridViewImageCellLayout.Stretch;
            dataGridView.Columns.Add(imgCol);
        }


        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif) | *.jpg; *.jpeg; *.png; *.gif";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Image selectedImage = Image.FromFile(openFileDialog.FileName);
                        pictureBox.Image = selectedImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image: " + ex.Message);
                    }
                }
            }
        }


        private void txtCategory_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPrice_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBrand_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            Product product = new Product(txtName.Text, txtCategory.Text, txtPrice.Text, txtBrand.Text, pictureBox.Image);
            collection.InsertOne(product);
            readData();
        }


        private void btnEdit_Click(object sender, EventArgs e)
        {
            displaySelectedData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete this item?", "Confirmation", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in dataGridView.SelectedRows)
                    {
                        Product product = (Product)row.DataBoundItem;
                        collection.DeleteOne(p => p.id == product.id);
                    }
                    readData();
                }
            }
            else
            {
                MessageBox.Show("Please select an item to delete.", "Information");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

        }
        private void readData()
        {
            List<Product> list = collection.AsQueryable().ToList();
            foreach (Product p in list)
            {
                p.SetImage();
            }
            dataGridView.DataSource = list;
            dataGridView.Columns["ImageData"].Visible = false;
        }

        private void displaySelectedData()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView.SelectedRows[0];
                Product selectedProduct = (Product)selectedRow.DataBoundItem;

                txtName.Text = selectedProduct.name;
                txtCategory.Text = selectedProduct.category;
                txtPrice.Text = selectedProduct.price;
                txtBrand.Text = selectedProduct.brand;
                pictureBox.Image = selectedProduct.image;
            }
        }

        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView.Columns[e.ColumnIndex].Name == "Image")
            {
                if (e.Value != null)
                {
                    string imagePath = e.Value.ToString();
                    if (File.Exists(imagePath))
                    {
                        e.Value = Image.FromFile(imagePath);
                    }
                }
                else
                {
                    e.Value = null;
                }
            }
        }

        private void dataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 5) // Check if it's the column with image
            {
                e.PaintBackground(e.CellBounds, true);

                // Get the byte array from the cell value
                byte[] bytes = (byte[])e.Value;

                if (bytes != null && bytes.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        // Load the image from the byte array
                        Image img = Image.FromStream(ms);

                        // Calculate the center of the cell
                        int x = (e.CellBounds.Width - img.Width) / 2 + e.CellBounds.X;
                        int y = (e.CellBounds.Height - img.Height) / 2 + e.CellBounds.Y;

                        // Draw the image in the center of the cell
                        e.Graphics.DrawImage(img, new Rectangle(x, y, img.Width, img.Height));
                    }
                }

                e.Handled = true;
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

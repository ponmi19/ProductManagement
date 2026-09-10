using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ProductManagement
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = "Server=localhost;Database=Product;Trusted_Connection=True;TrustServerCertificate=True;";
        private string currentUser = "ADMIN";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadProductTypes();
            SearchProducts();
        }

        private void LoadProductTypes()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sql = "SELECT PRODUCTTYPEID, PRODUCTTYPENAME FROM PRODUCTTYPE";
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbProductType.DataSource = dt;
                    cmbProductType.DisplayMember = "PRODUCTTYPENAME";
                    cmbProductType.ValueMember = "PRODUCTTYPEID";

                    DataTable dtSearch = dt.Copy();
                    DataRow row = dtSearch.NewRow();
                    row["PRODUCTTYPEID"] = "";
                    row["PRODUCTTYPENAME"] = "-- ทั้งหมด --";
                    dtSearch.Rows.InsertAt(row, 0);

                    cmbSearchType.DataSource = dtSearch;
                    cmbSearchType.DisplayMember = "PRODUCTTYPENAME";
                    cmbSearchType.ValueMember = "PRODUCTTYPEID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการโหลดประเภทสินค้า: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SearchProducts()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sql = @"SELECT p.PRODUCTID, p.PRODUCTNAME, t.PRODUCTTYPENAME, 
                                          CASE WHEN p.PRODUCTSTATUS = 0 THEN N'ใช้งาน' ELSE N'ยกเลิก' END AS STATUS_NAME,
                                          p.PRODUCTSTATUS, p.PRODUCTTYPEID, p.CREATEUSER, p.CREATEDATE
                                   FROM PRODUCTMASTER p
                                   INNER JOIN PRODUCTTYPE t ON p.PRODUCTTYPEID = t.PRODUCTTYPEID
                                   WHERE (p.PRODUCTID LIKE @Keyword OR p.PRODUCTNAME LIKE @Keyword)";

                    if (cmbSearchType.SelectedValue != null && !string.IsNullOrEmpty(cmbSearchType.SelectedValue.ToString()))
                    {
                        sql += " AND p.PRODUCTTYPEID = @TypeId";
                    }

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Keyword", "%" + txtSearchKeyword.Text.Trim() + "%");
                    if (cmbSearchType.SelectedValue != null && !string.IsNullOrEmpty(cmbSearchType.SelectedValue.ToString()))
                    {
                        cmd.Parameters.AddWithValue("@TypeId", cmbSearchType.SelectedValue.ToString());
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvProductMaster.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการค้นหาข้อมูล: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchProducts();
        }

        private void btnSearch_Click_1(object sender, EventArgs e)
        {
            SearchProducts();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnNew_Click_1(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductId.Text) || string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("กรุณากรอกรหัสและชื่อสินค้าให้ครบถ้วน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string checkSql = "SELECT COUNT(1) FROM PRODUCTMASTER WHERE PRODUCTID = @ProductId";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@ProductId", txtProductId.Text.Trim());
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    int status = chkIsActive.Checked ? 0 : 9;

                    if (count == 0)
                    {
 
                        string insertSql = @"INSERT INTO PRODUCTMASTER 
                                            (PRODUCTID, PRODUCTNAME, PRODUCTTYPEID, PRODUCTSTATUS, CREATEUSER, CREATEDATE)
                                             VALUES (@ProductId, @ProductName, @TypeId, @Status, @User, GETDATE())";
                        SqlCommand insCmd = new SqlCommand(insertSql, conn);
                        insCmd.Parameters.AddWithValue("@ProductId", txtProductId.Text.Trim());
                        insCmd.Parameters.AddWithValue("@ProductName", txtProductName.Text.Trim());
                        insCmd.Parameters.AddWithValue("@TypeId", cmbProductType.SelectedValue.ToString());
                        insCmd.Parameters.AddWithValue("@Status", status);
                        insCmd.Parameters.AddWithValue("@User", currentUser);
                        insCmd.ExecuteNonQuery();

                        MessageBox.Show("เพิ่มข้อมูลสำเร็จ", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
               
                        string updateSql = @"UPDATE PRODUCTMASTER 
                                             SET PRODUCTNAME = @ProductName, 
                                                 PRODUCTTYPEID = @TypeId, 
                                                 PRODUCTSTATUS = @Status, 
                                                 UPDATEUSER = @User, 
                                                 UPDATEDATE = GETDATE()
                                             WHERE PRODUCTID = @ProductId";
                        SqlCommand updCmd = new SqlCommand(updateSql, conn);
                        updCmd.Parameters.AddWithValue("@ProductId", txtProductId.Text.Trim());
                        updCmd.Parameters.AddWithValue("@ProductName", txtProductName.Text.Trim());
                        updCmd.Parameters.AddWithValue("@TypeId", cmbProductType.SelectedValue.ToString());
                        updCmd.Parameters.AddWithValue("@Status", status);
                        updCmd.Parameters.AddWithValue("@User", currentUser);
                        updCmd.ExecuteNonQuery();

                        MessageBox.Show("แก้ไขข้อมูลสำเร็จ", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    SearchProducts();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการบันทึกข้อมูล: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            btnSave_Click(sender, e);
        }

        private void btnCancelProduct_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductId.Text))
            {
                MessageBox.Show("กรุณาเลือกรหัสสินค้าที่ต้องการยกเลิก", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("คุณต้องการยกเลิกสินค้ารหัสนี้ใช่หรือไม่?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = @"UPDATE PRODUCTMASTER 
                                       SET PRODUCTSTATUS = 9, 
                                           UPDATEUSER = @User, 
                                           UPDATEDATE = GETDATE()
                                       WHERE PRODUCTID = @ProductId";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@ProductId", txtProductId.Text.Trim());
                        cmd.Parameters.AddWithValue("@User", currentUser);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("ยกเลิกรายการสินค้าเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SearchProducts();
                        ClearForm();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาดในการยกเลิกสินค้า: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancelProduct_Click_1(object sender, EventArgs e)
        {
            btnCancelProduct_Click(sender, e);
        }

      
        private void dgvProductMaster_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProductMaster.Rows[e.RowIndex];
                txtProductId.Text = row.Cells["PRODUCTID"].Value.ToString();
                txtProductName.Text = row.Cells["PRODUCTNAME"].Value.ToString();
                cmbProductType.SelectedValue = row.Cells["PRODUCTTYPEID"].Value.ToString();

                int status = Convert.ToInt32(row.Cells["PRODUCTSTATUS"].Value);
                chkIsActive.Checked = (status == 0);

                txtProductId.ReadOnly = true; 
            }
        }
        private void ClearForm()
        {
            txtProductId.Text = "";
            txtProductName.Text = "";
            txtProductId.ReadOnly = false;
            chkIsActive.Checked = true;
            if (cmbProductType.Items.Count > 0) cmbProductType.SelectedIndex = 0;
        }
    }
}
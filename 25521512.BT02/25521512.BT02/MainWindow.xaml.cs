using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _25521512.BT02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<SanPham> lstSanPham;
        List<string> lstDonViTinh;
        SanPham SanPhamDuocChon = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void QuanLySanPham_Loaded(object sender, RoutedEventArgs e)
        {
            lstDonViTinh = new List<string> { "Thùng", "Chai", "Hộp", "KG", "Bịch" };
            cbbDonViTinh.ItemsSource = lstDonViTinh;
            lstSanPham = new List<SanPham>();
            QuanLySanPham QLSP = new QuanLySanPham();
            QLSP.LoadTatCaSanPham(ref lstSanPham);
            dgSanPham.ItemsSource = lstSanPham;
        }

        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SanPham sp = new SanPham();

                if (string.IsNullOrWhiteSpace(tbMaSanPham.Text))
                {
                    if (lstSanPham == null || lstSanPham.Count == 0)
                    {
                        sp.MaSanPham = "SP001";
                    }
                    else
                    {
                        string maCuoi = lstSanPham.Last().MaSanPham;
                        int soMoi = int.Parse(maCuoi.Substring(2)) + 1;
                        sp.MaSanPham = "SP" + soMoi.ToString("D3");
                    }
                }
                else
                {
                    sp.MaSanPham = tbMaSanPham.Text;
                }
                sp.TenSanPham = string.IsNullOrWhiteSpace(tbTenSanPham.Text) ? null : tbTenSanPham.Text;

                sp.DonGia = string.IsNullOrWhiteSpace(tbDonGia.Text)
                            ? (double?)null
                            : Convert.ToDouble(tbDonGia.Text);

                sp.SoLuong = string.IsNullOrWhiteSpace(tbSoLuong.Text)
                             ? (int?)null
                             : Convert.ToInt32(tbSoLuong.Text);

                sp.DonViTinh = cbbDonViTinh.SelectedItem == null
                               ? null
                               : cbbDonViTinh.SelectedItem.ToString();

                QuanLySanPham QLSP = new QuanLySanPham();
                int kq = QLSP.ThemSanPham(ref lstSanPham, sp);
                if (kq == 1)
                {
                    MessageBox.Show("Thêm thành công !!");
                    dgSanPham.ItemsSource = null;
                    dgSanPham.ItemsSource = lstSanPham;

                    tbMaSanPham.Clear();
                    tbTenSanPham.Clear();
                    tbDonGia.Clear();
                    tbSoLuong.Clear();
                    cbbDonViTinh.SelectedIndex = -1;
                    tbMaSanPham.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                QuanLySanPham QLSP = new QuanLySanPham();
                if (SanPhamDuocChon == null) return;
                int kq = QLSP.XoaSanPham(ref lstSanPham, SanPhamDuocChon);
                if (kq == 1)
                {
                    MessageBox.Show("Xóa thành công !!");
                    dgSanPham.ItemsSource = lstSanPham;
                    dgSanPham.Items.Refresh();
                    SanPhamDuocChon = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (SanPhamDuocChon == null)
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm cần sửa!");
                    return;
                }

                SanPham spMoi = new SanPham();
                spMoi.MaSanPham = SanPhamDuocChon.MaSanPham;
                spMoi.TenSanPham = string.IsNullOrWhiteSpace(tbTenSanPham.Text) ? null : tbTenSanPham.Text;

                spMoi.DonGia = string.IsNullOrWhiteSpace(tbDonGia.Text)
                               ? (double?)null
                               : Convert.ToDouble(tbDonGia.Text);

                spMoi.SoLuong = string.IsNullOrWhiteSpace(tbSoLuong.Text)
                                ? (int?)null
                                : Convert.ToInt32(tbSoLuong.Text);

                spMoi.DonViTinh = cbbDonViTinh.SelectedItem == null
                                  ? null
                                  : cbbDonViTinh.SelectedItem.ToString();

                QuanLySanPham QLSP = new QuanLySanPham();
                int kq = QLSP.SuaSanPham(ref lstSanPham, spMoi);
                if (kq == 1)
                {
                    MessageBox.Show("Sửa thành công !!");
                    dgSanPham.ItemsSource = null;
                    dgSanPham.ItemsSource = lstSanPham;

                    dgSanPham.Items.Refresh();
                    tbMaSanPham.Clear();
                    tbTenSanPham.Clear();
                    tbSoLuong.Clear();
                    tbDonGia.Clear();
                    cbbDonViTinh.SelectedIndex = -1;
                    tbMaSanPham.IsEnabled = true;
                    SanPhamDuocChon = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }


        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                QuanLySanPham QLSP = new QuanLySanPham();
                string tuKhoa = tbTimKiem.Text;

                List<SanPham> ketQua = QLSP.TimKiemSanPham(lstSanPham, tuKhoa);
                if (ketQua.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy !!");
                    dgSanPham.ItemsSource = null;
                    dgSanPham.ItemsSource = lstSanPham;
                    return;
                }
                dgSanPham.ItemsSource = null;
                dgSanPham.ItemsSource = ketQua;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        private void dgSanPham_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSanPham.SelectedItem is SanPham selectedItem)
            {
                SanPhamDuocChon = selectedItem;
                tbMaSanPham.Text = SanPhamDuocChon.MaSanPham;
                tbTenSanPham.Text = SanPhamDuocChon.TenSanPham;
                tbSoLuong.Text = SanPhamDuocChon.SoLuong.ToString();
                tbDonGia.Text = SanPhamDuocChon.DonGia.ToString();
                cbbDonViTinh.SelectedItem = SanPhamDuocChon.DonViTinh;
                tbMaSanPham.IsEnabled = false;
            }
        }
    }
}

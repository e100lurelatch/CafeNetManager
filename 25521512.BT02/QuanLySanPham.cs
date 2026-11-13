using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _25521512.BT02
{
    public class QuanLySanPham
    {
        public void LoadTatCaSanPham(ref List<SanPham> lstSanPham)
        {
            lstSanPham = new List<SanPham>();
            SanPham sp1 = new SanPham("SP001", "Bột giặt", 1000, 5000, "Bịch");
            SanPham sp2 = new SanPham("SP002", "Kem đánh răng", 800, 30000, "Hộp");
            SanPham sp3 = new SanPham("SP003", "Nước ngọt pepsi", 500, 180000, "Thùng");
            lstSanPham.Add(sp1);
            lstSanPham.Add(sp2);
            lstSanPham.Add(sp3);
        }


        public int ThemSanPham(ref List<SanPham> lstSanPham, SanPham sp)
        {
            try
            {
                lstSanPham.Add(sp);
                return 1;
            }
            catch
            {
                throw;
            }
        }
        public int XoaSanPham(ref List<SanPham> lstSanPham, SanPham sp)
        {
            try
            {
                lstSanPham.Remove(sp);
                return 1;
            }
            catch
            {
                throw;
            }
        }
        public int SuaSanPham(ref List<SanPham> lstSanPham, SanPham spMoi)
        {
            SanPham spCanSua = lstSanPham.FirstOrDefault(sp => sp.MaSanPham == spMoi.MaSanPham);

            if (spCanSua != null)
            {
                spCanSua.TenSanPham = spMoi.TenSanPham;
                spCanSua.SoLuong = spMoi.SoLuong;
                spCanSua.DonGia = spMoi.DonGia;
                spCanSua.DonViTinh = spMoi.DonViTinh;
                return 1; 
            }
            return 0; 
        }
        public List<SanPham> TimKiemSanPham(List<SanPham> lstSanPham, string tuKhoa)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa))
            {
                return lstSanPham;
            }

            string key = tuKhoa.Trim().ToLower();
            return lstSanPham.Where(sp =>
                sp.MaSanPham.ToLower().Contains(key) ||
                sp.TenSanPham.ToLower().Contains(key)
            ).ToList();
        }

    }

}

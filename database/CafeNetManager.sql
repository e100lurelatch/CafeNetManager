/* =====================================================================
   CafeNetManager - FILE SQL CHUNG CỦA CẢ NHÓM
   Ai thêm bảng thì dán CREATE TABLE của mình xuống cuối file, ghi tên + module.
   Mở bằng SSMS -> Execute (F5). Chạy lại nhiều lần không lỗi (có IF NOT EXISTS).
   ===================================================================== */
IF DB_ID(N'CafeNetManager') IS NULL CREATE DATABASE CafeNetManager;
GO
USE CafeNetManager;
GO

/* ---------------------------------------------------------------------
   [TẠM] Bảng món - thuộc Module 01 (Quân). Bảo tạo tạm để OrderDetails có khóa ngoại.
   Quân sửa lại cột/tên bảng tùy ý rồi báo Bảo cập nhật.
   --------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.Drinks') IS NULL
CREATE TABLE Drinks (
    DrinkId     INT IDENTITY PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Category    NVARCHAR(50)  NOT NULL,
    Price       DECIMAL(12,0) NOT NULL CHECK (Price >= 0),
    IsAvailable BIT NOT NULL DEFAULT 1
);
GO

/* =====================================================================
   MODULE 02 - BẢO: Bàn, Order, Chi tiết order
   ===================================================================== */
IF OBJECT_ID(N'dbo.CafeTables') IS NULL
CREATE TABLE CafeTables (
    TableId  INT IDENTITY PRIMARY KEY,
    Name     NVARCHAR(50) NOT NULL UNIQUE,
    Capacity INT NOT NULL DEFAULT 4,
    Status   INT NOT NULL DEFAULT 0 CHECK (Status IN (0,1,2))  -- 0 Trống, 1 Có khách, 2 Đặt trước
);
GO

IF OBJECT_ID(N'dbo.Orders') IS NULL
CREATE TABLE Orders (
    OrderId   INT IDENTITY PRIMARY KEY,
    TableId   INT NOT NULL REFERENCES CafeTables(TableId),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    Status    INT NOT NULL DEFAULT 0 CHECK (Status IN (0,1,2))  -- 0 Chưa thanh toán, 1 Đã thanh toán, 2 Hủy
);
GO

IF OBJECT_ID(N'dbo.OrderDetails') IS NULL
CREATE TABLE OrderDetails (
    OrderDetailId INT IDENTITY PRIMARY KEY,
    OrderId   INT NOT NULL REFERENCES Orders(OrderId) ON DELETE CASCADE,
    DrinkId   INT NOT NULL REFERENCES Drinks(DrinkId),
    Quantity  INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(12,0) NOT NULL,      -- giá tại lúc gọi (menu đổi giá không ảnh hưởng đơn cũ)
    Note      NVARCHAR(200) NOT NULL DEFAULT N''
);
GO

-- Chuyển bàn: chỉ chuyển sang bàn đang Trống
CREATE OR ALTER PROCEDURE sp_TransferTable @FromTableId INT, @ToTableId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF (SELECT Status FROM CafeTables WHERE TableId = @ToTableId) <> 0
    BEGIN
        RAISERROR(N'Bàn đích không trống', 16, 1);
        RETURN;
    END
    BEGIN TRAN;
        UPDATE Orders     SET TableId = @ToTableId WHERE TableId = @FromTableId AND Status = 0;
        UPDATE CafeTables SET Status = 1 WHERE TableId = @ToTableId;
        UPDATE CafeTables SET Status = 0 WHERE TableId = @FromTableId;
    COMMIT;
END
GO

-- Dữ liệu mẫu
IF NOT EXISTS (SELECT 1 FROM Drinks)
INSERT INTO Drinks(Name, Category, Price) VALUES
 (N'Cà phê đen', N'Cà phê', 20000), (N'Cà phê sữa', N'Cà phê', 25000), (N'Bạc xỉu', N'Cà phê', 28000),
 (N'Trà đào', N'Trà', 30000), (N'Trà vải', N'Trà', 30000),
 (N'Sinh tố bơ', N'Sinh tố', 35000), (N'Bánh flan', N'Bánh', 15000);

IF NOT EXISTS (SELECT 1 FROM CafeTables)
INSERT INTO CafeTables(Name, Capacity) VALUES
 (N'Bàn 1',2),(N'Bàn 2',2),(N'Bàn 3',4),(N'Bàn 4',4),(N'Bàn 5',4),(N'Bàn 6',4),
 (N'Bàn 7',4),(N'Bàn 8',4),(N'Bàn 9',4),(N'Bàn 10',4),(N'Bàn 11',6),(N'Bàn 12',6);
GO

/* ---------------------------------------------------------------------
   MODULE 02 - BẢO (bổ sung): Quản lý bàn, Ưu đãi, Thanh toán / Hóa đơn
   Chạy lại cả file vẫn an toàn (có IF NOT EXISTS).
   --------------------------------------------------------------------- */

-- Quản lý bàn: xóa mềm (giữ lịch sử hóa đơn)
IF COL_LENGTH('dbo.CafeTables', 'IsActive') IS NULL
    ALTER TABLE CafeTables ADD IsActive BIT NOT NULL CONSTRAINT DF_CafeTables_IsActive DEFAULT 1;
GO

-- Ưu đãi / mã giảm giá
IF OBJECT_ID(N'dbo.Promotions') IS NULL
CREATE TABLE Promotions (
    PromotionId    INT IDENTITY PRIMARY KEY,
    Code           NVARCHAR(20)  NOT NULL UNIQUE,              -- VD: GIAM10
    Name           NVARCHAR(100) NOT NULL,
    DiscountType   INT NOT NULL CHECK (DiscountType IN (0,1)), -- 0: giảm %, 1: giảm số tiền
    DiscountValue  DECIMAL(12,2) NOT NULL CHECK (DiscountValue > 0),
    MinOrderAmount DECIMAL(12,0) NOT NULL DEFAULT 0,
    MaxDiscount    DECIMAL(12,0) NULL,                         -- giảm tối đa (cho loại %)
    StartDate      DATE NOT NULL,
    EndDate        DATE NOT NULL,
    UsageLimit     INT  NULL,                                  -- NULL = không giới hạn
    UsedCount      INT  NOT NULL DEFAULT 0,
    IsActive       BIT  NOT NULL DEFAULT 1,
    CONSTRAINT CK_Promotions_Date CHECK (EndDate >= StartDate)
);
GO

-- Hóa đơn thanh toán
IF OBJECT_ID(N'dbo.Invoices') IS NULL
CREATE TABLE Invoices (
    InvoiceId      INT IDENTITY PRIMARY KEY,
    OrderId        INT NOT NULL UNIQUE REFERENCES Orders(OrderId),
    TableId        INT NOT NULL REFERENCES CafeTables(TableId),
    SubTotal       DECIMAL(12,0) NOT NULL,
    PromotionId    INT NULL REFERENCES Promotions(PromotionId) ON DELETE SET NULL,
    PromotionCode  NVARCHAR(20) NULL,                          -- giữ mã dạng chữ dù ưu đãi bị xóa
    DiscountAmount DECIMAL(12,0) NOT NULL DEFAULT 0,
    Total          DECIMAL(12,0) NOT NULL,
    PaymentMethod  NVARCHAR(20) NOT NULL,                      -- Tiền mặt / PayPal
    CashReceived   DECIMAL(12,0) NULL,
    ChangeAmount   DECIMAL(12,0) NULL,
    TransactionRef NVARCHAR(50) NULL,                          -- mã giao dịch PayPal
    PaidAt         DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Ưu đãi mẫu
IF NOT EXISTS (SELECT 1 FROM Promotions)
INSERT INTO Promotions(Code, Name, DiscountType, DiscountValue, MinOrderAmount, MaxDiscount, StartDate, EndDate, UsageLimit) VALUES
 (N'GIAM10',     N'Giảm 10% mọi đơn',            0, 10,    0,      30000, DATEADD(DAY,-7,CAST(GETDATE() AS DATE)),  DATEADD(MONTH,2,CAST(GETDATE() AS DATE)), NULL),
 (N'KHAITRUONG', N'Khai trương giảm 20.000đ',    1, 20000, 100000, NULL,  DATEADD(DAY,-1,CAST(GETDATE() AS DATE)),  DATEADD(DAY,30,CAST(GETDATE() AS DATE)),  50),
 (N'HSSV15',     N'Học sinh sinh viên giảm 15%', 0, 15,    50000,  25000, DATEADD(MONTH,-1,CAST(GETDATE() AS DATE)), DATEADD(MONTH,6,CAST(GETDATE() AS DATE)), NULL),
 (N'TET2026',    N'Ưu đãi Tết (đã hết hạn)',     0, 20,    0,      NULL,  '2026-01-20', '2026-02-20', NULL);
GO
/* ========================= HẾT MODULE 02 ========================= */

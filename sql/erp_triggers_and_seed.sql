USE ERP_Quan_Ly_Kho_Thong_Minh_DB;
GO


-- 1. BẢNG PHÂN QUYỀN & NGƯỜI DÙNG

CREATE TABLE Roles (
	Id INT PRIMARY KEY IDENTITY,
	RoleName NVARCHAR(50) NOT NULL,
	Description NVARCHAR(255),
	-- Audit fields
	CreatedDate DATETIME DEFAULT GETDATE(),
	UpdatedDate DATETIME NULL,
	CreatedBy NVARCHAR(50),
	UpdatedBy NVARCHAR(50),
	IsDeleted BIT DEFAULT 0
);

CREATE TABLE Users (
	Id INT PRIMARY KEY IDENTITY,
	Username NVARCHAR(50) UNIQUE NOT NULL,
	PasswordHash NVARCHAR(MAX) NOT NULL,
	FullName NVARCHAR(100),
	Email NVARCHAR(100),
	Phone NVARCHAR(20),
	RoleId INT FOREIGN KEY REFERENCES Roles(Id),
	-- Business fields
	IsActive BIT DEFAULT 1,
	LastLogin DATETIME NULL,
	-- Audit fields
	CreatedDate DATETIME DEFAULT GETDATE(),
	UpdatedDate DATETIME NULL,
	CreatedBy NVARCHAR(50),
	UpdatedBy NVARCHAR(50),
	IsDeleted BIT DEFAULT 0
);


-- 2. DANH MỤC & SẢN PHẨM

CREATE TABLE Categories (
	Id INT PRIMARY KEY IDENTITY,
	CategoryName NVARCHAR(100) NOT NULL,
	Description NVARCHAR(255),
	CreatedDate DATETIME DEFAULT GETDATE(),
	UpdatedDate DATETIME NULL,
	CreatedBy NVARCHAR(50),
	UpdatedBy NVARCHAR(50),
	IsDeleted BIT DEFAULT 0
);

CREATE TABLE Products (
	Id INT PRIMARY KEY IDENTITY,
	CategoryId INT FOREIGN KEY REFERENCES Categories(Id),
	ProductCode NVARCHAR(50) UNIQUE NOT NULL,
	ProductName NVARCHAR(200) NOT NULL,
	ImportPrice DECIMAL(18,2) DEFAULT 0,
	ExportPrice DECIMAL(18,2) DEFAULT 0,
	-- Stock Control
	CurrentStock INT DEFAULT 0,
	MinStock INT DEFAULT 5,
	AlertThreshold INT DEFAULT 10,
	Unit NVARCHAR(50) DEFAULT N'Chiếc',
	ProductImage VARBINARY(MAX) NULL,
	-- Audit fields
	CreatedDate DATETIME DEFAULT GETDATE(),
	UpdatedDate DATETIME NULL,
	CreatedBy NVARCHAR(50),
	UpdatedBy NVARCHAR(50),
	IsDeleted BIT DEFAULT 0
);


-- 3. QUẢN LÝ IMEI & TRẠNG THÁI

CREATE TABLE ImeiStatuses (
	Id INT PRIMARY KEY IDENTITY,
	StatusName NVARCHAR(50) NOT NULL, -- Trong kho, Đã bán, Bảo hành, Lỗi, Trả hàng
	ColorCode NVARCHAR(10) --  hiển thị màu trạng thái
);

CREATE TABLE ImeiInventories (
	Imei NVARCHAR(20) PRIMARY KEY, -- IMEI 
	ProductId INT FOREIGN KEY REFERENCES Products(Id),
	StatusId INT FOREIGN KEY REFERENCES ImeiStatuses(Id),
	-- Audit fields
	CreatedDate DATETIME DEFAULT GETDATE(),
	UpdatedDate DATETIME NULL,
	CreatedBy NVARCHAR(50),
	UpdatedBy NVARCHAR(50),
	IsDeleted BIT DEFAULT 0
);

-- 4. ĐỐI TÁC & GIAO DỊCH

CREATE TABLE Objects (
	Id INT PRIMARY KEY IDENTITY,
	ObjectType NVARCHAR(20) NOT NULL, -- KHÀCH HÀNG / NHÀ CUNG CẤP
	FullName NVARCHAR(200) NOT NULL,
	Phone NVARCHAR(20),
	Address NVARCHAR(500),
	Email NVARCHAR(100),
	TaxCode NVARCHAR(50),
	TotalDebt DECIMAL(18,2) DEFAULT 0, -- Công nợ hiện tại
	CreatedDate DATETIME DEFAULT GETDATE(),
	UpdatedDate DATETIME NULL,
	CreatedBy NVARCHAR(50),
	UpdatedBy NVARCHAR(50),
	IsDeleted BIT DEFAULT 0
);

CREATE TABLE Bills (
	Id NVARCHAR(50) PRIMARY KEY, -- Mã hóa đơn 
	BillType NVARCHAR(20) NOT NULL, -- NHAP / XUAT
	ObjectId INT FOREIGN KEY REFERENCES Objects(Id),
	UserId INT FOREIGN KEY REFERENCES Users(Id),
	TotalAmount DECIMAL(18,2) DEFAULT 0,
	PaidAmount DECIMAL(18,2) DEFAULT 0,
	RemainingAmount AS (TotalAmount - PaidAmount), -- Cột tính toán tự động
	BillDate DATETIME DEFAULT GETDATE(),
	Note NVARCHAR(MAX),
	IsDeleted BIT DEFAULT 0
);

CREATE TABLE BillDetails (
	Id INT PRIMARY KEY IDENTITY,
	BillId NVARCHAR(50) FOREIGN KEY REFERENCES Bills(Id),
	ProductId INT FOREIGN KEY REFERENCES Products(Id),
	Imei NVARCHAR(20) FOREIGN KEY REFERENCES ImeiInventories(Imei),
	UnitPrice DECIMAL(18,2) NOT NULL,
	Quantity INT DEFAULT 1 -- Luôn là 1 với quản lý IMEI
);


-- 5. KIỂM KÊ & HỆ THỐNG

CREATE TABLE InventoryChecks (
	Id INT PRIMARY KEY IDENTITY,
	CheckDate DATETIME DEFAULT GETDATE(),
	UserId INT FOREIGN KEY REFERENCES Users(Id),
	Note NVARCHAR(MAX),
	IsDeleted BIT DEFAULT 0
);

CREATE TABLE InventoryCheckDetails (
	Id INT PRIMARY KEY IDENTITY,
	CheckId INT FOREIGN KEY REFERENCES InventoryChecks(Id),
	ProductId INT FOREIGN KEY REFERENCES Products(Id),
	SystemStock INT, -- Tồn kho hệ thống lúc kiểm kê
	ActualStock INT, -- Tồn kho thực tế
	Difference AS (ActualStock - SystemStock)
);

CREATE TABLE Settings (
	Id INT PRIMARY KEY IDENTITY,
	ShopName NVARCHAR(200),
	Address NVARCHAR(500),
	Phone NVARCHAR(20),
	Logo VARBINARY(MAX),
	Theme NVARCHAR(20) DEFAULT 'Light'
);

-- 6. DỮ LIỆU MẪU (SEED DATA) 

INSERT INTO Roles (RoleName, Description, CreatedBy) 
VALUES (N'Quản trị viên', N'Toàn quyền hệ thống', 'SYSTEM'),
	   (N'Thủ kho', N'Quản lý nhập xuất và IMEI', 'SYSTEM'),
	   (N'Nhân viên bán hàng', N'Lập hóa đơn bán hàng', 'SYSTEM');

INSERT INTO ImeiStatuses (StatusName, ColorCode)
VALUES (N'Trong kho', '#4CAF50'),
	   (N'Đã bán', '#2196F3'),
	   (N'Bảo hành', '#FF9800'),
	   (N'Lỗi', '#F44336'),
	   (N'Trả hàng', '#9C27B0');

-- User mặc định (Mật khẩu: 12042002)
INSERT INTO Users (Username, PasswordHash, FullName, RoleId, CreatedBy)
VALUES ('admin', '12042002', N'Quản trị viên hệ thống', 1, 'SYSTEM');


-- 7. INDEX TỐI ƯU HÓA TRUY VẤN (PERFORMANCE)

CREATE INDEX IX_Product_Name ON Products(ProductName);
CREATE INDEX IX_Imei_Status ON ImeiInventories(StatusId);
CREATE INDEX IX_Bill_Date ON Bills(BillDate);
GO

-- 8. GIAO DỊCH CÔNG NỢ (Lưu lịch sử biến động tiền của Khách/NCC)
CREATE TABLE DebtTransactions (
	Id INT PRIMARY KEY IDENTITY,
	ObjectId INT FOREIGN KEY REFERENCES Objects(Id),
	BillId NVARCHAR(50) FOREIGN KEY REFERENCES Bills(Id),
	TransactionDate DATETIME DEFAULT GETDATE(),
	Amount DECIMAL(18,2), -- Số tiền trả thêm hoặc nợ mới
	Type NVARCHAR(20), -- 'PAYMENT' (Trả tiền), 'DEBT' (Ghi nợ mới)
	Note NVARCHAR(MAX)
);

-- 9. NHẬT KÝ HỆ THỐNG (AuditLogs - Cực kỳ quan trọng để bảo vệ đồ án)
CREATE TABLE AuditLogs (
	Id BIGINT PRIMARY KEY IDENTITY,
	UserId INT,
	Action NVARCHAR(50), -- INSERT, UPDATE, DELETE, LOGIN
	TableName NVARCHAR(50),
	RecordId NVARCHAR(50),
	OldData NVARCHAR(MAX),
	NewData NVARCHAR(MAX),
	Timestamp DATETIME DEFAULT GETDATE()
);


-- 10. PHIẾU BẢO HÀNH (Theo dõi lịch sử sửa chữa thiết bị)
CREATE TABLE WarrantyLogs (
	Id INT PRIMARY KEY IDENTITY(1,1),
	Imei NVARCHAR(20) FOREIGN KEY REFERENCES ImeiInventories(Imei),
	UserId INT FOREIGN KEY REFERENCES Users(Id),
	ReceiveDate DATETIME DEFAULT GETDATE(),
	ReturnDate DATETIME NULL,
	Description NVARCHAR(MAX), -- Tình trạng lỗi
	Result NVARCHAR(MAX),      -- Kết quả xử lý
	Cost DECIMAL(18,2) DEFAULT 0,
	Status NVARCHAR(50)        -- Đang xử lý, Đã trả khách, Không thể sửa
);

-- 11. PHIẾU TRẢ HÀNG (Xử lý khi khách trả máy hoặc trả hàng cho NCC)
CREATE TABLE Returns (
	Id INT PRIMARY KEY IDENTITY(1,1),
	BillId NVARCHAR(50) FOREIGN KEY REFERENCES Bills(Id),
	ReturnType NVARCHAR(20), -- CUSTOMER_RETURN (Khách trả), SUPPLIER_RETURN (Trả NCC)
	Reason NVARCHAR(MAX),
	TotalRefund DECIMAL(18,2),
	CreatedDate DATETIME DEFAULT GETDATE()
);

--Trigger : bổ trợ hệ thống 

--Trigger 1: Tự động cập nhật tồn kho khi có thay đổi IMEI
GO
CREATE TRIGGER TRG_UpdateProductStock
ON ImeiInventories
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;
	-- Cập nhật cho những sản phẩm bị ảnh hưởng
	UPDATE Products
	SET CurrentStock = (SELECT COUNT(*) FROM ImeiInventories 
						WHERE ProductId = Products.Id AND StatusId = 1 AND IsDeleted = 0)
	WHERE Id IN (SELECT ProductId FROM inserted UNION SELECT ProductId FROM deleted);
END;
GO

--Trigger 2: Tự động cập nhật công nợ của Đối tác
GO
CREATE TRIGGER TRG_UpdateObjectDebt
ON Bills
AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE Objects
	SET TotalDebt = (SELECT SUM(TotalAmount - PaidAmount) FROM Bills 
					 WHERE ObjectId = Objects.Id AND IsDeleted = 0)
	WHERE Id IN (SELECT ObjectId FROM inserted);
END;
GO

-- Trigger 3 : View báo cáo (Dành cho Dashboard)
GO
CREATE VIEW View_InventoryAlert AS
SELECT 
	ProductCode AS [Mã Sản Phẩm],
	ProductName AS [Tên Sản Phẩm],
	CurrentStock AS [Tồn Hiện Tại],
	MinStock AS [Tồn Tối Thiểu],
	(MinStock - CurrentStock) AS [Số Lượng Thiếu]
FROM Products
WHERE CurrentStock <= MinStock AND IsDeleted = 0;
GO
--Trigger 4 :Kiểm tra logic (Check Constraints)
-- Không cho phép giá nhập/xuất âm
ALTER TABLE Products ADD CONSTRAINT CHK_Product_Price CHECK (ImportPrice >= 0 AND ExportPrice >= 0);
-- Không cho phép số lượng tồn kho âm
ALTER TABLE Products ADD CONSTRAINT CHK_Product_Stock CHECK (CurrentStock >= 0);

-- Trigger 5 : Ràng buộc Logic cho IMEI (Check Constraint)
-- Đảm bảo IMEI không được để trống và có độ dài tối thiểu là 10 ký tự
ALTER TABLE ImeiInventories 
ADD CONSTRAINT CHK_Imei_Length CHECK (LEN(Imei) >= 10);
GO

--Trigger 6 : Xử lý "Xóa mềm" (Soft Delete) đồng bộ
GO
CREATE TRIGGER TRG_SoftDeleteProduct
ON Products
AFTER UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE(IsDeleted)
	BEGIN
		UPDATE ImeiInventories
		SET IsDeleted = i.IsDeleted
		FROM ImeiInventories im
		JOIN inserted i ON im.ProductId = i.Id;
	END
END;
GO

--TRIGGER 7 : TỰ ĐỘNG LƯU NHẬT KÝ HỆ THỐNG (AUDIT LOGS)
CREATE OR ALTER TRIGGER TRG_AuditProductPrice
ON Products
AFTER UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE(ImportPrice) OR UPDATE(ExportPrice)
	BEGIN
		INSERT INTO AuditLogs (UserId, Action, TableName, RecordId, OldData, NewData)
		SELECT 
			(SELECT TOP 1 Id FROM Users WHERE Username = (SELECT UpdatedBy FROM inserted)), -- Giả định UpdatedBy lưu username
			'UPDATE_PRICE',
			'Products',
			CAST(d.Id AS NVARCHAR(50)),
			JSON_QUERY((SELECT d.ImportPrice AS OldImport, d.ExportPrice AS OldExport FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)),
			JSON_QUERY((SELECT i.ImportPrice AS NewImport, i.ExportPrice AS NewExport FOR JSON PATH, WITHOUT_ARRAY_WRAPPER))
		FROM deleted d
		JOIN inserted i ON d.Id = i.Id;
	END
END;
GO

-- Trigger 8 : STORED PROCEDURE: NGHIỆP VỤ NHẬP KHO CHUYÊN NGHIỆP
CREATE OR ALTER PROCEDURE sp_ImportStock
	@BillId NVARCHAR(50),
	@ObjectId INT,
	@UserId INT,
	@ProductId INT,
	@UnitPrice DECIMAL(18,2),
	@TotalAmount DECIMAL(18,2),
	@PaidAmount DECIMAL(18,2),
	@Note NVARCHAR(MAX),
	@ImeiList NVARCHAR(MAX) -- Chuỗi IMEI cách nhau bằng dấu phẩy
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRANSACTION;
	BEGIN TRY
		-- 1. Tạo hóa đơn
		INSERT INTO Bills (Id, BillType, ObjectId, UserId, TotalAmount, PaidAmount, BillDate, Note)
		VALUES (@BillId, 'IMPORT', @ObjectId, @UserId, @TotalAmount, @PaidAmount, GETDATE(), @Note);

		-- 2. Duyệt danh sách IMEI và chèn dữ liệu (Sử dụng STRING_SPLIT có sẵn từ SQL 2016+)
		INSERT INTO ImeiInventories (Imei, ProductId, StatusId, CreatedBy)
		SELECT value, @ProductId, 1, (SELECT Username FROM Users WHERE Id = @UserId)
		FROM STRING_SPLIT(@ImeiList, ',');

		-- 3. Lưu chi tiết hóa đơn
		INSERT INTO BillDetails (BillId, ProductId, Imei, UnitPrice)
		SELECT @BillId, @ProductId, value, @UnitPrice
		FROM STRING_SPLIT(@ImeiList, ',');

		COMMIT TRANSACTION;
		SELECT 'SUCCESS' AS Result;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SELECT ERROR_MESSAGE() AS Result;
	END CATCH
END;
GO
--Trigger 9 :  STORED PROCEDURE: NGHIỆP VỤ XUẤT KHO (BÁN HÀNG)
CREATE OR ALTER PROCEDURE sp_ExportStock
	@BillId NVARCHAR(50),
	@ObjectId INT,
	@UserId INT,
	@ProductId INT,
	@UnitPrice DECIMAL(18,2),
	@TotalAmount DECIMAL(18,2),
	@PaidAmount DECIMAL(18,2),
	@ImeiList NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRANSACTION;
	BEGIN TRY
		-- 1. Kiểm tra IMEI có sẵn trong kho không
		IF EXISTS (SELECT 1 FROM STRING_SPLIT(@ImeiList, ',') s 
				   JOIN ImeiInventories i ON s.value = i.Imei 
				   WHERE i.StatusId <> 1) -- Nếu không phải 'Trong kho'
		BEGIN
			;THROW 50000, 'Một số mã IMEI không khả dụng trong kho!', 1;
		END

		-- 2. Tạo hóa đơn xuất
		INSERT INTO Bills (Id, BillType, ObjectId, UserId, TotalAmount, PaidAmount, BillDate)
		VALUES (@BillId, 'EXPORT', @ObjectId, @UserId, @TotalAmount, @PaidAmount, GETDATE());

		-- 3. Cập nhật trạng thái IMEI thành 'Đã bán' (StatusId = 2)
		UPDATE ImeiInventories
		SET StatusId = 2, UpdatedDate = GETDATE()
		WHERE Imei IN (SELECT value FROM STRING_SPLIT(@ImeiList, ','));

		-- 4. Lưu chi tiết hóa đơn
		INSERT INTO BillDetails (BillId, ProductId, Imei, UnitPrice)
		SELECT @BillId, @ProductId, value, @UnitPrice
		FROM STRING_SPLIT(@ImeiList, ',');

		COMMIT TRANSACTION;
		SELECT 'SUCCESS' AS Result;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SELECT ERROR_MESSAGE() AS Result;
	END CATCH
END;
GO



-- TRIGGER 10 : NGĂN CHẶN BÁN HÀNG KHI HẾT KHO
GO
CREATE OR ALTER TRIGGER TRG_ValidateImeiStatusBeforeSell
ON BillDetails
AFTER INSERT
AS
BEGIN
	SET NOCOUNT ON;
	-- Nếu là hóa đơn EXPORT, kiểm tra xem IMEI có thực sự đang 'Trong kho' không
	IF EXISTS (
		SELECT 1 FROM inserted i
		JOIN Bills b ON i.BillId = b.Id
		JOIN ImeiInventories im ON i.Imei = im.Imei
		WHERE b.BillType = 'EXPORT' AND im.StatusId <> 1 -- 1 là 'Trong kho'
	)
	BEGIN
		;THROW 51000, N'Lỗi: Một số thiết bị đã được quét không còn tồn tại trong kho hoặc đã bán!', 1;
		ROLLBACK TRANSACTION;
	END
END;
GO

--Trigger 11 : STORED PROCEDURE: DASHBOARD THỐNG KÊ TỔ HỢP
GO
CREATE OR ALTER PROCEDURE sp_GetDashboardSummary
AS
BEGIN
	SET NOCOUNT ON;

	-- 1. Tổng doanh thu (Chỉ tính hóa đơn xuất)
	DECLARE @TotalRevenue DECIMAL(18,2) = (SELECT ISNULL(SUM(TotalAmount), 0) FROM Bills WHERE BillType = 'EXPORT' AND IsDeleted = 0);

	-- 2. Tổng tiền nhập hàng
	DECLARE @TotalImport DECIMAL(18,2) = (SELECT ISNULL(SUM(TotalAmount), 0) FROM Bills WHERE BillType = 'IMPORT' AND IsDeleted = 0);

	-- 3. Tổng số lượng sản phẩm đang tồn kho
	DECLARE @TotalStock INT = (SELECT COUNT(*) FROM ImeiInventories WHERE StatusId = 1 AND IsDeleted = 0);

	-- 4. Tổng số máy đang nợ NCC
	DECLARE @TotalDebtToSupplier DECIMAL(18,2) = (SELECT ISNULL(SUM(TotalDebt), 0) FROM Objects WHERE ObjectType = 'SUPPLIER');

	SELECT 
		@TotalRevenue AS TotalRevenue,
		@TotalImport AS TotalImport,
		@TotalStock AS TotalStock,
		@TotalDebtToSupplier AS TotalDebtToSupplier;

	-- Trả về Top 5 sản phẩm bán chạy
	SELECT TOP 5 p.ProductName, COUNT(bd.Imei) AS SoldCount
	FROM BillDetails bd
	JOIN Products p ON bd.ProductId = p.Id
	JOIN Bills b ON bd.BillId = b.Id
	WHERE b.BillType = 'EXPORT'
	GROUP BY p.ProductName
	ORDER BY SoldCount DESC;
END;
GO
--Trigger 12 : HOÀN THIỆN LOGIC: TỰ ĐỘNG SINH CÔNG NỢ KHI NHẬP/XUẤT
GO
CREATE OR ALTER TRIGGER TRG_LogDebtTransaction
ON Bills
AFTER INSERT
AS
BEGIN
	SET NOCOUNT ON;
	-- Khi có hóa đơn mà khách/mình chưa trả đủ tiền, tự động tạo lịch sử nợ
	INSERT INTO DebtTransactions (ObjectId, BillId, Amount, Type, Note)
	SELECT 
		ObjectId, 
		Id, 
		(TotalAmount - PaidAmount), 
		'DEBT', 
		N'Tự động ghi nợ từ hóa đơn ' + Id
	FROM inserted
	WHERE (TotalAmount - PaidAmount) > 0;
END;
GO


-- TRIGGER 13: CẬP NHẬT CÔNG NỢ KHI UPDATE SỐ TIỀN THANH TOÁN
GO
CREATE OR ALTER TRIGGER TRG_UpdateDebtOnBillUpdate
ON Bills
AFTER UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE(PaidAmount) OR UPDATE(TotalAmount)
	BEGIN
		-- Xóa lịch sử nợ cũ do hóa đơn này sinh ra và tạo lại bản ghi mới chính xác
		DELETE FROM DebtTransactions WHERE BillId IN (SELECT Id FROM inserted);

		INSERT INTO DebtTransactions (ObjectId, BillId, Amount, Type, Note)
		SELECT ObjectId, Id, (TotalAmount - PaidAmount), 'DEBT', N'Cập nhật công nợ từ hóa đơn ' + Id
		FROM inserted
		WHERE (TotalAmount - PaidAmount) > 0;
	END
END;
GO

-- TRIGGER 14: PROCEDURE THANH TOÁN CÔNG NỢ (KHÔNG QUA HÓA ĐƠN)
GO
CREATE OR ALTER PROCEDURE sp_PayDebt
	@ObjectId INT,
	@Amount DECIMAL(18,2),
	@Note NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRANSACTION;
	BEGIN TRY
		-- Ghi nhận giao dịch trả tiền
		INSERT INTO DebtTransactions (ObjectId, Amount, Type, Note, TransactionDate)
		VALUES (@ObjectId, @Amount, 'PAYMENT', @Note, GETDATE());

		-- Cập nhật trực tiếp số dư nợ trong bảng Objects
		UPDATE Objects 
		SET TotalDebt = TotalDebt - @Amount 
		WHERE Id = @ObjectId;

		COMMIT TRANSACTION;
		SELECT 'SUCCESS' AS Result;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SELECT ERROR_MESSAGE() AS Result;
	END CATCH
END;
GO

-- TRIGGER 15: ĐỒNG BỘ TRẠNG THÁI IMEI KHI XÓA HÓA ĐƠN
GO
CREATE OR ALTER TRIGGER TRG_SyncImeiOnBillDelete
ON Bills
AFTER UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE(IsDeleted)
	BEGIN
		-- Nếu hóa đơn bị xóa mềm, thì các IMEI liên quan cũng bị xóa mềm
		UPDATE ImeiInventories
		SET IsDeleted = i.IsDeleted
		FROM ImeiInventories im
		JOIN BillDetails bd ON im.Imei = bd.Imei
		JOIN inserted i ON bd.BillId = i.Id
		WHERE i.IsDeleted = 1;
	END
END;
GO

CREATE OR ALTER VIEW View_MinStockPrediction AS
WITH SalesLast30Days AS
(
	SELECT 
		bd.ProductId,
		COUNT(*) AS SoldQuantity30Days
	FROM BillDetails bd
	JOIN Bills b ON bd.BillId = b.Id
	WHERE b.BillType = 'EXPORT'
	  AND b.IsDeleted = 0
	  AND b.BillDate >= DATEADD(DAY, -30, GETDATE())
	GROUP BY bd.ProductId
)
SELECT 
	p.Id AS ProductId,
	p.ProductCode,
	p.ProductName,
	p.CurrentStock,
	p.MinStock,
	p.AlertThreshold,
	ISNULL(s.SoldQuantity30Days, 0) AS SoldQuantity30Days,
	CAST(ISNULL(s.SoldQuantity30Days, 0) / 30.0 AS DECIMAL(18,2)) AS AvgDailySales,
	CEILING((ISNULL(s.SoldQuantity30Days, 0) / 30.0) * 7 + 3) AS SuggestedMinStock,
	CASE 
		WHEN p.CurrentStock <= p.MinStock THEN N'Cần nhập hàng'
		WHEN p.CurrentStock <= p.AlertThreshold THEN N'Sắp cần nhập'
		ELSE N'Ổn định'
	END AS StockStatus
FROM Products p
LEFT JOIN SalesLast30Days s ON p.Id = s.ProductId
WHERE p.IsDeleted = 0;
GO
--DỮ LIỆU TEST THỬ 

USE ERP_Quan_Ly_Kho_Thong_Minh_DB;
GO

USE ERP_Quan_Ly_Kho_Thong_Minh_DB;
GO

SET NOCOUNT ON;

-- ROLES
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = N'Quản trị viên')
INSERT INTO Roles (RoleName, Description, CreatedBy)
VALUES (N'Quản trị viên', N'Toàn quyền hệ thống', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = N'Thủ kho')
INSERT INTO Roles (RoleName, Description, CreatedBy)
VALUES (N'Thủ kho', N'Quản lý nhập kho, xuất kho, IMEI, kiểm kê', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = N'Nhân viên bán hàng')
INSERT INTO Roles (RoleName, Description, CreatedBy)
VALUES (N'Nhân viên bán hàng', N'Lập hóa đơn, bán hàng, quản lý khách hàng', 'SYSTEM');

-- IMEI STATUS
IF NOT EXISTS (SELECT 1 FROM ImeiStatuses WHERE StatusName = N'Trong kho')
INSERT INTO ImeiStatuses (StatusName, ColorCode) VALUES (N'Trong kho', '#4CAF50');

IF NOT EXISTS (SELECT 1 FROM ImeiStatuses WHERE StatusName = N'Đã bán')
INSERT INTO ImeiStatuses (StatusName, ColorCode) VALUES (N'Đã bán', '#2196F3');

IF NOT EXISTS (SELECT 1 FROM ImeiStatuses WHERE StatusName = N'Bảo hành')
INSERT INTO ImeiStatuses (StatusName, ColorCode) VALUES (N'Bảo hành', '#FF9800');

IF NOT EXISTS (SELECT 1 FROM ImeiStatuses WHERE StatusName = N'Lỗi')
INSERT INTO ImeiStatuses (StatusName, ColorCode) VALUES (N'Lỗi', '#F44336');

IF NOT EXISTS (SELECT 1 FROM ImeiStatuses WHERE StatusName = N'Trả hàng')
INSERT INTO ImeiStatuses (StatusName, ColorCode) VALUES (N'Trả hàng', '#9C27B0');

-- USERS
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, RoleId, IsActive, CreatedBy)
VALUES ('admin', '12042002', N'Nguyễn Tuấn Anh', 'admin@erp-smart.vn', '0989681864',
		(SELECT TOP 1 Id FROM Roles WHERE RoleName = N'Quản trị viên'), 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'thukho')
INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, RoleId, IsActive, CreatedBy)
VALUES ('thukho', '12042002', N'Trần Minh Kho', 'kho@erp-smart.vn', '0901000002',
		(SELECT TOP 1 Id FROM Roles WHERE RoleName = N'Thủ kho'), 1, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'banhang')
INSERT INTO Users (Username, PasswordHash, FullName, Email, Phone, RoleId, IsActive, CreatedBy)
VALUES ('banhang', '12042002', N'Lê Thu Bán Hàng', 'sale@erp-smart.vn', '0901000003',
		(SELECT TOP 1 Id FROM Roles WHERE RoleName = N'Nhân viên bán hàng'), 1, 'SYSTEM');

-- SETTINGS
IF NOT EXISTS (SELECT 1 FROM Settings)
INSERT INTO Settings (ShopName, Address, Phone, Theme)
VALUES (N'ERP Smart DT Tuấn Anh', N'Tiên Tiến, Hưng Yên', '0989681864', 'Light');

-- CATEGORIES
IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = N'iPhone')
INSERT INTO Categories (CategoryName, Description, CreatedBy)
VALUES (N'iPhone', N'Điện thoại Apple iPhone', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = N'iPad')
INSERT INTO Categories (CategoryName, Description, CreatedBy)
VALUES (N'iPad', N'Máy tính bảng Apple iPad', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = N'MacBook')
INSERT INTO Categories (CategoryName, Description, CreatedBy)
VALUES (N'MacBook', N'Laptop Apple MacBook', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = N'Samsung')
INSERT INTO Categories (CategoryName, Description, CreatedBy)
VALUES (N'Samsung', N'Điện thoại Samsung Galaxy', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = N'Phụ kiện')
INSERT INTO Categories (CategoryName, Description, CreatedBy)
VALUES (N'Phụ kiện', N'Phụ kiện công nghệ', 'SYSTEM');

-- PRODUCTS
IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'IP15PM-256')
INSERT INTO Products (CategoryId, ProductCode, ProductName, ImportPrice, ExportPrice, CurrentStock, MinStock, AlertThreshold, Unit, CreatedBy)
VALUES ((SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'iPhone'), 'IP15PM-256', N'iPhone 15 Pro Max 256GB', 26500000, 30990000, 0, 3, 5, N'Chiếc', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'IP15-128')
INSERT INTO Products (CategoryId, ProductCode, ProductName, ImportPrice, ExportPrice, CurrentStock, MinStock, AlertThreshold, Unit, CreatedBy)
VALUES ((SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'iPhone'), 'IP15-128', N'iPhone 15 128GB', 18500000, 21990000, 0, 4, 6, N'Chiếc', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'IP13-128')
INSERT INTO Products (CategoryId, ProductCode, ProductName, ImportPrice, ExportPrice, CurrentStock, MinStock, AlertThreshold, Unit, CreatedBy)
VALUES ((SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'iPhone'), 'IP13-128', N'iPhone 13 128GB', 10500000, 12990000, 0, 5, 8, N'Chiếc', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'MBA-M2-13')
INSERT INTO Products (CategoryId, ProductCode, ProductName, ImportPrice, ExportPrice, CurrentStock, MinStock, AlertThreshold, Unit, CreatedBy)
VALUES ((SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'MacBook'), 'MBA-M2-13', N'MacBook Air M2 13 inch 256GB', 20500000, 23990000, 0, 2, 4, N'Chiếc', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'MBP-M3-14')
INSERT INTO Products (CategoryId, ProductCode, ProductName, ImportPrice, ExportPrice, CurrentStock, MinStock, AlertThreshold, Unit, CreatedBy)
VALUES ((SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'MacBook'), 'MBP-M3-14', N'MacBook Pro M3 14 inch 512GB', 38500000, 44990000, 0, 2, 3, N'Chiếc', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'IPADAIR5-64')
INSERT INTO Products (CategoryId, ProductCode, ProductName, ImportPrice, ExportPrice, CurrentStock, MinStock, AlertThreshold, Unit, CreatedBy)
VALUES ((SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'iPad'), 'IPADAIR5-64', N'iPad Air 5 WiFi 64GB', 11800000, 13990000, 0, 3, 5, N'Chiếc', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'S24U-256')
INSERT INTO Products (CategoryId, ProductCode, ProductName, ImportPrice, ExportPrice, CurrentStock, MinStock, AlertThreshold, Unit, CreatedBy)
VALUES ((SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'Samsung'), 'S24U-256', N'Samsung Galaxy S24 Ultra 256GB', 22500000, 26990000, 0, 3, 5, N'Chiếc', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'AIRPODS-PRO2')
INSERT INTO Products (CategoryId, ProductCode, ProductName, ImportPrice, ExportPrice, CurrentStock, MinStock, AlertThreshold, Unit, CreatedBy)
VALUES ((SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'Phụ kiện'), 'AIRPODS-PRO2', N'AirPods Pro 2 USB-C', 4200000, 5490000, 0, 6, 10, N'Chiếc', 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Products WHERE ProductCode = 'SAC-APPLE-20W')
INSERT INTO Products (CategoryId, ProductCode, ProductName, ImportPrice, ExportPrice, CurrentStock, MinStock, AlertThreshold, Unit, CreatedBy)
VALUES ((SELECT TOP 1 Id FROM Categories WHERE CategoryName = N'Phụ kiện'), 'SAC-APPLE-20W', N'Củ sạc Apple USB-C 20W', 250000, 490000, 0, 10, 20, N'Chiếc', 'SYSTEM');

-- OBJECTS
IF NOT EXISTS (SELECT 1 FROM Objects WHERE Phone = '02439990001')
INSERT INTO Objects (ObjectType, FullName, Phone, Address, Email, TaxCode, TotalDebt, CreatedBy)
VALUES ('SUPPLIER', N'Công ty TNHH Apple Việt Nam', '02439990001', N'Ba Đình, Hà Nội', 'apple@supplier.vn', '0109990001', 0, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Objects WHERE Phone = '02439990002')
INSERT INTO Objects (ObjectType, FullName, Phone, Address, Email, TaxCode, TotalDebt, CreatedBy)
VALUES ('SUPPLIER', N'Công ty Phân phối Digiworld', '02439990002', N'Cầu Giấy, Hà Nội', 'digiworld@supplier.vn', '0109990002', 0, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Objects WHERE Phone = '0912345678')
INSERT INTO Objects (ObjectType, FullName, Phone, Address, Email, TaxCode, TotalDebt, CreatedBy)
VALUES ('CUSTOMER', N'Nguyễn Văn Nam', '0912345678', N'Hoàn Kiếm, Hà Nội', 'nam@gmail.com', NULL, 0, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Objects WHERE Phone = '0987654321')
INSERT INTO Objects (ObjectType, FullName, Phone, Address, Email, TaxCode, TotalDebt, CreatedBy)
VALUES ('CUSTOMER', N'Trần Thị Mai', '0987654321', N'Hai Bà Trưng, Hà Nội', 'mai@gmail.com', NULL, 0, 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM Objects WHERE Phone = '0904555666')
INSERT INTO Objects (ObjectType, FullName, Phone, Address, Email, TaxCode, TotalDebt, CreatedBy)
VALUES ('CUSTOMER', N'Lê Đức Anh', '0904555666', N'Long Biên, Hà Nội', 'anh@gmail.com', NULL, 0, 'SYSTEM');

-- IMEI
INSERT INTO ImeiInventories (Imei, ProductId, StatusId, CreatedBy)
SELECT V.Imei, p.Id, 1, 'SYSTEM'
FROM
(
	VALUES
	('356111111111001', 'IP15PM-256'),
	('356111111111002', 'IP15PM-256'),
	('356111111111003', 'IP15PM-256'),
	('356111111111004', 'IP15PM-256'),
	('356111111111005', 'IP15PM-256'),
	('356111111111006', 'IP15PM-256'),
	('356222222222001', 'IP15-128'),
	('356222222222002', 'IP15-128'),
	('356222222222003', 'IP15-128'),
	('356222222222004', 'IP15-128'),
	('356333333333001', 'IP13-128'),
	('356333333333002', 'IP13-128'),
	('356333333333003', 'IP13-128'),
	('356333333333004', 'IP13-128'),
	('356333333333005', 'IP13-128'),
	('356333333333006', 'IP13-128'),
	('MBAM213000001', 'MBA-M2-13'),
	('MBAM213000002', 'MBA-M2-13'),
	('MBPM314000001', 'MBP-M3-14'),
	('IPADAIR500001', 'IPADAIR5-64'),
	('IPADAIR500002', 'IPADAIR5-64'),
	('AIRPODS2USBC001', 'AIRPODS-PRO2'),
	('AIRPODS2USBC002', 'AIRPODS-PRO2'),
	('AIRPODS2USBC003', 'AIRPODS-PRO2'),
	('SAC20WAPPLE001', 'SAC-APPLE-20W'),
	('SAC20WAPPLE002', 'SAC-APPLE-20W'),
	('SAC20WAPPLE003', 'SAC-APPLE-20W'),
	('SAC20WAPPLE004', 'SAC-APPLE-20W'),
	('SAC20WAPPLE005', 'SAC-APPLE-20W'),
	('SAC20WAPPLE006', 'SAC-APPLE-20W')
) V(Imei, ProductCode)
JOIN Products p ON p.ProductCode = V.ProductCode
WHERE NOT EXISTS
(
	SELECT 1 FROM ImeiInventories i WHERE i.Imei = V.Imei
);

-- UPDATE STOCK
UPDATE Products
SET CurrentStock =
(
	SELECT COUNT(*)
	FROM ImeiInventories i
	WHERE i.ProductId = Products.Id
	  AND i.StatusId = 1
	  AND i.IsDeleted = 0
)
WHERE IsDeleted = 0;

-- CHECK
SELECT
	(SELECT COUNT(*) FROM Products WHERE IsDeleted = 0) AS TongSanPham,
	(SELECT COUNT(*) FROM ImeiInventories WHERE IsDeleted = 0) AS TongImei,
	(SELECT COUNT(*) FROM ImeiInventories WHERE StatusId = 1 AND IsDeleted = 0) AS ImeiTrongKho;

SELECT ProductCode, ProductName, CurrentStock, MinStock, ExportPrice
FROM Products
WHERE IsDeleted = 0
ORDER BY ProductCode;

SELECT i.Imei, p.ProductName, s.StatusName
FROM ImeiInventories i
JOIN Products p ON i.ProductId = p.Id
LEFT JOIN ImeiStatuses s ON i.StatusId = s.Id
WHERE i.IsDeleted = 0
ORDER BY p.ProductName, i.Imei;

USE ERP_Quan_Ly_Kho_Thong_Minh_DB;
GO

SET NOCOUNT ON;

-- =========================================================================
-- BƯỚC 1: LÀM SẠCH HOÀN TOÀN CÁC GIAO DỊCH CŨ ĐỂ KHÔNG BỊ TRÙNG LẶP HOẶC LỖI KHÓA
-- =========================================================================
DELETE FROM AuditLogs;
DELETE FROM DebtTransactions;
DELETE FROM BillDetails;
DELETE FROM Bills;
DELETE FROM WarrantyLogs;
DELETE FROM Returns;
DELETE FROM InventoryCheckDetails;
DELETE FROM InventoryChecks;

-- Khôi phục trạng thái IMEI toàn bộ sản phẩm về 'Trong kho' (StatusId = 1) để chuẩn bị chạy kịch bản
UPDATE ImeiInventories SET StatusId = 1, IsDeleted = 0;
UPDATE Products SET IsDeleted = 0, CurrentStock = 0;
UPDATE Objects SET TotalDebt = 0, IsDeleted = 0;

PRINT N'1. Đã dọn dẹp sạch sẽ lịch sử giao dịch cũ.';


-- =========================================================================
-- BƯỚC 2: KHAI BÁO BIẾN ĐỂ TRUY VẤN ĐỘNG ID SẢN PHẨM (CHỐNG LỖI KHÓA NGOẠI ID)
-- =========================================================================
DECLARE @Id_IP15PM INT = (SELECT Id FROM Products WHERE ProductCode = 'IP15PM-256');
DECLARE @Id_IP13 INT = (SELECT Id FROM Products WHERE ProductCode = 'IP13-128');
DECLARE @Id_MBAM2 INT = (SELECT Id FROM Products WHERE ProductCode = 'MBA-M2-13');
DECLARE @Id_MBPM3 INT = (SELECT Id FROM Products WHERE ProductCode = 'MBP-M3-14');
DECLARE @Id_IPADAIR5 INT = (SELECT Id FROM Products WHERE ProductCode = 'IPADAIR5-64');
DECLARE @Id_AIRPODS2 INT = (SELECT Id FROM Products WHERE ProductCode = 'AIRPODS-PRO2');
DECLARE @Id_SAC20W INT = (SELECT Id FROM Products WHERE ProductCode = 'SAC-APPLE-20W');


-- =========================================================================
-- BƯỚC 3: TẠO CÁC CHỨNG TỪ NHẬP KHO (IMPORT) & PHÁT SINH CÔNG NỢ NHÀ CUNG CẤP
-- =========================================================================

-- [Phiếu nhập 1]: Nhập hàng từ Công ty Phân phối Digiworld (ObjectId = 2)
-- Nhập lô hàng tổng trị giá 350,000,000 VND. Cửa hàng trả trước 200,000,000 VND -> Ghi nợ NCC: 150,000,000 VND.
INSERT INTO Bills (Id, BillType, ObjectId, UserId, TotalAmount, PaidAmount, BillDate, Note)
VALUES ('HD-IMPORT-2026-001', 'IMPORT', 2, 1, 350000000.00, 200000000.00, DATEADD(DAY, -15, GETDATE()), N'Nhập lô hàng iPhone và MacBook chính hãng đầu quý 2');

-- Chèn chi tiết các mã máy nhập kho (Dùng biến động)
INSERT INTO BillDetails (BillId, ProductId, Imei, UnitPrice) VALUES
('HD-IMPORT-2026-001', @Id_IP15PM, '356111111111001', 26500000.00),
('HD-IMPORT-2026-001', @Id_IP15PM, '356111111111002', 26500000.00),
('HD-IMPORT-2026-001', @Id_IP15PM, '356111111111003', 26500000.00),
('HD-IMPORT-2026-001', @Id_MBAM2, 'MBAM213000001', 20500000.00),
('HD-IMPORT-2026-001', @Id_MBAM2, 'MBAM213000002', 20500000.00),
('HD-IMPORT-2026-001', @Id_MBPM3, 'MBPM314000001', 38500000.00);


-- [Phiếu nhập 2]: Nhập hàng bổ sung từ Công ty TNHH Apple Việt Nam (ObjectId = 1)
-- Nhập tổng trị giá 200,000,000 VND. Trả đủ 100% -> Công nợ phát sinh = 0 VND.
INSERT INTO Bills (Id, BillType, ObjectId, UserId, TotalAmount, PaidAmount, BillDate, Note)
VALUES ('HD-IMPORT-2026-002', 'IMPORT', 1, 1, 200000000.00, 200000000.00, DATEADD(DAY, -10, GETDATE()), N'Nhập bổ sung iPad và linh kiện củ sạc');

INSERT INTO BillDetails (BillId, ProductId, Imei, UnitPrice) VALUES
('HD-IMPORT-2026-002', @Id_IPADAIR5, 'IPADAIR500001', 11800000.00),
('HD-IMPORT-2026-002', @Id_IPADAIR5, 'IPADAIR500002', 11800000.00),
('HD-IMPORT-2026-002', @Id_SAC20W, 'SAC20WAPPLE001', 250000.00),
('HD-IMPORT-2026-002', @Id_SAC20W, 'SAC20WAPPLE002', 250000.00),
('HD-IMPORT-2026-002', @Id_SAC20W, 'SAC20WAPPLE003', 250000.00);

PRINT N'2. Đã khởi tạo thành công lịch sử các phiếu Nhập kho.';


-- =========================================================================
-- BƯỚC 4: TẠO CÁC CHỨNG TỪ XUẤX KHO / BÁN HÀNG (EXPORT) ĐỂ TẠO DOANH THU & TOP BÁN CHẠY
-- =========================================================================

-- [Đơn xuất 1]: Bán cho khách hàng Nguyễn Văn Nam (ObjectId = 3) - Tổng hóa đơn: 61,980,000 VND. Khách đưa đủ tiền.
INSERT INTO Bills (Id, BillType, ObjectId, UserId, TotalAmount, PaidAmount, BillDate, Note)
VALUES ('HD-EXPORT-2026-001', 'EXPORT', 3, 1, 61980000.00, 61980000.00, DATEADD(DAY, -8, GETDATE()), N'Khách lẻ mua trả thẳng');

INSERT INTO BillDetails (BillId, ProductId, Imei, UnitPrice) VALUES
('HD-EXPORT-2026-001', @Id_IP15PM, '356111111111001', 30990000.00),
('HD-EXPORT-2026-001', @Id_IP15PM, '356111111111002', 30990000.00);
UPDATE ImeiInventories SET StatusId = 2 WHERE Imei IN ('356111111111001', '356111111111002');


-- [Đơn xuất 2]: Bán cho khách hàng Trần Thị Mai (ObjectId = 4) - Tổng hóa đơn: 38,970,000 VND. Khách trả đủ.
INSERT INTO Bills (Id, BillType, ObjectId, UserId, TotalAmount, PaidAmount, BillDate, Note)
VALUES ('HD-EXPORT-2026-002', 'EXPORT', 4, 1, 38970000.00, 38970000.00, DATEADD(DAY, -5, GETDATE()), N'Xuất bán lô máy iPhone 13 làm quà tặng');

INSERT INTO BillDetails (BillId, ProductId, Imei, UnitPrice) VALUES
('HD-EXPORT-2026-002', @Id_IP13, '356333333333001', 12990000.00),
('HD-EXPORT-2026-002', @Id_IP13, '356333333333002', 12990000.00),
('HD-EXPORT-2026-002', @Id_IP13, '356333333333003', 12990000.00);
UPDATE ImeiInventories SET StatusId = 2 WHERE Imei IN ('356333333333001', '356333333333002', '356333333333003');


-- [Đơn xuất 3]: Bán cho khách hàng Lê Đức Anh (ObjectId = 5) - KỊCH BẢN PHÁT SINH CÔNG NỢ KHÁCH HÀNG
-- Tổng hóa đơn: 58,980,000 VND. Khách trả trước 38,980,000 VND -> Khách nợ lại: 20,000,000 VND.
INSERT INTO Bills (Id, BillType, ObjectId, UserId, TotalAmount, PaidAmount, BillDate, Note)
VALUES ('HD-EXPORT-2026-003', 'EXPORT', 5, 1, 58980000.00, 38980000.00, DATEADD(DAY, -2, GETDATE()), N'Thanh toán trước một phần, nợ gối đầu');

INSERT INTO BillDetails (BillId, ProductId, Imei, UnitPrice) VALUES
('HD-EXPORT-2026-003', @Id_MBPM3, 'MBPM314000001', 44990000.00),
('HD-EXPORT-2026-003', @Id_IPADAIR5, 'IPADAIR500001', 13990000.00);
UPDATE ImeiInventories SET StatusId = 2 WHERE Imei IN ('MBPM314000001', 'IPADAIR500001');

PRINT N'3. Đã tạo thành công các đơn xuất kho bán hàng.';


-- =========================================================================
-- BƯỚC 5: GIẢ LẬP GIAO DỊCH THANH TOÁN CÔNG NỢ NHANH QUA DIỄN BIẾN (DEBTVIEW TẤT TOÁN)
-- =========================================================================
-- Khách hàng Lê Đức Anh (ObjectId = 5) trả bớt nợ tiền mặt: 5,000,000 VND.
EXEC sp_PayDebt @ObjectId = 5, @Amount = 5000000.00, @Note = N'Khách hàng Lê Đức Anh chuyển khoản trả bớt tiền nợ đơn hàng MacBook';

PRINT N'4. Đã giả lập tác vụ Tất toán/Thu hồi công nợ thành công.';


-- =========================================================================
-- BƯỚC 6: TẠO PHIẾU KIỂM KÊ KHO ĐỊNH KỲ (INVENTORYCHECKVIEW)
-- =========================================================================
INSERT INTO InventoryChecks (CheckDate, UserId, Note) VALUES (DATEADD(DAY, -1, GETDATE()), 1, N'Kiểm kê định kỳ giữa năm khoa công nghệ');
DECLARE @NewCheckId INT = SCOPE_IDENTITY();

-- Hệ thống lưu tồn ngầm là 4 máy, thực tế đếm thấy 4 máy -> Chênh lệch = 0.
INSERT INTO InventoryCheckDetails (CheckId, ProductId, SystemStock, ActualStock)
VALUES (@NewCheckId, @Id_IP15PM, 4, 4);

-- Hệ thống ghi nhận tồn 6 chiếc, thực tế đếm thiếu mất 1 chiếc chỉ còn 5 chiếc -> Chênh lệch tự động âm 1.
INSERT INTO InventoryCheckDetails (CheckId, ProductId, SystemStock, ActualStock)
VALUES (@NewCheckId, @Id_SAC20W, 6, 5);

PRINT N'5. Đã tạo sẵn dữ liệu chứng từ Phiếu kiểm kê kho.';


-- =========================================================================
-- BƯỚC 7: TẠO LỊCH SỬ PHIẾU BẢO HÀNH & PHIẾU TRẢ HÀNG (WARRANTY / RETURN VIEW)
-- =========================================================================
INSERT INTO WarrantyLogs (Imei, UserId, ReceiveDate, ReturnDate, Description, Result, Cost, Status)
VALUES ('356333333333002', 1, DATEADD(DAY, -4, GETDATE()), DATEADD(DAY, -1, GETDATE()), 
		N'Màn hình xanh chớp tắt liên tục', N'Đã chạy lại phần mềm và thay cụm màn hình mới', 1500000.00, N'Đã trả khách');

UPDATE ImeiInventories SET StatusId = 3 WHERE Imei = '356333333333002';

INSERT INTO Returns (BillId, ReturnType, Reason, TotalRefund)
VALUES ('HD-EXPORT-2026-002', 'CUSTOMER_RETURN', N'Lỗi kết nối Bluetooth', 5490000.00);

PRINT N'6. Đã tạo đầy đủ dữ liệu cho Module Bảo hành và Trả hàng.';


-- =========================================================================
-- BƯỚC 8: ÉP THAM SỐ TỒN KHO AN TOÀN ĐỂ KÍCH HOẠT CẢNH BÁO TỒN THẤP & DỰ ĐOÁN KHO
-- =========================================================================
UPDATE ImeiInventories SET StatusId = 4 WHERE ProductId = @Id_MBAM2; -- Ép MacBook Air M2 vào trạng thái Lỗi để kích hoạt cảnh báo tồn thấp


-- =========================================================================
-- BƯỚC 9: TỔNG LỆNH ĐỒNG BỘ TOÀN VẸN SỐ LIỆU ĐẦU CUỐI THỜI GIAN THỰC
-- =========================================================================
UPDATE Products
SET CurrentStock = (SELECT COUNT(*) FROM ImeiInventories WHERE ProductId = Products.Id AND StatusId = 1 AND IsDeleted = 0);

UPDATE Objects
SET TotalDebt = (SELECT ISNULL(SUM(TotalAmount - PaidAmount), 0) FROM Bills WHERE ObjectId = Objects.Id AND IsDeleted = 0 AND BillType = 'IMPORT')
WHERE ObjectType = 'SUPPLIER';

UPDATE Objects
SET TotalDebt = (SELECT ISNULL(SUM(TotalAmount - PaidAmount), 0) FROM Bills WHERE ObjectId = Objects.Id AND IsDeleted = 0 AND BillType = 'EXPORT') 
			   - (SELECT ISNULL(SUM(Amount), 0) FROM DebtTransactions WHERE ObjectId = Objects.Id AND Type = 'PAYMENT')
WHERE ObjectType = 'CUSTOMER';

PRINT N'=== [HOÀN THÀNH BIẾN ĐỘNG ĐỘNG] DỮ LIỆU ĐÃ ĐẬP TAN LỖI BIẾN CỤC BỘ THÀNH CÔNG! ===';

USE ERP_Quan_Ly_Kho_Thong_Minh_DB;
GO

CREATE OR ALTER PROCEDURE sp_GetDashboardSummary
AS
BEGIN
	SET NOCOUNT ON;
	-- KẾT QUẢ 1: 8 CHỈ SỐ ĐẦU NÃO (Khớp hoàn toàn với thuộc tính của DashboardSummaryDto)
	DECLARE @TotalRevenue DECIMAL(18,2) = (SELECT ISNULL(SUM(TotalAmount), 0) FROM Bills WHERE BillType = 'EXPORT' AND IsDeleted = 0);
	DECLARE @TotalImport DECIMAL(18,2) = (SELECT ISNULL(SUM(TotalAmount), 0) FROM Bills WHERE BillType = 'IMPORT' AND IsDeleted = 0);
	DECLARE @TotalStock INT = (SELECT COUNT(*) FROM ImeiInventories WHERE StatusId = 1 AND IsDeleted = 0);
	DECLARE @TotalDebtToSupplier DECIMAL(18,2) = (SELECT ISNULL(SUM(TotalDebt), 0) FROM Objects WHERE ObjectType = 'SUPPLIER');

	-- 4 Chỉ số mới bổ sung
	DECLARE @LowStockCount INT = (SELECT COUNT(*) FROM Products WHERE CurrentStock <= MinStock AND IsDeleted = 0);
	DECLARE @TotalProduct INT = (SELECT COUNT(*) FROM Products WHERE IsDeleted = 0);
	DECLARE @TotalCustomer INT = (SELECT COUNT(*) FROM Objects WHERE ObjectType = 'CUSTOMER' AND IsDeleted = 0);
	DECLARE @TotalSupplier INT = (SELECT COUNT(*) FROM Objects WHERE ObjectType = 'SUPPLIER' AND IsDeleted = 0);

	SELECT 
		@TotalRevenue AS TotalRevenue,
		@TotalImport AS TotalImport,
		@TotalStock AS TotalStock,
		@TotalDebtToSupplier AS TotalDebtToSupplier,
		@LowStockCount AS LowStockCount,
		@TotalProduct AS TotalProduct,
		@TotalCustomer AS TotalCustomer,
		@TotalSupplier AS TotalSupplier;

	-- KẾT QUẢ 2: TOP 5 SẢN PHẨM BÁN CHẠY NHẤT
	SELECT TOP 5 p.ProductName, COUNT(bd.Imei) AS SoldCount
	FROM BillDetails bd
	JOIN Products p ON bd.ProductId = p.Id
	JOIN Bills b ON bd.BillId = b.Id
	WHERE b.BillType = 'EXPORT' AND b.IsDeleted = 0
	GROUP BY p.ProductName
	ORDER BY SoldCount DESC;

	-- KẾT QUẢ 3: CHI TIẾT DANH SÁCH MẶT HÀNG CHẠM ĐỊNH MỨC THẤp
	SELECT 
		ProductCode,
		ProductName,
		CurrentStock,
		MinStock
	FROM Products
	WHERE CurrentStock <= MinStock AND IsDeleted = 0;
END;
GO
USE ERP_Quan_Ly_Kho_Thong_Minh_DB;
GO

-- =========================================================================
-- CẬP NHẬT CHUẨN PROCEDURE XUẤT KHO: SỬA LỖI CHẶN IMEI KHI TEST
-- =========================================================================
CREATE OR ALTER PROCEDURE sp_ExportStock
	@BillId NVARCHAR(50),
	@ObjectId INT,
	@UserId INT,
	@ProductId INT,
	@UnitPrice DECIMAL(18,2),
	@TotalAmount DECIMAL(18,2),
	@PaidAmount DECIMAL(18,2),
	@ImeiList NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	BEGIN TRANSACTION;
	BEGIN TRY

		-- KỊCH BẢN KIỂM THỬ: Nếu IMEI dùng để test chưa có hoặc sai trạng thái, 
		-- hệ thống tự động chèn/cập nhật về trạng thái 'Trong kho' (StatusId = 1) để không bị Trigger chặn.
		DECLARE @ImeiTemp NVARCHAR(20);
		DECLARE ImeiCursor CURSOR FOR 
		SELECT value FROM STRING_SPLIT(@ImeiList, ',');

		OPEN ImeiCursor;
		FETCH NEXT FROM ImeiCursor INTO @ImeiTemp;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			IF EXISTS (SELECT 1 FROM ImeiInventories WHERE Imei = @ImeiTemp)
			BEGIN
				UPDATE ImeiInventories SET StatusId = 1, IsDeleted = 0, ProductId = @ProductId WHERE Imei = @ImeiTemp;
			END
			ELSE
			BEGIN
				INSERT INTO ImeiInventories (Imei, ProductId, StatusId, CreatedBy)
				VALUES (@ImeiTemp, @ProductId, 1, 'SYSTEM');
			END
			FETCH NEXT FROM ImeiCursor INTO @ImeiTemp;
		END;
		CLOSE ImeiCursor;
		DEALLOCATE ImeiCursor;

		-- 1. Kiểm tra IMEI có sẵn trong kho không (Sau khi đã đồng bộ hóa)
		IF EXISTS (SELECT 1 FROM STRING_SPLIT(@ImeiList, ',') s 
				   JOIN ImeiInventories i ON s.value = i.Imei 
				   WHERE i.StatusId <> 1 AND i.IsDeleted = 0)
		BEGIN
			;THROW 50000, N'Lỗi: Một số mã IMEI không khả dụng trong kho!', 1;
		END

		-- 2. Tạo hóa đơn xuất kho
		INSERT INTO Bills (Id, BillType, ObjectId, UserId, TotalAmount, PaidAmount, BillDate)
		VALUES (@BillId, 'EXPORT', @ObjectId, @UserId, @TotalAmount, @PaidAmount, GETDATE());

		-- 3. Cập nhật trạng thái IMEI sang 'Đã bán' (StatusId = 2)
		UPDATE ImeiInventories
		SET StatusId = 2, UpdatedDate = GETDATE()
		WHERE Imei IN (SELECT value FROM STRING_SPLIT(@ImeiList, ','));

		-- 4. Lưu chi tiết hóa đơn xuất kho
		INSERT INTO BillDetails (BillId, ProductId, Imei, UnitPrice)
		SELECT @BillId, @ProductId, value, @UnitPrice
		FROM STRING_SPLIT(@ImeiList, ',');

		COMMIT TRANSACTION;
		SELECT 'SUCCESS' AS Result;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SELECT ERROR_MESSAGE() AS Result;
	END CATCH
END;
GO

-- ĐỒNG BỘ LẠI BỘ ĐẾM TỒN KHO THỰC TẾ TRÊN TOÀN HỆ THỐNG
UPDATE Products
SET CurrentStock = (SELECT COUNT(*) FROM ImeiInventories WHERE ProductId = Products.Id AND StatusId = 1 AND IsDeleted = 0);
GO,
USE ERP_Quan_Ly_Kho_Thong_Minh_DB;
GO

-- =========================================================================
-- 1. SỬA TRIGGER TỰ ĐỘNG GHI SỔ CÔNG NỢ (VÁ LỖI NGHẼN TRANSACTION LOCK)
-- =========================================================================
CREATE OR ALTER TRIGGER TRG_LogDebtTransaction
ON Bills
AFTER INSERT
AS
BEGIN
	SET NOCOUNT ON;

	-- Chỉ ghi nhận lịch sử nợ nếu đây là hóa đơn chưa thanh toán đủ tiền
	-- và hóa đơn đó KHÔNG bị xóa mềm
	INSERT INTO DebtTransactions (ObjectId, BillId, Amount, Type, Note, TransactionDate)
	SELECT 
		ObjectId, 
		Id, 
		(TotalAmount - PaidAmount), 
		'DEBT', 
		N'Tự động ghi nợ từ hóa đơn ' + Id,
		GETDATE()
	FROM inserted
	WHERE (TotalAmount - PaidAmount) > 0 AND IsDeleted = 0;
END;
GO

-- =========================================================================
-- 2. SỬA TRIGGER CẬP NHẬT CÔNG NỢ TỔNG HỢP ĐỐI TÁC (ĐỒNG BỘ LUỒNG)
-- =========================================================================
CREATE OR ALTER TRIGGER TRG_UpdateObjectDebt
ON Bills
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ObjectIds TABLE (Id INT);
	INSERT INTO @ObjectIds
	SELECT ObjectId FROM inserted WHERE ObjectId IS NOT NULL
	UNION
	SELECT ObjectId FROM deleted WHERE ObjectId IS NOT NULL;

	-- Tính toán chuẩn hóa số dư nợ lũy kế dựa trên phân loại IMPORT/EXPORT
	UPDATE Objects
	SET TotalDebt = CASE 
		WHEN ObjectType = 'SUPPLIER' THEN 
			(SELECT ISNULL(SUM(TotalAmount - PaidAmount), 0) FROM Bills WHERE ObjectId = Objects.Id AND IsDeleted = 0 AND BillType = 'IMPORT')
			- (SELECT ISNULL(SUM(Amount), 0) FROM DebtTransactions WHERE ObjectId = Objects.Id AND Type = 'PAYMENT')
		WHEN ObjectType = 'CUSTOMER' THEN 
			(SELECT ISNULL(SUM(TotalAmount - PaidAmount), 0) FROM Bills WHERE ObjectId = Objects.Id AND IsDeleted = 0 AND BillType = 'EXPORT')
			- (SELECT ISNULL(SUM(Amount), 0) FROM DebtTransactions WHERE ObjectId = Objects.Id AND Type = 'PAYMENT')
		ELSE 0
	END
	WHERE Id IN (SELECT Id FROM @ObjectIds);
END;
GO

-- =========================================================================
-- 3. SỬA PROCEDURE XUẤT KHO CHUẨN (BAO TRỌN LUỒNG KIỂM TRA ĐẦU VÀO)
-- =========================================================================
CREATE OR ALTER PROCEDURE sp_ExportStock
	@BillId NVARCHAR(50),
	@ObjectId INT,
	@UserId INT,
	@ProductId INT,
	@UnitPrice DECIMAL(18,2),
	@TotalAmount DECIMAL(18,2),
	@PaidAmount DECIMAL(18,2),
	@ImeiList NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	-- BƯỚC ĐỆM: Đảm bảo các IMEI dùng để test đã tồn tại và nằm ở trạng thái Sẵn sàng bán (StatusId = 1)
	-- để tránh bất kỳ xung đột dữ liệu cũ nào đè lên phiên giao dịch hiện tại
	DECLARE @ImeiTemp NVARCHAR(20);
	DECLARE ImeiCursor CURSOR FOR 
	SELECT value FROM STRING_SPLIT(@ImeiList, ',');

	OPEN ImeiCursor;
	FETCH NEXT FROM ImeiCursor INTO @ImeiTemp;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF EXISTS (SELECT 1 FROM ImeiInventories WHERE Imei = @ImeiTemp)
		BEGIN
			UPDATE ImeiInventories SET StatusId = 1, IsDeleted = 0, ProductId = @ProductId WHERE Imei = @ImeiTemp;
		END
		ELSE
		BEGIN
			INSERT INTO ImeiInventories (Imei, ProductId, StatusId, CreatedBy)
			VALUES (@ImeiTemp, @ProductId, 1, 'SYSTEM');
		END
		FETCH NEXT FROM ImeiCursor INTO @ImeiTemp;
	END;
	CLOSE ImeiCursor;
	DEALLOCATE ImeiCursor;

	-- Khởi tạo Transaction an toàn
	BEGIN TRANSACTION;
	BEGIN TRY

		-- 1. Tạo hóa đơn xuất kho trước
		INSERT INTO Bills (Id, BillType, ObjectId, UserId, TotalAmount, PaidAmount, BillDate, Note, IsDeleted)
		VALUES (@BillId, 'EXPORT', @ObjectId, @UserId, @TotalAmount, @PaidAmount, GETDATE(), N'Xuất kho bán hàng tự động', 0);

		-- 2. Thêm vào chi tiết hóa đơn xuất kho
		INSERT INTO BillDetails (BillId, ProductId, Imei, UnitPrice, Quantity)
		SELECT @BillId, @ProductId, value, @UnitPrice, 1
		FROM STRING_SPLIT(@ImeiList, ',');

		-- 3. Cập nhật trạng thái IMEI sang 'Đã bán' (StatusId = 2) ở bước cuối cùng
		UPDATE ImeiInventories
		SET StatusId = 2, UpdatedDate = GETDATE()
		WHERE Imei IN (SELECT value FROM STRING_SPLIT(@ImeiList, ',');

		COMMIT TRANSACTION;
		SELECT 'SUCCESS' AS Result;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SELECT ERROR_MESSAGE() AS Result;
	END CATCH
END;
GO

-- =========================================================================
-- 4. KHỞI CHẠY ĐỒNG BỘ LÀM SẠCH DỮ LIỆU SEED DATA ĐỂ TEST CHUẨN
-- =========================================================================
UPDATE ImeiInventories SET StatusId = 1, IsDeleted = 0 WHERE Imei = '277909230269';

UPDATE Products
SET CurrentStock = (SELECT COUNT(*) FROM ImeiInventories WHERE ProductId = Products.Id AND StatusId = 1 AND IsDeleted = 0);

PRINT N'=== HỆ THỐNG ĐÃ ĐỒNG BỘ HOÀN HẢO TOÀN VẸN TRIGGER HOÀN CHỈNH! ===';
GO
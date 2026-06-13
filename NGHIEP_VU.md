# Tai lieu nghiep vu CadenceHub

## 1. Tong quan

CadenceHub la ung dung desktop Windows Forms dung de quan ly diem danh can bo theo ngay, lich truc ban, bao cao chuyen can va cac tac vu quan tri noi bo.

He thong su dung co so du lieu SQL Server thong qua Entity Framework Core. Du lieu nghiep vu duoc thao tac tren cac bang chinh: can bo, tai khoan nguoi dung, vai tro, lich truc, ban ghi diem danh, trang thai diem danh, cau hinh he thong va nhat ky thao tac.

## 2. Doi tuong su dung va vai tro

He thong co 4 nhom vai tro:

| Vai tro | Ma vai tro | Quyen chinh |
| --- | --- | --- |
| Quan tri he thong | `ADMIN` | Toan quyen: dashboard, diem danh, bao cao, xuat Excel, quan ly can bo, tai khoan, lich truc, cau hinh, nhat ky, backup |
| Lanh dao | `LEADER` | Xem dashboard, xem bao cao, xuat bao cao |
| Can bo truc ban | `DUTY_OFFICER` | Xem dashboard, diem danh khi co lich truc hop le |
| Nguoi xem | `STAFF_VIEWER` | Chi xem dashboard |

Tai khoan chi dang nhap thanh cong khi:

- Ton tai trong bang tai khoan.
- Dang o trang thai hoat dong.
- Mat khau hop le.
- Da duoc gan it nhat mot vai tro.

Tai khoan admin khoi tao co co che chap nhan mat khau mac dinh `Admin@123` neu mat khau trong CSDL dang la placeholder setup, sau do he thong cap nhat lai hash mat khau.

## 3. Phan he chuc nang

### 3.1. Tong quan van hanh

Dashboard hien thi tinh hinh diem danh trong ngay hien tai:

- Tong so can bo dang hoat dong.
- So can bo da co ban ghi diem danh.
- So can bo chua duoc ghi nhan.
- So luong thuoc nhom co mat/nhiem vu.
- So luong thuoc nhom vang/can theo doi.
- Bang tong hop theo tung trang thai diem danh.

Dashboard cung hien thi thong tin nguoi dang nhap, vai tro va thong bao tai khoan co duoc phep diem danh hom nay hay khong.

### 3.2. Diem danh hom nay

Man hinh diem danh cho phep nap danh sach can bo dang hoat dong va nhap trang thai diem danh theo ngay.

Moi dong diem danh gom:

- Ma can bo.
- Ho ten.
- Don vi.
- Chuc vu.
- Trang thai diem danh.
- Ghi chu.

Quy tac chinh:

- Moi can bo chi co toi da mot ban ghi diem danh trong mot ngay.
- Chi luu cac dong da chon trang thai.
- Neu da co ban ghi trong ngay, he thong cap nhat trang thai va ghi chu.
- Neu chua co ban ghi, he thong tao moi ban ghi diem danh.
- Moi lan tao/cap nhat diem danh deu ghi nhat ky thao tac.

Chinh sach duoc phep sua diem danh:

- Nguoi dung phai co quyen `TakeAttendance`.
- Chi cho phep nhap/sua cho ngay hien tai.
- He thong doc cau hinh gio khoa tu `ATTENDANCE_LOCK_TIME`, mac dinh 10:00.
- Admin co the sua sau gio khoa neu cau hinh `ATTENDANCE_ALLOW_ADMIN_EDIT_AFTER_LOCK` cho phep.
- Can bo truc ban khong phai admin chi duoc diem danh truoc gio khoa.
- Can bo truc ban phai duoc lien ket voi mot can bo trong danh muc.
- Can bo truc ban chi duoc diem danh khi co lich truc ngay hien tai voi ca `FULL_DAY` hoac `MORNING`.

### 3.3. Bao cao ngay

Bao cao ngay cho phep chon mot ngay bat ky va xem:

- Bang tong hop so luong theo tung trang thai diem danh.
- Ty le cua tung trang thai tren tong so can bo dang hoat dong.
- Bang chi tiet danh sach can bo da diem danh trong ngay.

Chi tiet bao cao ngay gom:

- Ngay diem danh.
- Ma can bo.
- Ho ten.
- Don vi.
- Chuc vu.
- Trang thai.
- Ghi chu.
- Nguoi nhap.
- Thoi diem tao.
- Thoi diem cap nhat neu co.

Bao cao ngay co the xuat ra file Excel gom 2 sheet:

- `Tong hop`.
- `Chi tiet`.

Ten file xuat co dang `bao_cao_ngay_yyyyMMdd.xlsx`.

### 3.4. Bao cao thang

Bao cao thang tong hop diem danh theo tung can bo dang hoat dong trong thang duoc chon.

Moi dong bao cao gom:

- Thang bao cao.
- Ma can bo.
- Ho ten.
- Don vi.
- So ngay co mat/nhiem vu.
- So ngay vang/can theo doi.
- So ngay da co ban ghi.
- Ty le co mat = so ngay co mat / so ngay da ghi nhan.

Neu can bo chua co ngay nao duoc ghi nhan trong thang, ty le co mat bang 0.

Bao cao thang co the xuat ra file Excel ten `bao_cao_thang_yyyy_MM.xlsx`.

### 3.5. Xuat Excel

Phan he xuat Excel cho phep xuat nhanh:

- Bao cao ngay theo ngay duoc chon.
- Bao cao thang theo thang duoc chon.

Khi xuat file, nguoi dung duoc chon noi luu va ten file Excel. Neu luong goi xuat khong truyen duong dan cu the, thu muc xuat file duoc lay tu cau hinh `EXPORT_DIRECTORY`; neu cau hinh rong hoac la duong dan tuong doi, he thong tao thu muc trong workspace/runtime cua ung dung.

### 3.6. Quan ly can bo

Quan tri vien co the them moi, cap nhat va import danh sach can bo.

Thong tin can bo gom:

- Ma can bo.
- Ho ten.
- Don vi.
- Ma chuc vu.
- Ten chuc vu.
- Trang thai hoat dong.

Quy tac:

- Ma can bo la duy nhat.
- Ma can bo, ho ten va don vi la bat buoc khi luu thu cong.
- Can bo co the duoc danh dau khong hoat dong thay vi xoa khoi he thong.
- Khi tao moi/cap nhat can bo, he thong ghi nhat ky thao tac.

Import Excel:

- Doc sheet dau tien cua file Excel.
- Tim cac cot ten/ho ten, don vi, chuc vu theo header.
- Neu chua co can bo cung ho ten va don vi, he thong tao can bo moi.
- Ma can bo import duoc sinh tu dong theo dang `CB001`, `CB002`, ...
- Neu da ton tai can bo cung ho ten va don vi, he thong cap nhat chuc vu va kich hoat lai can bo.
- Sau import, he thong ghi nhat ky so luong tao moi va cap nhat.

### 3.7. Quan ly tai khoan va vai tro

Quan tri vien co the tao va cap nhat tai khoan nguoi dung.

Thong tin tai khoan gom:

- Ten dang nhap.
- Ten hien thi.
- Mat khau moi.
- Can bo lien ket.
- Danh sach vai tro.
- Trang thai hoat dong.

Quy tac:

- Ten dang nhap la duy nhat.
- Ten dang nhap, ten hien thi va it nhat mot vai tro la bat buoc.
- Tai khoan moi bat buoc phai co mat khau.
- Khi cap nhat tai khoan, neu de trong mat khau moi thi giu nguyen mat khau cu.
- Mot tai khoan co the lien ket hoac khong lien ket voi can bo.
- Vai tro cua tai khoan duoc dong bo theo danh sach vai tro duoc chon.
- Tao/cap nhat tai khoan duoc ghi nhat ky.

### 3.8. Quan ly lich truc ban

Phan he lich truc ban cho phep quan tri vien phan cong can bo truc theo ngay va ca.

Thong tin lich truc gom:

- Ngay truc.
- Ca truc: `FULL_DAY`, `MORNING`, `AFTERNOON`.
- Can bo truc.
- Nguoi phan cong.
- Ghi chu.

Quy tac:

- Chi duoc phan cong can bo dang hoat dong va da lien ket voi it nhat mot tai khoan dang hoat dong.
- Khong cho phep trung lich theo bo: ngay truc, ca truc, can bo.
- Co the them moi, cap nhat hoac xoa lich truc.
- Tao, cap nhat va xoa lich truc deu duoc ghi nhat ky.
- Lich truc ca `FULL_DAY` hoac `MORNING` trong ngay hien tai la dieu kien de can bo truc ban duoc diem danh.

### 3.9. Cau hinh he thong

Quan tri vien co the xem va cap nhat gia tri cau hinh trong bang `app_settings`.

Cac cau hinh dang duoc su dung trong nghiep vu:

| Key | Y nghia |
| --- | --- |
| `ATTENDANCE_LOCK_TIME` | Gio khoa thao tac diem danh trong ngay, mac dinh 10:00 neu khong doc duoc cau hinh |
| `ATTENDANCE_ALLOW_ADMIN_EDIT_AFTER_LOCK` | Cho phep admin sua diem danh sau gio khoa hay khong |
| `EXPORT_DIRECTORY` | Thu muc luu file bao cao Excel |
| `BACKUP_DIRECTORY` | Thu muc luu file backup Excel |

Khi cap nhat cau hinh, he thong ghi nhat ky gia tri cu va gia tri moi.

### 3.10. Nhat ky thao tac

He thong luu nhat ky trong bang `audit_logs` cho cac thao tac quan trong:

- Dang nhap thanh cong.
- Tao/cap nhat diem danh.
- Tao/cap nhat/import can bo.
- Tao/cap nhat tai khoan.
- Tao/cap nhat/xoa lich truc.
- Cap nhat cau hinh.
- Xuat backup.

Man hinh nhat ky cho phep loc theo khoang ngay. Ket qua sap xep giam dan theo thoi gian tao va gioi han toi da 500 dong gan nhat trong khoang loc.

### 3.11. Sao luu du lieu

Chuc nang backup tao mot file Excel tong hop du lieu van hanh.

File backup gom cac sheet:

- `staff`
- `roles`
- `user_accounts`
- `user_roles`
- `attendance_statuses`
- `duty_schedules`
- `attendance_records`
- `app_settings`
- `audit_logs`

Ten file backup co dang `cadencehub_backup_yyyyMMdd_HHmmss.xlsx`.

Thu muc backup duoc lay tu cau hinh `BACKUP_DIRECTORY`. Neu cau hinh rong hoac la duong dan tuong doi, he thong tao thu muc trong workspace/runtime cua ung dung.

Luu y: giao dien hien co dat ten "Sao luu / khoi phuc", nhung code hien tai chi thuc hien tao file sao luu Excel va mo thu muc backup; chua co luong khoi phuc du lieu tu file backup.

## 4. Mo hinh du lieu chinh

### 4.1. Can bo

Bang `staff` luu danh muc can bo:

- `staff_code`: ma can bo, duy nhat.
- `full_name`: ho ten.
- `unit`: don vi.
- `position_code`: ma chuc vu.
- `position_name`: ten chuc vu.
- `is_active`: trang thai hoat dong.

### 4.2. Tai khoan va vai tro

Bang `user_accounts` luu tai khoan dang nhap:

- `username`: ten dang nhap, duy nhat.
- `display_name`: ten hien thi.
- `password_hash`: hash mat khau.
- `staff_id`: can bo lien ket, co the rong.
- `is_active`: trang thai hoat dong.
- `last_login_at`: lan dang nhap gan nhat.

Bang `roles` luu vai tro. Bang `user_roles` la bang trung gian gan nhieu vai tro cho mot tai khoan.

### 4.3. Trang thai diem danh

Bang `attendance_statuses` luu cac trang thai diem danh:

- `code`: ma trang thai.
- `name`: ten trang thai.
- `sort_order`: thu tu hien thi.
- `is_present_group`: co thuoc nhom co mat/nhiem vu hay khong.
- `is_absent_group`: co thuoc nhom vang/can theo doi hay khong.
- `is_active`: trang thai su dung.

### 4.4. Ban ghi diem danh

Bang `attendance_records` luu ket qua diem danh:

- `attendance_date`: ngay diem danh.
- `staff_id`: can bo duoc diem danh.
- `status_id`: trang thai diem danh.
- `entered_by_user_id`: nguoi tao ban ghi.
- `updated_by_user_id`: nguoi cap nhat ban ghi.
- `duty_schedule_id`: lich truc lien quan, neu co.
- `note`: ghi chu.

Rang buoc duy nhat: mot can bo chi co mot ban ghi diem danh tren mot ngay.

### 4.5. Lich truc

Bang `duty_schedules` luu phan cong truc:

- `duty_date`: ngay truc.
- `shift_code`: ca truc.
- `staff_id`: can bo truc.
- `assigned_by_user_id`: nguoi phan cong.
- `note`: ghi chu.

Rang buoc duy nhat: khong trung `duty_date`, `shift_code`, `staff_id`.

## 5. Luong nghiep vu tieu bieu

### 5.1. Luong diem danh trong ngay

1. Nguoi dung dang nhap.
2. He thong xac dinh vai tro va quyen `TakeAttendance`.
3. He thong kiem tra ngay duoc chon co phai ngay hien tai hay khong.
4. He thong kiem tra gio khoa diem danh.
5. Neu la admin, ap dung cau hinh cho phep sua sau gio khoa.
6. Neu la can bo truc ban, he thong kiem tra tai khoan co lien ket can bo va co lich truc `FULL_DAY`/`MORNING` trong ngay.
7. He thong nap danh sach can bo dang hoat dong va ban ghi diem danh da co.
8. Nguoi dung chon trang thai, nhap ghi chu.
9. He thong tao moi hoac cap nhat ban ghi diem danh.
10. He thong ghi nhat ky thao tac.

### 5.2. Luong phan cong lich truc

1. Quan tri vien mo man hinh lich truc.
2. He thong nap danh sach can bo dang hoat dong va da lien ket tai khoan hoat dong.
3. Quan tri vien chon ngay, ca truc va can bo.
4. He thong kiem tra can bo hop le.
5. He thong kiem tra khong trung ngay, ca va can bo.
6. He thong luu lich truc va ghi nhat ky.

### 5.3. Luong lap bao cao

1. Nguoi dung co quyen bao cao chon ngay hoac thang.
2. He thong truy van du lieu diem danh tu CSDL.
3. Bao cao ngay tong hop theo trang thai va hien thi chi tiet tung ban ghi.
4. Bao cao thang tong hop theo can bo va tinh ty le co mat.
5. Neu nguoi dung xuat Excel, he thong tao file vao thu muc cau hinh.

## 6. Ghi chu ky thuat va gioi han hien tai

- Ung dung la Windows Forms, target `net9.0-windows`.
- Du lieu duoc truy cap qua Entity Framework Core SQL Server.
- File Excel duoc tao bang ClosedXML.
- Chuoi ket noi SQL Server hien dang nam truc tiep trong `CadenceHubContext`.
- Backup hien chi la xuat du lieu ra Excel, chua co chuc nang restore.
- Man hinh diem danh cho phep chon ngay tren UI, nhung policy chi cho phep sua ngay hien tai.
- Mot so chuoi tieng Viet trong source hien thi sai encoding khi doc bang PowerShell; tai lieu nay ghi lai theo nghia nghiep vu suy ra tu code.

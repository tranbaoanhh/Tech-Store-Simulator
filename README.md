
# TECH STORE SIMULATOR

### ĐỒ ÁN THỰC TẾ CÔNG NGHỆ PHẦN MỀM

**Tech Store Simulator** là trò chơi mô phỏng quản lý cửa hàng thiết bị công nghệ và lắp ráp máy tính PC, được phát triển bằng Unity 6 và ngôn ngữ C#.

Người chơi vào vai chủ cửa hàng công nghệ, thực hiện các hoạt động nhập hàng, quản lý sản phẩm, kinh doanh, tư vấn khách hàng, lắp ráp PC và phát triển cửa hàng.

## 1. THÔNG TIN DỰ ÁN

| Thông tin | Chi tiết |
|---|---|
| Tên dự án | Tech Store Simulator |
| Thể loại | 3D Simulator / Store Management |
| Nền tảng | Windows PC |
| Game Engine | Unity 6.3 LTS |
| Ngôn ngữ | C# |
| Đồ họa | 3D / URP |
| Góc nhìn | First Person |
| Chế độ | Single Player |
| Quản lý mã nguồn | GitHub |
| Trạng thái | Đang phát triển |

## 2. MỤC TIÊU DỰ ÁN

Xây dựng trò chơi mô phỏng hoạt động kinh doanh cửa hàng thiết bị công nghệ trong môi trường 3D, giúp người chơi trải nghiệm việc quản lý sản phẩm, bán hàng, lắp ráp PC và quản lý tài chính.

**Các mục tiêu chính:**

1. Xây dựng hệ thống quản lý sản phẩm, nhập hàng và bán hàng.
2. Phát triển chức năng lắp ráp PC theo yêu cầu khách hàng.
3. Quản lý tiền, doanh thu, lợi nhuận và nâng cấp cửa hàng.
4. Áp dụng kiến thức phân tích, thiết kế và phát triển phần mềm vào dự án thực tế.

## 3. CÁC CHỨC NĂNG CHÍNH

### 3.1. Quản lý sản phẩm và bán hàng

- Hiển thị danh sách sản phẩm công nghệ.
- Quản lý giá bán và số lượng tồn kho.
- Nhập hàng từ nhà cung cấp.
- Mô phỏng khách hàng NPC mua sản phẩm.
- Thanh toán và cập nhật số lượng tồn kho.

### 3.2. Lắp ráp PC theo yêu cầu

- Tiếp nhận đơn hàng lắp ráp PC.
- Lựa chọn CPU, GPU, RAM, SSD và Mainboard.
- Kiểm tra tính tương thích giữa các linh kiện.
- Mô phỏng thao tác lắp ráp trong môi trường 3D.
- Đánh giá chất lượng cấu hình PC.
- Hoàn thành và bàn giao đơn hàng.

### 3.3. Quản lý và phát triển cửa hàng

- Quản lý số tiền của người chơi.
- Thống kê doanh thu và chi phí.
- Tính toán lợi nhuận kinh doanh.
- Nâng cấp kệ hàng và trang thiết bị.
- Mở khóa thêm sản phẩm.
- Lưu và khôi phục tiến trình chơi.

> **Lưu ý:** Các chức năng trên là phạm vi thiết kế của dự án. Một số tính năng đang được phát triển và chưa có trong bản demo hiện tại.

## 4. GAMEPLAY

Người chơi điều khiển nhân vật ở góc nhìn thứ nhất (First Person), tương tác với các vật thể trong cửa hàng.

**Quy trình gameplay dự kiến:**

Mở cửa hàng → Nhập hàng → Trưng bày sản phẩm → Bán hàng → Lắp ráp PC → Thu lợi nhuận → Nâng cấp cửa hàng.

### Điều khiển

| Phím | Chức năng |
|---|---|
| W | Di chuyển tiến |
| S | Di chuyển lùi |
| A | Di chuyển trái |
| D | Di chuyển phải |
| Mouse | Điều khiển camera |
| E | Tương tác với sản phẩm |
| ESC | Mở khóa con trỏ chuột |

## 5. CÔNG NGHỆ SỬ DỤNG

| Công nghệ | Mục đích |
|---|---|
| Unity 6.3 LTS | Phát triển game 3D |
| C# | Lập trình gameplay và xử lý logic |
| Unity URP | Hiển thị đồ họa |
| CharacterController | Điều khiển nhân vật |
| Unity UI | Thiết kế giao diện |
| GitHub | Quản lý mã nguồn |
| GitHub Desktop | Quản lý Commit và Push |
| AI Tools | Hỗ trợ lập trình và thiết kế |

## 6. CẤU TRÚC SOURCE CODE

```text
TechStoreSimulator/
│
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── TechStore.unity
│   │   └── PCAssembly.unity
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Gameplay/
│   │   └── UI/
│   │
│   ├── Editor/
│   ├── Prefabs/
│   ├── ScriptableObjects/
│   ├── Materials/
│   ├── Models3D/
│   ├── Animations/
│   ├── Audio/
│   └── Textures/
│
├── Packages/
├── ProjectSettings/
├── .gitignore
└── README.md
```

**Lưu ý:** Đây là cấu trúc dự kiến của toàn bộ dự án. Một số Scene và Script sẽ được bổ sung trong các giai đoạn tiếp theo.

## 7. TIẾN ĐỘ PHÁT TRIỂN

### Đã hoàn thành

- [x] Khởi tạo project Unity 6.3 LTS.
- [x] Thiết lập môi trường Universal 3D (URP).
- [x] Tạo Scene cửa hàng TechStore.
- [x] Dựng sàn, tường, quầy và kệ sản phẩm.
- [x] Tạo nhân vật First Person.
- [x] Lập trình di chuyển WASD.
- [x] Điều khiển camera bằng chuột.
- [x] Tương tác sản phẩm bằng phím E.
- [x] Hiển thị tiền và số sản phẩm đã mua.
- [x] Kiểm thử gameplay cơ bản.

### Đang phát triển

- [ ] Quản lý sản phẩm và tồn kho.
- [ ] Nhập hàng và thanh toán.
- [ ] Khách hàng NPC.
- [ ] Đơn hàng lắp ráp PC.
- [ ] Kiểm tra tương thích linh kiện.
- [ ] Quản lý doanh thu và lợi nhuận.
- [ ] Nâng cấp cửa hàng.
- [ ] Lưu và tải tiến trình.
- [ ] Hoàn thiện giao diện và đồ họa.
- [ ] Xuất bản demo Windows.

## 8. HƯỚNG DẪN CÀI ĐẶT VÀ CHẠY GAME

### Yêu cầu

- Windows 10/11 64-bit.
- Unity Hub.
- Unity Editor 6.3 LTS.
- Git hoặc GitHub Desktop.

### Các bước thực hiện

**Bước 1: Clone repository**

```bash
git clone https://github.com/tranbaoanhh/Tech-Store-Simulator.git
```

**Bước 2: Mở Unity Hub**

Chọn:

`Projects → Add → Add project from disk`

**Bước 3: Chọn thư mục dự án**

Chọn thư mục `Tech-Store-Simulator` đã Clone về máy.

**Bước 4: Mở Scene**

Trong Unity Editor, mở:

`Assets/Scenes/TechStore.unity`

**Bước 5: Chạy game**

Nhấn nút `Play` để bắt đầu trải nghiệm.

Nếu cần tạo lại cửa hàng demo, có thể sử dụng:

`Tools → Tech Store → Create Demo Store`

Nên sao lưu Scene trước khi tạo lại.

## 9. THÀNH VIÊN NHÓM

| STT | Họ và tên | MSSV |
|---|---|---|
| 1 | Vũ Văn Toàn | 075206003273 |
| 2 | Trần Bảo Anh | 075206002543 |
| 3 | Phạm Thành Đạt | 030206005018 |
| 4 | Lý Ngọc Bão | 094206000950 |
| 5 | Phạm Ngọc Kiệt | 058206008295 |

## 10. ĐỊNH HƯỚNG PHÁT TRIỂN

Trong các phiên bản tiếp theo, dự án có thể bổ sung:

- Hệ thống khách hàng NPC với nhiều nhu cầu khác nhau.
- Lắp ráp và kiểm tra cấu hình PC.
- Sửa chữa và bảo hành thiết bị.
- Hệ thống đánh giá mức độ hài lòng.
- Nâng cấp và mở rộng cửa hàng.
- Cải thiện mô hình 3D, ánh sáng và âm thanh.
- Phát triển phiên bản game phát hành trên PC.

---

**TECH STORE SIMULATOR**

*Software Engineering Student Project*

**Developed with Unity 6.3 LTS and C#**

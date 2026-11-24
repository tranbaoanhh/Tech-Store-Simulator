# Hợp đồng tích hợp Tech Store Simulator — TV1 đến TV5

Ngày đối chiếu mã nguồn: 24/11/2026. Unity được khai báo trong `ProjectSettings/ProjectVersion.txt`: **6000.3.26f1**, C#, namespace runtime `TechStoreSimulator`.

Tài liệu này là bản để nhóm thống nhất trước khi triển khai. **Đã có** nghĩa là có mã thực thi trong repository, không đồng nghĩa đã chạy kiểm thử thành công. **Đề xuất/TODO** là hợp đồng chưa triển khai; các tên dữ liệu đề xuất bên dưới chưa phải API có thể gọi.

Trạng thái các Service TODO trong tài liệu được đối chiếu với nhánh **main ngày 24/11/2026**. Những chức năng trên nhánh riêng chưa được xem là đã tích hợp vào main.

## 1. Kiến trúc hiện tại và giới hạn

| Khu vực | Mã nguồn | Trạng thái thực tế |
|---|---|---|
| Core | `Assets/Scripts/Core/GameManager.cs` | Singleton theo scene, tiền prototype, lượt mua, trạng thái, events, khôi phục hai giá trị và HUD OnGUI |
| Player | `Assets/Scripts/Core/PlayerController.cs` | CharacterController; WASD, chuột, Space nhảy khi đứng trên đất, C ngồi/đứng có kiểm tra khoảng trống, Esc đổi khóa chuột; raycast tương tác E và HUD gợi ý |
| Mua prototype | `Assets/Scripts/Gameplay/ProductInteraction.cs` | Giá/tên riêng trên component; mua thành công thì tắt GameObject, chưa cập nhật tồn kho model |
| Models | `Assets/Scripts/Models/*.cs` | Các lớp dữ liệu `[Serializable]`, trường public; chưa tự kiểm tra tính hợp lệ |
| Services | `ProductService`, `SalesService`, `FinanceService`, `AssemblyService`, `SaveGameService` trong `Assets/Scripts/Services/` | Các lớp rỗng, chỉ có TODO; chưa có API nghiệp vụ |
| Gameplay mở rộng | `ShelfController`, `CustomerAI`, `AssemblyController` trong `Assets/Scripts/Gameplay/` | MonoBehaviour rỗng, chưa có quản lý kệ, AI hoặc lắp ráp |
| UI | `MainMenuUI`, `InventoryUI`, `SalesUI`, `AssemblyUI` trong `Assets/Scripts/UI/` | MonoBehaviour rỗng; chưa kết nối UI hoặc lưu/tải |
| Công cụ Editor | `Assets/Editor/TechStoreSceneBuilder.cs` | Tạo demo với sản phẩm cấu hình bằng tên/giá, Player và GameManager; không phải bộ nạp dữ liệu nghiệp vụ |
| Tests | `Assets/Tests/Editor/GameManagerTests.cs`, `Assets/Tests/PlayMode/GameManagerLifecycleTests.cs` | Kết quả thực tế do TV1 cung cấp trong Unity 6.3 LTS: EditMode **19/19 PASS**, PlayMode **1/1 PASS**, tổng **20/20 PASS**. Chạy trong Unity Editor, chưa phải CI của GitHub |

Runtime hiện nằm trong assembly mặc định `Assembly-CSharp`; không tạo thêm runtime assembly chỉ để tích hợp. Play Mode test assembly đang dùng reflection để truy cập GameManager.

Luồng đang chạy theo mã:

```text
PlayerController.Interact(): raycast từ camera → tìm ProductInteraction trên collider hoặc cha
  → hiển thị ProductInteraction.DisplayName
  → nhấn E → ProductInteraction.TryBuy()
      → nếu GameManager.Instance tồn tại: TrySpend(price)
          → false: không tắt sản phẩm, không đổi tiền/lượt mua
          → true: tiền giảm, lượt mua tăng, events phát; sau đó tắt GameObject
```

Chưa có liên kết giữa `ProductInteraction` và `Product.id`/`Product.stock`. `Configure(string productName, int cost)` chỉ gán tên và giá; `DisplayName` trả chuỗi tên kèm giá; `TryBuy()` trả `void`. Không dùng việc GameObject bị tắt làm nguồn tồn kho chính thức.

## 2. API GameManager đã có — giữ tương thích

Nguồn: `Assets/Scripts/Core/GameManager.cs`.

| API public hiện có | Kiểu/chữ ký | Hợp đồng hiện tại |
|---|---|---|
| `Instance` | `static GameManager`, getter public, setter private | Được gán trong Awake; bản trùng bị vô hiệu hóa và lên lịch hủy GameObject |
| `Money` | `int`, getter public, setter private | Số dư prototype, đơn vị VND |
| `ItemsPurchased` | `int`, getter public, setter private | Số lần `TrySpend` thành công; mỗi lần tăng đúng 1, không biểu diễn số lượng sản phẩm trong đơn |
| `Status` | `string`, chỉ đọc | Thông báo hiện tại, không phải mã lỗi nghiệp vụ ổn định |
| `TrySpend(int amount)` | `bool` | Chi tiền và tăng lượt mua nếu hợp lệ; không có product ID, order ID hay transaction ID |
| `TryRestoreState(int money, int itemsPurchased)` | `bool` | Gán đồng thời tiền và lượt mua hợp lệ; không đọc/ghi file và không hoàn tất một giao dịch |
| `MoneyChanged` | `event Action<int>` | Payload là số dư mới, không phải chênh lệch |
| `ItemsPurchasedChanged` | `event Action<int>` | Payload là tổng lượt mua mới, không phải số lượt tăng |
| `StatusChanged` | `event Action<string>` | Payload là thông báo mới |

### 2.1. Khởi tạo, giao dịch và trạng thái

- `startingMoney` là trường private `[SerializeField, Min(0)]`, mặc định **10.000.000 VND**. Inspector điều chỉnh được; OnValidate và Awake chặn giá trị âm. Awake đặt lượt mua về 0.
- `showPrototypeHUD` là trường private `[SerializeField]`, mặc định `true`. Chỉ điều khiển HUD tiền/trạng thái của GameManager, không tắt tâm ngắm và gợi ý của PlayerController.
- Trạng thái ban đầu: `Cham E vao san pham de mua.`

| Thao tác/điều kiện | Kết quả | Tiền/lượt mua | Status được gán |
|---|---|---|---|
| `TrySpend`, `amount <= 0` | `false` | Giữ nguyên | `Gia mua phai lon hon 0!` |
| `TrySpend`, số dư nhỏ hơn amount | `false` | Giữ nguyên | `Khong du tien!` |
| `TrySpend`, lượt mua bằng `int.MaxValue` sau khi qua kiểm tra tiền | `false` | Giữ nguyên | `Da dat gioi han so luot mua!` |
| `TrySpend`, hợp lệ và đủ tiền | `true` | Trừ amount; lượt mua +1 | `Mua thanh cong!` |
| `TryRestoreState`, một trong hai giá trị âm | `false` | Giữ nguyên cả hai | `Du lieu khoi phuc khong hop le!` |
| `TryRestoreState`, cả hai không âm | `true` | Gán cả hai; chấp nhận 0 và `int.MaxValue` | `Da khoi phuc tien va so luot mua.` |

Events chỉ phát cho giá trị **thực sự thay đổi**. Toàn bộ ba giá trị được gán trước khi phát theo thứ tự Money → ItemsPurchased → Status. Hai lần mua thành công liên tiếp vẫn phát events tiền/lượt mua nhưng có thể không phát StatusChanged vì chuỗi thành công không đổi. Khôi phục cùng dữ liệu không bắt buộc phát events tiền/lượt mua. Giao dịch thất bại có thể chỉ phát StatusChanged, hoặc không phát event nếu thông báo đã giống trước đó.

Events được gọi đồng bộ bằng C# delegate. Subscriber không được ném exception hoặc gọi lại giao dịch/khôi phục ngay trong callback; UI chỉ cập nhật hiển thị. Hiện chưa có cơ chế cô lập exception của subscriber hay chống giao dịch lồng nhau.

### 2.2. Vòng đời và ranh giới tài chính

GameManager **chỉ là cơ chế tiền của prototype**, chưa phải FinanceService hoàn chỉnh. Không có cộng doanh thu, chi phí theo loại, lợi nhuận, sổ giao dịch, hoàn tiền hoặc chống xử lý lặp theo ID. Không dùng `TrySpend` với số âm để cộng tiền; không dùng `TryRestoreState` như API cộng doanh thu hay hoàn tiền.

Chưa có `DontDestroyOnLoad`. Khi GameManager chính bị hủy, OnDestroy xóa Instance; bản trùng không được xóa Instance chính. Khi sang scene khác, không được giả định tiền được giữ. TV1 cần thống nhất cách chuyển snapshot hoặc vòng đời service trước khi triển khai chuyển scene.

Đối tượng Unity đã hủy có thể là fake-null: dùng `manager == null` để kiểm tra còn dùng được; kiểm thử giải phóng tham chiếu static cần `ReferenceEquals`. Test Edit Mode gọi lifecycle tường minh; test Play Mode kiểm tra callback tự động. Không sao chép cách gọi lifecycle bằng reflection của test vào mã gameplay.

## 3. Dữ liệu hiện có và chủ sở hữu đề xuất

Các trường sau đã tồn tại, nhưng chưa có service quản lý hoặc kiểm tra dữ liệu:

| Model hiện có | Trường hiện có | Chủ quản đề xuất |
|---|---|---|
| `Product` | `string id`, `string displayName`, `int price`, `int stock` | TV2: danh mục, giá và tồn kho |
| `Customer` | `string id`, `string displayName`, `int budget` | TV3: khách hàng |
| `Order` | `string id`, `int total`, `bool completed` | TV3: đơn hàng |
| `Component` | `string id`, `string category`, `string socket` | TV4: metadata tương thích; phối hợp TV2 quản lý ID |
| `PCConfiguration` | `string cpuId`, `string mainboardId`, `string ramId`, `string gpuId`, `string ssdId` | TV4: cấu hình được chọn |

`Component` ở đây là `TechStoreSimulator.Component`, khác `UnityEngine.Component`; dùng tên đầy đủ khi có xung đột. `Order` chưa có dòng hàng, customer ID, transaction ID hoặc cấu hình PC. `Product.price` chưa tách giá nhập/giá bán. Nhóm phải thống nhất các thiếu hụt này trước khi tính lợi nhuận.

## 4. TV2 → TV3: sản phẩm và tồn kho — đề xuất, chưa có API

TV2 giữ nguồn dữ liệu chuẩn trong ProductService tương lai. TV3 yêu cầu tra cứu bằng `Product.id`, không tìm sản phẩm bằng tên hiển thị hay GameObject. Chốt quy tắc ID không rỗng, duy nhất, ổn định qua lưu/tải; không tái sử dụng ID đã xuất hiện trong giao dịch.

Thông tin tối thiểu TV2 cung cấp cho TV3: ID, tên hiển thị, giá áp dụng, lượng còn có thể bán. Giá trị `Product.stock` hiện có phải được định nghĩa rõ là tồn thực hay tồn khả dụng nếu bổ sung giữ hàng. Cung cấp bản sao/snapshot hoặc kết quả chỉ đọc; các trường public hiện tại chưa tự bảo vệ khỏi sửa trực tiếp.

Các khả năng cần thống nhất rồi mới triển khai (chưa có chữ ký phương thức):

1. Đọc danh mục và tra cứu một ID; trả thất bại rõ khi ID không tồn tại.
2. Kiểm tra và giữ đủ số lượng trước thanh toán; không cho số lượng <= 0 hoặc tồn kho âm.
3. Xác nhận xuất/nhập kho hoặc hủy giữ hàng theo cùng mã giao dịch; xử lý lặp không cập nhật kho lần nữa.
4. Báo thay đổi tồn kho cho TV3/TV5 sau khi dữ liệu được chốt.

TV3 không tự sửa `Product.stock`. Khi người dùng xác nhận mua, kiểm tra lại tồn kho và giá; không dựa vào snapshot UI cũ. Phần giữ hàng, version snapshot và events kho đều đang TODO.

## 5. TV3: kết quả bán hàng và giao dịch — đề xuất, chưa có API

**Lưu ý tích hợp TV3 (trạng thái ngày 24/11/2026 do TV1 cung cấp):** PR #8 của TV1 đã **Merge vào main**; PR #7 của TV3 vẫn đang **Open**. TV3 có chỉnh sửa `GameManager.cs`, vì vậy phải cập nhật nhánh theo main và xử lý xung đột nếu có trước khi tích hợp. Phải giữ nguyên các API `Instance`, `Money`, `ItemsPurchased`, `Status`, `TrySpend(int)`, `TryRestoreState(int, int)` và các events `MoneyChanged`, `ItemsPurchasedChanged`, `StatusChanged` của TV1, cùng hợp đồng tại mục 2. Sau khi xử lý xung đột, chạy lại cả 19 EditMode tests và 1 PlayMode test; không mặc định kết quả trước đó vẫn áp dụng cho mã đã hợp nhất.

Phân biệt hai nghiệp vụ: **cửa hàng mua/nhập hàng** làm giảm tiền và tăng kho; **cửa hàng bán cho khách** làm tăng tiền và giảm kho. Luồng E hiện tại chỉ là hành động mua của prototype, chưa tự đồng nghĩa với một trong hai luồng quản lý cửa hàng đầy đủ.

### 5.1. Quy tắc tránh trừ tiền hai lần

- Khi còn dùng prototype, `ProductInteraction.TryBuy()` là đường mua đã có và **chỉ nó gọi `GameManager.TrySpend` cho lần tương tác đó**. Không thêm lần gọi TrySpend trong UI, NPC hoặc subscriber events cho cùng hành động.
- Khi thay bằng luồng nghiệp vụ, TV1/TV3 phải chọn một bộ điều phối duy nhất. ProductInteraction chuyển sang gửi yêu cầu tới bộ điều phối; không giữ lời gọi TrySpend cũ song song. Việc thay mã này là công việc triển khai sau khi nhóm chốt hợp đồng.
- GameManager events là thông báo trạng thái, không phải lệnh ghi sổ. FinanceService tương lai không được nghe MoneyChanged rồi trừ thêm tiền.
- Mỗi giao dịch có một mã ổn định, một kết quả cuối và một nơi thực hiện thay đổi tiền. Kiểm tra mã đã xử lý trước khi retry. GameManager hiện không hỗ trợ cơ chế này.
- Khi FinanceService được tích hợp, phải chốt thời điểm chuyển nguồn tiền chuẩn từ prototype; không duy trì hai số dư được ghi độc lập.

### 5.2. Kết quả nghiệp vụ cần bàn giao

TV3 đề xuất kết quả gồm: mã giao dịch, order ID, chiều thu/chi, các dòng product ID + số lượng + đơn giá đã chốt, tổng VND, thành công/thất bại và mã lý do. Đây là schema đề xuất, **chưa có DTO/event tương ứng trong repository**. Không suy luận kết quả từ chuỗi `Status` hoặc chỉ từ `Order.completed`.

Luồng mục tiêu: kiểm tra yêu cầu → kiểm tra/giữ kho qua TV2 → kiểm tra tài chính → chốt kho và tiền qua bộ điều phối → đánh dấu đơn hoàn tất → phát kết quả cho UI. Chỉ phát thành công sau khi cả kho và tiền đã chốt. Nếu bất kỳ bước nào thất bại, phải giải phóng giữ hàng và giữ/khôi phục trạng thái nhất quán; không dùng TrySpend để giả lập rollback vì nó tăng lượt mua.

Hiện chưa có transaction nguyên tử, reservation, rollback, API cộng tiền hay sổ giao dịch. Đây là điều kiện phải triển khai trước khi bật bán hàng NPC thật; không thể ghép hai lời gọi service độc lập rồi xem là đã an toàn.

## 6. TV4: đầu vào kiểm tra cấu hình PC — đề xuất

Đầu vào đã có thể biểu diễn: một `PCConfiguration` chứa năm ID. TV4 cần tra cứu mỗi ID sang Product và metadata `TechStoreSimulator.Component`; **chưa có mapping Product.id ↔ Component.id**, đề xuất dùng cùng ID và xác nhận với TV2.

| Dữ liệu cần nhận | Nguồn dự kiến | Hiện trạng |
|---|---|---|
| Năm ID CPU/mainboard/RAM/GPU/SSD | UI chọn linh kiện hoặc đơn lắp ráp | Có model PCConfiguration; chưa có UI/đơn truyền dữ liệu |
| ID hợp lệ, category, socket | Danh mục TV2 và metadata TV4 | Có trường dữ liệu, chưa có tra cứu hoặc chuẩn category/socket |
| Giá, lượng có thể cấp | TV2 | Có price/stock trong model; chưa có service cấp dữ liệu |
| Ngân sách/yêu cầu khách, order ID | TV3 | Customer có budget; Order chưa liên kết customer hoặc cấu hình |
| Chuẩn RAM, khe lưu trữ, điện năng/công suất nguồn, kích thước nếu đưa vào phạm vi | TV4 phối hợp TV2 | Chưa có trong model; không được tuyên bố đã kiểm tra các tiêu chí này |

Bản tối thiểu đề xuất kiểm tra: đủ năm ID, tra cứu được, đúng loại linh kiện và socket CPU-mainboard khớp theo quy ước đã chốt. Trả kết quả hợp lệ/không hợp lệ cùng danh sách lý do theo linh kiện; dữ liệu thiếu phải báo thiếu, không mặc nhiên coi tương thích. Chưa có API kiểm tra hoặc kiểu kết quả trong AssemblyService.

Kiểm tra tương thích chỉ đọc dữ liệu, không tự trừ tiền hay xuất kho. Khi xác nhận lắp ráp, bộ điều phối giao dịch mới yêu cầu cấp linh kiện; phải kiểm tra lại kho/giá. UI AssemblyUI chỉ hiển thị kết quả, không tự suy luận socket.

## 7. TV5: HUD và lưu/tải

### 7.1. HUD dùng API đã có

1. Lấy một GameManager còn sống sau khi Awake hoàn tất. Không giả định Awake của UI chạy sau Awake của GameManager; có thể bind ở Start hoặc cơ chế bind/rebind do TV1 thống nhất.
2. Lưu instance đã bind. Đăng ký `MoneyChanged`, `ItemsPurchasedChanged`, `StatusChanged`; sau đó đọc ngay Money, ItemsPurchased, Status để vẽ trạng thái ban đầu. Không chờ event khởi tạo, vì có thể đã phát trước khi UI đăng ký.
3. Handler nhận tổng giá trị mới và cập nhật UI; không gọi TrySpend, không sửa tồn kho, không tự lưu từng trường riêng lẻ như một giao dịch.
4. Hủy đăng ký trên đúng instance đã lưu khi OnDisable/OnDestroy; tránh đăng ký trùng khi bật UI lại. Sau khi đổi scene, bind lại instance mới và đọc snapshot mới.
5. Khi HUD thật được nối, TV1 tắt `showPrototypeHUD` trong Inspector để tránh hai HUD tiền. Gợi ý tương tác của PlayerController là luồng riêng, không bị biến này điều khiển.

GameManager không tham chiếu script UI của TV5. Các lớp UI hiện tại vẫn rỗng; các bước trên là hướng dẫn tích hợp, chưa phải code đã nối.

### 7.2. Lưu/tải với điểm nối hiện có

SaveGameService hiện rỗng. TV5 có thể thiết kế snapshot prototype gồm phiên bản schema, Money và ItemsPurchased; schema này **chưa tồn tại**. Khi lưu, đọc cả hai từ cùng instance ở trạng thái ổn định. Không ghi Status như dữ liệu tài chính chuẩn; trạng thái được GameManager tạo lại khi khôi phục.

Luồng tải đề xuất:

```text
Đọc file → parse và kiểm tra phiên bản/schema, kiểu int, giá trị không âm
  → đợi GameManager sẵn sàng
  → gọi TryRestoreState(savedMoney, savedItemsPurchased) đúng một lần
      → true: HUD cập nhật qua events hoặc đọc lại snapshot
      → false: báo lỗi tải; tiền/lượt mua cũ giữ nguyên
```

Không gọi TrySpend để tái hiện lịch sử khi tải: cách đó trừ tiền và tăng lượt mua lần nữa. Không dùng reflection để ghi private setter. Tạm dừng autosave trong quá trình tải; chỉ lưu lại khi tải hoàn tất để tránh ghi đè save bằng trạng thái khởi tạo/trung gian.

TryRestoreState chỉ kiểm tra hai số nguyên không âm, không xác minh tính đầy đủ của save hoặc tương quan với kho/đơn. Bản save toàn game cần thêm tồn kho, đơn hàng, cấu hình, trạng thái vật thể và quy trình áp dụng đồng bộ do các bên thống nhất. Khôi phục tiền hiện **không bật lại các sản phẩm đã bị SetActive(false)**. Chưa có lưu file, migration, backup hoặc khôi phục đồng bộ đa hệ thống.

## 8. Luồng dữ liệu chung và trách nhiệm

Luồng mục tiêu dưới đây là **đề xuất**, ngoại trừ luồng mua prototype đã mô tả ở mục 1:

```text
TV2: danh mục + kho ──snapshot/giữ hàng──→ TV3: đơn hàng + điều phối giao dịch
        │                                      │
        └──linh kiện + giá + khả dụng──→ TV4: kiểm tra cấu hình
                                               │ kết quả kiểm tra
                                               └──→ TV3
TV3 ──yêu cầu tiền, kết quả giao dịch──→ điểm tích hợp do TV1 thống nhất
TV1/Core + TV2 + TV3 + TV4 ──snapshot/events──→ TV5: HUD + lưu/tải
TV5 ──yêu cầu khôi phục đã kiểm tra──→ từng chủ quản qua bộ điều phối
```

| Thành viên | Trách nhiệm thống nhất trong tài liệu | Bàn giao/điểm phối hợp |
|---|---|---|
| TV1 | Core, Player, Singleton/lifecycle, điều phối tích hợp; giữ tương thích API prototype | Chốt nguồn tiền, vòng đời scene, thứ tự khởi tạo/khôi phục và việc thay đường mua E |
| TV2 | Danh mục sản phẩm, giá, kho, kệ | ID ổn định, snapshot chỉ đọc, kiểm tra/giữ/chốt kho cho TV3; dữ liệu linh kiện cho TV4 |
| TV3 | Khách hàng, đơn hàng, luồng mua/bán và phối hợp giao dịch tài chính | Kết quả giao dịch có ID, một nơi ghi tiền; cùng TV1 chốt người triển khai FinanceService và chống xử lý lặp |
| TV4 | Chọn/lắp ráp và kiểm tra cấu hình PC | Chuẩn category/socket, mapping ID, quy tắc tương thích, kết quả có lý do |
| TV5 | UI/HUD, lưu/tải và trình bày lỗi | Bind/unbind events; snapshot có phiên bản; không trực tiếp sửa tiền/kho |

TV1 phụ trách Core/Player/tích hợp theo yêu cầu dự án. Phân công chi tiết TV2–TV5 trong bảng là đề xuất theo các điểm bàn giao được yêu cầu; README hiện liệt kê thành viên nhưng chưa quy định quyền sở hữu từng script. Nhóm cần xác nhận trước khi sửa chéo, đặc biệt chủ quản FinanceService và bộ điều phối giao dịch.

## 9. Các quyết định cần chốt và tiêu chí tích hợp

Trước khi triển khai các TODO, nhóm xác nhận:

- Quy ước ID, category/socket; price là giá nhập hay giá bán; đơn vị VND nguyên và kiểm tra tràn khi nhân giá × lượng/tính tổng (GameManager hiện dùng int).
- Luồng E tương lai là nhập hàng hay mua prototype; thời điểm thay lời gọi TrySpend cũ; nơi duy nhất ghi tiền và xử lý lại transaction ID.
- Cách giữ/chốt/hủy kho và xử lý lỗi giữa kho, tiền, đơn hàng; schema kết quả giao dịch và mã lỗi riêng, không dựa vào Status hiển thị.
- Phạm vi kiểm tra PC và metadata cần bổ sung; việc giữ linh kiện khi chọn cấu hình.
- Vòng đời qua scene, thứ tự tải và schema save toàn game; cơ chế bind lại UI và tắt HUD prototype.

Tiêu chí nghiệm thu đề xuất: một lần mua chỉ ghi tiền một lần; thất bại không đổi tiền/lượt mua/kho; yêu cầu lặp không giao dịch lại; bán hàng tăng tiền qua API tài chính được thống nhất; cấu hình thiếu/sai socket bị từ chối có lý do; HUD hiển thị đúng khi bind muộn và bật lại; tải không tạo giao dịch mua mới; đổi scene không mất/nhân đôi trạng thái ngoài thiết kế.

Các tiêu chí này là công việc kiểm thử tích hợp tương lai. Bộ kiểm thử GameManager đã được TV1 chạy thực tế trong Unity Editor 6.3 LTS: **EditMode 19/19 PASS, PlayMode 1/1 PASS, tổng 20/20 PASS**; đây chưa phải kết quả CI của GitHub và chưa xác nhận các tiêu chí tích hợp tương lai. Tài liệu không thay đổi API, runtime code, Scene hoặc Prefab.

# Seed data (Development)

Cần chạy `sync-geo` một lần trước khi seed. Thao tác này gọi GHN khoảng 760 lần nên chỉ chạy khi DB chưa có địa giới.

```bash
dotnet run --project LuminaBox.Api -- sync-geo
```

| Lệnh | Việc |
|---|---|
| `POST api/seed` | Bộ seed đẹp. Chạy lại không tạo trùng. |
| `POST api/seed/edge-cases` | Bộ seed đẹp + bộ edge case. Chạy lại không tạo trùng. |
| `DELETE api/seed` | Xóa dữ liệu app, giữ địa giới. Dùng khi muốn bắt đầu lại. |

Sau `sync-geo`, muốn đổi từ bộ này sang bộ kia thì gọi `DELETE api/seed` rồi seed lại, không cần đồng bộ địa giới lần nữa.

## Bộ seed đẹp

| Tài khoản | Mật khẩu | Vai trò |
|---|---|---|
| `manager@luminabox.com` | `Manager@123` | Manager |
| `user@luminabox.com` | `User@123` | Người mua, 2 địa chỉ, 5 đơn mẫu |
| `user2@`, `user3@`, `user4@luminabox.com` | `User@123` | Người mua |
| `org1@luminabox.com` | `Organization@123` | Moonlight Bakery: online + STL-001, STL-002 |
| `org2@luminabox.com` | `Organization@123` | Luna Online Cakes: chỉ online |
| `org3@luminabox.com` | `Organization@123` | Trăng Vàng Heritage: online + STL-003 |
| `org4@luminabox.com` | `Organization@123` | Hoa Sen Patisserie: online + STL-004 |
| `org5@luminabox.com` | `Organization@123` | Mây Trà Artisan: chỉ online |

Dữ liệu gồm:

- 6 gian hàng có tọa độ
- 12 sản phẩm, 23 biến thể, 6 hộp bán riêng
- 8 voucher: `LUMINA10`, `WELCOME50K`, `TRUNGTHU30K`, `MOON15`, `LUNA20K`, `TRANGVANG10`, `HOASEN25K`, `MAYTRA12`
- 3 sự kiện
- 10 đơn mẫu trải đủ trạng thái (`LBDEMO0001` đến `LBDEMO0010`)

Đơn mẫu đều là COD. Đơn Stripe phải tạo thật, vì hoàn tiền cần giao dịch thật trên Stripe.

Thẻ test Stripe: `4242 4242 4242 4242`, hạn bất kỳ trong tương lai, CVC bất kỳ.

## Bộ edge case

Mật khẩu: người mua `User@123`, shop `Organization@123`.

### Tài khoản

| Tài khoản | Kết quả mong đợi |
|---|---|
| `edge.locked@luminabox.com` | Đăng nhập báo 403 |
| `edge.unverified@luminabox.com` | Đăng nhập báo 403 |
| `edge.noaddress@luminabox.com` | Đặt hàng bị chặn vì chưa có địa chỉ |
| `edge.cart@luminabox.com` | Giỏ có sẵn 6 dòng lỗi, ví có sẵn 6 voucher |
| `edge.bakery@luminabox.com` | Chủ Edge Bakery, xử lý các đơn edge case |

### Sản phẩm của Edge Bakery

| Sản phẩm | Kết quả mong đợi |
|---|---|
| Edge - Hết hàng | Trang chi tiết báo hết hàng |
| Edge - Còn đúng 1 cái | Mua 1 được; 2 người cùng mua thì chỉ 1 người thành công |
| Edge - Kho đang tắt | Không hiện nơi mua |
| Edge - Hai biến thể cùng loại và nhân | Chọn đúng biến thể và đúng giá |
| Edge - Chỉ bán tại 2 gian hàng | Phải chọn gian hàng, không có kho online |
| Edge - Giá 50 triệu | Hiển thị số lớn; thử giới hạn số tiền của Stripe |
| Edge - Tên sản phẩm rất dài... | Giao diện không vỡ |
| Edge - Sản phẩm đã ẩn, Edge - Sản phẩm nháp | Không hiện ở danh sách |
| Edge - Hộp còn 1 | Mua 2 hộp bị chặn |

### Shop lỗi

| Sản phẩm | Kết quả mong đợi |
|---|---|
| Edge - Shop chưa có hợp đồng Online | Thêm giỏ báo 409 |
| Edge - Shop thiếu điểm lấy hàng | Thêm giỏ được, tính phí ship báo lỗi |
| Edge - Gian hàng đã hết hạn thuê | Thêm giỏ báo 409 |
| Edge - Shop bị đình chỉ | Không hiện ở danh sách; chỉ có trong giỏ `edge.cart` |

### Voucher

| Mã | Kết quả mong đợi |
|---|---|
| `EDGE-EXPIRED` | Từ chối vì hết hạn |
| `EDGE-NOTSTARTED` | Không hiện ở danh sách voucher |
| `EDGE-EXHAUSTED` | Từ chối vì hết lượt |
| `EDGE-INACTIVE` | Từ chối vì đã tắt |
| `EDGE-MIN5M` | Từ chối vì chưa đủ giá trị đơn |
| `EDGE-OTHERSHOP` | Từ chối khi đơn không có Moonlight |
| `EDGE-FULL100` | Tiền hàng về 0, chỉ còn phí ship |

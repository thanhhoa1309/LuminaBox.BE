# MoonCake System – API theo chức năng

> Mục đích: liệt kê **API cần thiết cho từng chức năng** và **ý nghĩa** của từng endpoint.
> Nguồn: Controllers hiện có trong `MoonCake.API` + logic trong `docs/Requirements.md`.
> Quy ước quyền: **Public** = không cần token · **Auth** = đã đăng nhập · **User / Organization / Manager** = đúng role JWT.

---

## Mục lục

1. [Auth & tài khoản](#1-auth--tài-khoản)
2. [Gian hàng & hợp đồng thuê](#2-gian-hàng--hợp-đồng-thuê)
3. [Đăng ký / duyệt Organizer](#3-đăng-ký--duyệt-organizer)
4. [Hồ sơ Organizer & địa chỉ lấy hàng](#4-hồ-sơ-organizer--địa-chỉ-lấy-hàng)
5. [Catalog: loại bánh, nhân, sản phẩm, variant, hộp](#5-catalog-loại-bánh-nhân-sản-phẩm-variant-hộp)
6. [Voucher](#6-voucher)
7. [Giỏ hàng](#7-giỏ-hàng)
8. [Địa chỉ nhận hàng & phí ship GHN](#8-địa-chỉ-nhận-hàng--phí-ship-ghn)
9. [Đặt hàng & xử lý đơn](#9-đặt-hàng--xử-lý-đơn)
10. [Thanh toán Stripe](#10-thanh-toán-stripe)
11. [Ví nội bộ](#11-ví-nội-bộ)
12. [Đánh giá](#12-đánh-giá)
13. [Sự kiện (Event)](#13-sự-kiện-event)
14. [Thông báo](#14-thông-báo)
15. [Seed (dev)](#15-seed-dev)
16. [Ma trận theo actor](#16-ma-trận-theo-actor)

---

## 1. Auth & tài khoản

### 1.1 Đăng ký / đăng nhập / token

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| POST | `api/auth/register` | Public | Đăng ký tài khoản mới (`Role=User`), gửi OTP xác thực email. |
| POST | `api/auth/verify-otp` | Public | Xác thực email bằng OTP → tài khoản dùng được để login. |
| POST | `api/auth/resend-otp` | Public | Gửi lại OTP khi hết hạn / chưa nhận được. |
| POST | `api/auth/login` | Public | Đăng nhập email + mật khẩu → access token + refresh token. |
| POST | `api/auth/refresh-token` | Public | Đổi access token mới từ refresh token còn hiệu lực. |
| POST | `api/auth/logout` | Auth | Đăng xuất, vô hiệu hóa refresh token. |
| POST | `api/auth/forgot-password` | Public | Yêu cầu OTP đặt lại mật khẩu. |
| POST | `api/auth/reset-password` | Public | Đặt mật khẩu mới bằng OTP. |

### 1.2 Hồ sơ người dùng

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/users/me` | Auth | Lấy hồ sơ tài khoản đang đăng nhập (role, thông tin cá nhân). |
| PUT | `api/users/me` | Auth | Cập nhật một phần hồ sơ (họ tên, SĐT, địa chỉ text…). |
| GET | `api/users` | Manager | Danh sách user phân trang (quản trị). |
| GET | `api/users/{id}` | Manager | Chi tiết một user. |

**Chức năng dùng các API này:** Flow đăng ký–đăng nhập; FE biết role để hiện UI User / Organization / Manager.

---

## 2. Gian hàng & hợp đồng thuê

### 2.1 Quản lý gian hàng (Stall)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/stalls` | Public | Duyệt danh sách gian hàng (lọc trạng thái, tìm kiếm) — dùng khi User chọn thuê. |
| GET | `api/stalls/{id}` | Public | Chi tiết gian hàng (giá thuê, vị trí, trạng thái). |
| POST | `api/stalls` | Manager | Tạo gian hàng mới (+ địa chỉ GHN OwnerType=Stall). |
| PUT | `api/stalls/{id}` | Manager | Cập nhật thông tin gian hàng (partial). |
| DELETE | `api/stalls/{id}` | Manager | Xóa mềm gian hàng khi trống và không còn hợp đồng. |
| GET | `api/stalls/{id}/rentals` | Manager | Lịch sử thuê của một gian hàng. |

### 2.2 Hợp đồng thuê (StallRental)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/stall-rentals` | Manager | Danh sách hợp đồng toàn sàn (lọc status, stall, organizer). |
| GET | `api/stall-rentals/me` | Organization | Hợp đồng thuê của Organizer hiện tại. |
| GET | `api/stall-rentals/{id}` | Manager | Chi tiết một hợp đồng. |
| PUT | `api/stall-rentals/{id}/terminate` | Manager | Chấm dứt hợp đồng trước hạn → giải phóng stall. |

**Chức năng:** Manager quản lý mặt bằng; Organizer xem gian đang thuê; kết thúc hợp đồng (Flow 4, 15).

---

## 3. Đăng ký / duyệt Organizer

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| POST | `api/organizer-registrations` | User | Gửi đơn lần đầu: thuê stall **hoặc** bán online. |
| POST | `api/organizer-registrations/additional-stalls` | Organization | Organizer đã có hồ sơ xin thuê thêm stall. |
| POST | `api/organizer-registrations/additional-online` | Organization | Organizer xin thêm quyền bán online. |
| GET | `api/organizer-registrations/me` | Auth | Xem đơn đăng ký của chính mình. |
| GET | `api/organizer-registrations` | Manager | Danh sách đơn toàn sàn để duyệt. |
| GET | `api/organizer-registrations/{id}` | Manager | Chi tiết một đơn. |
| PUT | `api/organizer-registrations/{id}/approve` | Manager | Duyệt: Stall → kích hoạt ngay; Online → đặt hoa hồng, chờ User xác nhận. |
| PUT | `api/organizer-registrations/{id}/reject` | Manager | Từ chối đơn kèm lý do. |
| PUT | `api/organizer-registrations/{id}/confirm` | Chủ đơn | User xác nhận tỷ lệ hoa hồng đơn Online → trở thành Organizer. |
| PUT | `api/organizer-registrations/{id}/cancel` | Chủ đơn | User hủy đơn (Pending / AwaitingConfirmation với Online). |

**Chức năng:** Onboarding người bán (Flow 2–3); đổi `User.Role → Organization` khi đơn có hiệu lực.

**Body chính**

| Route | Body |
|---|---|
| `POST /` | `Type`, `BrandName`, `ContactPhone`, `BusinessInfo?`; Stall thêm `StallId`, `StartDate`, `EndDate` (`yyyy-MM-dd`) |
| `POST /additional-stalls` | `StallIds[]` (1–10, không trùng), `StartDate`, `EndDate` |
| `POST /additional-online` | Không có body; thông tin lấy từ hồ sơ Organizer |
| `PUT /{id}/approve` | `CommissionRate` (Online, 0–100, tối đa 2 số lẻ); Stall gửi `{}` hoặc bỏ trống |
| `PUT /{id}/reject` | `RejectReason` |
| `PUT /{id}/cancel` | `CancelReason?` |

**Lưu ý:** Sau khi được đổi role, FE gọi `POST api/auth/refresh-token` để nhận token mới. Đơn của người khác trả 404 ở `confirm` / `cancel`.

---

## 4. Hồ sơ Organizer & địa chỉ lấy hàng

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/organizers/me` | Organization | Xem hồ sơ thương hiệu của tôi. |
| PUT | `api/organizers/me` | Organization | Cập nhật brand / liên hệ / mô tả kinh doanh (partial). |
| GET | `api/organizers/me/address` | Organization | Lấy địa chỉ **điểm lấy hàng** GHN (bắt buộc trước khi nhận đơn ship). |
| PUT | `api/organizers/me/address` | Organization | Tạo/cập nhật địa chỉ pickup (mỗi Organizer đúng 1 địa chỉ). |
| GET | `api/organizers` | Manager | Danh sách Organizer (lọc status, tìm kiếm). |
| GET | `api/organizers/{id}` | Manager | Chi tiết Organizer. |
| PUT | `api/organizers/{id}/status` | Manager | Đình chỉ / mở lại (`Suspended` / `Active`). |

**Chức năng:** Quản lý hồ sơ bán; cấu hình điểm gửi hàng cho GHN; Manager ban/unban tạm thời.

---

## 5. Catalog: loại bánh, nhân, sản phẩm, variant, hộp

> Catalog thuộc **từng Organizer**. Giá bán nằm ở **ProductVariant** (cặp loại bánh × nhân), không nằm trên Product.

### 5.1 Loại bánh (`CakeCategory`)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/cake-categories` | Public | Duyệt loại bánh (có thể lọc theo organizer). |
| GET | `api/cake-categories/me` | Organization | Loại bánh của shop tôi. |
| GET | `api/cake-categories/{id}` | Public | Chi tiết loại bánh. |
| POST | `api/cake-categories` | Organization | Tạo loại bánh. |
| PUT | `api/cake-categories/{id}` | Organization | Sửa loại bánh. |
| DELETE | `api/cake-categories/{id}` | Organization | Xóa (nếu không còn variant đang dùng). |

### 5.2 Nhân (`Filling`)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/fillings` | Public | Duyệt loại nhân. |
| GET | `api/fillings/me` | Organization | Nhân của shop tôi. |
| GET | `api/fillings/{id}` | Public | Chi tiết nhân. |
| POST | `api/fillings` | Organization | Tạo nhân. |
| PUT | `api/fillings/{id}` | Organization | Sửa nhân. |
| DELETE | `api/fillings/{id}` | Organization | Xóa nhân (nếu không còn variant). |

### 5.3 Sản phẩm bánh lẻ (`Product`)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/products` | Public | Catalog public (chỉ Active), lọc organizer/stall/từ khóa. |
| GET | `api/products/me` | Organization | Sản phẩm của shop tôi (mọi status). |
| GET | `api/products/{id}` | Public | Chi tiết sản phẩm (+ ảnh, stall, variants). |
| POST | `api/products` | Organization | Tạo sản phẩm (mặc định `Draft`). |
| PUT | `api/products/{id}` | Organization | Sửa tên, mô tả, ảnh… |
| PUT | `api/products/{id}/stalls` | Organization | Gán / bỏ gán danh sách stall đang thuê Active (rỗng = online-only). |
| PUT | `api/products/{id}/status` | Organization | Đổi `Draft` / `Active` / `Inactive`. |
| DELETE | `api/products/{id}` | Organization | Xóa mềm (không còn variant). |

### 5.4 Biến thể giá (`ProductVariant`)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/product-variants` | Public | Danh sách variant (lọc theo product…). |
| GET | `api/product-variants/{id}` | Public | Chi tiết một cặp Category×Filling + giá. |
| POST | `api/product-variants` | Organization | Tạo variant: Product + Category + Filling + Price. |
| PUT | `api/product-variants/{id}` | Organization | Sửa giá / cặp (không trùng cặp trên cùng product). |
| DELETE | `api/product-variants/{id}` | Organization | Xóa variant. |

**Ý nghĩa nghiệp vụ:** Khi thêm giỏ / đặt hàng client gửi `ProductId + CategoryId + FillingId` → hệ thống match variant để lấy `UnitPrice` (không gửi `VariantId`).

### 5.5 Hộp combo (`BoxItem`)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/box-items` | Public | Danh sách hộp Active. |
| GET | `api/box-items/me` | Organization | Hộp của shop tôi. |
| GET | `api/box-items/{id}` | Public | Chi tiết hộp (capacity, giá cố định, ảnh, products). |
| GET | `api/box-items/{id}/available-products` | Public | Bánh có thể bỏ vào hộp (cùng organizer / stall). |
| POST | `api/box-items` | Organization | Tạo hộp combo. |
| PUT | `api/box-items/{id}` | Organization | Sửa hộp (tên, capacity, giá, products, ảnh…). |
| PUT | `api/box-items/{id}/stalls` | Organization | Gán stall cho hộp. |
| PUT | `api/box-items/{id}/status` | Organization | Đổi trạng thái bán. |
| DELETE | `api/box-items/{id}` | Organization | Xóa hộp. |

**Chức năng:** Flow 5 đăng bán; User duyệt catalog trước khi thêm giỏ / đặt hàng.

---

## 6. Voucher

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| POST | `api/vouchers/platform` | Manager | Tạo voucher sàn (`Scope=Platform`). |
| POST | `api/vouchers/organizer` | Organization | Tạo voucher gian hàng (cần Online Approved). |
| GET | `api/vouchers` | Manager | Quản lý danh sách voucher toàn sàn. |
| GET | `api/vouchers/me` | Organization | Voucher của Organizer tôi. |
| GET | `api/vouchers/{id}` | Manager / Organization | Chi tiết voucher (đúng quyền sở hữu/scope). |
| PUT | `api/vouchers/{id}` | Manager / Organization | Sửa điều kiện voucher. |
| PUT | `api/vouchers/{id}/status` | Manager / Organization | Bật / tắt voucher. |
| DELETE | `api/vouchers/{id}` | Manager / Organization | Xóa khi chưa có lượt dùng. |
| GET | `api/vouchers/available` | User | Voucher còn claim được. |
| POST | `api/vouchers/{id}/claim` | User | Nhận voucher vào “ví voucher” (`UserVoucher`). |
| GET | `api/vouchers/my` | User | Voucher đã claim, dùng khi checkout. |
| DELETE | `api/vouchers/my/{userVoucherId}` | User | Bỏ voucher đã claim khỏi ví. |

**Chức năng:** Phát hành khuyến mãi; User phải **claim trước**, rồi chọn khi tạo order (tối đa 1 sàn + 1 gian hàng / đơn).

---

## 7. Giỏ hàng

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/carts/me` | Auth | Xem giỏ, nhóm theo Organizer + Stall (có Quantity + UnitPrice). |
| POST | `api/carts/items/product` | Auth | Thêm bánh lẻ: Category + Filling + Quantity (+ StallId nếu nhiều chi nhánh); giá lấy từ variant. |
| POST | `api/carts/items/box` | Auth | Thêm hộp combo: Quantity (+ StallId nếu cần); giá lấy từ BoxItem. |
| PUT | `api/carts/items/{itemId}` | Auth | Cập nhật Quantity (Product có thể đổi Category/Filling → re-resolve giá). |
| DELETE | `api/carts/items/{itemId}` | Auth | Xóa một dòng giỏ. |

**Ý nghĩa:** Cart lưu lựa chọn mua kèm giá snapshot. Product: `CategoryId` + `FillingId` + `Quantity` → match `ProductVariant` lấy `UnitPrice`. Box: `Quantity` + `UnitPrice` từ `BoxItem`; selection bánh trong hộp chọn lúc checkout. Trùng cùng variant/stall → cộng dồn Quantity.

---

## 8. Địa chỉ nhận hàng & phí ship GHN

### 8.1 Địa chỉ của User (nhận hàng)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/shipping-addresses` | Auth | Danh sách địa chỉ nhận của tôi (có default). |
| GET | `api/shipping-addresses/{id}` | Auth | Chi tiết một địa chỉ. |
| POST | `api/shipping-addresses` | Auth | Thêm địa chỉ (tỉnh/phường GHN + chi tiết). |
| PUT | `api/shipping-addresses/{id}` | Auth | Sửa địa chỉ (partial). |
| DELETE | `api/shipping-addresses/{id}` | Auth | Xóa địa chỉ. |

### 8.2 Ước tính / master data GHN

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/shipping/provinces` | Auth | Danh sách tỉnh/thành GHN (form địa chỉ). |
| GET | `api/shipping/wards?provinceId=` | Auth | Phường/xã theo tỉnh. |
| POST | `api/shipping/fee` | Auth | Ước phí ship trước khi đặt (From = địa chỉ Organizer). |

**Chức năng:** Chuẩn bị checkout; khi tạo order bắt buộc `ShippingAddressId` của User; phí ship cộng vào `TotalAmount`.

---

## 9. Đặt hàng & xử lý đơn

### 9.1 Tạo / xem đơn (User & chung)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| POST | `api/orders` | Auth | Đặt hàng trực tiếp: chọn Category+Filling+SL, địa chỉ, voucher, payment method → 1..n order theo `(OrganizerId, StallId)`. |
| POST | `api/orders/from-cart` | Auth | Checkout từ các `CartItemId` đã chọn (Product dùng C+F+SL+giá trên cart; Box kèm Selections); sau đó xóa mềm cart item tương ứng. |
| GET | `api/orders/me` | Auth | Đơn mua của tôi (lọc status, ngày). |
| GET | `api/orders/{id}` | Auth | Chi tiết đơn (buyer hoặc organizer của đơn). |

### 9.2 Organizer xử lý đơn

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/orders/organizer/me` | Organization | Đơn thuộc shop tôi. |
| POST | `api/orders/{id}/confirm` | Organization | Xác nhận đơn Pending; nếu Stripe đã Success → **settle ví + hoa hồng**. |
| POST | `api/orders/{id}/reject` | Organization | Từ chối đơn Pending (refund nếu đã thanh toán Stripe). |
| POST | `api/orders/{id}/cancel` | Auth | Hủy đơn: buyer chỉ khi Pending & chưa Stripe Success; organizer khi Pending/Confirmed. |
| POST | `api/orders/{id}/ship` | Organization | Tạo vận đơn GHN → `Delivering`. |
| POST | `api/orders/{id}/sync-shipping` | Organization | Đồng bộ trạng thái vận đơn từ GHN. |
| POST | `api/orders/{id}/mark-delivered` | Organization | Đánh dấu đã giao → `Delivered` (mở quyền feedback). |

**Chức năng:** Flow đặt hàng → thanh toán → confirm → giao → hoàn tất (Flow 9–11).

---

## 10. Thanh toán Stripe

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| POST | `api/payments/checkout` | Auth | Tạo Stripe Checkout Session cho payment Pending (cửa sổ ~10 phút). Chỉ Stripe; 1 order/group. |
| GET | `api/payments/{id}` | Auth | Xem trạng thái / chi tiết thanh toán. |
| POST | `api/payments/{id}/sync` | Auth | Kéo trạng thái từ Stripe về hệ thống (khi chưa có webhook). |
| PATCH | `api/payments/{id}/cancel` | Auth | Hủy payment đang Pending. |
| GET | `api/payments/stripe-return` | Public | URL return sau khi User thanh toán xong trên Stripe. |
| GET | `api/payments/stripe-cancel` | Public | URL khi User hủy trên trang Stripe. |
| POST | `api/payments/stripe-webhook` | Public | Webhook Stripe (production) cập nhật Success/Failed. |

**Ý nghĩa:** `Payment.Success` **không** tự Confirm order — Organizer vẫn phải Confirm. COD tạo Payment kèm order nhưng chưa có luồng checkout riêng.

---

## 11. Ví nội bộ

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/wallets/me` | Auth | Số dư ví của tài khoản hiện tại (Organizer xem tiền đã settle). |
| GET | `api/wallets/me/transactions` | Auth | Lịch sử giao dịch (`OrderEarning`, `OrderReversal`…). |

**Ý nghĩa:** Không có API nạp/rút tay. Tiền vào ví khi Organizer **Confirm** đơn Stripe (trừ hoa hồng); reverse khi refund.

---

## 12. Đánh giá

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| POST | `api/feedbacks` | User | Đánh giá một `OrderItem` của đơn đã `Delivered` (1 lần / item). |
| GET | `api/feedbacks/me` | User | Danh sách đánh giá tôi đã viết. |
| GET | `api/feedbacks/{id}` | User | Chi tiết một đánh giá của tôi. |

**Chưa có API:** Organizer phản hồi; Manager ẩn/hiện; list public theo sản phẩm.

---

## 13. Sự kiện (Event)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/events` | Public | Duyệt event (sàn + organizer đã publish). |
| GET | `api/events/{id}` | Public | Chi tiết event + ảnh. |
| GET | `api/events/me` | Organization | Event của Organizer tôi. |
| POST | `api/events` | Manager / Organization | Tạo event (Manager = sàn `OrganizerId=null`; Org = event shop). |
| PUT | `api/events/{id}` | Manager / Organization | Sửa event (đúng owner). |
| DELETE | `api/events/{id}` | Manager / Organization | Xóa event. |

**Chức năng:** Banner / marketing (Flow 16).

---

## 14. Thông báo

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| GET | `api/notifications` | Auth | Danh sách thông báo của tôi (phân trang). |
| GET | `api/notifications/unread-count` | Auth | Số thông báo chưa đọc (badge FE). |
| PUT | `api/notifications/{id}/read` | Auth | Đánh dấu đã đọc 1 thông báo. |
| PUT | `api/notifications/read` | Auth | Đánh dấu đã đọc hàng loạt. |

**Ý nghĩa:** Bổ trợ realtime/email khi có đơn mới, duyệt Organizer, đổi trạng thái đơn…

---

## 15. Seed (dev)

| Method | Route | Quyền | Ý nghĩa |
|---|---|---|---|
| POST | `api/seed` | *(dev)* | Sinh dữ liệu mẫu: users, categories, fillings, stalls, organizers, products, boxes, vouchers, events. Idempotent. Cần có Ward (`sync-geo`) trước. |
| DELETE | `api/seed` | *(dev)* | Truncate toàn bộ app data (giữ Provinces/Districts/Wards + migration history). |

**Tài khoản mẫu sau seed**

| Email | Password | Role |
|---|---|---|
| `manager@luminabox.com` | `Manager@123` | Manager |
| `user@luminabox.com` | `User@123` | User |
| `user2@luminabox.com` | `User@123` | User |
| `user3@luminabox.com` | `User@123` | User |
| `user4@luminabox.com` | `User@123` | User |
| `org1@luminabox.com` | `Organization@123` | Organization (Moonlight Bakery + STL-001) |
| `org2@luminabox.com` | `Organization@123` | Organization (Luna Online) |

> Không dùng trên production.

---

## 16. Ma trận theo actor

### Guest / Public
Đăng ký–login; xem stalls, products, variants, box-items, cake-categories, fillings, events.

### User (đã đăng nhập, role User)
Hồ sơ; địa chỉ nhận; shipping fee; cart; claim voucher; tạo order; thanh toán Stripe; hủy đơn Pending chưa trả; feedback; notifications; đơn đăng ký Organizer lần đầu.

### Organization
Hồ sơ + địa chỉ pickup; catalog đầy đủ; voucher shop; đơn hàng shop (confirm/reject/ship…); ví; event shop; thuê thêm stall / xin online; xem rentals của tôi.

### Manager
CRUD stall; duyệt/từ chối registration; suspend Organizer; voucher sàn; event sàn; terminate rental; danh sách users / organizers / rentals.

---

## Phụ lục – Chuỗi API gợi ý theo kịch bản FE

### A. User mua hàng (Stripe)

1. `GET api/products` / `GET api/box-items` → chọn hàng  
2. `POST api/carts/items/product` (Category + Filling + Quantity) hoặc box (Quantity)  
3. `GET/POST api/shipping-addresses` → chọn địa chỉ nhận  
4. `POST api/shipping/fee` → xem phí  
5. `GET api/vouchers/my` → chọn voucher đã claim  
6. `POST api/orders/from-cart` (CartItemIds + ShippingAddressId + PaymentMethod=Stripe; Box kèm Selections)  
7. `POST api/payments/checkout` → mở Stripe  
8. `POST api/payments/{id}/sync` (hoặc chờ webhook/return)  
9. Chờ Organizer `confirm` → `ship` → `mark-delivered`  
10. `POST api/feedbacks`

### B. Organizer bán online lần đầu

1. `POST api/auth/register` → verify → login  
2. `POST api/organizer-registrations` (Type=Online)  
3. Manager `approve` (kèm CommissionRate)  
4. User `confirm`  
5. `PUT api/organizers/me/address` (pickup)  
6. Tạo category / filling / product / variants / gán stalls (nếu có)  
7. `PUT api/products/{id}/status` → Active  
8. Nhận đơn tại `GET api/orders/organizer/me`

### C. Manager tạo gian + duyệt thuê

1. `POST api/stalls`  
2. User `POST api/organizer-registrations` (Type=Stall)  
3. `PUT api/organizer-registrations/{id}/approve`  
4. Theo dõi `GET api/stall-rentals`

---

*Liên kết:* nghiệp vụ chi tiết xem [`Requirements.md`](./Requirements.md); mục còn mở xem [`Pending-Requirements.md`](./Pending-Requirements.md).

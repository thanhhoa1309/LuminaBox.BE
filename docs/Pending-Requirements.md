# Pending Requirements – Trạng thái sau khi cập nhật Requirements.md

> Cập nhật: 2026-11-03  
> Nội dung đã chốt đã chuyển vào `docs/Requirements.md` (bản viết lại). File này chỉ giữ phần **còn mở / chưa code**.

---

## Đã chuyển vào Requirements.md

| Mục cũ | Ghi chú |
|---|---|
| Product/Box gắn nhiều stall | BR-10, Flow 5, 8, 9 |
| User chọn stall khi mua; tách order `(OrganizerId, StallId)` | BR-07, BR-18 mục 8, Flow 8–9 |
| ProductImage / BoxImage | Flow 5 |
| Event + EventImage | Flow 16 |
| ProductVariant + order chọn Category/Filling | BR-05, BR-06, Flow 5, 9 |
| ShippingAddress dùng chung + GHN | Flow 12, BR-14, BR-15 |
| Stripe + Wallet settle lúc Confirm | Flow 10, 13, BR-16 |

---

## Còn mở

| # | Nội dung | Ghi chú |
|---|---|---|
| 1 | Field địa chỉ text trên hồ sơ Organizer (ngoài ShippingAddress) | Hoãn — dùng `ShippingAddress` OwnerType=Organizer |
| 2 | TopUp / Payment / Refund trên ví | Enum đã có; chưa API |
| 3 | Feedback: Organizer reply, Manager ẩn/hiện, list public | Chỉ User tạo/xem me |
| 4 | API báo cáo / đối soát Commission tay | Settle tự động khi Confirm Stripe |
| 5 | Checkout COD đầy đủ | Payment COD tạo kèm order; checkout API chưa hỗ trợ |
| 6 | Ban/unban quyền Online chi tiết (yêu cầu + duyệt) | Hiện dùng Suspend Organizer |

---

Khi chốt thêm mục trong bảng “Còn mở”: cập nhật `Requirements.md` trước, rồi mới code theo quy trình 4 bước.

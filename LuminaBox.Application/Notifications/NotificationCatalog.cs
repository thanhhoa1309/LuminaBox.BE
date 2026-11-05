using System.Globalization;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Notifications
{
    // Business services must build notifications through these factories instead of writing raw copy.
    // The actor ({actorName}) is resolved by the dispatcher from the current user, so it is never passed here.
    public static class NotificationCatalog
    {
        private const string OrganizerRegistrationEntity = "OrganizerRegistration";
        private const string OrderEntity = "Order";
        private const string StallRentalEntity = "StallRental";

        // ── Organizer registration ────────────────────────────────────────────────

        public static NotificationCommand RegistrationCreated(Guid registrationId, string brandName)
            => new(
                NotificationType.RegistrationCreated,
                NotificationAudience.ForManagers(),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.Manager,
                    "Có đơn đăng ký Organizer mới",
                    "{actorName} vừa gửi đơn đăng ký Organizer cho thương hiệu \"{brandName}\", cần bạn xét duyệt.",
                    NotificationChannel.Email, NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName),
                OrganizerRegistrationEntity,
                registrationId);

        public static NotificationCommand AdditionalStallRequested(Guid registrationId, string brandName, string stallCode)
            => new(
                NotificationType.RegistrationCreated,
                NotificationAudience.ForManagers(),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.Manager,
                    "Có yêu cầu thuê thêm gian hàng",
                    "Organizer {actorName} (thương hiệu \"{brandName}\") vừa gửi yêu cầu thuê thêm gian hàng {stallCode}, cần bạn xét duyệt.",
                    NotificationChannel.Email, NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, stallCode: stallCode),
                OrganizerRegistrationEntity,
                registrationId);

        public static NotificationCommand AdditionalOnlineRequested(Guid registrationId, string brandName)
            => new(
                NotificationType.RegistrationCreated,
                NotificationAudience.ForManagers(),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.Manager,
                    "Có yêu cầu đăng ký bán online",
                    "Organizer {actorName} (thương hiệu \"{brandName}\") vừa gửi yêu cầu đăng ký bán online, cần bạn xét duyệt và đề xuất tỷ lệ hoa hồng.",
                    NotificationChannel.Email, NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName),
                OrganizerRegistrationEntity,
                registrationId);

        // Stall only: a stall rental does not grant online selling (BR-04), so the copy must not promise it
        public static NotificationCommand OrganizerApproved(Guid userId, Guid registrationId, string brandName, string stallCode)
            => new(
                NotificationType.OrganizerApproved,
                NotificationAudience.ForUser(userId, NotificationRecipientRole.User),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.User,
                    "Đơn thuê gian hàng đã được duyệt",
                    "Đơn thuê gian hàng {stallCode} của thương hiệu \"{brandName}\" đã được {actorName} duyệt.",
                    NotificationChannel.Email, NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, stallCode: stallCode),
                OrganizerRegistrationEntity,
                registrationId);

        // The registrant is the actor here, so the managers are told instead of the registrant
        public static NotificationCommand OnlineRegistrationConfirmed(Guid registrationId, string brandName, decimal commissionRate)
            => new(
                NotificationType.OrganizerApproved,
                NotificationAudience.ForManagers(),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.Manager,
                    "Đơn bán online đã được xác nhận",
                    "{actorName} đã đồng ý tỷ lệ hoa hồng {commissionRate}% cho thương hiệu \"{brandName}\".",
                    NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, commissionRate: commissionRate.ToString("0.##", CultureInfo.InvariantCulture)),
                OrganizerRegistrationEntity,
                registrationId);

        public static NotificationCommand RegistrationCancelledByOwner(Guid registrationId, string brandName)
            => new(
                NotificationType.RegistrationCancelled,
                NotificationAudience.ForManagers(),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.Manager,
                    "Đơn đăng ký Organizer đã bị hủy",
                    "{actorName} đã hủy đơn đăng ký Organizer của thương hiệu \"{brandName}\".",
                    NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName),
                OrganizerRegistrationEntity,
                registrationId);

        // The registrant is still a User on the first registration and already an Organizer on additional ones
        public static NotificationCommand OrganizerCommissionProposed(
            Guid userId,
            NotificationRecipientRole recipientRole,
            Guid registrationId,
            string brandName,
            decimal commissionRate)
            => new(
                NotificationType.RegistrationAwaitingConfirmation,
                NotificationAudience.ForUser(userId, recipientRole),
                NotificationRoleTemplates.For(
                    recipientRole,
                    "Xác nhận tỷ lệ hoa hồng",
                    recipientRole == NotificationRecipientRole.Organizer
                        ? "Đơn đăng ký bán online cho thương hiệu \"{brandName}\" đã được {actorName} duyệt với tỷ lệ hoa hồng {commissionRate}%. Hãy xác nhận để bắt đầu bán online."
                        : "Đơn đăng ký bán online cho thương hiệu \"{brandName}\" đã được {actorName} duyệt với tỷ lệ hoa hồng {commissionRate}%. Hãy xác nhận để kích hoạt tài khoản Organizer.",
                    NotificationChannel.Email, NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, commissionRate: commissionRate.ToString("0.##", CultureInfo.InvariantCulture)),
                OrganizerRegistrationEntity,
                registrationId);

        public static NotificationCommand OrganizerRejected(Guid userId, Guid registrationId, string brandName, string reason)
            => new(
                NotificationType.OrganizerRejected,
                NotificationAudience.ForUser(userId, NotificationRecipientRole.User),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.User,
                    "Đơn đăng ký Organizer bị từ chối",
                    "Đơn đăng ký Organizer cho thương hiệu \"{brandName}\" của bạn đã bị {actorName} từ chối. Lý do: {reason}",
                    NotificationChannel.Email, NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, reason: reason),
                OrganizerRegistrationEntity,
                registrationId);

        // The registrant is still a User on the first registration and already an Organizer on additional ones
        public static NotificationCommand StallRegistrationOverdueCancelled(
            Guid userId,
            NotificationRecipientRole recipientRole,
            Guid registrationId,
            string brandName,
            string stallCode)
            => new(
                NotificationType.RegistrationCancelled,
                NotificationAudience.ForUser(userId, recipientRole),
                NotificationRoleTemplates.For(
                    recipientRole,
                    "Đơn đăng ký thuê gian hàng đã bị hủy",
                    "Đơn đăng ký thuê gian hàng {stallCode} của thương hiệu \"{brandName}\" đã bị hủy do quá ngày bắt đầu thuê mà chưa được duyệt.",
                    NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, stallCode: stallCode),
                OrganizerRegistrationEntity,
                registrationId);

        // ── Stall rental ──────────────────────────────────────────────────────────

        public static NotificationCommand StallRentalActivated(
            Guid organizerUserId,
            Guid rentalId,
            string brandName,
            string stallCode)
            => new(
                NotificationType.StallRentalActivated,
                NotificationAudience.ForUser(organizerUserId, NotificationRecipientRole.Organizer),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.Organizer,
                    "Hợp đồng thuê gian hàng đã có hiệu lực",
                    "Hợp đồng thuê gian hàng {stallCode} của thương hiệu \"{brandName}\" đã bắt đầu có hiệu lực.",
                    NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, stallCode: stallCode),
                StallRentalEntity,
                rentalId);

        public static NotificationCommand StallRentalExpired(
            Guid organizerUserId,
            Guid rentalId,
            string brandName,
            string stallCode)
            => new(
                NotificationType.StallRentalExpired,
                NotificationAudience.ForUser(organizerUserId, NotificationRecipientRole.Organizer),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.Organizer,
                    "Hợp đồng thuê gian hàng đã hết hạn",
                    "Hợp đồng thuê gian hàng {stallCode} của thương hiệu \"{brandName}\" đã hết hạn.",
                    NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, stallCode: stallCode),
                StallRentalEntity,
                rentalId);

        public static NotificationCommand StallRentalTerminated(
            Guid organizerUserId,
            Guid rentalId,
            string brandName,
            string stallCode)
            => new(
                NotificationType.StallRentalTerminated,
                NotificationAudience.ForUser(organizerUserId, NotificationRecipientRole.Organizer),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.Organizer,
                    "Hợp đồng thuê gian hàng đã bị chấm dứt",
                    "Hợp đồng thuê gian hàng {stallCode} của thương hiệu \"{brandName}\" đã bị {actorName} chấm dứt trước hạn.",
                    NotificationChannel.Email, NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, stallCode: stallCode),
                StallRentalEntity,
                rentalId);

        // ── Order ─────────────────────────────────────────────────────────────────

        public static NotificationCommand OrderCreated(
            Guid customerUserId,
            Guid organizerUserId,
            Guid orderId,
            string orderCode,
            string brandName)
            => new(
                NotificationType.OrderCreated,
                NotificationAudience.ForOrderParties(customerUserId, organizerUserId),
                NotificationRoleTemplates
                    .For(
                        NotificationRecipientRole.User,
                        "Đặt hàng thành công",
                        "Đơn hàng #{orderCode} tại {brandName} của bạn đã được tạo và đang chờ cửa hàng xác nhận.",
                        NotificationChannel.Realtime)
                    .And(
                        NotificationRecipientRole.Organizer,
                        "Có đơn hàng mới",
                        "{actorName} vừa đặt đơn hàng #{orderCode}, đang chờ bạn xác nhận.",
                        NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, orderCode: orderCode),
                OrderEntity,
                orderId,
                orderId);

        public static NotificationCommand OrderConfirmed(Guid customerUserId, Guid orderId, string orderCode, string brandName)
            => OrderStatusChanged(
                NotificationType.OrderConfirmed,
                customerUserId,
                orderId,
                orderCode,
                brandName,
                "Đơn hàng đã được xác nhận",
                "Đơn hàng #{orderCode} của bạn đã được {brandName} xác nhận.");

        public static NotificationCommand OrderDelivering(Guid customerUserId, Guid orderId, string orderCode, string brandName)
            => OrderStatusChanged(
                NotificationType.OrderDelivering,
                customerUserId,
                orderId,
                orderCode,
                brandName,
                "Đơn hàng đang được giao",
                "{brandName} đang giao đơn hàng #{orderCode} cho bạn.");

        public static NotificationCommand OrderDelivered(Guid customerUserId, Guid orderId, string orderCode, string brandName)
            => OrderStatusChanged(
                NotificationType.OrderDelivered,
                customerUserId,
                orderId,
                orderCode,
                brandName,
                "Giao hàng thành công",
                "Đơn hàng #{orderCode} từ {brandName} đã được giao thành công. Hãy để lại đánh giá cho sản phẩm nhé.");

        public static NotificationCommand OrderCancelled(
            Guid customerUserId,
            Guid organizerUserId,
            Guid orderId,
            string orderCode,
            string brandName,
            bool cancelledByOrganizer,
            string? reason = null)
        {
            var hasReason = !string.IsNullOrWhiteSpace(reason);

            var (audience, templates) = cancelledByOrganizer
                ? (
                    NotificationAudience.ForUser(customerUserId, NotificationRecipientRole.User),
                    NotificationRoleTemplates.For(
                        NotificationRecipientRole.User,
                        "Đơn hàng đã bị hủy",
                        hasReason
                            ? "Đơn hàng #{orderCode} của bạn đã bị {brandName} hủy. Lý do: {reason}"
                            : "Đơn hàng #{orderCode} của bạn đã bị {brandName} hủy.",
                        NotificationChannel.Realtime))
                : (
                    NotificationAudience.ForUser(organizerUserId, NotificationRecipientRole.Organizer),
                    NotificationRoleTemplates.For(
                        NotificationRecipientRole.Organizer,
                        "Khách hàng đã hủy đơn",
                        hasReason
                            ? "{actorName} đã hủy đơn hàng #{orderCode}. Lý do: {reason}"
                            : "{actorName} đã hủy đơn hàng #{orderCode}.",
                        NotificationChannel.Realtime));

            return new NotificationCommand(
                NotificationType.OrderCancelled,
                audience,
                templates,
                NotificationTokenKeys.Create(brandName: brandName, orderCode: orderCode, reason: reason),
                OrderEntity,
                orderId,
                orderId);
        }

        private static NotificationCommand OrderStatusChanged(
            NotificationType type,
            Guid customerUserId,
            Guid orderId,
            string orderCode,
            string brandName,
            string title,
            string content)
            => new(
                type,
                NotificationAudience.ForUser(customerUserId, NotificationRecipientRole.User),
                NotificationRoleTemplates.For(
                    NotificationRecipientRole.User,
                    title,
                    content,
                    NotificationChannel.Realtime),
                NotificationTokenKeys.Create(brandName: brandName, orderCode: orderCode),
                OrderEntity,
                orderId,
                orderId);
    }
}

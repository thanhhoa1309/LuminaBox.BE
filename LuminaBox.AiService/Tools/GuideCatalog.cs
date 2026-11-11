namespace LuminaBox.AiService.Tools;

/// <param name="Id">Shared with the FE guide registry (LuminaBox_FE src/features/guide/guides.ts). Never rename.</param>
/// <param name="Description">What the user wants to do — the model matches the question against this.</param>
/// <param name="Roles">Empty = any signed-in user.</param>
public sealed record GuideEntry(string Id, string Description, string Label, params string[] Roles);

/// <summary>
/// Screens the assistant can lead the user to. Only ids are sent to the FE; routes and buttons live in the FE.
/// Adding a guide: add it here AND in the FE registry with the same id.
/// </summary>
public static class GuideCatalog
{
    public static readonly IReadOnlyList<GuideEntry> All = new GuideEntry[]
    {
        new("add_address", "Add, edit or choose a default delivery address", "Sổ địa chỉ"),
        new("update_personal_info", "Change full name or phone number", "Hồ sơ của tôi"),
        new("change_password", "Change the account password", "Hồ sơ của tôi"),
        new("update_shop_profile", "Edit the shop profile (shop name, description, contact)", "Hồ sơ của tôi", "Organization"),
        new("add_pickup_address", "Add a pickup address for online orders of a shop", "Địa chỉ lấy hàng", "Organization"),
    };

    public static GuideEntry? Find(string? id) => All.FirstOrDefault(g => g.Id == id);

    public static bool IsAllowed(GuideEntry guide, string role)
        => guide.Roles.Length == 0 || guide.Roles.Contains(role, StringComparer.Ordinal);
}

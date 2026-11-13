using System.Text.Json;

namespace WazobiaMail;

/// <summary>
/// Email, SMS, or WhatsApp template entity.
/// </summary>
public sealed class Template
{
    /// <summary>
    /// Gets or sets the template ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning account ID.
    /// </summary>
    public long? AccountId { get; set; }

    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique template slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delivery channel (<c>email</c>, <c>sms</c>, or <c>whatsapp</c>).
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the approval/lifecycle status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rejection reason if rejected.
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Gets or sets the template subject line.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTML or primary body content.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plain-text body content.
    /// </summary>
    public string TextBody { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Meta WhatsApp template ID if synced.
    /// </summary>
    public string? MetaTemplateId { get; set; }

    /// <summary>
    /// Gets or sets the Meta WhatsApp template name if synced.
    /// </summary>
    public string? MetaTemplateName { get; set; }

    /// <summary>
    /// Gets or sets the language code (for example <c>en_US</c>).
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets structured template components.
    /// </summary>
    public JsonElement? Components { get; set; }

    /// <summary>
    /// Gets or sets declared template variables.
    /// </summary>
    public JsonElement? Variables { get; set; }

    /// <summary>
    /// Gets or sets optional visual builder design JSON.
    /// </summary>
    public JsonElement? Design { get; set; }

    /// <summary>
    /// Gets or sets the preview image URL.
    /// </summary>
    public string? PreviewImageUrl { get; set; }

    /// <summary>
    /// Gets or sets whether this is a system template.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Gets or sets the template revision number.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public string? UpdatedAt { get; set; }
}

/// <summary>
/// Paginated list of templates.
/// </summary>
public sealed class TemplateList
{
    /// <summary>
    /// Gets or sets the templates.
    /// </summary>
    public IReadOnlyList<Template> Items { get; set; } = Array.Empty<Template>();

    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Gets or sets the requested limit.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the requested offset.
    /// </summary>
    public int Offset { get; set; }
}

/// <summary>
/// Request payload for creating a template.
/// </summary>
public sealed class CreateTemplateRequest
{
    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the channel (<c>email</c>, <c>sms</c>, or <c>whatsapp</c>).
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// Gets or sets the template category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the subject line.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the template body.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional plain-text body.
    /// </summary>
    public string? TextBody { get; set; }

    /// <summary>
    /// Gets or sets the template language code.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets optional structured components.
    /// </summary>
    public object? Components { get; set; }

    /// <summary>
    /// Gets or sets optional template variable definitions.
    /// </summary>
    public object? Variables { get; set; }

    /// <summary>
    /// Gets or sets optional editor design JSON.
    /// </summary>
    public object? Design { get; set; }

    /// <summary>
    /// Gets or sets whether to submit WhatsApp templates to Meta automatically.
    /// </summary>
    public bool? AutoSubmitMeta { get; set; }

    /// <summary>
    /// Gets or sets whether this is a system template.
    /// </summary>
    public bool? IsSystem { get; set; }
}

/// <summary>
/// Request payload for updating a template.
/// </summary>
public sealed class UpdateTemplateRequest
{
    /// <summary>
    /// Gets or sets the updated name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the updated subject line.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the updated body.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the updated plain-text body.
    /// </summary>
    public string? TextBody { get; set; }

    /// <summary>
    /// Gets or sets the updated category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the updated language code.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets updated components.
    /// </summary>
    public object? Components { get; set; }

    /// <summary>
    /// Gets or sets updated variable definitions.
    /// </summary>
    public object? Variables { get; set; }

    /// <summary>
    /// Gets or sets updated design JSON.
    /// </summary>
    public object? Design { get; set; }

    /// <summary>
    /// Gets or sets an updated lifecycle status.
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// Request payload for rendering a template preview.
/// </summary>
public sealed class PreviewTemplateRequest
{
    /// <summary>
    /// Gets or sets sample variable substitutions for preview rendering.
    /// </summary>
    public IDictionary<string, object?>? SampleVariables { get; set; }
}

/// <summary>
/// Rendered template preview output.
/// </summary>
public sealed class TemplatePreview
{
    /// <summary>
    /// Gets or sets the template channel.
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rendered subject line.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rendered HTML/primary body.
    /// </summary>
    public string RenderedBody { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rendered plain-text body.
    /// </summary>
    public string RenderedText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the template can currently be used for sending.
    /// </summary>
    public bool CanSend { get; set; }
}

/// <summary>
/// Query filters for listing templates.
/// </summary>
public sealed class TemplateListParams
{
    /// <summary>
    /// Gets or sets the channel filter.
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// Gets or sets the category filter.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the status filter.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets a search keyword filter.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items to return.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Gets or sets the pagination offset.
    /// </summary>
    public int? Offset { get; set; }
}

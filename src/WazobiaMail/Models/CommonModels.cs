namespace WazobiaMail;

/// <summary>
/// Standard WazobiaMail API response envelope.
/// </summary>
/// <typeparam name="T">The typed payload contained in <see cref="Data"/>.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the API call succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the human-readable status message from the API.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response payload data.
    /// </summary>
    public T Data { get; set; } = default!;
}

/// <summary>
/// Collection envelope for endpoints returning <c>{"items": [...]}</c>.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class ItemsEnvelope<T>
{
    /// <summary>
    /// Gets or sets the returned items.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}

/// <summary>
/// Pagination metadata returned by paginated collection endpoints.
/// </summary>
public sealed class Pagination
{
    /// <summary>
    /// Gets or sets the current 1-based page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of matching items.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPrev { get; set; }
}

/// <summary>
/// Paginated collection envelope containing <see cref="Items"/> and <see cref="Pagination"/>.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the items on the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Gets or sets the pagination metadata.
    /// </summary>
    public Pagination Pagination { get; set; } = new();
}

/// <summary>
/// Binary file response returned by download endpoints (such as invoice PDFs or support attachments).
/// </summary>
public sealed class BinaryDownload
{
    /// <summary>
    /// Initializes a new <see cref="BinaryDownload"/> instance.
    /// </summary>
    public BinaryDownload(
        byte[] content,
        string? contentType,
        IReadOnlyDictionary<string, string[]> headers)
    {
        Content = content;
        ContentType = contentType;
        Headers = headers;
    }

    /// <summary>
    /// Gets the raw downloaded bytes.
    /// </summary>
    public byte[] Content { get; }

    /// <summary>
    /// Gets the response <c>Content-Type</c> header if present.
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    /// Gets all HTTP response headers.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Headers { get; }
}

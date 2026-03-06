namespace PDFCanon;

/// <summary>Configuration options for <see cref="PDFCanonClient"/>.</summary>
public sealed class PDFCanonClientOptions
{
    /// <summary>The API key used to authenticate requests. Falls back to the <c>PDFCANON_API_KEY</c> environment variable.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Base address of the PDFCanon API.</summary>
    public Uri BaseAddress { get; set; } = new("https://api.pdfcanon.io/api/");

    /// <summary>Timeout for establishing a connection.</summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Timeout for reading a full response.</summary>
    public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromSeconds(120);

    /// <summary>Maximum number of retry attempts for transient failures.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Optional pre-configured <see cref="System.Net.Http.HttpClient"/> to use.
    /// When provided, <see cref="ConnectTimeout"/> and <see cref="ReadTimeout"/> are ignored.
    /// </summary>
    public HttpClient? HttpClient { get; set; }
}

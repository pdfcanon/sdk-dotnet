using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PDFCanon.Models;

/// <summary>Full structural diff report returned by the <c>POST /compare</c> endpoint.</summary>
public sealed class StructuralDiffReport
{
    [JsonPropertyName("summary")]
    public DiffSummary? Summary { get; set; }

    [JsonPropertyName("metadata")]
    public List<MetadataDiffEntry>? Metadata { get; set; }

    [JsonPropertyName("pages")]
    public PagesDiff? Pages { get; set; }

    [JsonPropertyName("objects")]
    public ObjectsDiff? Objects { get; set; }

    [JsonPropertyName("formFields")]
    public List<FormFieldDiff>? FormFields { get; set; }

    [JsonPropertyName("annotations")]
    public List<AnnotationDiff>? Annotations { get; set; }

    [JsonPropertyName("security")]
    public List<SecurityDiffEntry>? Security { get; set; }

    [JsonPropertyName("fonts")]
    public List<FontDiff>? Fonts { get; set; }
}

public sealed class DiffSummary
{
    [JsonPropertyName("totalChanges")]
    public int TotalChanges { get; set; }

    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("identical")]
    public bool Identical { get; set; }
}

public sealed class MetadataDiffEntry
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("oldValue")]
    public string? OldValue { get; set; }

    [JsonPropertyName("newValue")]
    public string? NewValue { get; set; }
}

public sealed class PagesDiff
{
    [JsonPropertyName("added")]
    public List<PageInfo>? Added { get; set; }

    [JsonPropertyName("removed")]
    public List<PageInfo>? Removed { get; set; }

    [JsonPropertyName("reordered")]
    public List<PageReorder>? Reordered { get; set; }
}

public sealed class PageInfo
{
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("contentHash")]
    public string? ContentHash { get; set; }
}

public sealed class PageReorder
{
    [JsonPropertyName("oldPosition")]
    public int OldPosition { get; set; }

    [JsonPropertyName("newPosition")]
    public int NewPosition { get; set; }

    [JsonPropertyName("contentHash")]
    public string? ContentHash { get; set; }
}

public sealed class ObjectsDiff
{
    [JsonPropertyName("added")]
    public int Added { get; set; }

    [JsonPropertyName("removed")]
    public int Removed { get; set; }

    [JsonPropertyName("totalA")]
    public int TotalA { get; set; }

    [JsonPropertyName("totalB")]
    public int TotalB { get; set; }
}

public sealed class FormFieldDiff
{
    [JsonPropertyName("fieldName")]
    public string? FieldName { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("oldValue")]
    public string? OldValue { get; set; }

    [JsonPropertyName("newValue")]
    public string? NewValue { get; set; }
}

public sealed class AnnotationDiff
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }
}

public sealed class SecurityDiffEntry
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("oldValue")]
    public string? OldValue { get; set; }

    [JsonPropertyName("newValue")]
    public string? NewValue { get; set; }
}

public sealed class FontDiff
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }
}

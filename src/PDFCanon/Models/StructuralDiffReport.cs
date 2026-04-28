using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (DTOs)

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

/// <summary>High-level summary of the structural diff result.</summary>
public sealed class DiffSummary
{
    [JsonPropertyName("totalChanges")]
    public int TotalChanges { get; set; }

    [JsonPropertyName("severity")]
    public string? Severity { get; set; }

    [JsonPropertyName("identical")]
    public bool Identical { get; set; }
}

/// <summary>A single metadata field that changed between the two documents.</summary>
public sealed class MetadataDiffEntry
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("oldValue")]
    public string? OldValue { get; set; }

    [JsonPropertyName("newValue")]
    public string? NewValue { get; set; }
}

/// <summary>Page-level additions, removals, and reorders between the two documents.</summary>
public sealed class PagesDiff
{
    [JsonPropertyName("added")]
    public List<PageInfo>? Added { get; set; }

    [JsonPropertyName("removed")]
    public List<PageInfo>? Removed { get; set; }

    [JsonPropertyName("reordered")]
    public List<PageReorder>? Reordered { get; set; }
}

/// <summary>Identity information for a single page.</summary>
public sealed class PageInfo
{
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("contentHash")]
    public string? ContentHash { get; set; }
}

/// <summary>A page that changed position between the two documents.</summary>
public sealed class PageReorder
{
    [JsonPropertyName("oldPosition")]
    public int OldPosition { get; set; }

    [JsonPropertyName("newPosition")]
    public int NewPosition { get; set; }

    [JsonPropertyName("contentHash")]
    public string? ContentHash { get; set; }
}

/// <summary>Summary count of PDF object additions and removals.</summary>
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

/// <summary>A form field that was added, removed, or whose value changed.</summary>
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

/// <summary>An annotation that was added or removed.</summary>
public sealed class AnnotationDiff
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }
}

/// <summary>A security-related setting that changed between the two documents.</summary>
public sealed class SecurityDiffEntry
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("oldValue")]
    public string? OldValue { get; set; }

    [JsonPropertyName("newValue")]
    public string? NewValue { get; set; }
}

/// <summary>A font that was added or removed.</summary>
public sealed class FontDiff
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }
}

using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a block in the Markdown hierarchy.
/// </summary>
/// <summary>
/// Represents a block in the Markdown hierarchy.
/// </summary>
public class Block
{
	/// <summary>
	/// The sanitized title of the block.
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// The order index of the block.
	/// </summary>
	public int Index { get; set; }

	/// <summary>
	/// The nesting level of the block (e.g., 1 for H1, 2 for H2).
	/// </summary>
	public int Level { get; set; }

	/// <summary>
	/// The parent block in the hierarchy.
	/// </summary>
	public Block? Parent { get; set; }

	/// <summary>
	/// The filename associated with this block.
	/// </summary>
	public string Filename { get; set; }

	/// <summary>
	/// The Markdown header for this block based on its level.
	/// </summary>
	public string Header => Level > 0 ? new string('#', Level) + " " + Title : "";

	/// <summary>
	/// The content text within this block.
	/// </summary>
	public List<string> Text { get; set; } = new();

	/// <summary>
	/// The child blocks nested under this block.
	/// </summary>
	public List<Block> Children { get; set; } = new();

	/// <summary>
	/// The next sibling block.
	/// </summary>
	public Block? Next { get; set; }

	/// <summary>
	/// The previous sibling block.
	/// </summary>
	public Block? Previous { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Block"/> class.
	/// </summary>
	/// <param name="title">The raw title extracted from the Markdown header.</param>
	/// <param name="index">The order index of the block.</param>
	/// <param name="level">The nesting level of the block.</param>
	/// <param name="parent">The parent block in the hierarchy.</param>
	public Block(string title, int index, int level, Block? parent)
	{
		Title = SanitizeTitle(title);
		Index = index;
		Level = level;
		Parent = parent;

		// Generate filename based on the sanitized title
		Filename = GenerateFilename();
	}

	/// <summary>
	/// Sanitizes the title by removing unwanted characters.
	/// </summary>
	/// <param name="title">The raw title.</param>
	/// <returns>The sanitized title.</returns>
	private string SanitizeTitle(string title)
	{
		if (string.IsNullOrWhiteSpace(title))
			return "Untitled";

		// Remove leading and trailing '#' characters and whitespace
		title = title.Trim('#', ' ').Trim();

		// Remove unwanted characters: #, ', ", :
		title = Regex.Replace(title, @"[#'"":]", "");

		// Optionally, further sanitization can be done here
		return title;
	}

	/// <summary>
	/// Generates a filename based on the sanitized title.
	/// </summary>
	/// <returns>The generated filename with a .md extension.</returns>
	private string GenerateFilename()
	{
		// Replace spaces with underscores for consistency and safety
		string safeTitle = Regex.Replace(Title, @"\s+", "_");

		// Remove any remaining invalid filename characters
		safeTitle = Regex.Replace(safeTitle, @"[^a-zA-Z0-9_\-]", "");

		// Ensure the filename is not empty
		if (string.IsNullOrWhiteSpace(safeTitle))
			safeTitle = "untitled";

		// Append .md extension
		return $"{safeTitle}.md";
	}

	/// <summary>
	/// Extracts the title from the header line.
	/// Example: "## Chapter 1" -> "Chapter 1"
	/// </summary>
	/// <param name="header">Header line.</param>
	/// <returns>Extracted title.</returns>
	public static string ExtractTitle(string header)
	{
		if (string.IsNullOrWhiteSpace(header))
			return "Untitled";

		return header.TrimStart('#').Trim();
	}
}
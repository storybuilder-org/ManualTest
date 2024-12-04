using System.Text;

public class Block
{
	public string Header;
	public string Title;
	public List<Block> Children;
	public string Filename;
	public List<string> Text;
	public int Level;
	public Block Next;
	public Block Previous;
	public Block Parent;
	public int Index;

	public Block(string line, int index, int level, Block parent)
	{
		// Extract the title by removing '#' characters and trimming whitespace.
		Title = line.TrimStart('#').Trim();
		Index = index;
		Level = level;
		Parent = parent;
		Text = new();
		Children = new();
		ParseFilename(line);
	}

	private void ParseFilename(string header)
	{
		string temp = header.Replace("#", " ");
		Title = temp.Trim();
		StringBuilder filename = new StringBuilder(Title);
		filename.Replace(" ", "_");

		// Replace invalid filename characters
		char[] invalidChars = Path.GetInvalidFileNameChars();
		foreach (char c in invalidChars)
		{
			filename.Replace(c.ToString(), "_");
		}

		filename.Replace("__", "_");
		filename.Append(".md");
		Filename = filename.ToString();
	}
}
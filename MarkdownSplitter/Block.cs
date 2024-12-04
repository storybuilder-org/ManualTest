using System.Text;  

namespace MarkdownSplitter;

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

    public Block(string header, int index, Block parent) 
    {
        Header = header;
        Children = new List<Block>();
        Text = new List<string>();
        ParseFilename(header);
        Level = header.IndexOf(" ");
		Index = index;
		Parent = parent;
		if (Level == -1) { Level = 0;}
    }

    private void ParseFilename(string header)
    {
        string temp = header.Replace("#", " ");
        Title = temp.Trim();
        StringBuilder filename = new(Title);
        filename = filename.Replace(" ", "_");

        // replace invalid filename characters
        filename.Replace('<', '_');
        filename.Replace('>', '_');
        filename.Replace(':', '_');
        filename.Replace('"', '_');
        filename.Replace('/', '_');
        filename.Replace('\\','_');
        filename.Replace('|', '_');
        filename.Replace('?', '_');
        filename.Replace('*', '_');

        filename.Replace("__", "_");
        filename.Append(".md");
        Filename = filename.ToString();
    }
}
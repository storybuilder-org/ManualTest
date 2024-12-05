using System.Diagnostics;
using System.Text;

namespace MarkdownSplitter
{
	/// <summary>
	/// Generate the StoryBuilder User Manual website by processing
	/// the output of a Scrivener Compile to MultiMarkdown.
	/// The output of the Compile command is written to compilerFolder.
	/// </summary>
	public class Splitter
	{
		public string MarkdownFolder { get; set; }
		private string repositoryPath;
		private string docsFolder;
		private string splitMarker;
		private readonly Block[] nestingLevel = new Block[7];
		private Block? previousBlock;
		private string mediaFolder;

		public Splitter()
		{
			MarkdownFolder = "manual.md";
			repositoryPath = Directory.GetCurrentDirectory();
			docsFolder = "docs";
			splitMarker = "#";
		}

		public void EmptyDocsFolder()
		{
			repositoryPath = Directory.GetParent(MarkdownFolder)!.FullName;
			docsFolder = Path.Join(repositoryPath, "docs");
			DirectoryInfo di = new(docsFolder);
			if (di.Exists)
				di.Delete(true);
			di.Create();

			mediaFolder = Path.Combine(docsFolder, "media");
			Directory.CreateDirectory(mediaFolder);
		}

		public bool ProcessMarkdownFolder()
		{
			bool found = false;
			try
			{
				foreach (string currentFile in Directory.EnumerateFiles(MarkdownFolder, "*.*"))
				{
					string fileName = Path.GetFileName(currentFile);
					if (fileName.EndsWith(".md"))
					{
						ProcessMarkdownFile(currentFile);
						found = true;
					}
					else if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					{
						// Move images to media folder
						File.Copy(currentFile, Path.Combine(mediaFolder, fileName), true);
					}
					else
					{
						// Other files remain in docs folder
						File.Copy(currentFile, Path.Combine(docsFolder, fileName), true);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			return found;
		}

		public void SplitMarkdownFile()
		{
			Block root = nestingLevel[0];
			root.Filename = "index.md";
			foreach (Block child in root.Children)
				RecurseMarkdownBlocks(child);
		}

		public void CreateIndexFile()
		{
			File.WriteAllLines(Path.Combine(docsFolder, "index.md"), new[]
			{
				"---",
				"title: Home",
				"layout: home",
				"---",
				"StoryCAD is a comprehensive outlining tool for fiction writers, designed to help organize and structure stories effectively. It provides a range of features to assist with plotting, character development, and world-building. Writers can outline stories at the scene level, use pre-built templates for various plot structures, and explore tools like Dramatic Situations and Stock Scenes to refine their narrative.",
				"The software allows users to approach their stories methodically, offering workflows to manage the complexity of storytelling. StoryCAD is flexible, supporting various fiction forms and genres while helping writers visualize and address each story element independently.",
				"This manual will guide you through the features and tools available in StoryCAD, helping you make the most of its capabilities to plan and develop your stories."
			});
		}

		public void CreateChildMarkdownFiles()
		{
			previousBlock = nestingLevel[0];
			for (int i = 0; i < nestingLevel[0].Children.Count; i++)
			{
				ChainBlocks(nestingLevel[0].Children[i], i);
			}

			foreach (var child in nestingLevel[0].Children)
				WriteChildFile(child, nestingLevel[0]);
		}

		private void ProcessMarkdownFile(string currentFile)
		{
			string[] markdown = File.ReadAllLines(currentFile);
			Block current = new Block("Home", 0, 0, null);
			nestingLevel[0] = current;

			int index = 0;
			foreach (string line in markdown)
			{
				if (line.StartsWith(splitMarker))
				{
					index++;
					int level = line.TakeWhile(c => c == '#').Count();
					Block parent = nestingLevel[level - 1];
					current = new Block(line, index, level, parent);
					parent.Children.Add(current);
					nestingLevel[level] = current;
				}
				else
				{
					current?.Text.Add(line);
				}
			}
		}

		private void RecurseMarkdownBlocks(Block block)
		{
			WriteMarkdownBlock(block);
			foreach (Block child in block.Children)
				RecurseMarkdownBlocks(child);
		}

		private void WriteMarkdownBlock(Block block)
		{
			string filepath = Path.Combine(docsFolder, block.Filename);
			using StreamWriter file = new(filepath);
			file.WriteLine("---");
			file.WriteLine($"title: {block.Title}");
			file.WriteLine("layout: default");
			file.WriteLine("nav_enabled: true");
			file.WriteLine($"nav_order: {block.Index}");
			file.WriteLine($"parent: {block.Parent.Title}");
			file.WriteLine("---");
			file.WriteLine(block.Header);
			foreach (string line in block.Text)
				file.WriteLine(CleanupMarkdown(line));
		}

		private void WriteChildFile(Block block, Block parent)
		{
			StringBuilder sb = new();
			sb.AppendLine("---");
			sb.AppendLine($"title: {block.Title}");
			sb.AppendLine("layout: default");
			sb.AppendLine("nav_enabled: true");
			sb.AppendLine($"nav_order: {block.Index}");
			sb.AppendLine($"parent: {block.Parent.Title}");
			sb.AppendLine("---");
			sb.AppendLine(block.Header);

			foreach (var text in block.Text)
				sb.AppendLine(CleanupMarkdown(text));

			foreach (var child in block.Children)
			{
				// Convert .md links to .html
				string htmlLink = Path.ChangeExtension(child.Filename, ".html");
				sb.AppendLine($"[{child.Title}]({htmlLink}) <br/><br/>");
			}

			File.WriteAllText(Path.Combine(docsFolder, block.Filename), sb.ToString());

			foreach (var child in block.Children)
				WriteChildFile(child, block);
		}
		private string CleanupMarkdown(string line)
		{
			if (line.Contains("[Front Page (Image)](Front_Page_(Image).md)"))
				return "";

			if (line == " <br/>")
				return "";

			// Handle reference-style images: ![][refName]
			// Convert them to ![](/media/refName.png)
			if (line.Contains("![]["))
			{
				int startIndex = line.IndexOf("![][") + 4;
				int endIndex = line.IndexOf("]", startIndex);
				if (startIndex > 3 && endIndex > startIndex)
				{
					string refName = line.Substring(startIndex, endIndex - startIndex);
					refName = NormalizeImageName(refName);
					line = $"![](/media/{refName}.png)";
				}
			}

			// Handle inline images: ![AltText](imageName)
			// Add .png and prepend /media
			if (line.IndexOf("![") > -1 && line.Contains("]("))
			{
				string[] tokens = line.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
				if (tokens.Length > 1 && tokens[0].Contains("!["))
				{
					string imageName = tokens[1];
					imageName = NormalizeImageName(imageName);
					line = line.Replace($"({tokens[1]})", $"(media/{imageName}.png)");
				}
			}

			// Convert .md links to .html
			if (line.Contains(".md"))
				line = line.Replace(".md", ".html");

			return line;
		}

		// This helper function ensures the image name is "cleaned" before we add "/media/" and ".png"
		private string NormalizeImageName(string imageName)
		{
			// Remove any leading/trailing spaces
			imageName = imageName.Trim();

			// Remove any existing /media/ segment
			imageName = imageName.Replace("/media/", "");

			// Remove any trailing .png (or .PNG) extension if present
			if (imageName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				imageName = imageName.Substring(0, imageName.Length - 4);

			return imageName;
		}


		private void ChainBlocks(Block current, int index)
		{
			previousBlock!.Next = current;
			current.Previous = previousBlock;

			StringBuilder sb = new();
			sb.AppendLine(" <br/>");
			sb.AppendLine(" <br/>");
			if (previousBlock.Previous?.Filename == "index.md")
				previousBlock.Previous.Title = "Home";

			previousBlock.Text.Add(sb.ToString());
			previousBlock = current;
		}
	}
}

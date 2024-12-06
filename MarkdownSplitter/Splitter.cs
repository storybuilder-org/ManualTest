using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownSplitter
{
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
					if (fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
					{
						ProcessMarkdownFile(currentFile);
						found = true;
					}
					else if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					{
						File.Copy(currentFile, Path.Combine(mediaFolder, fileName), true);
					}
					else
					{
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
			string? parentDirectory = Directory.GetParent(docsFolder)?.FullName;
			if (parentDirectory == null)
			{
				Console.WriteLine("Unable to determine the parent directory of the docs folder.");
				return;
			}

			string indexPath = Path.Combine(parentDirectory, "index.md");
			File.WriteAllLines(indexPath, new[]
			{
				"---",
				"title: Home",
				"layout: home",
				"nav_enabled: true",
				"nav_order: 1",
				"has_toc: false",
				"---",
				"StoryCAD is a comprehensive outlining tool for fiction writers...",
				"This manual will guide you through the features and tools..."
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

					// Sanitize the title
					string rawTitle = line.Trim();
					string sanitizedTitle = SanitizeTitle(rawTitle);

					Block newBlock = new Block(sanitizedTitle, index, level, parent);
					// Ensure title is trimmed and sanitized
					newBlock.Title = sanitizedTitle;
					parent.Children.Add(newBlock);
					nestingLevel[level] = newBlock;
					current = newBlock;
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
			string outputDir = GetOutputDirectoryForBlock(block);
			Directory.CreateDirectory(outputDir);

			string relativeMediaPath = Path.GetRelativePath(outputDir, mediaFolder).Replace("\\", "/");
			string filepath = Path.Combine(outputDir, block.Filename);
			using StreamWriter file = new(filepath);
			file.WriteLine("---");
			file.WriteLine($"title: {SanitizeTitle(block.Title)}");
			file.WriteLine("layout: default");
			file.WriteLine("nav_enabled: true");
			file.WriteLine($"nav_order: {block.Index}");
			if (block.Parent != null)
				file.WriteLine($"parent: {SanitizeTitle(block.Parent.Title)}");
			file.WriteLine("has_toc: false");
			file.WriteLine("---");
			file.WriteLine(block.Header);
			foreach (string line in block.Text)
				file.WriteLine(CleanupMarkdown(line, relativeMediaPath));
		}

		private void WriteChildFile(Block block, Block parent)
		{
			string outputDir = GetOutputDirectoryForBlock(block);
			Directory.CreateDirectory(outputDir);

			string relativeMediaPath = Path.GetRelativePath(outputDir, mediaFolder).Replace("\\", "/");

			StringBuilder sb = new();
			sb.AppendLine("---");
			sb.AppendLine($"title: {SanitizeTitle(block.Title)}");
			sb.AppendLine("layout: default");
			sb.AppendLine("nav_enabled: true");
			sb.AppendLine($"nav_order: {block.Index}");
			if (block.Parent != null)
				sb.AppendLine($"parent: {SanitizeTitle(block.Parent.Title)}");
			sb.AppendLine("has_toc: false");
			sb.AppendLine("---");
			sb.AppendLine(block.Header);

			foreach (var text in block.Text)
				sb.AppendLine(CleanupMarkdown(text, relativeMediaPath));

			foreach (var child in block.Children)
			{
				string htmlLink = Path.ChangeExtension(child.Filename, ".html");
				sb.AppendLine($"[{SanitizeTitle(child.Title)}]({htmlLink}) <br/><br/>");
			}

			File.WriteAllText(Path.Combine(outputDir, block.Filename), sb.ToString());

			foreach (var child in block.Children)
				WriteChildFile(child, block);
		}

		private string CleanupMarkdown(string line, string relativeMediaPath)
		{
			if (line.Contains("[Front Page (Image)](Front_Page_(Image).md"))
				return "";

			if (line.Trim() == "<br/>")
				return "";

			if (line.Contains("![][")) // Reference-style images
			{
				int startIndex = line.IndexOf("![][") + 4;
				int endIndex = line.IndexOf("]", startIndex);
				if (startIndex > 3 && endIndex > startIndex)
				{
					string refName = line.Substring(startIndex, endIndex - startIndex);
					refName = NormalizeImageName(refName);
					line = $"![]({relativeMediaPath}/{refName}.png)";
				}
			}

			if (line.Contains("![") && line.Contains("](")) // Inline images
			{
				int start = line.IndexOf("](") + 2;
				int end = line.IndexOf(')', start);
				if (start > 1 && end > start)
				{
					string imageName = line.Substring(start, end - start);
					imageName = NormalizeImageName(imageName);
					string newImagePath = $"{relativeMediaPath}/{imageName}.png";
					line = line.Substring(0, start) + newImagePath + line.Substring(end);
				}
			}

			if (line.Contains(".md"))
				line = line.Replace(".md", ".html");

			return line;
		}

		private string NormalizeImageName(string imageName)
		{
			imageName = imageName.Trim();
			imageName = Path.GetFileNameWithoutExtension(imageName);
			imageName = Regex.Replace(imageName, @"[^a-zA-Z0-9\-_]", "-");
			return imageName.Trim('-');
		}

		private void ChainBlocks(Block current, int index)
		{
			previousBlock!.Next = current;
			current.Previous = previousBlock;

			StringBuilder sb = new();
			sb.AppendLine("<br/>");
			sb.AppendLine("<br/>");
			if (previousBlock.Previous?.Filename == "index.md")
				previousBlock.Previous.Title = "Home";

			previousBlock.Text.Add(sb.ToString());
			previousBlock = current;
		}

		private string GetOutputDirectoryForBlock(Block block)
		{
			if (block.Parent == null || block.Parent == nestingLevel[0])
			{
				if (block == nestingLevel[0])
				{
					// root block (index.md) -> docs
					return docsFolder;
				}
				else
				{
					// Top-level child block: create a directory under docs
					string safeTitle = SanitizeFolderName(block.Title);
					return Path.Combine(docsFolder, safeTitle);
				}
			}
			else
			{
				// Deeper levels go into the top-level parent's directory
				Block topLevelParent = GetTopLevelParent(block);
				string safeTitle = SanitizeFolderName(topLevelParent.Title);
				return Path.Combine(docsFolder, safeTitle);
			}
		}


		private Block GetTopLevelParent(Block block)
		{
			Block current = block;
			while (current.Parent != null && current.Parent != nestingLevel[0])
			{
				current = current.Parent;
			}
			return current;
		}

		private string SanitizeFolderName(string name)
		{
			// Replace invalid chars (anything not alphanumeric or space) with a space
			string sanitized = Regex.Replace(name.Trim(), @"[^a-zA-Z0-9 ]+", " ");

			// Collapse multiple spaces
			sanitized = Regex.Replace(sanitized, @"\s+", " ");

			return sanitized.Trim();
		}

		private string SanitizeTitle(string title)
		{
			// Remove leading and trailing '#' characters and whitespace
			title = title.Trim('#', ' ').Trim();

			// Remove unwanted characters: #, ', ", :
			title = Regex.Replace(title, @"[#'"":]", "");

			return title;
		}
	}
}

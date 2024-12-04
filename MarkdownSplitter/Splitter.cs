﻿using System.Diagnostics;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MarkdownSplitter
{
	/// <summary>
	/// Generate the StoryBuilder User Manual website by processing
	/// the output of a Scrivener Compile to MultiMarkdown.
	/// The output of the Compile command is written to compilerFolder.
	///
	/// The .md markdown files are rewritten as a tree of nodes (blocks) each 
	/// of which contains links to its child nodes, according to its depth
	/// or indentation nestingLevel in the Scrivener file.
	/// 
	/// The workflow is as follows:
	///    Empty the (output) /docs folder. This folder is where the final
	///       GitHub Pages .md files (and .png) images go. When your local repository
	///       changes (your manual edits processed by Compile and MarkdownSplitter)
	///       are pushed to the online repository, the /docs folder will be processed
	///       by YAML to generate the test website.
	///    Process the Scrivener project's Compile output folder contents. This is
	///       the .md folder (typically manual.md) and contains single MultiMarkdown
	///       .md file and its .png image files.
	///    Create the index.md file in the docs folder by reading the .md markdown
	///       file and reformatting it as a series of blocks.
	///    Write the child .md files for each block as a set of link statements. That
	///       is, each child node is a mark-down link in its parent's .md file.
	/// </summary>
	public class Splitter
	{
		public string MarkdownFolder;
		private string repositoryPath;
		private string docsFolder;
		private string splitMarker = "#";
		private readonly Block[] nestingLevel = new Block[7];
		private Block? previousBlock;

		public Splitter()
		{
			MarkdownFolder = @"manual.md";
			repositoryPath = Directory.GetCurrentDirectory();
			this.docsFolder = @"docs";
			this.splitMarker = splitMarker;
			this.previousBlock = null;
		}

		/// <summary>
		/// Creates an empty github pages (/docs) folder
		/// </summary>
		public void EmptyDocsFolder()
		{
			repositoryPath = Directory.GetParent(MarkdownFolder)!.FullName;
			docsFolder = Path.Join(repositoryPath, @"docs");
			DirectoryInfo di = new(docsFolder);
			if (di.Exists)
			{
				di.Delete(true);
			}

			di.Create();
		}

		/// <summary>
		/// Process the folder written by Scrivener Compile. This contains a single
		/// .md markdown file and a set of .png image files. Find the .md file and
		/// process it. All other files (the images) are copied to the /docs folder.
		///
		/// If the .md file can't be found, the Compile wasn't performed properly.
		/// </summary>
		/// <returns></returns>
		public bool ProcessMarkdownFolder()
		{
			bool found = false;
			try
			{
				var txtFiles = Directory.EnumerateFiles(MarkdownFolder, "*.*");
				foreach (string currentFile in txtFiles)
				{
					string fileName = currentFile.Substring(MarkdownFolder.Length + 1);
					if (fileName.EndsWith(".md"))
					{
						ProcessMarkdownFile(currentFile);
						found = true;
					}
					else
						File.Copy(currentFile, Path.Combine(docsFolder, fileName));
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
			using StreamWriter file = new(Path.Combine(docsFolder, "index.md"));
			file.WriteLine("---");
			file.WriteLine("title: Home");
			file.WriteLine("layout: home");
			file.WriteLine("---");
			file.WriteLine("Unused for now");
			file.Close();
		}

		/// <summary>
		/// Write separate .md files from the single .md file
		/// written by Scrivener Compile.
		/// </summary>
		public void CreateChildMarkdownFiles()
		{
			previousBlock = nestingLevel[0];
			// Generate previous and next links for each child     
			for (int i = 0; i < nestingLevel[0].Children.Count; i++)
			{
				ChainBlocks(nestingLevel[0].Children[i], i);
			}

			//Creates the other md files.
			foreach (var child in nestingLevel[0].Children)
			{
				WriteChildFile(child, nestingLevel[0]);
			}
		}

		/// <summary>
		/// Process the Compiler's .md MultiMarkdown file.
		///
		/// It's parsed as a series of linked Block objects. A new block is
		/// created every time a Markdown Header (#, ##, etc.) is found in the
		/// Compiler output file, and added to its parent block. The nestingLevel of
		/// nesting for a block is determined by its heading nestingLevel.
		///
		/// If a line doesn't start a new block, it's added to the current block.
		/// </summary>
		/// <param name="currentFile"></param>
		private void ProcessMarkdownFile(string currentFile)
		{
			// Read the .md file into memory.
			string[] markdown = File.ReadAllLines(currentFile);

			// Initialize the root block.
			Block current = new Block("Home", 0, 0, null);
			nestingLevel[0] = current; // Level zero is the root.

			int Index = 0;
			foreach (string line in markdown)
			{
				if (line.StartsWith(splitMarker))
				{
					Index++;

					// Determine the heading level based on the number of '#' characters.
					int level = line.TakeWhile(c => c == '#').Count();

					// Get the correct parent from nestingLevel.
					Block parent = nestingLevel[level - 1];

					// Create the new block with the correct parent.
					current = new Block(line, Index, level, parent);

					// Add the new block to its parent's children.
					parent.Children.Add(current);

					// Update the nesting level.
					nestingLevel[level] = current;
				}
				else
				{
					if (current != null)
						current.Text.Add(line);
				}
			}
		}


		/// <summary>
		/// Write the 
		/// </summary>
		/// <param name="block"></param>
		private void RecurseMarkdownBlocks(Block block)
		{
			WriteMarkdownBlock(block);
			foreach (Block child in block.Children)
				RecurseMarkdownBlocks(child);
		}

		private void WriteMarkdownBlock(Block block)
		{
			string filepath = Path.Combine(docsFolder, block.Filename);
			using (StreamWriter file = new(filepath))
			{
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
				file.Close();
			}
		}

		private void WriteChildFile(Block block, Block Parent)
		{
			StringBuilder sb = new();
			sb.AppendLine("---");
			sb.AppendLine($"title: {block.Title}");
			sb.AppendLine("layout: default");
			sb.AppendLine("nav_enabled: true");
			sb.AppendLine($"nav_order: {block.Index}");
			sb.AppendLine($"parent: {block.Parent.Title}");
			sb.AppendLine("---");
			sb.AppendLine(block.Header); // This writes the header.

			foreach (var text in block.Text)
			{
				sb.AppendLine(CleanupMarkdown(text)); // This writes all the text.
			}

			foreach (var child in block.Children)
			{
				sb.AppendLine($"[{child.Title}]({child.Filename}) <br/><br/>"); // This writes any links.
			}

			// Write the entire content, including navigation, to the markdown file.
			File.WriteAllText(Path.Combine(docsFolder, block.Filename), sb.ToString());

			// Recursively process children blocks.
			foreach (var child in block.Children)
			{
				WriteChildFile(child, block);
			}
		}

		private string CleanupMarkdown(string line)
		{
			if (line.Contains("[Front Page (Image)](Front_Page_(Image).md)"))
			{
				return "" /*"![](StoryBuilder.png)"*/;
			}

			if (line == " <br/>")
			{
				line = "";
			}

			if (line.IndexOf("![") > -1)
			{
				string[] tokens = line.Split(new char[] { '[', ']' });
				string line2 = "![](" + tokens[3] + ".png)";
				return line2;
			}

			return line;
		}

		private void ChainBlocks(Block current, int index)
		{
			previousBlock.Next = current;
			Debug.Assert(previousBlock != null, nameof(previousBlock) + " != null");
			current.Previous = previousBlock;

			// Append the previous and next links to the previous block's text.
			StringBuilder sb = new();
			sb.AppendLine($" <br/>");
			sb.AppendLine($" <br/>");
			if (previousBlock.Previous != null)
			{
				if (previousBlock.Previous.Filename == "index.md")
				{
					previousBlock.Previous.Title = "Home";
				}
			}

			previousBlock.Text.Add(sb.ToString());
			previousBlock = current;
		}
	}
}
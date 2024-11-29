# About StoryBuilder's User Manual

The production StoryBuilder User Manual is the GitHub Pages website located
at https://storybuilder-org.github.io/StoryBuilder-2/ which is 
associated with the project's repository:
https://github.com/storybuilder-org/StoryBuilder-2
This manual is also the online help system for StoryCAD.

The User Manual is generated from a series of static pages in 
Markdown (.md) format, but StoryBuilder's user manual is stored
and editied as a Scrivener project and compiled as a MultiMarkdown file.

Scrivener is a popular WSIWYG ('what you see is what you get') text editor
which allows generating its output in multiple formats.Scrivener's 
perfect for large, complicated documents containing many sections, 
such as novels and non-fiction books, or in our case, the user manual. 
The output of the Scrivener compile is post-processed to produce
the GitHub Pages website.

## Testing the user manual

Because it's the production website,https://storybuilder-org.github.io/StoryBuilder-2/
can't be modified to test and review documentation changes. Accordingly,
this repository, https://github.com/storybuilder-org/StoryBuilder-Manual 
was created. It has its own GitHub Pages website,
https://storybuilder-org.github.io/StoryBuilder-Manual/, which is for
testing changes to the documentation. 

The StoryBuilder-Manual repo is **self-contained** and has everything needed
to update the User Manual:

- The Scrivener project (/manual.scriv folder)
- A folder to hold the Scrivener compile MultiMarkdown output (/manual.md)
- MarkdownSplitter, a utility to post-process the markdown output
- The GitHub Pages the website's built from (/docs folder)

## Setup for testing

You MUST have a Scrivener license and Scrivener 3.0 installed
to update the StoryBuilder User Manual.

NOTE: The Scrivener project can only be edited by one person at 
a time. If multiple people are working on documentation changes,
they must manually coordinate with each other.

## Edit the User Manual

1. IMPORTANT: Make sure you are in the 'gh-pages' branch. 
2. Navigate to the manual.scriv folder in your StoryBUilder-Manual 
repository clone  and open the manual.scrivx file using Scrivener.
3. Make you text changes as per any large Scrivener document, but with several provisos:
4. Formatting is controlled by section selection (Heading1, Heading2, No Heading, etc.)
These generate Markdown sections (#, ##, etc.)
5. For technical reasons, Scrivener's edit tool bar options for bold and italics aren't brought
converted to Markdown syntax. To use bold and italics (other than those associated with sections),
use Markdown syntax right in your scrivx file (**text** for bold and *text* for italics.) Be
sure to check your work after you've generated the test website.

The StoryBuilder Scrivener project has a great many images, mostly 
screenshots of StoryBuilder. Because the StoryBuilder-Manual repository
is self-contained, the images are kept in the Scrivener project itself,
under the Research node. To include a new or changed image into the manuscript:

1. Take the image as a .png file using any screen capture software, such as Snag-It.
2. In Scrivener, in the manual.scrivx document, click on the Research node in the Binder.
3. Select File | Import | Files on the menu. Using the file picker, select your .png 
image file, which will be imported at the end of the Research folder.
4. At the point in the Manuscript you wish to import the image into your document, 
select Insert | Image Linked to Document and select your .png image from  under the 
Reserch folder in manual.scrivx.
5. Save your work.

Formatting is controlled by section selection (Heading1, Heading2, No Heading, etc.)
There are two 


## Build and test the StoryBuilder User Manual

1. After you've made your revisions and saved them to manual.scrivx (see above):
2. Select File | Compile from the Scrivener menu.
3. On the Compile Overview popup window, set Compile For: to MultiMarkdown and
select 'Markdown Splitter' under Markdown Formats.
4. Click on the Compile button.
5. In the destination file picker, select manual.md in the manual.md folder. Again,
this folder is contained within the StoryBuilder-Manual repository's gh-branch. 
6. Allow the Compile to run to completion (that is, the Compile window closes.)
 
The output of the compile, in Markdown (.md) format, will be in the markdown.md
folder and will consist of a single .md file (manual.md) 
and all of the .png images contained in it. 

The website **could** be generated direclty from this file, but would consist of
a single, very large web page. We prefer a collapsable document, consisting of
multiple web pages and links: hypertext, which can be drilled into by expanding
and contracting links. In order to do so, we split the one .md file into a 
hierarchy of multiple files, each of which contains links to its child markdown
pages, using a program, MarkdownSplitter, which is also included in the
StoryBuilder-Manual repository.

7. Find and click on MarkdownSPlitter shortcut in the SStoryBuilder-Manual repository.
8. Click on the 'Select manual.md folder' button and browse to the \manual.md
folder, which contains your Scrivener Compile output. 
9. Click Run. The StoryBuilder-Manual repo contains a /docs folder. The MarkdownSplitter 
program will replace the contents of the \docs folder with the manual.md contents 
reformatted into the set of .md markdown files.

The configuration for GitHub Pages in StoryBuilder-Manual is to process 
the /docs folder in the manual branch to generate the 
https://storybuilder-org.github.io/StoryBuilder-Manual/ 
test version of the user manual.

10. Commit your changes to the 
[StoryBuilder-Manual](https://github.com/storybuilder-org/StoryBuilder-Manual).
11. Push your changes. This expose the /docs folder and cause the GitHub Pages
action to invoke the YAML compiler to build the website. 
12. There is no need to merge the 'manual' branch changes to master as  you edit.
Just repeat the above process until you're satisfied. Keep the 'manual' branch
for further revisions.
13. Once you've reached the desired result, and when you're ready to update
the production User Manual with you changes, you will want to PR and merge the
gh-pages branch into main. DO NOT delete gh-pages, however.

## Update the production User Manual

When you are completely satisfied with your documentation changes:

1. Create a new branch on the StoryBuilder production repository. 
2. Delete the contents of the /docs folder and copy the contents of the 
StoryBuilder-Manual repository's /docs folder into the StoryBuilder 
repository's /docs folder.  
3. Create a PR and merge the new branch'es changes into the master branch's
/docs folder. This will invoke the YAML compiler and build the production
StoryBuilder User Manual website.

# StoryCAD Test Manual

This repo is a staging repo for the real StoryCAD Manual stored in the StoryCAD Repo.
This means that all manual changes are made and tested here before being sent to the offical manual.

# Development
The test manual is live at: https://storybuilder-org.github.io/StoryBuilder-Manual/
Pushes to this repo will appear at the above test manual URL.

You can edit it at using the built-in GitHub Editor or locally using a Markdown Editor (We recomend VS Code with a Markdown Editor)
The StoryCAD manual uses the [Just The Docs template](https://just-the-docs.com/) and can be ran locally for testing, more information about local testing can be found on the [here](https://just-the-docs.com/#getting-started)

# Making changes to the StoryCAD Manual
All changes to the live manual must first be tested here, after which they can be moved to the live StoryCAD manual in the StoryCAD repo.
Once changes are ready to be shipped publically, a StoryCAD Maintainer will copy the /doc/ folder to the StoryCAD Repo.

### Notes:
- To add a new page you need a top block
i.e
```
---
title: Concept Tab
layout: default
nav_enabled: true
nav_order: 33
parent: Story Overview Form
has_toc: false
---
```
If your file does not have one of these then it will not be made into a web page.

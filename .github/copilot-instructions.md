Project Coding Standards (Developer Notes) - Updated December 28, 2025

These notes describe the preferred conventions, formatting rules, and documentation patterns used in this project. They are guidelines for generating code, documentation, and diffs, not strict system‑level commands. Copilot should follow them when they apply to the task at hand.

Project Overview
• 	Target Framework: .NET Framework 4.8
• 	Primary Language: VB.NET
• 	Application Type: Ember Media Manager (Movie/TV scraper and media manager)
• 	Architecture: Modular addon‑based system

Naming Conventions (VB.NET)
• 	Private constants: 
• 	Private fields: , 
• 	Public properties: , 
• 	Method parameters: , 
• 	Local variables: , 
• 	Public methods: , 
• 	Private methods: , 
• 	Event handlers: 

VB.NET Guidelines
• 	Use  and 
• 	Prefer explicit types
• 	Use  / 
• 	Use  for IDisposable
• 	Use XML documentation for public APIs
• 	Keep methods focused and avoid deep nesting

Markdown File Generation (Formatting Preferences)
These notes describe how markdown should be formatted when Copilot is asked to generate markdown files. They are not global rules for all tasks.
Code Block Safety
When generating markdown files that contain code:
• 	Prefer inline code using single backticks
• 	For multi‑line examples, use 4‑space indentation
• 	Avoid triple backticks inside generated markdown content
• 	Method signatures should be plain text or inline code
Pre‑Generation Notes
Before generating markdown content, Copilot may:
• 	Use inline code for short examples
• 	Use 4‑space indentation for multi‑line examples
• 	Avoid fenced code blocks inside fenced blocks
Post‑Generation Notes
After generating markdown content, Copilot may:
• 	Check for accidental triple backticks
• 	Ensure examples follow the formatting preferences above

File Modification Preferences
When Copilot is asked to modify existing files, the preferred format is:
Unified Diff (Preferred for Edits)
• 	Show only the lines that change
• 	Include line numbers
• 	Use unified diff format
• 	Avoid rewriting the entire file unless explicitly requested
This format is helpful for reviewing changes and keeping diffs clean.
When Full Files Are Acceptable
Copilot may provide full file output when:
• 	Creating a new file
• 	The file is very small
• 	A near‑complete rewrite is requested
• 	Generating templates or scaffolding

Code Update Preferences
• 	Provide only the necessary changes unless a full rewrite is requested
• 	Preserve existing structure and indentation
• 	Match existing comment style
• 	Include file paths when helpful

Documentation Preferences
• 	Use inline code for identifiers
• 	Use 4‑space indentation for code blocks
• 	Use clear headings and lists

Code Style Guidelines
• 	Prefer specific types
• 	Use nullable types when appropriate
• 	Prefer strongly typed collections
• 	Use auto‑properties when appropriate
• 	Use specific exception types
• 	Document public APIs with XML comments

File Organization
• 	Group related functionality
• 	Separate UI and business logic
• 	Follow existing namespace patterns

Dependencies
• 	Minimize external dependencies
• 	Document new packages
• 	Ensure .NET Framework 4.8 compatibility

Testing Considerations
• 	Test scrapers with real data
• 	Validate XML/NFO parsing
• 	Test UI responsiveness
• 	Consider edge cases

Comments and Documentation
• 	Explain “why,” not “what”
• 	Keep comments up to date
• 	Use TODO comments for planned improvements

Best Practices
• 	Follow existing patterns
• 	Extract complex logic into helper methods
• 	Validate inputs
• 	Avoid magic values
• 	Keep interfaces clean
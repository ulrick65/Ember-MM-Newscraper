Project Coding Standards (Developer Notes) - Updated December 28, 2025

These notes describe the preferred conventions, formatting rules, and documentation patterns used in this project. They are guidelines for generating code, documentation, and diffs, not strict system‑level commands. Copilot should follow them when they apply to the task at hand.

Project Overview
• 	Target Framework: .NET Framework 4.8
• 	Primary Language: VB.NET
• 	Application Type: Ember Media Manager (Movie/TV scraper and media manager)
• 	Architecture: Modular addon‑based system

Naming Conventions (VB.NET)
• 	Private fields: _camelCase (e.g., _dbElement, _cancelled)
• 	Public properties: PascalCase (e.g., ImageType, DBElement)
• 	Method parameters: camelCase (e.g., sender, scrapeType)
• 	Local variables: camelCase (e.g., tContentType, iProgress)
• 	Methods (public/private): PascalCase (e.g., SaveAllImages, CreateListImages)
• 	Event handlers: ControlName_EventName (e.g., bwImgDownload_ProgressChanged)

VB.NET Guidelines
• 	Use Option Strict On and Option Explicit On
• 	Use Using blocks for IDisposable objects
• 	Use XML documentation for public APIs
• 	Keep methods focused and avoid deep nesting

Markdown File Generation (Formatting Preferences)
These notes describe how markdown should be formatted when Copilot is asked to generate markdown files. They are not global rules for all tasks.

Code Block Safety
When generating markdown files that contain code:
• 	Prefer inline code using single backticks for short snippets
• 	For multi‑line code examples, ALWAYS use 4‑space indentation — NOT triple backticks
• 	Triple backticks inside markdown content cause rendering/truncation issues — avoid them
• 	Method signatures should be plain text or inline code
• 	When providing instruction templates or examples that contain code blocks, use 4‑space indentation

Example of correct multi-line code in markdown:

    Private Sub Example()
        Dim value As String = "test"
    End Sub

NOT:
    ```vb
    Private Sub Example()
    ```

Output Completion Guidelines
• 	For large markdown files, break into sections and deliver one section at a time
• 	Always complete the current section before pausing
• 	If a document will exceed ~300 lines, warn the user and offer to split delivery
• 	Never stop mid-section — finish through the next horizontal rule (---) or heading
• 	End each chunk at a natural boundary (---, ## heading, or *End of file*)

File Modification Preferences

Large File Workflow (500+ lines)
When working with large files, use this collaborative approach:

1. To locate code, tell the user:
   - The filename in backticks: `filename.vb`
   - A search term or method signature in a code block
   - The user will find it and send back a direct link

2. To provide code changes, use Search/Replace format:

   **File:** `filename.vb`

   **Search for:**
   (code block with unique identifier - method signature or 2-3 identifying lines)

   **Replace with:**
   (code block with the new code)

   **Instructions:** (what to replace - entire method, up to End Sub, etc.)

3. For small additions:
   - "Add this after line X" or "Add this block after `MethodName()`"

Search/Replace Rules
- Always include the filename with backticks
- Search block: Use method signature or unique identifying lines (2-3 lines minimum)
- Replace block: Include complete replacement code
- Instructions: Specify scope (entire method, up to End Sub, insert after X, etc.)
- For short methods (<20 lines): Include entire method in both blocks

Small Files (<500 lines)
- Provide the full updated section or method directly
- Still include filename for clarity

When Full Files Are Acceptable
- Creating a new file
- The file is very small (<100 lines)
- A near-complete rewrite is requested
- Generating templates or scaffolding

DO NOT USE
- Unified diff format (<<<<<<< ORIGINAL / >>>>>>> UPDATED)
- Diff patches

Code Update Preferences
- Preserve existing structure and indentation
- Match existing comment style
- Always include filename with backticks

Documentation Preferences
• 	Use inline code for identifiers
• 	Use 4‑space indentation for code blocks
• 	Use clear headings and lists
• 	For markdown documentation standards, see: EmberMediaManager/docs/process-docs/DocumentationProcess.md

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
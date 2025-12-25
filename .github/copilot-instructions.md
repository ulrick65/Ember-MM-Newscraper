# Project Coding Standards - Updated 2025-12-25

## Project Overview
- Target Framework: .NET Framework 4.8
- Primary Language: Visual Basic .NET (VB.NET)
- Application Type: Ember Media Manager - Movie/TV Show scraper and media management tool
- Architecture: Addon-based modular system with scrapers and utilities

## Naming Conventions (VB.NET)
- Private constants: Use camelCase with 'k' prefix (e.g., kInfoPrefix)
- Private fields: Use underscore prefix with camelCase (e.g., _httpClient, _movieData)
- Public properties: Use PascalCase (e.g., FolderPath, MovieTitle)
- Method parameters: Use camelCase (e.g., folderPath, movieId)
- Local variables: Use camelCase (e.g., nfoFiles, movieList)
- Public methods: Use PascalCase (e.g., ProcessFiles, ScrapeMovieData)
- Private methods: Use PascalCase (e.g., ValidateFolder, ParseXmlData)
- Event handlers: Use descriptive names with pattern ObjectName_EventName (e.g., btnSave_Click)

## VB.NET Specific Guidelines
- Use explicit Option Strict On and Option Explicit On in all files
- Prefer explicit type declarations over Object when type is known
- Use String.IsNullOrEmpty() and String.IsNullOrWhiteSpace() for string validation
- Use Try-Catch blocks appropriately with specific exception types
- Dispose of IDisposable objects properly using Using statements
- Follow standard VB.NET XML documentation comments for public APIs
- Use meaningful variable names that clearly indicate purpose
- Keep methods focused and avoid deeply nested logic

## CRITICAL: Markdown File Generation Rules - MUST FOLLOW!

### Rule 1: Code Block Boundary Protection (CRITICAL!)
**PROBLEM:** When generating markdown file content, Copilot MUST wrap the entire file in a single outer code block. Using triple backticks (```) INSIDE this content breaks the outer block and corrupts the file generation.

**SOLUTION - ABSOLUTE REQUIREMENTS:**
1. **NEVER use triple backticks (```) anywhere inside generated markdown content**
2. **ALWAYS use single backticks for inline code:** `code here`
3. **ALWAYS use indentation (4 spaces) for code blocks instead of fenced blocks**
4. **VERIFY before sending:** Scan entire response for triple backticks inside markdown generation

### Rule 2: Method Signature Formatting (MANDATORY!)
When documenting VB.NET code in markdown files:
- ✅ CORRECT: Write as plain text: `ProcessMovieFiles(folderPath As String, recursive As Boolean)`
- ✅ CORRECT: Write as description: "The ProcessMovieFiles method takes a folder path and recursion flag"
- ❌ WRONG: Using fenced code blocks for signatures breaks the outer markdown block

### Rule 3: Code Example Formatting (MANDATORY!)
When including VB.NET examples in generated markdown:

**Option A - Inline code (preferred):**
Use single backticks: `Dim movies As New List(Of Movie)()`

**Option B - Indented code block (for multi-line):**
Indent with 4 spaces:

    Dim movies As New List(Of Movie)()
    For Each movie In movies
        Console.WriteLine(movie.Title)
    Next

**NEVER use this (breaks generation):**
Using triple backticks inside markdown file generation will terminate the outer code block

### Rule 4: Pre-Generation Checklist (EXECUTE BEFORE EVERY MARKDOWN FILE GENERATION!)
Before generating ANY markdown file content, Copilot MUST:
1. ✓ Confirm: "I will NOT use triple backticks inside the markdown content"
2. ✓ Confirm: "I will use single backticks for inline code: `code`"
3. ✓ Confirm: "I will use 4-space indentation for multi-line code examples"
4. ✓ Confirm: "I will format method signatures as plain text or inline code only"

### Rule 5: Post-Generation Verification (EXECUTE AFTER GENERATION!)
After generating markdown file content, Copilot MUST:
1. ✓ Scan the entire generated content for triple backticks (``)
2. ✓ If found: STOP, revise to use single backticks or indentation
3. ✓ Verify all code examples use only inline backticks or 4-space indentation
4. ✓ Confirm the markdown content is valid and complete

### Rule 6: Emergency Fallback Rules
If unsure how to format something in markdown:
- Default to plain text description
- Use inline code with single backticks if absolutely needed
- Break complex examples into simple inline segments
- NEVER risk using triple backticks "just to try it"

## Response Generation Process

### For Markdown Files:
1. **PAUSE** - Review all markdown generation rules above
2. **EXECUTE** - Pre-generation checklist (Rule 4)
3. **GENERATE** - Content using ONLY inline backticks and indentation
4. **VERIFY** - Post-generation verification (Rule 5)
5. **DELIVER** - Only after confirming no triple backticks exist in content

### For Code Updates:
- Provide only the specific changes needed, not full file rewrites unless necessary
- Include full file path with code suggestions
- Preserve existing code structure and style
- Match existing comment styles and indentation

### For Documentation:
- Use inline code formatting exclusively: `PropertyName`, `MethodName()`
- Use 4-space indentation for code blocks in markdown files
- Structure with clear headings, lists, and links

## Code Style Guidelines

### VB.NET Type Declaration
- Use specific types when known: `Dim movieList As List(Of Movie)`
- Avoid Object type unless necessary for interop or reflection
- Use nullable types when appropriate: `Dim rating As Integer?`
- Prefer strongly-typed collections: `List(Of T)`, `Dictionary(Of TKey, TValue)`

### Property Declaration (VB.NET)
- Use Property blocks with explicit Get/Set when logic is needed
- Use auto-implemented properties for simple cases: `Public Property Title As String`
- Follow existing patterns in the codebase

### Error Handling
- Use specific exception types in Catch blocks
- Log exceptions appropriately for debugging
- Clean up resources in Finally blocks or use Using statements
- Provide meaningful error messages

### XML Documentation
- Document all public APIs with XML comments
- Include parameter descriptions and return value documentation
- Provide usage examples for complex methods
- Keep documentation synchronized with code changes

## File Organization
- Keep related functionality together in modules or classes
- Separate UI logic from business logic
- Use appropriate namespaces for different addon components
- Follow existing project structure for new addons

## Dependencies and References
- Minimize external dependencies when possible
- Document any new NuGet packages or references required
- Ensure compatibility with .NET Framework 4.8
- Test addon functionality in isolation when possible

## Testing Considerations
- Test scraper addons against actual data sources when possible
- Validate XML/NFO file parsing thoroughly
- Test UI addons for responsiveness and error handling
- Consider edge cases (missing data, network failures, etc.)

## Comments and Documentation
- Write clear, concise comments explaining "why" not "what"
- Document complex algorithms or business logic
- Keep comments synchronized with code changes
- Use TODO comments for planned improvements: `' TODO: Implement caching`

## Best Practices
- Follow existing code patterns and conventions in the solution
- Extract complex logic into separate methods with descriptive names
- Keep methods focused on a single responsibility
- Validate input parameters at method boundaries
- Use meaningful names that indicate purpose and type
- Avoid magic numbers and strings - use named constants
- Keep addon interfaces clean and well-documented
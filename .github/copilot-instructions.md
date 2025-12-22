# Project Coding Standards - Updated 2025-09-04

## Naming Conventions
- Private constants: Use camelCase with 'k' prefix (e.g., kInfoPrefix)
- Private fields: Use camelCase with underscore prefix (e.g., _httpClient)
- Public properties: Use PascalCase (e.g., FolderPath)
- Method parameters: Use camelCase (e.g., folderPath)
- Local variables: Use camelCase (e.g., nfoFiles)
- Public methods: Use PascalCase (e.g., ProcessFiles)
- Private methods: Use PascalCase (e.g., ValidateFolder)

## .axaml File Formatting Guidelines
- All Avalonia `.axaml` files must use explicit closing tags for every block element.
- Do not use self-closing tags (e.g., `<TextBlock ... />`) for any controls, borders, panels, or other XAML elements.
- Example (correct): `<TextBlock Text="Example"></TextBlock>`
- Example (incorrect): `<TextBlock Text="Example" />`
- This rule applies to all controls, panels, borders, images, and custom elements in `.axaml` files.
- Always match the indentation and block style used in the rest of the project.

## Markdown File Generation Guidelines - Important!
- When generating markdown files, ensure ALL content stays within the markdown code block
- **NEVER** include method signatures or code snippets that could break the markdown code block boundary
- Use plain text descriptions instead of formatted code blocks within markdown file generation
- Format method signatures as: `MethodName(parameters)` or as plain text descriptions
- Example: Write "FormatMessage(RichMessage message)" instead of using code block formatting
- Always verify the entire markdown content is contained within a single code block when generating files
- If including code examples in markdown files, use inline code formatting with backticks rather than code blocks
- Ensure all markdown files are well-structured with headings, lists, and links as needed

## Additional Markdown Safety Rules - Critical!
- **BEFORE generating any markdown file**: Read and confirm understanding of markdown code block rules
- **DURING generation**: If you need to include code examples, ALWAYS use inline backticks: `code here`
- **NEVER use triple backticks (```) inside a markdown file generation** - this breaks the outer code block
- **AFTER generation**: Mentally verify that no triple backticks appear in the generated content
- **If unsure**: Use plain text descriptions instead of any code formatting
- **For complex examples**: Break into multiple inline code segments rather than code blocks
- **Remember**: The entire markdown file must be wrapped in a single outer code block

## Code Style Guidelines - Important!
- Target .NET 9 framework
- **Type Declaration Rules:**
  - Use `var` for:
    - Simple assignments with `new()` constructors: `var list = new List<string>();`
    - String literals: `var name = "example";`
    - Numeric literals: `var count = 5;`
    - Boolean literals: `var isValid = true;`
    - Obviously simple method calls: `var result = ToString();`
    - LINQ expressions where type is obvious
  - Use explicit types for:
    - Method return values: `string result = SomeMethod();`
    - Complex expressions: `DateTime startTime = DateTime.Now;`
    - Interface/abstract types: `IEnumerable<string> items = GetItems();`
    - When type isn't immediately obvious from right side
    - Generic type parameters: `List<MovieData> movies = new();`
    - Nullable types: `string? path = GetPath();`
- **Property Declaration Rules:**
  - **ALWAYS use block properties with explicit get/set bodies**
  - **NEVER use expression-bodied properties (=>)**
  - **NEVER use auto-properties for interface implementations**
  - Examples:
    ```csharp
    // ✅ CORRECT - Block property
    public string Name {
        get {
            return _name;
        }
        set {
            _name = value;
        }
    }
    
    // ✅ CORRECT - Read-only block property
    public ObservableCollection<RichMessage> MessagesCollection {
        get {
            return _messages;
        }
    }
    
    // ❌ WRONG - Expression-bodied property
    public ObservableCollection<RichMessage> MessagesCollection => _messages;
    
    // ❌ WRONG - Auto-property in implementation
    public string Name { get; set; }
    ```
- Follow existing naming conventions in the codebase
- Provide full file paths with code suggestions when suggesting code changes
- Whenever possible, only present code updates for existing files with the updated code
    - Don't rewrite entire files unless absolutely necessary 
- Match existing comment styles
- Extract complex logic into separate methods with descriptive names
- Keep methods focused on a single responsibility
- Add XML documentation for public methods and properties
- Use primary constructors for classes when appropriate according to IDE0290
- Use static abstract members for interfaces when appropriate according to IDE0291
    - Be careful with Avaloia controls that use static abstract members
    - Be careful with Avalonia and the use of static abstract members when Avalonia requires public for binding.
- Be careful with using directives:
  - Use `System` namespace explicitly when needed
  - Avoid unnecessary `using` directives
  - Remove unused `using` directives automatically

## Response Generation Process
- For markdown files: Always pause and review markdown formatting rules before starting
- For code updates: Provide only the specific changes needed, not full file rewrites
- For documentation: Use inline code formatting exclusively within markdown generation
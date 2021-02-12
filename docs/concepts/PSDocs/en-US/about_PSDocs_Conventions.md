# PSDocs_Conventions

## about_PSDocs_Conventions

## SHORT DESCRIPTION

Describes PSDocs Conventions including how to use and author them.

## LONG DESCRIPTION

PSDocs generates documents dynamically from input.
When generating multiple documents it is often necessary to name or annotate them in a structured manner.
Conventions achieve this by hooking into the pipeline to trigger custom actions defined in a script block.

### Using conventions

A convention once defined can be included by using the `-Convention` parameter of `Invoke-PSDocument`.
To use a convention specify the name of the convention by name.
For example:

```powershell
Invoke-PSDocument -Convention 'ExampleConvention';
```

If multiple conventions are specified in an array, all are executed in they are specified.
As a result, the convention specified last may override state set by earlier conventions.

### Defining conventions

To define a convention, add a `Export-PSDocumentConvention` block within a `.Doc.ps1` file.
When executed the `.Doc.ps1` must be in an included path or module with `-Path` or `-Module`.

The `Export-PSDocumentConvention` block works similar to the `Document` block.
Each convention must have a unique name.
For example:

```powershell
# Synopsis: An example convention.
Export-PSDocumentConvention 'ExampleConvention' {
    # Add code here
}
```

### Begin Process End blocks

Conventions define three executable blocks `Begin`, `Process`, `End` similar to a PowerShell function.
Each block is injected in a different part of the pipeline as follows:

- `Begin` occurs before the document definition is called.
- `Process` occurs directly after the document definition is called.
- `End` occurs after all documents have been generated.

Convention block limitations:

- `Begin` can not use document specific variables such as `$Document`.
- `End` can not use automatic variables except `$PSDocs.Output`.

By default, the `Process` block used.
For example:

```powershell
# Synopsis: The default { } executes the process block
Export-PSDocumentConvention 'ExampleConvention' {
    # Process block
}

# Synopsis: With optional -Process parameter name
Export-PSDocumentConvention 'ExampleConvention' -Process {
    # Process block
}
```

To use `Begin` or `End` explicitly add these blocks.
For example:

```powershell
Export-PSDocumentConvention 'ExampleConvention' -Process {
    # Process block
} -Begin {
    # Begin block
} -End {
    # End block
}
```

### Naming documents

Generated document output files are named based on InstanceName.
To alter the InstanceName of a document use the `InstanceName` property.

Syntax:

```text
$PSDocs.Document.InstanceName = value;
```

For example:

```powershell
# Synopsis: An example naming convention.
Export-PSDocumentConvention 'TestNamingConvention1' {
    $PSDocs.Document.InstanceName = 'NewName';
}
```

### Setting output path

Generated document output files are named based on OutputPath.
To alter the OutputPath of a document use the `OutputPath` property.

Syntax:

```text
$PSDocs.Document.OutputPath = value;
```

For example:

```powershell
# Synopsis: An example naming convention.
Export-PSDocumentConvention 'TestNamingConvention1' {
    $newPath = Join-Path -Path $PSDocs.Document.OutputPath -ChildPath 'new';
    $PSDocs.Document.OutputPath = $newPath;
}
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSDocs/blob/main/docs/concepts/PSDocs/en-US/about_PSDocs_Conventions.md.

## SEE ALSO

- [Invoke-PSDocument](https://github.com/BernieWhite/PSDocs/blob/main/docs/commands/PSDocs/en-US/Invoke-PSDocument.md)

## KEYWORDS

- Conventions
- PSDocs

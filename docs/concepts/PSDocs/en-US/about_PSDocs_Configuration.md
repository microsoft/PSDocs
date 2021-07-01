# PSDocs_Configuration

## about_PSDocs_Configuration

## SHORT DESCRIPTION

Describes custom configuration that can be used within PSDocs document definitions.

## LONG DESCRIPTION

PSDocs lets you generate dynamic markdown documents using PowerShell blocks known as document definitions.
Document definitions can read custom configuration set at runtime or within options to change rendering.
Within a document definition, PSDocs exposes custom configuration through the `$PSDocs` automatic variable.

### Setting configuration

To specify custom configuration, set a property of the `configuration` object.
Configuration can be set at runtime or as YAML by configuring `ps-docs.yaml`.

For example:

```yaml
# Example: ps-docs.yaml

# YAML: Using the configuration YAML property to set custom configuration 'MODULE1_KEY1'
configuration:
  MODULE1_KEY1: Value1
```

To ensure each custom key is unique use a prefix followed by an underscore that represent your module.
Key names are not case sensitive, however we recommend you use uppercase for consistency.

### Reading configuration

The `$PSDocs` automatic variable can be used within a Document definition to read configuration.
Each custom configuration key is available under the `.Configuration` property.
Additionally, several helper methods are available for advanced usage.

Syntax:

```powershell
$PSDocs.Configuration.<configurationKey>
```

For example:

```powershell
# Get the value of the custom configuration 'MODULE1_KEY1'
$PSDocs.Configuration.MODULE1_KEY1
```

The following helper methods are available:

- `GetStringValues(string configurationKey)` - The configuration value as an array of strings.
This helper will always returns an array of strings.
The array will be empty if the configuration key is not defined or empty.
- `GetValueOrDefault(string configurationKey, object defaultValue)` - Returns the configuration value.
When the configuration key is not defined the default value will be used instead.
- `GetBoolOrDefault(string configurationKey, bool defaultValue)` - The configuration value as a boolean.
When the configuration key is not defined the default value will be used instead.

Syntax:

```powershell
$PSDocs.Configuration.helper()
```

For example:

```powershell
# Example using GetStringValues
$values = $PSDocs.Configuration.GetStringValues('SAMPLE_AUTHORS');

# Example using GetValueOrDefault
$value = $PSDocs.Configuration.GetValueOrDefault('SAMPLE_CONTENT_OWNER', 'defaultUser');

# Example using GetBoolOrDefault
if ($PSDocs.Configuration.GetBoolOrDefault('SAMPLE_USE_PARAMETERS_SNIPPET', $True)) {
    # Execute code
}
```

## NOTE

An online version of this document is available at https://github.com/Microsoft/PSDocs/blob/main/docs/concepts/PSDocs/en-US/about_PSDocs_Configuration.md.

## SEE ALSO

- [Invoke-PSDocument](https://github.com/Microsoft/PSDocs/blob/main/docs/commands/PSDocs/en-US/Invoke-PSDocument.md)

## KEYWORDS

- Configuration
- PSDocs
- GetStringValues
- GetValueOrDefault
- GetBoolOrDefault

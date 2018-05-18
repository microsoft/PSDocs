# Integration with DocFX

DocFX is a open source tool that converts markdown documentation into HTML. PSDocs can be used to dynamically generate markdown that can be processed by DocFX.

DocFX uses a `docfx.json` to determine what content to include and how to process each file. To process markdown files a `build.content` section should be added to reference the output location where PSDocs will generate markdown relative to the location of `docfx.json`.

An example `docfx.json` is provided below.

```json
{
  "metadata": [
  ],
  "build": {
    "content": [
      {
        "files": [
          "**/**.md",
          "**/toc.yml",
          "*.md",
          "toc.yml"
        ]
      }
    ],
    "resource": [
      {
        "files": [
          "**/media/**"
        ]
      }
    ],
    "overwrite": [
      {
        "files": [ ],
        "exclude": [
          "out/**",
          "build/**"
        ]
      }
    ],
    "dest": "_site",
    "globalMetadataFiles": [],
    "fileMetadataFiles": [],
    "template": [
      "default"
    ],
    "postProcessors": [],
    "noLangKeyword": false,
    "keepFileLink": false,
    "cleanupCacheHistory": false,
    "disableGitFeatures": false
  }
}
```

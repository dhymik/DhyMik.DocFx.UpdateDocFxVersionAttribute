## DhyMik.BuildTasks.UpdateDocFxVersionAttribute
### Versions

##### v1.0.0 (2021-06-21)

Initial release.

##### v1.0.1 (2021-06-23)

- Changes to [doc/example.directory.build.targets](doc/example.directory.build.targets):  
 Improved the target that calls `DocFx serve` after successful run of DocFx.

- No change to files included in Nuget package.  
 Nuget package not updated, remains on v1.0.0

##### v1.1.0 (2021-06-24)

- Added generation of `globalMetadataVariables.css` file with css variables generated from `globalMetadata` attributes in `docfx.json`. The file is generated in the `styles` subfolder of the last specified template foder, or in the project root if no template is specified. This adds an alternative way to include `version` and other custom data in DocFx output. See readme.md for details.
- Enhanced the example in [readme.md](readme.md) to include an example using the auto-generated css variables.

- Nuget package updated to version 1.1.0



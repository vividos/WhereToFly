# Release checklist

- [ ] Increase version number in the files:
  * [ ] AndroidManifest.xml
  * [ ] VersionAssemblyInfo.cs
  * [ ] Directory.Build.props
  * [ ] Package.appxmanifest
  * [ ] SonarCloud.cmd
- [ ] Add changes for the version to Changelog.md
- [ ] Update all NuGet packages
- [ ] Run unit tests and fix fails
- [ ] Run StyleCop and fix all warnings
- [ ] Run /analyze build and fix all warnings
- [ ] Run SonarCloud build and fix all warnings
- [ ] Run Release build and check out all pages
- [ ] Carry out all [test cases](TestCases.md)
- [ ] Build app using Microsoft App Center
- [ ] Distribute Release using App Center
- [ ] Tag git change with version tag, in the format `version-1.2.3` and push tag to github

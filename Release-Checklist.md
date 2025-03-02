# Release checklist

- [ ] Increase version number in the files:
  * [ ] version.json
  * [ ] SonarCloud.cmd
- [ ] Add changes for the version to Changelog.md
- [ ] Update all NuGet packages
- [ ] Run unit tests and fix fails
- [ ] Run StyleCop and fix all warnings
- [ ] Run /analyze build and fix all warnings
- [ ] Run SonarCloud build and fix all warnings
- [ ] Run Release build and check out all pages
- [ ] Carry out all [test cases](TestCases.md)
- [ ] Build app using GitHub actions
- [ ] Distribute Release using App Center
- [ ] Tag git change with version tag, in the format `version-1.2.3` and push tag to github

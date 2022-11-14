# versionBump
bumping versions in github with c# using cli wrap

## Goals
- Version bump is automated
  - run on schedule
- No direct commits to main
  - all change should be raise via a PR
- Verion detail is file based
  - 3 segement version major.minor.build
  - build will be incremented automattically (build = build + 1)
  - developers bump the major.minor sgements as needed
- Creates Release Branch (not tag)
  - create a release branch named as per current version in file
- Bump Main via PR
  - no direct commits to main
  - create version bump branch
  - increment current version
  - commit and push
  - raise a PR

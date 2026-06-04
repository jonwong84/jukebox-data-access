# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-06-04

### Added
- New CRUD operations for Genre to allow for managing genres

### Updated
- Removed CreatedAt and UpdatedAt properties from responses

## [1.0.1] - 2026-06-02

### Updated
- Fixed SonarScan failure in CircleCI when no PR is open for a feature branch

## [1.0.0] - 2026-06-02

### Added
- Initial release of Jukebox.DataAccess
- `Jukebox.DataAccess` — data access layer with CRUD operations for song metadata
- `Jukebox.DataAccess.Contracts` — request and result contracts for data access operations
- `Jukebox.DataAccess.EntityFramework` — EF Core DbContext and entity models
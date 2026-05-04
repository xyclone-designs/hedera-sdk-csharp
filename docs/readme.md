# Documentation (DocFX)

This folder contains the documentation source for the project, built using DocFX.

DocFX generates a static documentation website from Markdown files and API metadata.

---

## Prerequisites

Make sure you have the following installed:

* .NET SDK
* DocFX (install as a global tool)

```
dotnet tool install -g docfx
```

Verify installation:

```
docfx --version
```

---

## Getting Started

### Navigate to the docs folder 

```
cd docs
```

The docfx file is already present. The command below will download dependencies and generate the docs to be served locally.

```
docfx docfx.json --serve
```

Open in your browser:

```
http://localhost:8080
```

---
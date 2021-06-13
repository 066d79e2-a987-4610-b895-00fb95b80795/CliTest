# Nrat

A .NET test runner implemented on top of `dotnet test`.

### Install

Clone the git repository. From the repository root, run:
```
dotnet pack -c Release
dotnet tool install -g --add-source ./nupkg clitest
```

### Usage

Run `clitest` from the root of your solution.

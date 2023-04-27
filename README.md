<p>
  <a href="https://one-beyond.com">
    <img src="Logo.png" width="300" alt="One Beyond" />
  </a>
</p>

[![Nuget version](https://img.shields.io/nuget/v/OneBeyond.Studio.Crosscuts?style=plastic)](https://www.nuget.org/packages/OneBeyond.Studio.Crosscuts)
[![Nuget downloads](https://img.shields.io/nuget/dt/OneBeyond.Studio.Crosscuts?style=plastic)](https://www.nuget.org/packages/OneBeyond.Studio.Crosscuts)
[![License](https://img.shields.io/github/license/OneBeyond/onebeyond-studio-core?style=plastic)](LICENSE)

# Introduction
Beyond Studio Core is a set of .NET libraries that can be used by developers to create their solutions based on Clean Architecture.
One of the examples on how these libraries can be used in a project can be found [here](https://github.com/onebeyond/onebeyond-studio-obelisk).

# Getting Started

### Supported .NET version:

7.0

# Documentation

For more detailed documentation, please refer to our [Wiki](https://github.com/onebeyond/onebeyond-studio-core/wiki)

# Contributing

If you want to contribute, we are currently accepting PRs and/or proposals/discussions in the issue tracker.

# One Beyond Studio Core Libraries Dependencies

```mermaid
 graph BT;
 B[Domain.SharedKernel] --> A[Crosscuts];
 C[Application.SharedKernel] --> B[Domain.SharedKernel];
 I[Infrastructure.Azure] --> A[Crosscuts];
 N[Hosting] --> C[Application.SharedKernel]
 D[DataAccess.EFCore] --> C[Application.SharedKernel];
 O[Hosting.AspNet]
 J[EntityAuditing.Domain] --> B[Domain.SharedKernel];
 K[EntityAuditing.Infrastructure] --> J[EntityAuditing.Domain];
 K[EntityAuditing.Infrastructure] --> C[Application.SharedKernel];
 L[EntityAuditing.SqlServer] --> K[EntityAuditing.Infrastructure];
 L[EntityAuditing.SqlServer] --> D[DataAccess.EFCore];
 M[EntityAuditing.AzureTableStorage] --> K[EntityAuditing.Infrastructure];
 P[Infrastructure.RabbitMQ] ==> A[Crosscuts];
```

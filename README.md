<p>
  <a href="https://one-beyond.com">
    <img src="Logo.png" width="300" alt="One Beyond" />
  </a>
</p>

[![Nuget version](https://img.shields.io/nuget/v/OneBeyond.Studio.Crosscuts?style=plastic)](https://www.nuget.org/packages/OneBeyond.Studio.Crosscuts)
[![Nuget downloads](https://img.shields.io/nuget/dt/OneBeyond.Studio.Crosscuts?style=plastic)](https://www.nuget.org/packages/OneBeyond.Studio.Crosscuts)
[![License](https://img.shields.io/github/license/OneBeyond/onebeyond-studio-core?style=plastic)](LICENSE)


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

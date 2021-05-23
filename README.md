# signalr-mediatr-lab


```cmd
dotnet new api -n BackendApi -o backend/src/BackendApi
dotnet new gitignore
ng new frontend --skip-install --minimal
npm i
npm install @ngrx/store --save
npm install @microsoft/signalr --save
```

 - open csproj
 - save solution
 - update nuget
 - install 

```xml
  <ItemGroup>
    <PackageReference Include="MediatR" Version="9.0.0" />
    <PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.SpaServices.Extensions" Version="5.0.6" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.1.4" />
  </ItemGroup>
```

 - startup
```csharp
app.UseSpa(spa =>
{
    if (env.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
    }
});
```

 - ng s
 - dotnet watch run

 - app.component.ts
```typescript
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    Hello world
  `,
  styles: []
})
export class AppComponent {
}

```
 - navigate http://localhost:5000 -> output expected Hello world
 - commit and PR



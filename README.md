# Интеграция со сторонней системой документооборота

Для реализации интеграции сторонней системы с системой MRS.DocumentМanagement необходимо создать проект **DocumentManagement.Connection.НазваниеСистемы** в папке Connections со следующими свойствами (версия x.x.x ставится текущая) и зависимостями: 
```xml
 <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <RootNamespace>MRS.DocumentManagement.Connection.НазваниеСистемы</RootNamespace>
    <Authors>Brio MRS</Authors>
    <Product>Brio MRS Connection</Product>
    <Company>Brio MRS©</Company>
    <AssemblyVersion>X.X.X.X</AssemblyVersion>
    <Version>X.X.X</Version>
 </PropertyGroup>

<ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="5.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="5.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
    <PackageReference Include="Serilog" Version="2.10.0" />
  </ItemGroup>

 <ItemGroup>
    <ProjectReference Include="..\DocumentManagement.Connection.Interface\DocumentManagement.Connection.Interface.csproj" />
    <ProjectReference Include="..\DocumentManagement.Connection.Utils\DocumentManagement.Connection.Utils.csproj" />
    <ProjectReference Include="..\DocumentManagement.General.Utils\DocumentManagement.General.Utils.csproj" />
  </ItemGroup>
```

  В проекте **DocumentManagement.Connection.НазваниеСистемы** необходимо реализовать следующие интерфейсы: 

- _IConnectionMeta_ (используется для создания объекта стороннего подключения);
- _IConnection_ (используется для постановления первичного подключения, отвечает за установление связи со сторонним api и создание нужных объектов для работы с ним);
- _IConnectionStorage_ (используется для загрузки\скачивания\удаления физических файлов в сторонней системе);

Для поддержки **синхронизации** данных, необходимо реализовать: 

- _AConnectionContext_ (Абстрактный класс, предоставляющий доступ с объектам синхронизации);
- _ISynchronizer\<ObjectiveExternalDto>_ (является прослойкой для работы с **Объектами Задач** между MRS.DM и сторонней системой);
- _ISynchronizer\<ProjectExternalDto>_ (является прослойкой для работы с **Проектами** между MRS.DM и сторонней системой);

Добавить статический класс расширение _НазваниеСистемыServiceCollectionExtensions_ с методом

```c#
public static IServiceCollection AddНазваниеСистемы(this IServiceCollection services)
{
        services.AddScoped<НазваниеСистемыConnection>();
        return services;
}
``` 

для работы с DI и прописать в нем добавление реализации _IConnection_ нужного типа (Scoped, Singleton, Transient). По стандартам Microsoft класс должен находится в неймспейсе _Microsoft.Extensions.DependencyInjection_.

Любой проект интеграции обязан быть покрыт тестами. Проект с тестами создается в папке Tests и называется **DocumentManagement.Connection.НазваниеСистемы.Tests**.
# Описание

> **Система управления документами**, СУД, DMS (англ. Document management system) — компьютерная система (или набор компьютерных программ), используемая для отслеживания и хранения электронных документов и/или образов (изображений и иных артефактов) бумажных документов.

Платформа **BRIO Mixed Reality System** визуализирует объекты цифрового мира, встраивая их в реальную физическую обстановку без пространственных нарушений. BRIO MRS предоставляет инструменты работы с цифровыми моделями здания и инженерных систем, непосредственно на строительной площадке в режиме реального времени. 

Основная задача проекта **BRIO MRS Document Management**, это реализовать хранение, обработку и преобразование данных, для эффективного управления процессами проектирования, строительства, эксплуатации и ликвидации объектов на всех стадиях жизненного цикла. В связи с тем, что кроме локального документооборота, BRIO MRS DM представляет пользователю возможность интеграции с другими системами, API которых не всегда поддерживается в рамках разработки на Unity, было решено вынести бизнес-логику в отдельный проект. Общение между клиентом (Unity) и сервером (.Net 5) происходит с помощью http-запросов. 

# Структура решения
## Интерфейсы

| Название | Описание |
| ---  | ---     |
| DocumentManagement._Interface_ | Проект содержит интерфейсы, которые реализуются как на стороне _Unity_, так и на стороне _DM_. Позволяют скрыть слой передачи данных. Так же проекте определяются _Data Transfer Objects (Dtos)_, с которыми данные интерфейсы и работают. |
| DocumentManagement._Connection.Interface_ | Интерфейсы для работы с внешней системой документооборота. Используются для интеграции _BRIO MRS DM_ с системами управления строительством. Интерфейсы для внешних подключений используют свой тип _Dto_. |
| DocumentManagement._General.Interface_ | Общие интерфейсы, модели и enums, использующиеся как в интеграциях с внешними _СУС_, так и в работе с _Unity_.|

## Основные проекты

| Название | Описание |
| ---  | ---     |
| DocumentManagement._Api_ | Уровень принятия и обработки http-запросов выделен в отдельный проект для возможной безболезненной замены протокола. Принимает и обрабатывает запросы, приходящие от клиента (_Unity_).|
| DocumentManagement | Основные сервисы для работы с документооборотом. В данном проекте реализуются основные интерфейсы (_DocumentManagement.Interface_), преобразовываются данные из _Dto_ в модели базы данных и формируются запросы к базе данных с использованием _Entity Framework (EF) Core_. |
| DocumentManagement._Database_ | Уровень базы данных. Хранит миграции, контекст базы данных и модели базы данных. Модели БД нужны для хранения данных в приемлемом для базы данных формате. Обычно такие модели содержат в себе внешние ключи, мосты для связей многие-ко-многим, разложение списков данных на составляющие. |
| DocumentManagement._Synchronizer_ | Синхронизатор. Отвечает за синхронизацию данных локальной базы данных с данными из внешнего документооборота на момент синхронизации.  |

## Внешние подключения

| Название | Описание |
| ---  | ---     |
| DocumentManagement.Connection._BIM360_ | Интеграция с системой управления строительством [BIM360](https://www.autodesk.com/bim-360/)  |
| DocumentManagement.Connection._GoogleDrive_ | Интеграция с файловой системой [GoogleDrive](https://www.google.com/intl/ru_ru/drive/) |
| DocumentManagement.Connection._LementPro_ | Интеграция с системой управления строительством [LementPro](https://www.lement.pro/ru/) |
| DocumentManagement.Connection._Tdms_ | Интеграция с системой управления строительством [Tdms](https://tdms.ru/) |
| DocumentManagement.Connection._YandexDisk_ | Интеграция с с файловой системой [YandexDisk](https://disk.yandex.ru/) |
| DocumentManagement.Connection._MrsPro_ | Интеграция с системой управления строительством [MrsPro](https://mrspro.ru/solutions/strojkontrol/) |

## Утилиты

| Название | Описание |
| ---  | ---     |
| DocumentManagement._Utils_ | Дополнительные утилиты, использующиеся локальным документооборотом. |
| DocumentManagement._Connection.Utils_ | Дополнительные утилиты, общие для внешних подключений. |
| DocumentManagement._General.Utils_ | Утилиты, доступные всем проектам. |

## Тесты

Все тесты расположены в отдельной папке с названием Tests. 

# Интеграция со сторонней системой документооборота
## Реализация

Для реализации интеграции сторонней системы с системой MRS.DocumentМanagement необходимо создать проект **DocumentManagement.Connection.НазваниеСистемы** в папке Connections со следующими свойствами (версия x.x.x ставится текущая) и начальными зависимостями: 
```xml
 <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <RootNamespace>Brio.Docs.Connection.НазваниеСистемы</RootNamespace>
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

Любой проект интеграции обязан быть покрыт тестами. Проект с тестами создается в папке Tests и называется **DocumentManagement.Connection.НазваниеСистемы.Tests**. В тестовом проекте так же стоит прописать namespace и текущую версию:
```xml
 <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <RootNamespace>Brio.Docs.Connection.НазваниеСистемы.Tests</RootNamespace>
    <Version>x.x.x</Version>
  </PropertyGroup>
```

## Регистрация внешнего подключения в MRS и первая синхронизация данных

Для работы с _интеграцией\внешним подключением_ в тестовом режиме, необходимо создать тестового пользователя и привязать к нему интеграцию. 

Необходимо: 

1. Зарегистрировать интеграцию командой: `GET /ConnectionTypes/register`
2. Если все было сделано правильно, то команда `GET /ConnectionTypes` выдаст список возможных подключений, в котором будет находится новая интеграция. Для дальнейших действий необходимо запомнить _{ConnectionTypeId}_ только что созданого типа внешнего подключения;
3. Создать пользователя командой `POST /Users` и контентом вида
    ```json
    {
        "login": "наименованиесистемы",
        "password": "123",
        "name": "Фамилия Имя"
    }
    ```
    Где _**наименованиесистемы**_ это имя интеграции, к которой будет привязан пользователь. Написанное маленькими буквами, слитно. **_Фамилию и Имя_** пользователя необходимо заменить на настоящие Имя и Фамилию, чтобы во время демонстрации функционала в тестовом режиме интеграция выглядела презентабельно. Всем тестовым пользователям изначально присваивается **_пароль_** 123, чтобы его было легко запомнить и просто вводить во время ручного тестирования.

    Если пользователь создался успешно, то нужно запомнить его _{User.Id}_
4. Создать подключение командой `POST /Connections` и контентом
    ```json
        {
            "connectionTypeID": { "id" : ConnectionTypeId },
            "userID": { "id" : UserId },
            "authFieldValues": {
                "password": "passwordValue",
                "login": "loginValue"
            }
        }
    ```
    Где **_ConnectionTypeId_** это тип внешнего подключения, а **_UserId_** идентификатор пользователя. Значения **_authFieldValues_** могут разниться в зависимости от внешнего подключения.
5. Командой `GET /Connections​/connect​/{UserId}` необходимо подключиться к внешней системе.
6. Командой `GET ​/Connections​/synchronization​/{UserId}` провести первую синхронизацию.
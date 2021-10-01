### MathCore.EF7

Набор библиотек-расширений и шаблонов реализаций для обеспечения взаимодействия с базой данных, Api и клиентом для серверных приложений.

### Repositories

Для работы с базой данных доступны репозитории и фабрика:

```C#
public static IServiceCollection AddTestRepositories(this IServiceCollection services) =>
	services
	   .AddScoped(typeof(IRepository<>), typeof(Your_DbRepository<>))
	   .AddScoped(typeof(IRepository<,>), typeof(Your_DbRepository<,>));

public static IServiceCollection AddTestRepositoryFactories(this IServiceCollection services) =>
	services
	   .AddScoped(typeof(IRepository<>), typeof(Your_DbContextFactoryRepository<>))
	   .AddScoped(typeof(IRepository<,>), typeof(Your_DbContextFactoryRepository<,>));
```
где:
Your_DbRepository - реализация репозитория, унаследованная от базового класса DbRepository
Your_DbContextFactoryRepository - реализация фабрики репозитория, унаследованная от базового класса DbContextFactoryRepository

[пример реализации](https://github.com/Infarh/MathCore.EF7/blob/dev/Tests/DAL/DAL/Repositories/Test_DbRepository.cs)

Вызов репозитория происходит при запросе экземпляра интерфейса `IRepository<Student>`, где Student - некий класс `унаследованный от Entity`, в данном контексте у сущности id представляет тип `int32`
При необходимости выбрать другой тип ключа сущность должна быть унаследована от `Entity<Tkey>`, где Tkey - тип ключа (например `Guid`), в этом случае обращение к репозиторию происходит при запросе `IRepository<Student,Guid>`

Вызов фабрики происходит сжожим способом.

Возможности репозитория:
<details>	
  <br />
  <summary><b>⚙️ Конечные точки</b></summary>
  	<ul>
      <li>Task<bool> <b>IsEmpty</b>(CancellationToken Cancel = default)</li>
	    <li>Task<bool> <b>ExistId</b>(TKey Id, CancellationToken Cancel = default)</li>
  	  <li>Task<int> <b>GetCount</b>(CancellationToken Cancel = default)</li>
	    <li>Task<IEnumerable<TEntity>> <b>GetAll</b>(CancellationToken Cancel = default)</li>
      <li>Task<IEnumerable<TEntity>> <b>Get</b>(int Skip, int Count, CancellationToken Cancel = default)</li>
      <li>Task<IPage<TEntity>> <b>GetPage</b>(int PageNumber, int PageSize, CancellationToken Cancel = default)</li>
      <li>Task<TEntity> <b>GetById</b>(TKey Id, CancellationToken Cancel = default)</li>
      <li>Task<int> <b>SaveChanges</b>(CancellationToken Cancel = default)</li>
      <li>Task<bool> <b>Exist</b>(TEntity item, CancellationToken Cancel = default)</li>
	    <li>Task<TEntity> <b>Add</b>(TEntity item, CancellationToken Cancel = default)</li>
  	  <li>Task <b>AddRange</b>(IEnumerable<TEntity> items, CancellationToken Cancel = default)</li>
       <li>Task<TEntity> <b>Update</b>(TEntity item, CancellationToken Cancel = default)</li>
	    <li>Task<TEntity> <b>UpdateById</b>(TKey id, Action<TEntity> ItemUpdated, CancellationToken Cancel = default)</li>
  	  <li>Task <b>UpdateRange</b>(IEnumerable<TEntity> items, CancellationToken Cancel = default)</li>
      <li>Task<TEntity> <b>Delete</b>(TEntity item, CancellationToken Cancel = default)</li>
	    <li>Task <b>DeleteRange</b>(IEnumerable<TEntity> items, CancellationToken Cancel = default)</li>
  	  <li>Task<TEntity> <b>DeleteById</b>(TKey id, CancellationToken Cancel = default)</li>
	</ul>	
</details>
При необходимости реализации можно переопределить унаследовавшись.

### Context
Упростить инициализацию экземпляра БД при старте приложения поможет [`DBInitializer<TContext>`](https://github.com/Infarh/MathCore.EF7/blob/devs/MathCore.EF7/Contexts/DBInitializer.cs)
или аналогично [`DBFactoryInitializer<TContext>`](https://github.com/Infarh/MathCore.EF7/blob/dev/MathCore.EF7/Contexts/DBFactoryInitializer.cs)
[пример реализации](https://github.com/Infarh/MathCore.EF7/blob/dev/Tests/DAL/DAL/TestContextInitializer.cs)

подключаем сервис `services.AddTransient<TestContextInitializer>();`
Вызываем сервис 
```C#
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TestContextInitializer context)
        {
            context.InitializeAsync().Wait();
	...
```

### Clients

Для взаимодействия с клиентом реализован `WebRepository<TEntity, TKey>`
Возможности клиента:
<details>	
  <br />
  <summary><b>⚙️ Конечные точки</b></summary>
  	<ul>
      <details> <summary>Get </summary>
      <ul>
      <li>Task<bool> <b>IsEmpty</b>(CancellationToken Cancel = default)</li>
	    <li>Task<bool> <b>ExistId</b>(TKey Id, CancellationToken Cancel = default)</li>
  	  <li>Task<int> <b>GetCount</b>(CancellationToken Cancel = default)</li>
	    <li>Task<IEnumerable<TEntity>> <b>GetAll</b>(CancellationToken Cancel = default)</li>
      <li>Task<IEnumerable<TEntity>> <b>Get</b>(int Skip, int Count, CancellationToken Cancel = default)</li>
      <li>Task<IPage<TEntity>> <b>GetPage</b>(int PageNumber, int PageSize, CancellationToken Cancel = default)</li>
      <li>Task<TEntity> <b>GetById</b>(TKey Id, CancellationToken Cancel = default)</li>
      <li>Task<int> <b>SaveChanges</b>(CancellationToken Cancel = default)</li>
      </ul>
      </details>
      <details> <summary>Post </summary>
      <ul>
      <li>Task<bool> <b>Exist</b>(TEntity item, CancellationToken Cancel = default)</li>
	    <li>Task<TEntity> <b>Add</b>(TEntity item, CancellationToken Cancel = default)</li>
  	  <li>Task <b>AddRange</b>(IEnumerable<TEntity> items, CancellationToken Cancel = default)</li>
      </ul>
      </details>
	  <details> <summary>Put </summary>
      <ul>
      <li>Task<TEntity> <b>Update</b>(TEntity item, CancellationToken Cancel = default)</li>
	    <li>Task<TEntity> <b>UpdateById</b>(TKey id, Action<TEntity> ItemUpdated, CancellationToken Cancel = default)</li>
  	  <li>Task <b>UpdateRange</b>(IEnumerable<TEntity> items, CancellationToken Cancel = default)</li>
      </ul>
      </details>
	  <details> <summary>Delete </summary>
      <ul>
      <li>Task<TEntity> <b>Delete</b>(TEntity item, CancellationToken Cancel = default)</li>
	    <li>Task <b>DeleteRange</b>(IEnumerable<TEntity> items, CancellationToken Cancel = default)</li>
  	  <li>Task<TEntity> <b>DeleteById</b>(TKey id, CancellationToken Cancel = default)</li>
      </ul>
      </details>
	    <br />
	</ul>	
</details>

При необходимости реализации можно переопределить унаследовавшись.

### Controllers

Шаблон реализации представлен в `ApiController<TEntity, TKey>` или `ApiController<TEntity>` аналогично репозиториям.
Конечные точки идентичны точкам WebRepository.
<details>	
  <br />
  <summary><b>⚙️ Конечные точки</b></summary>
  	<ul>
      <details> <summary>Get </summary>
      <ul>
      <li>[HttpGet("isempty")] <br/>Task<bool> <b>IsEmpty</b>()</li>
	    <li>[HttpGet("exist/{Id}")]<br/> Task<bool> <b>ExistId</b>(TKey Id)</li>
  	  <li>[HttpGet("count")]<br/> Task<int> <b>GetCount</b>()</li>
	    <li>[HttpGet]<br/> Task<IEnumerable<TEntity>> <b>GetAll</b>()</li>
      <li>[HttpGet("items[[{Skip:int}/{Count:int}]]")]<br/> Task<IEnumerable<TEntity>> <b>Get</b>(int Skip, int Count)</li>
      <li>[HttpGet("page/{PageNumber:int}/{PageSize:int}")]<br/>
	  [HttpGet("page[[{PageNumber:int}/{PageSize:int}]]")]<br/>
	  Task<IPage<TEntity>> <b>GetPage</b>(int PageNumber, int PageSize)</li>
      <li>[HttpGet("{Id}")] <br/>Task<TEntity> <b>GetById</b>(TKey Id)</li>
      <li>[HttpGet("save")][HttpPost("save")][HttpPut("save")] <br/>Task<int> <b>SaveChanges</b>()</li>
      </ul>
      </details>
      <details> <summary>Post </summary>
      <ul>
      <li>[HttpPost("exist")] <br/>Task<bool> <b>Exist</b>(TEntity item)</li>
	    <li>[HttpPost]<br/>Task<TEntity> <b>Add</b>(TEntity item)</li>
  	  <li>[HttpPost("range")]<br/>Task <b>AddRange</b>(IEnumerable<TEntity> items)</li>
      </ul>
      </details>
	  <details> <summary>Put </summary>
      <ul>
      <li>[HttpPut]<br/>Task<TEntity> <b>Update</b>(TEntity item)</li>
	    <li>[HttpPut("{id}")]<br/>Task<TEntity> <b>UpdateById</b>(TKey id, Action<TEntity> ItemUpdated)</li>
  	  <li>[HttpPut("range")]<br/>Task <b>UpdateRange</b>(IEnumerable<TEntity> items)</li>
      </ul>
      </details>
	  <details> <summary>Delete </summary>
      <ul>
      <li>[HttpDelete]<br/>Task<TEntity> <b>Delete</b>(TEntity item)</li>
	    <li>[HttpDelete("range")]<br/>Task <b>DeleteRange</b>(IEnumerable<TEntity> items)</li>
  	  <li>[HttpDelete("{id}")]<br/>Task<TEntity> <b>DeleteById</b>(TKey id)</li>
      </ul>
      </details>
	    <br />
	</ul>	
</details>
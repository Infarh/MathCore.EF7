using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Controllers
{
    /// <inheritdoc/>
    public abstract class ApiController<T> : ApiController<T, int> where T : IEntity<int>
    {
        /// <inheritdoc/>
        protected ApiController(IRepository<T> repository, ILogger<ApiController<T>> logger) : base(repository, logger)
        {
        }
    }

    /// <summary> реализация контроллера для api </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    /// <typeparam name="TKey">тип ключа сущности</typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiController<T, TKey> : ControllerBase where T : IEntity<TKey>
    {
        /// <summary> клиент репозитория </summary>
        protected readonly IRepository<T, TKey> _Repository;
        /// <summary> логгер </summary>
        protected readonly ILogger<ApiController<T, TKey>> _Logger;

        /// <inheritdoc/>
        protected ApiController(IRepository<T, TKey> repository, ILogger<ApiController<T, TKey>> logger)
        {
            _Repository = repository;
            _Logger = logger;
        }
        #region Implementation of IRepository<T,in Tkey>

        /// <summary>Проверка репозитория на пустоту</summary>
        /// <returns>Истина, если в репозитории нет ни одной сущности</returns>
        [HttpGet("isempty")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public virtual async Task<IActionResult> IsEmpty()
        {
            return Ok(await _Repository.IsEmpty());
        }
        /// <summary>Существует ли сущность с указанным идентификатором</summary>
        /// <param name="Id">Проверяемый идентификатор сущности</param>
        /// <returns>Истина, если сущность с указанным идентификатором существует в репозитории</returns>
        [HttpGet("exist/{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(bool))]
        public virtual async Task<IActionResult> ExistId(TKey Id)
        {
            return await _Repository.ExistId(Id) ? Ok(true) : NotFound(false);
        }

        /// <summary>Существует ли в репозитории указанная сущность</summary>
        /// <param name="item">Проверяемая сущность</param>
        /// <returns>Истина, если указанная сущность существует в репозитории</returns>
        //[HttpGet("exist")] //ToDo проверить реализацию (ошибка передаваемых данных в строке запроса) Unsupported Media Type
        [HttpPost("exist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(bool))]
        public virtual async Task<IActionResult> Exist(T item)
        {
            return await _Repository.Exist(item) ? Ok(true) : NotFound(false);
        }

        /// <summary>Получить число хранимых сущностей</summary>
        /// <returns>число сущностей в репозитории</returns>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public virtual async Task<IActionResult> GetCount()
        {
            return Ok(await _Repository.GetCount());
        }

        /// <summary>Извлечь все сущности из репозитория</summary>
        /// <returns>Перечисление всех сущностей репозитория</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<ActionResult<IEnumerable<T>>> GetAll()
        {
            return Ok(await _Repository.GetAll());
        }
        /// <summary>Получить набор сущностей из репозитория в указанном количестве, предварительно пропустив некоторое количество</summary>
        /// <param name="Skip">Число предварительно пропускаемых сущностей</param>
        /// <param name="Count">Число извлекаемых из репозитория сущностей</param>
        /// <returns>Перечисление полученных из репозитория сущностей</returns>
        [HttpGet("items[[{Skip:int}/{Count:int}]]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<ActionResult<IEnumerable<T>>> Get(int Skip, int Count)
        {
            return Ok(await _Repository.Get(Skip, Count));
        }
        /// <summary>Получить страницу с сущностями из репозитория</summary>
        /// <param name="PageNumber">Номер страницы начиная с нуля</param>
        /// <param name="PageSize">Размер страницы</param>
        /// <returns>Страница с сущностями из репозитория</returns>
        [HttpGet("page/{PageNumber:int}/{PageSize:int}")]
        [HttpGet("page[[{PageNumber:int}/{PageSize:int}]]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<IPage<T>>> GetPage(int PageNumber, int PageSize)
        {
            var result = await _Repository.GetPage(PageNumber, PageSize);
            return result.Items.Any()
                ? Ok(result)
                : NotFound(result);
        }

        /// <summary>Получить сущность по указанному идентификатору</summary>
        /// <param name="Id">Идентификатор извлекаемой сущности</param>
        /// <returns>Сущность с указанным идентификатором в случае ее наличия и null, если сущность отсутствует</returns>
        [HttpGet("{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetById(TKey Id)
        {
            return await _Repository.GetById(Id) is { } item ? Ok(item) : NotFound();
        }

        /// <summary>Добавление сущности в репозиторий</summary>
        /// <param name="item">Добавляемая в репозиторий сущность</param>
        /// <returns>Добавленная в репозиторий сущность</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public virtual async Task<IActionResult> Add(T item)
        {
            var result = await _Repository.Add(item);
            if (result is null)
                return Conflict($"Conflicted: element with id={item.Id} is already exist");
            return CreatedAtAction(nameof(GetById), new { result.Id }, result);
        }

        /// <summary>Добавление перечисленных сущностей в репозиторий</summary>
        /// <param name="items">Перечисление добавляемых в репозиторий сущностей</param>
        /// <returns>Задача, завершающаяся при завершении операции добавления сущностей</returns>
        [HttpPost("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> AddRange(IEnumerable<T> items)
        {
            await _Repository.AddRange(items);
            return Ok();
        }
        /// <summary>Обновление сущности в репозитории</summary>
        /// <param name="item">Сущность, хранящая в себе информацию, которую надо обновить в репозитории</param>
        /// <returns>Сущность из репозитория с обновленными данными</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Update(T item)
        {
            if (await _Repository.Update(item) is not { } result)
                return NotFound();
            return AcceptedAtAction(nameof(GetById), new { result.Id }, result);
        }

        /// <summary>Обновление перечисленных сущностей</summary>
        /// <param name="items">Перечисление сущностей, информацию из которых надо обновить в репозитории</param>
        /// <returns>Задача, завершаемая при завершении операции обновления сущностей</returns>
        [HttpPut("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> UpdateRange(IEnumerable<T> items)
        {
            await _Repository.UpdateRange(items);
            return Ok();
        }
        /// <summary>Удаление сущности из репозитория</summary>
        /// <param name="item">Удаляемая из репозитория сущность</param>
        /// <returns>Удаленная из репозитория сущность</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> Delete(T item)
        {
            if (await _Repository.Delete(item) is not { } result)
                return NotFound();
            return Ok(result);
        }

        /// <summary>Удаление перечисления сущностей из репозитория</summary>
        /// <param name="items">Перечисление удаляемых сущностей</param>
        /// <returns>Задача, завершаемая при завершении операции удаления сущностей</returns>
        [HttpDelete("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> DeleteRange(IEnumerable<T> items)
        {
            await _Repository.DeleteRange(items);
            return Ok();
        }
        /// <summary>Удаление сущности по заданному идентификатору</summary>
        /// <param name="id">Идентификатор сущности, которую надо удалить</param>
        /// <returns>Удаленная из репозитория сущность</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> DeleteById(TKey id)
        {
            if (await _Repository.DeleteById(id) is not { } result)
                return NotFound();
            return Ok(result);
        }

        /// <summary> Сохранить изменения </summary>
        /// <returns>число изменений</returns>
        [HttpGet("save")]
        [HttpPost("save")]
        [HttpPut("save")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public virtual async Task<IActionResult> SaveChanges()
        {
            return Ok(await _Repository.SaveChanges());
        }

        #endregion
    }
}

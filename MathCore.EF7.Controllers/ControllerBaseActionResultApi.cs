using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathCore.EF7.Interfaces.Entities;
using MathCore.EF7.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MathCore.EF7.Controllers
{
    /// <inheritdoc/>
    public class ControllerBaseActionResultApi<T> : ControllerBaseActionResultApi<T, int> where T:IEntity<int>
    {
        /// <inheritdoc/>
        public ControllerBaseActionResultApi(IRepository<T, int> repository, ILogger<ControllerBaseActionResultApi<T, int>> logger) : base(repository, logger)
        {
        }
    }

    /// <summary> реализация контроллера для api </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    /// <typeparam name="Tkey">тип ключа сущности</typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerBaseActionResultApi<T, Tkey> : ControllerBase where T : IEntity<Tkey>
    {
        /// <summary> клиент репозитория </summary>
        protected readonly IRepository<T, Tkey> _Repository;
        /// <summary> логгер </summary>
        protected readonly ILogger<ControllerBaseActionResultApi<T, Tkey>> _Logger;

        /// <inheritdoc/>
        public ControllerBaseActionResultApi(IRepository<T, Tkey> repository, ILogger<ControllerBaseActionResultApi<T, Tkey>> logger)
        {
            _Repository = repository;
            _Logger = logger;
        }
        #region Implementation of IRepository<T,in Tkey>

        /// <summary>Проверка репозитория на пустоту</summary>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Истина, если в репозитории нет ни одной сущности</returns>
        [HttpGet("isempty")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<IActionResult> IsEmpty(CancellationToken Cancel = default)
        {
            return Ok(await _Repository.IsEmpty(Cancel));
        }
        /// <summary>Существует ли сущность с указанным идентификатором</summary>
        /// <param name="Id">Проверяемый идентификатор сущности</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Истина, если сущность с указанным идентификатором существует в репозитории</returns>
        [HttpGet("exist/{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(bool))]
        public async Task<IActionResult> ExistId(Tkey Id, CancellationToken Cancel = default)
        {
            return await _Repository.ExistId(Id, Cancel) ? Ok(true) : NotFound(false);
        }

        /// <summary>Существует ли в репозитории указанная сущность</summary>
        /// <param name="item">Проверяемая сущность</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Истина, если указанная сущность существует в репозитории</returns>
        [HttpGet("exist")]
        [HttpPost("exist")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(bool))]
        public async Task<IActionResult> Exist(T item, CancellationToken Cancel = default)
        {
            return await _Repository.Exist(item, Cancel) ? Ok(true) : NotFound(false);
        }

        /// <summary>Получить число хранимых сущностей</summary>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>число сущностей в репозитории</returns>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<IActionResult> GetCount(CancellationToken Cancel = default)
        {
            return Ok(await _Repository.GetCount(Cancel));
        }

        /// <summary>Извлечь все сущности из репозитория</summary>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Перечисление всех сущностей репозитория</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<T>>> GetAll(CancellationToken Cancel = default)
        {
            return Ok(await _Repository.GetAll(Cancel));
        }
        /// <summary>Получить набор сущностей из репозитория в указанном количестве, предварительно пропустив некоторое количество</summary>
        /// <param name="Skip">Число предварительно пропускаемых сущностей</param>
        /// <param name="Count">Число извлекаемых из репозитория сущностей</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Перечисление полученных из репозитория сущностей</returns>
        [HttpGet("items[[{Skip:int}:{Count:int}]]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<T>>> Get(int Skip, int Count, CancellationToken Cancel = default)
        {
            return Ok(await _Repository.Get(Skip, Count, Cancel));
        }
        /// <summary>Получить страницу с сущностями из репозитория</summary>
        /// <param name="PageNumber">Номер страницы начиная с нуля</param>
        /// <param name="PageSize">Размер страницы</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Страница с сущностями из репозитория</returns>
        [HttpGet("page/{PageNumber:int}/{PageSize:int}")]
        [HttpGet("page[[{PageNumber:int}:{PageSize:int}]]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IPage<T>>> GetPage(int PageNumber, int PageSize, CancellationToken Cancel = default)
        {
            var result = await _Repository.GetPage(PageNumber, PageSize, Cancel);
            return result.Items.Any()
                ? Ok(result)
                : NotFound(result);
        }

        /// <summary>Получить сущность по указанному идентификатору</summary>
        /// <param name="Id">Идентификатор извлекаемой сущности</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Сущность с указанным идентификатором в случае ее наличия и null, если сущность отсутствует</returns>
        [HttpGet("{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int Id, CancellationToken Cancel = default)
        {
            return await _Repository.GetById(Id, Cancel) is { } item ? Ok(item) : NotFound();
        }

        /// <summary>Добавление сущности в репозиторий</summary>
        /// <param name="item">Добавляемая в репозиторий сущность</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Добавленная в репозиторий сущность</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Add(T item, CancellationToken Cancel = default)
        {
            var result = await _Repository.Add(item, Cancel);
            return CreatedAtAction(nameof(GetById), new { Id = result.Id });
        }

        /// <summary>Добавление перечисленных сущностей в репозиторий</summary>
        /// <param name="items">Перечисление добавляемых в репозиторий сущностей</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Задача, завершающаяся при завершении операции добавления сущностей</returns>
        [HttpPost("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddRange(IEnumerable<T> items, CancellationToken Cancel = default)
        {
            await _Repository.AddRange(items, Cancel);
            return Ok();
        }
        /// <summary>Обновление сущности в репозитории</summary>
        /// <param name="item">Сущность, хранящая в себе информацию, которую надо обновить в репозитории</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Сущность из репозитория с обновленными данными</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(T item, CancellationToken Cancel = default)
        {
            if (await _Repository.Update(item, Cancel) is not { } result)
                return NotFound();
            return AcceptedAtAction(nameof(GetById), new { Id = result.Id });
        }

        /// <summary>Обновление сущности в репозитории</summary>
        /// <param name="id">Идентификатор обновляемой сущности</param>
        /// <param name="ItemUpdated">Метод обновления информации в заданной сущности</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Сущность из репозитория с обновленными данными</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateById(Tkey id, Action<T> ItemUpdated, CancellationToken Cancel = default)
        {
            if (await _Repository.UpdateById(id, ItemUpdated, Cancel) is not { } result)
                return NotFound();
            return AcceptedAtAction(nameof(GetById), new { Id = result.Id });
        }
        /// <summary>Обновление перечисленных сущностей</summary>
        /// <param name="items">Перечисление сущностей, информацию из которых надо обновить в репозитории</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Задача, завершаемая при завершении операции обновления сущностей</returns>
        [HttpPut("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateRange(IEnumerable<T> items, CancellationToken Cancel = default)
        {
            await _Repository.UpdateRange(items, Cancel);
            return Ok();
        }
        /// <summary>Удаление сущности из репозитория</summary>
        /// <param name="item">Удаляемая из репозитория сущность</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Удаленная из репозитория сущность</returns>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(T item, CancellationToken Cancel = default)
        {
            if (await _Repository.Delete(item, Cancel) is not { } result)
                return NotFound();
            return Ok(result);
        }

        /// <summary>Удаление перечисления сущностей из репозитория</summary>
        /// <param name="items">Перечисление удаляемых сущностей</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Задача, завершаемая при завершении операции удаления сущностей</returns>
        [HttpDelete("range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteRange(IEnumerable<T> items, CancellationToken Cancel = default)
        {
            await _Repository.DeleteRange(items, Cancel);
            return Ok();
        }
        /// <summary>Удаление сущности по заданному идентификатору</summary>
        /// <param name="id">Идентификатор сущности, которую надо удалить</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Удаленная из репозитория сущность</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById(Tkey id, CancellationToken Cancel = default)
        {
            if (await _Repository.DeleteById(id, Cancel) is not { } result)
                return NotFound();
            return Ok(result);
        }

        /// <summary> Сохранить изменения </summary>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>число изменений</returns>
        [HttpGet("save")]
        [HttpPost("save")]
        [HttpPut("save")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<IActionResult> SaveChanges(CancellationToken Cancel = default)
        {
            return Ok(await _Repository.SaveChanges(Cancel));
        }

        #endregion
    }
}

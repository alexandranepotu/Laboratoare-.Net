using laboratorul3.Features.Books;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using laboratorul3.Features.Books.Commands;
using laboratorul3.Features.Books.Queries;

namespace laboratorul3.Controllers
{
    [ApiController]
    [Route("books")]
    public class BooksController : ControllerBase
    {
        private readonly IMediator _mediator;
        public BooksController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookCreateDto dto)
        {
            var command = new CreateBookCommand(dto.Title, dto.Author, dto.Year);
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BookCreateDto dto)
        {
            var command = new UpdateBookCommand(id, dto.Title, dto.Author, dto.Year);
            var result = await _mediator.Send(command);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteBookCommand(id));
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetBookByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            var query = new GetBooksQuery(page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
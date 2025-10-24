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
            var command = new CreateBookCommand(dto.Title, dto.Author, dto.Year); //creeaza comanda
            var id = await _mediator.Send(command); //trimite comanda catre CreateBookHandler
            return CreatedAtAction(nameof(GetById), new { id }, id);  //201 created
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
        public async Task<ActionResult<BookReadDto>> GetById(int id)
        {
            var result = await _mediator.Send(new GetBookByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<BookReadDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetBooksQuery(page, pageSize));
            return Ok(result);
        }
    }
}
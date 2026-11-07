using Microsoft.AspNetCore.Mvc;
using RailCat.Catalog.Api.Models.DTOs;
using RailCat.Catalog.Api.Services;

namespace RailCat.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMenuService _menuService;

    public CategoriesController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _menuService.GetCategoriesAsync();
        return Ok(categories);
    }
}

[ApiController]
[Route("api/menu-items")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuItemsController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MenuItemResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? categoryId)
    {
        var items = await _menuService.GetMenuItemsAsync(categoryId);
        return Ok(items);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _menuService.GetMenuItemByIdAsync(id);
        if (item == null) return NotFound(new { message = $"Menu item '{id}' not found." });
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemDto dto)
    {
        var created = await _menuService.CreateMenuItemAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.ItemId }, created);
    }

    [HttpPatch("{id}/availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvailability(string id, [FromBody] UpdateMenuItemAvailabilityDto dto)
    {
        var success = await _menuService.UpdateAvailabilityAsync(id, dto);
        if (!success) return NotFound(new { message = $"Menu item '{id}' not found." });
        return Ok(new { message = "Availability updated successfully." });
    }
}

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IConcessionService _concessionService;

    public EmployeesController(IConcessionService concessionService)
    {
        _concessionService = concessionService;
    }

    [HttpPost("verify-concession")]
    [ProducesResponseType(typeof(EmployeeVerificationResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyConcession([FromBody] EmployeeVerificationRequestDto dto)
    {
        var result = await _concessionService.VerifyEmployeeAsync(dto);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _concessionService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterEmployeeDto dto)
    {
        var created = await _concessionService.RegisterEmployeeAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id = created.EmployeeId }, created);
    }
}

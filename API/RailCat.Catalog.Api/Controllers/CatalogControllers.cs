using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailCat.Catalog.Api.Models.DTOs;
using RailCat.Catalog.Api.Services;

namespace RailCat.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<MenuItemResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? categoryId)
    {
        var items = await _menuService.GetMenuItemsAsync(categoryId);
        return Ok(items);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _menuService.GetMenuItemByIdAsync(id);
        if (item == null) return NotFound(new { message = $"Menu item '{id}' not found." });
        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,STORE_MANAGER")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromForm] CreateMenuItemDto dto)
    {
        var created = await _menuService.CreateMenuItemAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.ItemId }, created);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,STORE_MANAGER")]
    [Consumes("application/json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> CreateFromJson([FromBody] CreateMenuItemDto dto)
    {
        var created = await _menuService.CreateMenuItemAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.ItemId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,STORE_MANAGER")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    [ProducesResponseType(typeof(MenuItemResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(string id, [FromForm] UpdateMenuItemDto dto)
    {
        var updated = await _menuService.UpdateMenuItemAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Menu item '{id}' not found." });
        return Ok(updated);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,STORE_MANAGER")]
    [Consumes("application/json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> UpdateFromJson(string id, [FromBody] UpdateMenuItemDto dto)
    {
        var updated = await _menuService.UpdateMenuItemAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Menu item '{id}' not found." });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN,STORE_MANAGER")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _menuService.DeleteMenuItemAsync(id);
        if (!success) return NotFound(new { message = $"Menu item '{id}' not found." });
        return NoContent();
    }

    [HttpPatch("{id}/availability")]
    [Authorize(Roles = "ADMIN,STORE_MANAGER,COUNTER_OPERATOR,KITCHEN_STAFF")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAvailability(string id, [FromBody] UpdateMenuItemAvailabilityDto dto)
    {
        var success = await _menuService.UpdateAvailabilityAsync(id, dto);
        if (!success) return NotFound(new { message = $"Menu item '{id}' not found." });
        return Ok(new { message = "Availability updated successfully." });
    }
}

[ApiController]
[Route("api/concession-policies")]
[Authorize]
public class ConcessionPoliciesController : ControllerBase
{
    private readonly IConcessionService _concessionService;

    public ConcessionPoliciesController(IConcessionService concessionService)
    {
        _concessionService = concessionService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,STORE_MANAGER,COUNTER_OPERATOR")]
    [ProducesResponseType(typeof(IEnumerable<ConcessionPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var policies = await _concessionService.GetPoliciesAsync();
        return Ok(policies);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "ADMIN,STORE_MANAGER,COUNTER_OPERATOR")]
    [ProducesResponseType(typeof(ConcessionPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(string id)
    {
        var policy = await _concessionService.GetPolicyByIdAsync(id);
        if (policy == null) return NotFound(new { message = $"Concession policy '{id}' not found." });
        return Ok(policy);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,STORE_MANAGER")]
    [ProducesResponseType(typeof(ConcessionPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateConcessionPolicyDto dto)
    {
        var updated = await _concessionService.UpdatePolicyAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Concession policy '{id}' not found." });
        return Ok(updated);
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
    [AllowAnonymous]
    [ProducesResponseType(typeof(EmployeeVerificationResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyConcession([FromBody] EmployeeVerificationRequestDto dto)
    {
        var result = await _concessionService.VerifyEmployeeAsync(dto);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,STORE_MANAGER,COUNTER_OPERATOR")]
    [ProducesResponseType(typeof(IEnumerable<RailwayEmployeeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? search)
    {
        var employees = await _concessionService.GetAllEmployeesAsync(status, search);
        return Ok(employees);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "ADMIN,STORE_MANAGER,COUNTER_OPERATOR")]
    [ProducesResponseType(typeof(RailwayEmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(string id)
    {
        var emp = await _concessionService.GetEmployeeByIdAsync(id);
        if (emp == null) return NotFound(new { message = $"Railway employee '{id}' not found." });
        return Ok(emp);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RailwayEmployeeResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterEmployeeDto dto)
    {
        var created = await _concessionService.RegisterEmployeeAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.EmployeeId }, created);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "ADMIN,STORE_MANAGER")]
    [ProducesResponseType(typeof(RailwayEmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] EmployeeStatusUpdateDto dto)
    {
        if (dto.Status != "ACTIVE" && dto.Status != "REJECTED" && dto.Status != "PENDING")
        {
            return BadRequest(new { message = "Status must be ACTIVE, REJECTED, or PENDING." });
        }

        var updated = await _concessionService.UpdateEmployeeStatusAsync(id, dto);
        if (updated == null) return NotFound(new { message = $"Railway employee '{id}' not found." });
        return Ok(updated);
    }

    [HttpPut("{id}/device-lock")]
    [Authorize(Roles = "ADMIN,STORE_MANAGER")]
    [ProducesResponseType(typeof(RailwayEmployeeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleDeviceLock(string id, [FromBody] EmployeeDeviceLockDto? dto)
    {
        var updated = await _concessionService.ToggleDeviceLockAsync(id, dto?.DeviceLocked);
        if (updated == null) return NotFound(new { message = $"Railway employee '{id}' not found." });
        return Ok(updated);
    }
}

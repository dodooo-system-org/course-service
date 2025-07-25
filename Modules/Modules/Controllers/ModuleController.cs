using System;
using Microsoft.AspNetCore.Mvc;
using course_service.Data.Entities;
using course_service.Modules.Modules.DTOs;
using course_service.Attributes;
using course_service.Modules.Modules.Interfaces;

namespace course_service.Modules.Modules.Controllers
{
    [Route("api/module")]
    [ApiController]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModuleController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpPost]
        [RequireAdmin]
        public async Task<ActionResult<ModuleEntity>> CreateOne([FromBody] CreateModuleDto moduleDto)
        {
            var module = await _moduleService.CreateOneAsync(moduleDto);
            return CreatedAtAction(nameof(GetOne), new { id = module.ModuleId }, module);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModuleEntity>>> GetList(Guid courseId)
        {
            var modules = await _moduleService.GetListAsync(courseId);
            return Ok(modules);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ModuleEntity>> GetOne(Guid id)
        {
            var module = await _moduleService.GetOneAsync(id);
            return Ok(module);
        }

        [HttpPut("{id}")]
        [RequireAdmin]
        public async Task<ActionResult<ModuleEntity>> UpdateOne(Guid id, [FromBody] UpdateModuleDto moduleDto)
        {
            var updatedModule = await _moduleService.UpdateOneAsync(id, moduleDto);
            return Ok(updatedModule);
        }
    }
}

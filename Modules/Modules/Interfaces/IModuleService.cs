using System;
using course_service.Data.Entities;
using course_service.Modules.Modules.DTOs;

namespace course_service.Modules.Modules.Interfaces;

public interface IModuleService
{
    Task<ModuleDto> CreateOneAsync(CreateModuleDto module);
    Task<ModuleDto> GetOneAsync(Guid id);
    Task<IEnumerable<ModuleDto>> GetListAsync();
    Task<ModuleDto> UpdateOneAsync(Guid id, UpdateModuleDto module);
}

using System;
using course_service.Data.Entities;
using course_service.Shared.DTOs;

namespace course_service.Modules.Category.DTOs;

public class UpdateCategoryDto : CreateCategoryDto
{
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

using System;
using course_service.Data.Entities;

namespace course_service.Modules.Category.DTOs;

public class UpdateCategoryDto : CreateCategoryDto
{
    public CategoryStatus Status { get; set; }
}

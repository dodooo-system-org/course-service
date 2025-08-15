using System;
using course_service.Shared.DTOs;

namespace course_service.Modules.Category.DTOs;

public class GetListCategoryDto : PaginationDto
{
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
}

